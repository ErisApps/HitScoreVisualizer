using HitScoreVisualizer.Settings;
using HitScoreVisualizer.Utilities.Services;
using SiraUtil.Affinity;
using TMPro;
// ReSharper disable InconsistentNaming

namespace HitScoreVisualizer.HarmonyPatches
{
	internal class EffectPoolsManualInstallerPatch : IAffinity
	{
		private readonly BloomFontProvider bloomFontProvider;
		private readonly HSVConfig hsvConfig;

		private EffectPoolsManualInstallerPatch(BloomFontProvider bloomFontProvider, HSVConfig hsvConfig)
		{
			this.bloomFontProvider = bloomFontProvider;
			this.hsvConfig = hsvConfig;
		}

		[AffinityPrefix]
		[AffinityPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
		internal void ManualInstallBindingsPrefix(FlyingScoreEffect ____flyingScoreEffectPrefab)
		{
			var text = ____flyingScoreEffectPrefab._text;
			text.richText = true;
			text.enableWordWrapping = false;
			text.overflowMode = TextOverflowModes.Overflow;

			// Configure font shader and italics
			text.font = hsvConfig.HitScoreBloom ? bloomFontProvider.BloomFont : bloomFontProvider.DefaultFont;
			text.fontStyle = hsvConfig.DisableItalics ? FontStyles.Normal : FontStyles.Italic;
		}
	}
}
