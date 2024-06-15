using HitScoreVisualizer.Services;
using HitScoreVisualizer.Settings;
using SiraUtil.Affinity;
using TMPro;

namespace HitScoreVisualizer.HarmonyPatches
{
	internal class EffectPoolsManualInstallerPatch : IAffinity
	{
		private readonly BloomFontProvider _bloomFontProvider;
		private readonly HSVConfig _hsvConfig;

		public EffectPoolsManualInstallerPatch(BloomFontProvider bloomFontProvider, HSVConfig hsvConfig)
		{
			_bloomFontProvider = bloomFontProvider;
			_hsvConfig = hsvConfig;
		}

		[AffinityPrefix]
		[AffinityPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
		internal void ManualInstallBindingsPrefix(FlyingScoreEffect ____flyingScoreEffectPrefab)
		{
			// Configure font shader and italics
			____flyingScoreEffectPrefab._text.font = _hsvConfig.HitScoreBloom
				? _bloomFontProvider.BloomFont
				: _bloomFontProvider.DefaultFont;
			____flyingScoreEffectPrefab._text.fontStyle = _hsvConfig.EnableItalics
				? FontStyles.Italic
				: FontStyles.Normal;
		}
	}
}
