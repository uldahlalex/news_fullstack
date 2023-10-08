using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using Newtonsoft.Json;
using NUnit.Framework;
using tests;
using Tests;

namespace PlaywrightTests;

[TestFixture]
public class CreateTests : PageTest
{
    //Here we're using the entire stack from UI and down (using Playwright)
    [TestCase("aslkdjlksadj", "Bob", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    [TestCase("dfgfdgfdgfdgfdgfdgfdg", "Rob", "https://asdsajdlksadjsalkjdlksadj.com/img.png", "bla bla bla")]
    [TestCase("exciting headline", "Dob", "https://coolimage.com/img.jpg", "cool story")]
    [TestCase("clickbaity headline", "Lob", "https://coolimage.com/img.jpg", "cool story bro")]
    public async Task ArticleCanSuccessfullyBeCreatedFromUi(string headline, string author, string imgurl, string body)
    {
        //ARRANGE
        Helper.TriggerRebuild();

        //ACT
        await Page.GotoAsync(Helper.ClientAppBaseUrl);
        await Page.GetByTestId("create_button").ClickAsync();
        await Page.GetByTestId("create_headline_form").Locator("input").FillAsync(headline);
        await Page.GetByTestId("create_author_form").Locator("input").FillAsync(author);
        await Page.GetByTestId("create_body_form").Locator("input").FillAsync(body);
        await Page.GetByTestId("create_img_form").Locator("input").FillAsync(imgurl);
        await Page.GetByTestId("create_submit_form").ClickAsync();


        //ASSERT
        await Expect(Page.GetByTestId("card_" + headline)).ToBeVisibleAsync(); //Exists in UI after creation
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            var expected = new Article()
            {
                ArticleId = 1, ArticleImgUrl = imgurl, Author = author, Body = body, Headline = headline
            }; //Article object from test case

            conn.QueryFirst<Article>("SELECT * FROM news.articles;").Should()
                .BeEquivalentTo(expected); //Should be equal to article found in DB
        }
    }

    //API test: Now we're not using the frontend, so we're "isolating" from the API layer and down (just using HttpClient, no Playwright)
    [TestCase("aslkdjlksadj", "Bob", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    [TestCase("dfgfdgfdgfdgfdgfdgfdg", "Rob", "https://asdsajdlksadjsalkjdlksadj.com/img.png", "bla bla bla")]
    [TestCase("exciting headline", "Dob", "https://coolimage.com/img.jpg", "cool story")]
    [TestCase("clickbaity headline", "Lob", "https://coolimage.com/img.jpg", "cool story bro")]
    public async Task ArticleCanSuccessfullyBeCreatedFromHttpRequest(string headline, string author, string imgurl,
        string body)
    {
        //ARRANGE
        Helper.TriggerRebuild();
        var testArticle = new Article()
        {
            ArticleId = 1, ArticleImgUrl = imgurl, Author = author, Body = body, Headline = headline
        };

        //ACT
        var httpResponse = await new HttpClient().PostAsJsonAsync(Helper.ApiBaseUrl + "/articles", testArticle);
        var articleFromResponseBody =
            JsonConvert.DeserializeObject<Article>(await httpResponse.Content.ReadAsStringAsync());


        //ASSERT
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            conn.QueryFirst<Article>("SELECT * FROM news.articles;").Should()
                .BeEquivalentTo(articleFromResponseBody); //Should be equal to article found in DB
        }
    }


    //When the form validation is violated, the submit cannot be clicked
    [TestCase("aslkdjlksadj", "NotAValidAuthorNameHere", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    [TestCase("", "NotAValidAuthorNameHere", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    public async Task ClientSideDataValidationShouldRejectBadValues(string headline, string author, string imgurl,
        string body)
    {
        await Page.GotoAsync(Helper.ClientAppBaseUrl);
        await Page.GetByTestId("create_button").ClickAsync();
        await Page.GetByTestId("create_headline_form").Locator("input").FillAsync(headline);
        await Page.GetByTestId("create_author_form").Locator("input").FillAsync(author);
        await Page.GetByTestId("create_body_form").Locator("input").FillAsync(body);
        await Page.GetByTestId("create_img_form").Locator("input").FillAsync(imgurl);
        await Expect(Page.GetByTestId("create_submit_form")).ToHaveAttributeAsync("aria-disabled", "true");
    }


    //Here we're testing that the API returns a bad request response and no article is created when bad values are sent
    [TestCase("aslkdjlksadj", "NotAValidAuthorNameHere", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    [TestCase("", "NotAValidAuthorNameHere", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    public async Task ServerSideDataValidationShouldRejectBadValues(string headline, string author, string imgurl,
        string body)
    {
        //ARRANGE
        Helper.TriggerRebuild();
        var testArticle = new Article()
        {
            ArticleId = 1, ArticleImgUrl = imgurl, Author = author, Body = body, Headline = headline
        };

        //ACT
        var httpResponse = await new HttpClient().PostAsJsonAsync(Helper.ApiBaseUrl + "/articles", testArticle);


        //ASSERT
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            conn.ExecuteScalar<int>("SELECT COUNT(*) FROM news.articles;").Should()
                .Be(0); //DB should be empty when create failed
        }
    }

    [TestCase("aslkdjlksadj", "Bob", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    public async Task ApiShouldRejectArticleWhenHeadlineAlreadyExists(string headline, string author, string imgurl,
        string body)
    {
        //ARRANGE
        Helper.TriggerRebuild();
        var testArticle = new Article()
        {
            ArticleId = 1, ArticleImgUrl = imgurl, Author = author, Body = body, Headline = headline
        };
        using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            conn.Execute(
                "INSERT INTO news.articles (headline, body, author, articleimgurl) VALUES (@headline, @body, @author, @imgurl) RETURNING *;",
                new { headline, author, imgurl, body });
        }

        //ACT
        var httpResponse = await new HttpClient().PostAsJsonAsync(Helper.ApiBaseUrl + "/articles", testArticle);


        //ASSERT
        httpResponse.Should().HaveError();
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            conn.ExecuteScalar<int>("SELECT COUNT(*) FROM news.articles;").Should()
                .Be(1); //DB should have just the pre-existing article, and not also the new one
        }
    }

    [TestCase("aslkdjlksadj", "Bob", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    public async Task UIShouldPresentErrorToastWhenHeadlineAlreadyExists(string headline, string author, string imgurl,
        string body)
    {
        //ARRANGE
        Helper.TriggerRebuild();
        using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            conn.Execute(
                "INSERT INTO news.articles (headline, body, author, articleimgurl) VALUES (@headline, @body, @author, @imgurl) RETURNING *;",
                new { headline, author, imgurl, body });
        }
        
        //ACT
        await Page.GotoAsync(Helper.ClientAppBaseUrl);
        await Page.GetByTestId("create_button").ClickAsync();
        await Page.GetByTestId("create_headline_form").Locator("input").FillAsync(headline);
        await Page.GetByTestId("create_author_form").Locator("input").FillAsync(author);
        await Page.GetByTestId("create_body_form").Locator("input").FillAsync(body);
        await Page.GetByTestId("create_img_form").Locator("input").FillAsync(imgurl);
        await Page.GetByTestId("create_submit_form").ClickAsync();
        
        //ASSERT
        var toastCssClasses = await Page.Locator("ion-toast").GetAttributeAsync("class");
        var classes = toastCssClasses.Split(' ');
        classes.Should().Contain("ion-color-danger");
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            conn.ExecuteScalar<int>("SELECT COUNT(*) FROM news.articles;").Should().Be(1); //DB should have just the pre-existing article, and not also the new one
        }
    }
 
}