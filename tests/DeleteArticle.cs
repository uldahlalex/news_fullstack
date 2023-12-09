using Dapper;
using FluentAssertions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using Tests;

namespace tests;

[TestFixture]
public class DeleteTests : PageTest
{
    [TestCase("aslkdjlksadj", "Bob", "sadjsalkdj", "salkdjlsakdjskladjlk")]
    public async Task ArticleCanSuccessfullyBeDeletedFromUi(string headline, string author, string imgurl, string body)
    {
        //ARRANGE
        Helper.TriggerRebuild();
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            //Insert an article to remove from UI
            conn.QueryFirst<Article>(
                "INSERT INTO news.articles (headline, body, author, articleimgurl) VALUES (@headline, @body, @author, @imgurl) RETURNING *;",
                new { headline, author, imgurl, body });
        }

        //ACT 
        await Page.GotoAsync(Helper.ClientAppBaseUrl);
        var card = Page.GetByTestId("card_" + headline);
        await card.ClickAsync();
        await Page.GetByTestId("open_edit").ClickAsync();
        await Page.GetByTestId("delete_button").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Yes" }).ClickAsync(); //Clicking a confirm button
        await Page.GotoAsync(Helper.ClientAppBaseUrl); //Going back to feed, where we will make an expectation


        //ASSERT
        await Expect(card).Not.ToBeVisibleAsync(); //Article card is now nowhere to be found (notice the "Not" part)
        await using (var conn = await Helper.DataSource.OpenConnectionAsync())
        {
            conn.ExecuteScalar<int>("SELECT COUNT(*) FROM news.articles;").Should()
                .Be(0); //And the article is also gone from the DB
        }
    }
    
}