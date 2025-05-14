using HitScoreVisualizer.Installers;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using SiraUtil.Zenject;
using Logger = IPA.Logging.Logger;

namespace HitScoreVisualizer;

[Plugin(RuntimeOptions.DynamicInit), NoEnableDisable]
public class Plugin
{
	internal static PluginMetadata Metadata { get; private set; } = null!;
	internal static Logger Log { get; private set; } = null!;
	internal static PluginConfig Config { get; private set; } = null!;

	[Init]
	public Plugin(Logger logger, Config config, PluginMetadata pluginMetadata, Zenjector zenject)
	{
		Metadata = pluginMetadata;
		Log = logger;
		Config = config.Generated<PluginConfig>();

		zenject.UseLogger(logger);
		zenject.UseMetadataBinder<Plugin>();

		zenject.Install<HsvAppInstaller>(Location.App, Config);
		zenject.Install<HsvMenuInstaller>(Location.Menu);
		zenject.Install<HsvPlayerInstaller>(Location.Player);
	}
}