using System.Net;
using System.Net.Http.Json;
using Dapper;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using tests;
using Tests;

namespace apitests;

[TestFixture]
public class CreateTests
{
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

  
 
}