using HitScoreVisualizer.Models;
using Hive.Versioning;

namespace HitScoreVisualizer.Utilities.Extensions;

internal static class HsvConfigExtensions
{
	public static void SetVersion(this HsvConfigModel config, Version version)
	{
		config.MajorVersion = version.Major;
		config.MinorVersion = version.Minor;
		config.PatchVersion = version.Patch;
	}

	public static Version GetVersion(this HsvConfigModel config)
	{
		return new(config.MajorVersion, config.MinorVersion, config.PatchVersion);
	}
}