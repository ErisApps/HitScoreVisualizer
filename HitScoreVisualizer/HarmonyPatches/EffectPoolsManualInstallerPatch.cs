using HitScoreVisualizer.Services;
using SiraUtil.Affinity;

namespace HitScoreVisualizer.HarmonyPatches
{
	internal class EffectPoolsManualInstallerPatch : IAffinity
	{
		private readonly BloomFontProvider _bloomFontProvider;

		public EffectPoolsManualInstallerPatch(BloomFontProvider bloomFontProvider)
		{
			_bloomFontProvider = bloomFontProvider;
		}

		[AffinityPrefix]
		[AffinityPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
		internal void ManualInstallBindingsPrefix(FlyingScoreEffect ____flyingScoreEffectPrefab)
		{
			_bloomFontProvider.ConfigureFont(ref ____flyingScoreEffectPrefab._text);
		}
	}
}
