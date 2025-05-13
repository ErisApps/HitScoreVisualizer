using System.IO;

namespace HitScoreVisualizer.Models;

internal class ConfigInfo
{
	public FileInfo File { get; }
	public string ConfigName { get; }
	public string Description { get; }
	public ConfigState State { get; }

	public HsvConfigModel? Config { get; set; }

	public ConfigInfo(FileInfo file, string description, ConfigState state)
	{
		File = file;
		ConfigName = Path.GetFileNameWithoutExtension(file.Name);
		Description = description;
		State = state;
	}
}