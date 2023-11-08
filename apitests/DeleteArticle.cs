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
public class DeleteTests : PageTest
{


    [TestCase("aslkdjlksadj", "Bob", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    public async Task ArticleCanSuccessfullyBeDeletedFromHttpClient(string headline, string author, string imgurl,
        string body)
    {
        //ARRANGE
        Helper.TriggerRebuild();
        //Insert an article to remove from UI
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            conn.QueryFirst<Article>(
                "INSERT INTO news.articles (headline, body, author, articleimgurl) VALUES (@headline, @body, @author, @imgurl) RETURNING *;",
                new { headline, author, imgurl, body });
        }

        //ACT
        var httpResponse = await new HttpClient().DeleteAsync(Helper.ApiBaseUrl + "/articles/1");
        
        //ASSERT
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            httpResponse.Should().BeSuccessful();
            conn.ExecuteScalar<int>("SELECT COUNT(*) FROM news.articles;").Should().Be(0); //Should be gone from DB
        }
    }
    
}