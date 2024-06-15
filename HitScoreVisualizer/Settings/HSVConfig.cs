using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace HitScoreVisualizer.Settings
{
	internal class HSVConfig
	{
		public virtual string? ConfigFilePath { get; set; }
		public virtual bool HitScoreBloom { get; set; }
		public virtual bool EnableItalics { get; set; } = true;
	}
}