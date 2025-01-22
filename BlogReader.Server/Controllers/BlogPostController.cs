using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Security;
using System.Runtime.InteropServices;


namespace BlogReader.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BlogPostController(ILogger<BlogPostController> logger) : ControllerBase
{
    private readonly ILogger<BlogPostController> _logger = logger;

    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        ReferenceHandler = ReferenceHandler.Preserve,
        WriteIndented = true
    };

    /// <summary>
    /// Obtém o último post de cada feed de blog e devolve-os em formato JSON
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    [HttpGet]
    public async Task<IActionResult> GetAsync()

    {
        try
        {
            string[] feedUrlList = await GetFeedUrlListAsync("BlogsFeedUrl.txt");

            var posts = await GetJsonPostsAsync(feedUrlList);

            return Ok(posts.Value);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

    }

    /// <summary>
    /// Lê os feeds de origem, captura o último post de cada feed e devolve-os em formato JSON
    /// </summary>
    /// <param name="feedUrlList">A lista de url dos feeds dos blogs</param>
    /// <returns>Uma string com no formato JSON</returns>
    private async Task<JsonResult> GetJsonPostsAsync(string[] feedUrlList)
    {
        try
        {
            // obtém os feeds a partir das urls
            var feeds = await GetSyndicationFeedsAsync(feedUrlList);

            // obtém o primeiro post de cada feed
            var feedItems = GetFirstPostFromFeeds(feeds);
           
            return new JsonResult(feedItems, _serializerOptions);

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }
    }

    /// <summary>
    /// Obtém o primeiro post de cada feed e retorna uma lista de objetos com os dados formatados
    /// </summary>
    /// <param name="feeds">Uma lista com os feeds</param>
    /// <returns></returns>
    private List<Object> GetFirstPostFromFeeds(List<SyndicationFeed> feeds)
    {
        var objectList = new List<Object>();
        
        foreach (var feed in feeds)
        {
            try
            {
                objectList.Add(new
                {
                    Title = feed.Title.Text,
                    Items = feed.Items.Select(item => new
                    {
                        Title = item.Title.Text,
                        Summary = item.Summary.Text,
                        PublishDate = item.PublishDate.ToString("d"),
                        Link = item.Links.First().Uri.ToString(),
                    }).First()
                });
            }
            catch (Exception ex)
            {
               _logger.LogError($"Não foi possíve ler o feed do blog {feed.Title.Text}.\nErro: {ex.Message}");
            }
        }
        return objectList;
    }

    /// <summary>
    /// Obtém os feeds a partir de uma lista de urls passadas como parâmetro
    /// </summary>
    /// <param name="feedUrlList"></param>
    /// <returns>
    /// A lista de feeds obtidos
    /// </returns>
    /// <exception cref="Exception"></exception>
    private static async Task<List<SyndicationFeed>> GetSyndicationFeedsAsync(string[] feedUrlList)
    {
        var feeds = new List<SyndicationFeed>();
        foreach (var feedUrl in feedUrlList)
        {
            try
            {
                feeds.Add(await GetSyndicationFeedAsync(feedUrl));
            }
            catch
            {
                throw;
            }
        }
        return feeds;
    }

    /// <summary>
    /// Obtém o feed/rss da url de origem passada como parâmetro
    /// </summary>
    /// <param name="sourceUrl">url do feed/rss</param>
    /// <returns>Um objeto SyndicationFeed que representa o feed/rss obtido da url passada como parâmetro</returns>
    /// <exception cref="ArgumentNullException">A url do feed não foi passada</exception>
    /// <exception cref="SecurityException">Sem permissão para acessar a url do feed/rss</exception>
    /// <exception cref="FileNotFoundException">Arquivo não encontrado na url de origem</exception>
    /// <exception cref="UriFormatException">A url do feed/rss fornecida não é válida. Formato incorreto</exception>
    /// <exception cref="HttpRequestException">Endereço não localizado</exception>"
    private static async Task<SyndicationFeed> GetSyndicationFeedAsync(string sourceUrl)
    {
        try
        {
            using XmlReader reader = XmlReader.Create(sourceUrl);
            SyndicationFeed feed = await Task.Run(() => SyndicationFeed.Load(reader));
            return feed;
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is SecurityException ||
                                   ex is FileNotFoundException || ex is UriFormatException ||
                                   ex is HttpRequestException)
        {
            throw ex switch
            {
                ArgumentNullException argumentEx => new("A url do feed não foi passada.", argumentEx),
                SecurityException securityEx => new("Sem permissão para acessar a url do feed/rss", securityEx),
                FileNotFoundException fileEx => new($"Arquivo não encontrado na url de origem: {sourceUrl}", fileEx),
                UriFormatException uriEx => new($"A url do feed/rss fornecida não é válida. Formato incorreto: {sourceUrl}", uriEx),
                HttpRequestException httpRequestEx => new($"Endereço não localizado.\nERRO MSG: {httpRequestEx.Message} ", httpRequestEx),
                _ => new Exception(ex.Message),
            };
        }
    }

    /// <summary>
    /// Obtém a partir de um arquivo de texto (.txt) as url de origem dos feeds. 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>Um array de string com as urls dos feeds</returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="Exception"></exception>
    /// <remarks>As urls no arquivo .txt devem estar separadas por ;(ponto e virgula), quebra de linha, ou espaço em branco</remarks>
    private static async Task<string[]> GetFeedUrlListAsync(string filePath)
    {
        try
        {
            using StreamReader reader = new(filePath);

            string fileContent = await reader.ReadToEndAsync();

            return [.. fileContent.Split(['\r', '\n', ';', ' '], StringSplitOptions.RemoveEmptyEntries)];
        }
        catch (Exception ex) when (ex is FileNotFoundException || ex is IOException)
        {
            switch (ex)
            {
                case FileNotFoundException fileEx:
                    throw new FileNotFoundException($"O arquivo {filePath} não foi encontrado", fileEx);
                case IOException ioEx:
                    throw new Exception($"Erro de leitura no arquivo {filePath}", ioEx);
                default: throw;
            }
        }
    }
}
