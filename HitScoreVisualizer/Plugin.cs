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
		internal static Version Version { get; private set; } = null!;

		[Init]
		public void Init(Logger logger, Config config, PluginMetadata pluginMetadata, Zenjector zenject)
		{
			Version = pluginMetadata.HVersion;

			zenject.UseLogger(logger);
			zenject.UseMetadataBinder<Plugin>();

			zenject.Install<HsvAppInstaller>(Location.App, config.Generated<HSVConfig>());
			zenject.Install<HsvMenuInstaller>(Location.Menu);
		}
	}
}