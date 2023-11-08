using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Microsoft.Playwright.NUnit;
using Newtonsoft.Json;
using NUnit.Framework;
using tests;
using Tests;

namespace PlaywrightTests;

[TestFixture]
public class UpdateTests : PageTest
{
  
    //API test: Now we're not using the frontend, so we're "isolating" from the API layer and down (just using HttpClient, no Playwright)
    [TestCase("aslkdjlksadj", "Bob", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    [TestCase("dfgfdgfdgfdgfdgfdgfdg", "Rob", "https://asdsajdlksadjsalkjdlksadj.com/img.png", "bla bla bla")]
    [TestCase("exciting headline", "Dob", "https://coolimage.com/img.jpg", "cool story")]
    [TestCase("clickbaity headline", "Lob", "https://coolimage.com/img.jpg", "cool story bro")]
    public async Task ArticleCanSuccessfullyBeUpdatedFromHttpRequest(string headline, string author, string imgurl,
        string body)
    {
        //ARRANGE
        Helper.TriggerRebuild();
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            //Insert an article to be updated
            conn.QueryFirst<Article>(
                "INSERT INTO news.articles (headline, body, author, articleimgurl) VALUES ('hardcodedHeadline', 'hardcodedBody', 'hardCodedAuthor', 'hardcodedArticleImgUrl') RETURNING *;");
        }

        var testArticle = new Article()
        {
            ArticleId = 1, ArticleImgUrl = imgurl, Author = author, Body = body, Headline = headline
        };

        //ACT
        var httpResponse = await new HttpClient().PutAsJsonAsync(Helper.ApiBaseUrl + "/articles/1", testArticle);
        var articleFromResponseBody =
            JsonConvert.DeserializeObject<Article>(await httpResponse.Content.ReadAsStringAsync());


        //ASSERT
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            conn.QueryFirst<Article>("SELECT * FROM news.articles;").Should()
                .BeEquivalentTo(articleFromResponseBody); //Should be equal to article found in DB
        }
    }

    
  
    
    //Here we're testing that the API returns a bad request response and artiel is not updated when presented with bad values
    [TestCase("aslkdjlksadj", "NotAValidAuthorNameHere", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    [TestCase("", "NotAValidAuthorNameHere", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    public async Task ServerSideDataValidationShouldRejectBadValues(string headline, string author, string imgurl,
        string body)
    {
        //ARRANGE
        Helper.TriggerRebuild();
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            //Insert an article to be updated
            conn.QueryFirst<Article>(
                "INSERT INTO news.articles (headline, body, author, articleimgurl) VALUES ('hardcodedHeadline', 'hardcodedBody', 'hardCodedAuthor', 'hardcodedArticleImgUrl') RETURNING *;");
        }
        
        var testArticle = new Article()
        {
            ArticleId = 1, ArticleImgUrl = imgurl, Author = author, Body = body, Headline = headline
        };

        //ACT
        var httpResponse = await new HttpClient().PutAsJsonAsync(Helper.ApiBaseUrl + "/articles/1", testArticle);


        //ASSERT
        httpResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

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
        var httpResponse = await new HttpClient().PostAsJsonAsync(Helper.ApiBaseUrl + "/articles/1", testArticle);


        //ASSERT
        httpResponse.Should().HaveError();
    }

  
}
