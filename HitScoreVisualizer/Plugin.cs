using System.Linq;
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
		internal static HSVConfig? HSVConfig;
		internal static TMPro.TMP_FontAsset? BloomFontAsset;

		public static string Name => _metadata?.Name!;
		public static Version Version => _metadata?.HVersion!;

		[Init]
		public void Init(Logger logger, Config config, PluginMetadata pluginMetadata, Zenjector zenject)
		{
			_metadata = pluginMetadata;
			HSVConfig = config.Generated<HSVConfig>();

			zenject.OnApp<HsvAppInstaller>().WithParameters(logger, HSVConfig);
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
			_harmonyInstance?.UnpatchAll(HARMONY_ID);
			_harmonyInstance = null;
		}

		internal static TMPro.TMP_FontAsset SetupBloomFont()
		{
			if (BloomFontAsset is null)
			{
				var cachedFont = UnityEngine.Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>().First(x =>
				x.name.Contains("Teko-Medium SDF"));

				var bloomFont = TMPro.TMP_FontAsset.CreateFontAsset(cachedFont.sourceFontFile);
				bloomFont.name = "Teko-Medium SDF Bloom";
				bloomFont.material.shader = UnityEngine.Resources.FindObjectsOfTypeAll<UnityEngine.Shader>().First(
					x => x.name.Contains("TextMeshPro/Distance Field"));

				BloomFontAsset = bloomFont;
			}
			return BloomFontAsset;
		}
	}
}