using System;
using HitScoreVisualizer.Models;
using Version = Hive.Versioning.Version;

namespace HitScoreVisualizer.Utilities.Extensions;

internal static class ConfigStateExtensions
{
	public static bool HasWarning(this ConfigState state)
	{
		return state is not ConfigState.Compatible;
	}

	public static string GetWarningMessage(this ConfigState state, string configName, Version configVersion)
	{
		return state switch
		{
			ConfigState.Broken => $"Config {configName} is not recognized as a valid HSV config file",
			ConfigState.Incompatible => $"Config {configName} is too old and cannot be migrated. Please manually update said config to a newer version of HSV",
			ConfigState.ValidationFailed => $"Config {configName} failed validation",
			ConfigState.NeedsMigration => $"Config {configName} is is made for an older version of HSV, but can be migrated (safely?). Targets {configVersion} while version {Plugin.Metadata.HVersion} is installed",
			ConfigState.Compatible => string.Empty,
			ConfigState.NewerVersion => $"Config {configName} is made for a newer version of HSV than is currently installed. Targets {configVersion} while only {Plugin.Metadata.HVersion} is installed",
			_ => throw new ArgumentOutOfRangeException(nameof(state), state, null)
		};
	}
}