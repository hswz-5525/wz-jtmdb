using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Jellyfin.Plugin.TMDBScraper.Configuration;

namespace Jellyfin.Plugin.TMDBScraper
{
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        public override string Name => "TMDB Scraper";
        public override Guid Id => new Guid("b0daa707-5dd5-4474-9f81-87a2d456e8dd");
        public override string Description => "从TMDB获取电影元数据的刮削插件";
        public static Plugin Instance { get; private set; }

        public override PluginInfo GetPluginInfo()
        {
            return new PluginInfo(
                Name,
                Version,
                Description,
                Id,
                true
            );
        }
    }
} 