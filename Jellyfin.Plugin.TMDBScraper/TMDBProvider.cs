using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.Movies;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Providers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using Jellyfin.Plugin.TMDBScraper.Models;
using Jellyfin.Data.Enums;

namespace Jellyfin.Plugin.TMDBScraper
{
    public class TMDBProvider : IRemoteMetadataProvider<Movie, MovieInfo>
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TMDBProvider> _logger;
        private readonly string _apiKey;
        private const string BaseUrl = "https://api.tmdb.org/3";
        private const string ImageBaseUrl = "https://image.tmdb.org/t/p/original";

        public TMDBProvider(IHttpClientFactory httpClientFactory, ILogger<TMDBProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiKey = Plugin.Instance.Configuration.TMDBApiKey;
        }

        public string Name => "TMDB Scraper";

        public Task<HttpResponseMessage> GetImageResponse(string url, CancellationToken cancellationToken)
        {
            return _httpClientFactory.CreateClient().GetAsync(url, cancellationToken);
        }

        public async Task<MetadataResult<Movie>> GetMetadata(MovieInfo info, CancellationToken cancellationToken)
        {
            try
            {
                var result = new MetadataResult<Movie>();
                var movie = new Movie();
                result.Item = movie;

                // 1. 搜索电影
                var searchUrl = $"{BaseUrl}/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(info.Name)}&language={Plugin.Instance.Configuration.PreferredLanguage}";
                var searchResponse = await GetJsonResponse<TMDBMovieSearchResponse>(searchUrl);

                if (searchResponse?.Results == null || !searchResponse.Results.Any())
                {
                    return result;
                }

                var tmdbMovie = searchResponse.Results.First();

                // 2. 获取详细信息
                var movieUrl = $"{BaseUrl}/movie/{tmdbMovie.Id}?api_key={_apiKey}&language={Plugin.Instance.Configuration.PreferredLanguage}";
                var movieDetails = await GetJsonResponse<TMDBMovie>(movieUrl);

                // 3. 获取演职人员信息
                var creditsUrl = $"{BaseUrl}/movie/{tmdbMovie.Id}/credits?api_key={_apiKey}";
                var credits = await GetJsonResponse<TMDBCredits>(creditsUrl);

                // 填充电影信息
                movie.Name = movieDetails.Title;
                movie.Overview = movieDetails.Overview;
                if (DateTime.TryParse(movieDetails.ReleaseDate, out var releaseDate))
                {
                    movie.PremiereDate = releaseDate;
                    movie.ProductionYear = releaseDate.Year;
                }

                // 添加图片
                if (!string.IsNullOrEmpty(movieDetails.PosterPath))
                {
                    result.Item.SetImage(new ItemImageInfo
                    {
                        Path = ImageBaseUrl + movieDetails.PosterPath,
                        Type = ImageType.Primary
                    }, 0);
                }
                if (!string.IsNullOrEmpty(movieDetails.BackdropPath))
                {
                    result.Item.SetImage(new ItemImageInfo
                    {
                        Path = ImageBaseUrl + movieDetails.BackdropPath,
                        Type = ImageType.Backdrop
                    }, 0);
                }

                // 添加演员信息
                if (credits?.Cast != null)
                {
                    var castMembers = credits.Cast.Select(cast => new PersonInfo
                    {
                        Name = cast.Name,
                        Role = cast.Character,
                        Type = PersonKind.Actor,
                        ImageUrl = !string.IsNullOrEmpty(cast.ProfilePath) ? ImageBaseUrl + cast.ProfilePath : null,
                        ProviderIds = new Dictionary<string, string> { { "tmdb", cast.Id.ToString() } }
                    }).ToList();

                    foreach (var person in castMembers)
                    {
                        result.AddPerson(person);
                    }
                }

                // 添加导演信息
                if (credits?.Crew != null)
                {
                    var directors = credits.Crew.Where(c => c.Job == "Director").Select(director => new PersonInfo
                    {
                        Name = director.Name,
                        Type = PersonKind.Director,
                        ImageUrl = !string.IsNullOrEmpty(director.ProfilePath) ? ImageBaseUrl + director.ProfilePath : null,
                        ProviderIds = new Dictionary<string, string> { { "tmdb", director.Id.ToString() } }
                    });

                    foreach (var director in directors)
                    {
                        result.AddPerson(director);
                    }
                }

                result.HasMetadata = true;
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting metadata for {Name}", info.Name);
                throw;
            }
        }

        public async Task<IEnumerable<RemoteSearchResult>> GetSearchResults(MovieInfo searchInfo, CancellationToken cancellationToken)
        {
            var results = new List<RemoteSearchResult>();
            try
            {
                var searchUrl = $"{BaseUrl}/search/movie?api_key={_apiKey}&query={Uri.EscapeDataString(searchInfo.Name)}&language={Plugin.Instance.Configuration.PreferredLanguage}";
                var response = await GetJsonResponse<TMDBMovieSearchResponse>(searchUrl);

                if (response?.Results != null)
                {
                    results.AddRange(response.Results.Select(movie => new RemoteSearchResult
                    {
                        Name = movie.Title,
                        ImageUrl = !string.IsNullOrEmpty(movie.PosterPath) ? ImageBaseUrl + movie.PosterPath : null,
                        Overview = movie.Overview,
                        PremiereDate = DateTime.TryParse(movie.ReleaseDate, out var date) ? date : null
                    }));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for {Name}", searchInfo.Name);
            }

            return results;
        }

        private async Task<T> GetJsonResponse<T>(string url)
        {
            using var client = _httpClientFactory.CreateClient();
            var response = await client.GetStringAsync(url);
            return JsonConvert.DeserializeObject<T>(response);
        }
    }
} 