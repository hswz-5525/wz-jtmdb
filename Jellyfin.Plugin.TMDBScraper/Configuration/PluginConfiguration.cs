using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TMDBScraper.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public string TMDBApiKey { get; set; }
        public string PreferredLanguage { get; set; }

        public PluginConfiguration()
        {
            TMDBApiKey = "";
            PreferredLanguage = "zh-CN";
        }
    }
} 