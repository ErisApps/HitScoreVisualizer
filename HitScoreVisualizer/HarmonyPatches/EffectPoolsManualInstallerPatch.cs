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
			// Configure font shader and italics
			____flyingScoreEffectPrefab._text.font = hsvConfig.HitScoreBloom
				? bloomFontProvider.BloomFont
				: bloomFontProvider.DefaultFont;
			____flyingScoreEffectPrefab._text.fontStyle = hsvConfig.EnableItalics
				? FontStyles.Italic
				: FontStyles.Normal;
		}
	}
}
