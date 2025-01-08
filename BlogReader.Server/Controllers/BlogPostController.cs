using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.ServiceModel.Syndication;
using System.Security;
using System.Runtime.CompilerServices;

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
    


    [HttpGet]
    public string Get()
    {
        try
        {
            string[] feedList = GetFeedList("BlogsFeedUrl.txt");

            var posts = new string[feedList.Length];

            for (int i = 0; i < posts.Length; i++)
                try { posts[i] = GetJsonPost(feedList[i]); }
                catch (Exception) { }

            return "[" + string.Join(",\n", posts) + "]";

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message, ex);
        }

    }

    /// <summary>
    /// Lê o feed de origem, captura o último post e devolve-o em formato JSON
    /// </summary>
    /// <param name="feed">Link do feed do blog</param>
    /// <returns>Uma string com no formato JSON</returns>
    public string GetJsonPost(string feedUrl)
    {

        SyndicationFeed feed = GetFeed(feedUrl);

        var feedItems = new
        {
            Title = feed.Title.Text,
            Items = feed.Items.Select(item => new
            {
                Title = item.Title.Text,
                Summary = item.Summary.Text,
                PublishDate = item.PublishDate.ToString("d"),
                Link = item.Links.First().Uri.ToString(),
            }).First()
        };

        return SerializerPost(feedItems);
    }

    /// <summary>
    /// Tranforma em formato JSON um objeto que representam um item de feed/rss.
    /// </summary>
    /// <param name="feedItem">objeto que representa um item de feeds/rss</param>
    /// <returns>String no formato JSON</returns>
    /// <exception cref="NotSupportedException"></exception>
    public static string SerializerPost(object feedItem)
    {
        try
        {
            return JsonSerializer.Serialize(feedItem, _serializerOptions);
        }
        catch (NotSupportedException ex)
        {
            throw new NotSupportedException("Operação de serialização não suportada.", ex);
        }
    }

    /// <summary>
    /// Obtém o feed/rss a partir de um endereço web 
    /// </summary>
    /// <param name="sourceUrl">url do feed/rss</param>
    /// <returns>O feed/rss</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="SecurityException"></exception>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="UriFormatException"></exception>
    private static SyndicationFeed GetFeed(string sourceUrl)
    {
        try
        {
            XmlReader reader = XmlReader.Create(sourceUrl);
            SyndicationFeed feed = SyndicationFeed.Load(reader);
            reader.Close();
            return feed;
        }
        catch (Exception ex) when (ex is ArgumentNullException || ex is SecurityException ||
                                   ex is FileNotFoundException || ex is UriFormatException || 
                                   ex is HttpRequestException)
        {
            switch (ex)
            {
                case ArgumentNullException argumentEx:
                    throw new ArgumentNullException("A url do feed não foi passada.", argumentEx);
                case SecurityException securityEx:
                    throw new SecurityException("Sem permissão para acessar a url do feed/rss", securityEx);
                case FileNotFoundException fileEx:
                    throw new FileNotFoundException($"Arquivo não encontrado na url de origem: {sourceUrl}", fileEx);
                case UriFormatException uriEx:
                    throw new UriFormatException($"A url do feed/rss fornecida não é válida. Formato incorreto: {sourceUrl}", uriEx);
                case HttpRequestException httpRequestEx:
                    throw new HttpRequestException($"Endereço não localizado.\nERRO MSG: {httpRequestEx.Message} ", httpRequestEx);
                default:
                    throw;
            }
        }
    }

    /// <summary>
    /// Obtém a partir de um arquivo de texto (.txt) as url de origem dos feeds. 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns>Um array contendo os endereços web dos feed</returns>
    /// <exception cref="FileNotFoundException"></exception>
    /// <exception cref="Exception"></exception>
    /// <remarks>As urls no arquivo .txt devem estar separadas por ;(ponto e virgula), quebra de linha, ou espaço em branco</remarks>
    public static string[] GetFeedList(string filePath)
    {
        try
        {
            using StreamReader reader = new(filePath);
            string fileContent = reader.ReadToEnd();
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
