namespace infrastructure.DataModels;

public class Article
{
    public string Headline { get; set; }
    public string Body { get; set; }
    public int ArticleId { get; set; }
    public string ArticleImgUrl { get; set; }
    public string Author { get; set; }
    public DateTimeOffset CreatedAt { get; set; }

}