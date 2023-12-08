using Dapper;
using FluentAssertions;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Tests;

namespace tests;

[TestFixture]
public class UpdateTests : PageTest
{
    [TestCase("aslkdjlksadj", "Bob", "sadjsalkdj", "salkdjlsakdjskladjlk", "blabla")]
    public async Task ArticleCanSuccessfullyBeUpdated(string headline, string author, string imgurl, string body,
        string newHeadline)
    {
        //ARRANGE
        Helper.TriggerRebuild();
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            //Insert an article to update
            conn.QueryFirst<Article>(
                "INSERT INTO news.articles (headline, body, author, articleimgurl) VALUES (@headline, @body, @author, @imgurl) RETURNING *;",
                new { headline, author, imgurl, body });
        }

        //ACT
        await Page.GotoAsync(Helper.ClientAppBaseUrl);
        await Page.GetByTestId("card_"+headline).ClickAsync();
        await Page.GetByTestId("open_edit").ClickAsync();
        await Page.GetByTestId("edit_headline_form").Locator("input").FillAsync(headline);
        await Page.GetByTestId("edit_author_form").Locator("input").FillAsync(author);
        await Page.GetByTestId("edit_body_form").Locator("input").FillAsync(body);
        await Page.GetByTestId("edit_img_form").Locator("input").FillAsync(imgurl);
        await Page.GetByTestId("edit_submit_form").ClickAsync();
        await Page.GotoAsync(Helper.ClientAppBaseUrl);

        //ASSERT
        //Article in DB is as is expected
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            conn.QueryFirst<Article>("SELECT * FROM news.articles").Should()
                .BeEquivalentTo(new Article()
                    { ArticleId = 1, ArticleImgUrl = imgurl, Author = author, Body = body, Headline = headline });
        }

        //Article with new headline is present on feed after update
        await Expect(Page.GetByTestId("card_" + headline)).ToBeVisibleAsync();
    }
    

    
    //When the form validation is violated, the submit cannot be clicked
    [TestCase("aslkdjlksadj", "NotAValidAuthorNameHere", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    [TestCase("", "NotAValidAuthorNameHere", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    public async Task ClientSideDataValidationShouldRejectBadValues(string headline, string author, string imgurl,
        string body)
    {
        Helper.TriggerRebuild();
       var article = new Article() { ArticleId = 1, ArticleImgUrl = imgurl, Author = author, Body = body, Headline = headline };

        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            //Insert an article to be updated
            conn.QueryFirst<Article>(
                "INSERT INTO news.articles (headline, body, author, articleimgurl) VALUES " +
                "(@headline, @body, @author, @articleimgurl) RETURNING *;", article);
        }

        await Page.GotoAsync(Helper.ClientAppBaseUrl);
        await Page.GetByTestId("card_"+headline).ClickAsync();
        await Page.GetByTestId("open_edit").ClickAsync();
        await Page.GetByTestId("edit_headline_form").Locator("input").FillAsync(headline);
        await Page.GetByTestId("edit_author_form").Locator("input").FillAsync(author);
        await Page.GetByTestId("edit_body_form").Locator("input").FillAsync(body);
        await Page.GetByTestId("edit_img_form").Locator("input").FillAsync(imgurl);
        await Expect(Page.GetByTestId("edit_submit_form")).ToHaveAttributeAsync("aria-disabled", "true");
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
    }
}
