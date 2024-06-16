using HitScoreVisualizer.Services;
using HitScoreVisualizer.Settings;
using SiraUtil.Affinity;
using TMPro;

namespace HitScoreVisualizer.HarmonyPatches
{
	internal class EffectPoolsManualInstallerPatch(BloomFontProvider bloomFontProvider, HSVConfig hsvConfig) : IAffinity
	{
		private readonly BloomFontProvider bloomFontProvider = bloomFontProvider;
		private readonly HSVConfig hsvConfig = hsvConfig;

		[AffinityPrefix]
		[AffinityPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
		internal void ManualInstallBindingsPrefix(FlyingScoreEffect ____flyingScoreEffectPrefab)
		{
			var text = ____flyingScoreEffectPrefab._text;
			text.richText = true;
			text.enableWordWrapping = false;
			text.overflowMode = TextOverflowModes.Overflow;

			// Configure font shader and italics
			text.font = hsvConfig.HitScoreBloom
				? bloomFontProvider.BloomFont
				: bloomFontProvider.DefaultFont;
			text.fontStyle = hsvConfig.EnableItalics
				? FontStyles.Italic
				: FontStyles.Normal;
		}
	}
}
