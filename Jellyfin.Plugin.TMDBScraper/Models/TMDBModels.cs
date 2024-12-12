using Newtonsoft.Json;
using System.Collections.Generic;

namespace Jellyfin.Plugin.TMDBScraper.Models
{
    public class TMDBMovieSearchResponse
    {
        [JsonProperty("results")]
        public List<TMDBMovie> Results { get; set; }
    }

    public class TMDBMovie
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("overview")]
        public string Overview { get; set; }

        [JsonProperty("release_date")]
        public string ReleaseDate { get; set; }

        [JsonProperty("poster_path")]
        public string PosterPath { get; set; }

        [JsonProperty("backdrop_path")]
        public string BackdropPath { get; set; }
    }

    public class TMDBCredits
    {
        [JsonProperty("cast")]
        public List<TMDBCast> Cast { get; set; }

        [JsonProperty("crew")]
        public List<TMDBCrew> Crew { get; set; }
    }

    public class TMDBCast
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("character")]
        public string Character { get; set; }

        [JsonProperty("profile_path")]
        public string ProfilePath { get; set; }
    }

    public class TMDBCrew
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("job")]
        public string Job { get; set; }

        [JsonProperty("profile_path")]
        public string ProfilePath { get; set; }
    }
} 