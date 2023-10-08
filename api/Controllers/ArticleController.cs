using api.TransferModels;
using infrastructure.DataModels;
using infrastructure.QueryModels;
using Microsoft.AspNetCore.Mvc;
using service;

namespace api.Controllers;

[ApiController]
public class ArticleController : ControllerBase
{
    private readonly ArticleService _articleService;

    public ArticleController(ArticleService articleService)
    {
        _articleService = articleService;
    }


    [HttpGet]
    [Route("/api/feed")]
    public IEnumerable<NewsFeedItem> GetFeed()
    {
        return _articleService.GetArticlesForFeed();
    }

    [HttpGet]
    [Route("/api/articles")]
    public IEnumerable<SearchArticleItem> Get([FromQuery] ArticleSearchRequestDto dto)
    {
        return _articleService.SearchForArticles(dto.SearchTerm, dto.PageSize);
    }

    [HttpGet]
    [Route("/api/articles/{articleId}")]
    public Article Get([FromRoute] int articleId)
    {
        return _articleService.GetArticle(articleId);
    }

    [HttpPost]
    [Route("/api/articles")]
    public Article Post([FromBody] CreateArticleRequestDto dto)
    {
        HttpContext.Response.StatusCode = StatusCodes.Status201Created;
        return _articleService.CreateArticle(dto.Headline, dto.Body, dto.ArticleImgUrl, dto.Author);
    }

    [HttpPut]
    [Route("/api/articles/{articleId}")]
    public Article Put(
        [FromRoute] int articleId,
        [FromBody] UpdateArticleRequestDto dto)
    {
        return
            _articleService.UpdateArticle(dto.Headline, articleId, dto.ArticleImgUrl, dto.Author, dto.Body);
    }

    [HttpDelete]
    [Route("/api/articles/{articleId}")]
    public object Delete([FromRoute] int articleId)
    {
        _articleService.DeleteArticle(articleId);
        return new { message = "Article has been deleted" };
    }
}