using System.Runtime.CompilerServices;
using HitScoreVisualizer.Utilities.Services;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace HitScoreVisualizer.Settings;

internal class HSVConfig
{
	public virtual string? ConfigFilePath { get; set; }
	public virtual HsvFontType FontType { get; set; }
	public virtual bool DisableItalics { get; set; }
	public virtual bool OverrideNoTextsAndHuds { get; set; }
}