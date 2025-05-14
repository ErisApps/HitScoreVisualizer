using System.IO;
using IPA.Utilities;

namespace HitScoreVisualizer.Utilities.Services;

internal class PluginDirectories
{
	private readonly DirectoryInfo configs;
	private readonly DirectoryInfo backups;

	public PluginDirectories()
	{
		var configsPath = Path.Combine(UnityGame.UserDataPath, nameof(HitScoreVisualizer));
		configs = new(configsPath);
		backups = Configs.CreateSubdirectory("Backups");
	}

	public DirectoryInfo Configs => CreateDirectoryIfNotExists(configs);

	public DirectoryInfo Backups => CreateDirectoryIfNotExists(backups);

	private static DirectoryInfo CreateDirectoryIfNotExists(DirectoryInfo directory)
	{
		if (!directory.Exists)
		{
			directory.Create();
		}
		return directory;
	}
}