using System.Runtime.CompilerServices;
using HitScoreVisualizer.Models;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace HitScoreVisualizer;

internal class PluginConfig
{
	public virtual string? ConfigFilePath { get; set; }
	public virtual HsvFontType FontType { get; set; }
	public virtual bool DisableItalics { get; set; }
	public virtual bool OverrideNoTextsAndHuds { get; set; }

	[Ignore]
	public ConfigFileInfo? SelectedConfig { get; set; }
}