using HitScoreVisualizer.Installers;
using HitScoreVisualizer.Settings;
using Hive.Versioning;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using IPA.Logging;
using SiraUtil.Zenject;

namespace HitScoreVisualizer
{
	[Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
	public class Plugin
	{
		internal static PluginMetadata Metadata { get; private set; } = null!;
		internal static Logger Log { get; private set; } = null!;
		internal static HSVConfig Config { get; private set; } = null!;

		[Init]
		public Plugin(Logger logger, Config config, PluginMetadata pluginMetadata, Zenjector zenject)
		{
			Metadata = pluginMetadata;
			Log = logger;
			Config = config.Generated<HSVConfig>();

			zenject.UseLogger(logger);
			zenject.UseMetadataBinder<Plugin>();

			zenject.Install<HsvAppInstaller>(Location.App, Config);
			zenject.Install<HsvMenuInstaller>(Location.Menu);
		}
	}
}