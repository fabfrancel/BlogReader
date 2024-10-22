using BlogReader.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace BlogReader.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogFeedController : ControllerBase
{
    private readonly ILogger<BlogFeedController> _logger;

    public BlogFeedController(ILogger<BlogFeedController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Obtém as URLs de feed gravadas em um arquivo de texto.
    /// </summary>
    /// <returns>Uma lista com endereços de feed.</returns>
    /// <exception cref="Exception"></exception>

    [HttpGet(Name = "GetBlogFeeds")]
    public IEnumerable<BlogFeed> Get()
    {
        string filePath = "BlogsFeedUrl.txt";
        string[] feeds = BlogPostController.GetFeedList(filePath);

        return Enumerable.Range(0, feeds.Length).Select(i => new BlogFeed
        {
            feedUrl = feeds[i]
        }).ToArray();
    }
}
