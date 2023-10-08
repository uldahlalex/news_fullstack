using Dapper;
using infrastructure.DataModels;
using infrastructure.QueryModels;
using Npgsql;

namespace infrastructure.Repositories;

public class ArticleRepository
{
    private NpgsqlDataSource _dataSource;

    public ArticleRepository(NpgsqlDataSource datasource)
    {
        _dataSource = datasource;
    }

    public bool IsHeadlineTaken(string headline)
    {
        using (var conn = _dataSource.OpenConnection())
        {
            return conn.ExecuteScalar<int>("SELECT COUNT(*) FROM news.articles WHERE headline = @headline;",
                new { headline }) != 0;
        }
    }

    public IEnumerable<NewsFeedItem> GetArticlesForFeed()
    {
        string sql = $@"
SELECT articleid as {nameof(NewsFeedItem.ArticleId)},
       headline as {nameof(NewsFeedItem.Headline)},
    articleimgurl as {nameof(NewsFeedItem.ArticleImgUrl)},
    LEFT(body, 50) as  {nameof(NewsFeedItem.Body)}
FROM news.articles;
";
        using (var conn = _dataSource.OpenConnection())
        {
            return conn.Query<NewsFeedItem>(sql);
        }
    }


    public Article UpdateArticle(string headline, int articleId, string articleImgUrl, string author, string body)
    {
        var sql = $@"
UPDATE news.articles SET body = @body, headline = @headline, articleimgurl = @articleImgUrl, author = @author
WHERE articleid = @articleId
RETURNING articleid as {nameof(Article.ArticleId)},
    body as {nameof(Article.Body)},
       headline as {nameof(Article.Headline)},
        author as {nameof(Article.Author)},
        articleimgurl as {nameof(Article.ArticleImgUrl)};
";

        using (var conn = _dataSource.OpenConnection())
        {
            return conn.QueryFirst<Article>(sql, new { headline, articleId, body, articleImgUrl, author });
        }
    }

    public Article CreateArticle(string headline, string body, string author, string articleImgUrl)
    {
        var sql = $@"
INSERT INTO news.articles (headline, body, author, articleimgurl) 
VALUES (@headline, @body, @author, @articleImgUrl)
RETURNING articles.articleid as {nameof(Article.ArticleId)},
    body as {nameof(Article.Body)},
       headline as {nameof(Article.Headline)},
        author as {nameof(Article.Author)},
        articleimgurl as {nameof(Article.ArticleImgUrl)};
";
        using (var conn = _dataSource.OpenConnection())
        {
            return conn.QueryFirst<Article>(sql, new { headline, author, articleImgUrl, body });
        }
    }

    public bool DeleteArticle(int articleId)
    {
        var sql = @"DELETE FROM news.articles WHERE articleid = @articleId;";
        using (var conn = _dataSource.OpenConnection())
        {
            return conn.Execute(sql, new { articleId }) == 1;
        }
    }

    public bool DoesArticleWithHeadlineExist(string headline)
    {
        var sql = @"SELECT COUNT(*) FROM news.articles WHERE headline = @headline;";
        using (var conn = _dataSource.OpenConnection())
        {
            return conn.ExecuteScalar<int>(sql, new { headline }) == 1;
        }
    }

    public IEnumerable<SearchArticleItem> GetArticles(string searchTerm, int pageSize)
    {
        var sql = $@"
SELECT articleid as {nameof(SearchArticleItem.ArticleId)},
       headline as {nameof(SearchArticleItem.Headline)}, 
       author as {nameof(SearchArticleItem.Author)}
FROM news.articles
WHERE headline LIKE LOWER(@searchTerm) OR body LIKE LOWER(@searchTerm) LIMIT @pageSize;
";

        using (var conn = _dataSource.OpenConnection())
        {
            return conn.Query<SearchArticleItem>(sql, 
                new {searchTerm = '%'+searchTerm+'%', pageSize});
        }

    }

    public Article getArticleById(int articleId)
    {
        var sql = $@"SELECT articleid as {nameof(Article.ArticleId)},
    body as {nameof(Article.Body)},
       headline as {nameof(Article.Headline)},
        author as {nameof(Article.Author)},
        articleimgurl as {nameof(Article.ArticleImgUrl)} FROM news.articles WHERE articleid = @articleId;";
        using (var conn = _dataSource.OpenConnection())
        {
            return conn.QueryFirst<Article>(sql, new {articleId});
        }
    }
}