using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TMDBScraper.Configuration
{
    public class ConfigurationPage : PluginPageInfo
    {
        public ConfigurationPage()
        {
            Name = "TMDB Scraper";
            EnableInMainMenu = true;
            MenuSection = "server";
            MenuIcon = "movie";
            DisplayName = "TMDB Scraper 设置";
            EmbeddedResourcePath = GetType().Namespace + ".configPage";
        }
    }
} 