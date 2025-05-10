using System;
using BeatSaberMarkupLanguage.Attributes;
using HitScoreVisualizer.Utilities.Extensions;
using Version = Hive.Versioning.Version;

namespace HitScoreVisualizer.Models;

internal class ConfigFileInfo(string fileName, string filePath)
{
	public string ConfigPath { get; } = filePath;

	[UIValue("config-name")]
	public string ConfigName { get; } = fileName;

	[UIValue("config-description")]
	public string ConfigDescription => State switch
	{
		ConfigState.NewerVersion => $"<color=\"red\">Config is too new. Targets version {Configuration?.GetVersion()}",
		ConfigState.Compatible => $"<color=\"green\">OK - {Configuration?.GetVersion()}",
		ConfigState.NeedsMigration => $"<color=\"orange\">Config made for HSV {Configuration?.GetVersion()}. Migration possible.",
		ConfigState.ValidationFailed => "<color=\"red\">Validation failed, please check the file again.",
		ConfigState.Incompatible => $"<color=\"red\">Config is too old. Targets version {Configuration?.GetVersion()}",
		ConfigState.Broken => "<color=\"red\">Invalid config. Not selectable...",
		_ => throw new ArgumentOutOfRangeException(nameof(State))
	};

	public HsvConfigModel? Configuration { get; set; }
	public ConfigState State { get; set; }
}