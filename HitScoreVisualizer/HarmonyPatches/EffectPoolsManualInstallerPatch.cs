using HitScoreVisualizer.Utilities.Services;
using SiraUtil.Affinity;
using TMPro;

// ReSharper disable InconsistentNaming

namespace HitScoreVisualizer.HarmonyPatches;

internal class EffectPoolsManualInstallerPatch : IAffinity
{
	private readonly BloomFontProvider bloomFontProvider;
	private readonly PluginConfig pluginConfig;

	private EffectPoolsManualInstallerPatch(BloomFontProvider bloomFontProvider, PluginConfig pluginConfig)
	{
		this.bloomFontProvider = bloomFontProvider;
		this.pluginConfig = pluginConfig;
	}

	[AffinityPrefix]
	[AffinityPriority(1000)]
	[AffinityPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
	internal void ManualInstallBindingsPrefix(FlyingScoreEffect ____flyingScoreEffectPrefab)
	{
		var text = ____flyingScoreEffectPrefab._text;
		text.richText = true;
		text.textWrappingMode = TextWrappingModes.NoWrap;
		text.overflowMode = TextOverflowModes.Overflow;

		// Configure font shader and italics
		text.fontStyle = pluginConfig.DisableItalics ? FontStyles.Normal : FontStyles.Italic;
		text.font = bloomFontProvider.GetFontForType(pluginConfig.FontType);
	}
}