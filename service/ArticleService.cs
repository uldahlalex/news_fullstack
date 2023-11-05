using System.ComponentModel.DataAnnotations;
using infrastructure.DataModels;
using infrastructure.QueryModels;
using infrastructure.Repositories;

namespace service;

public class ArticleService
{
    private readonly ArticleRepository _articleRepository;

    public ArticleService(ArticleRepository articleRepository )
    {
        _articleRepository = articleRepository;
    }

    public IEnumerable<NewsFeedItem> GetArticlesForFeed()
    {
        return _articleRepository.GetArticlesForFeed();
    }

    public Article CreateArticle(string headline, string body, string articleImgUrl, string author)
    {
        try
        {
            if (_articleRepository.IsHeadlineTaken(headline))
                throw new ValidationException("Article headline is taken");
            return _articleRepository.CreateArticle(headline, body, author, articleImgUrl);
      

        }
        catch (ValidationException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException?.Message);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException?.Message);
            throw new Exception("Could not create a new article");
        }
     
    }

    public Article UpdateArticle(string headline, int articleId, string articleImgUrl, string author, string body)
    {
        try
        {
            if (_articleRepository.IsHeadlineTaken(headline))
                throw new ValidationException("Article headline is taken");
            return _articleRepository.UpdateArticle(headline, articleId, body, author, articleImgUrl);

        }
        catch (ValidationException e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException?.Message);
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException?.Message);
            throw new Exception("Could not create a new article");
        }
    }

    public void DeleteArticle(int articleId)
    {
        var result = _articleRepository.DeleteArticle(articleId);
        if (!result)
        {
            throw new ArgumentException("Could not delete Article");
        }
    }

    public IEnumerable<SearchArticleItem> SearchForArticles(string searchTerm, int pageSize)
    {
        return _articleRepository.GetArticles(searchTerm, pageSize);
    }

    public Article GetArticle(int articleId)
    {
        return _articleRepository.GetArticleById(articleId);
    }
}