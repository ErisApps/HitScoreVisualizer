using System.Reflection;
using HarmonyLib;
using HitScoreVisualizer.Installers;
using HitScoreVisualizer.Settings;
using Hive.Versioning;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Loader;
using IPA.Logging;
using SiraUtil.Attributes;
using SiraUtil.Zenject;

namespace HitScoreVisualizer
{
	[Slog]
	[Plugin(RuntimeOptions.DynamicInit)]
	public class Plugin
	{
		private const string HARMONY_ID = "be.erisapps.HitScoreVisualizer";

		private static Harmony? _harmonyInstance;
		private static PluginMetadata? _metadata;

		public static string Name => _metadata?.Name!;
		public static Version Version => _metadata?.HVersion!;

		[Init]
		public void Init(Logger logger, Config config, PluginMetadata pluginMetadata, Zenjector zenject)
		{
			_metadata = pluginMetadata;

			zenject.OnApp<HsvAppInstaller>().WithParameters(logger, config.Generated<HSVConfig>());
			zenject.OnMenu<HsvMenuInstaller>();
			zenject.OnGame<HsvGameInstaller>();
		}

		[OnEnable]
		public void OnEnable()
		{
			_harmonyInstance = new Harmony(HARMONY_ID);
			_harmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
		}

		[OnDisable]
		public void OnDisable()
		{
			_harmonyInstance?.UnpatchSelf();
			_harmonyInstance = null;
		}
	}
}