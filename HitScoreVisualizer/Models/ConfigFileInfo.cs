using System;
using BeatSaberMarkupLanguage.Attributes;
using HitScoreVisualizer.Settings;
using Version = Hive.Versioning.Version;

namespace HitScoreVisualizer.Models
{
	internal class ConfigFileInfo(string fileName, string filePath)
	{
		public string ConfigPath { get; } = filePath;

		[UIValue("config-name")]
		public string ConfigName { get; } = fileName;

		[UIValue("config-description")]
		public string ConfigDescription => State switch
		{
			ConfigState.NewerVersion => $"<color=\"red\">Config is too new. Targets version {Version}",
			ConfigState.Compatible => $"<color=\"green\">OK - {Version}",
			ConfigState.NeedsMigration => $"<color=\"orange\">Config made for HSV {Version}. Migration possible.",
			ConfigState.ValidationFailed => "<color=\"red\">Validation failed, please check the file again.",
			ConfigState.Incompatible => $"<color=\"red\">Config is too old. Targets version {Version}",
			ConfigState.Broken => "<color=\"red\">Invalid config. Not selectable...",
			_ => throw new NotImplementedException()
		};

		public Configuration? Configuration { get; set; }
		public ConfigState State { get; set; }
		public Version? Version => Configuration?.Version;
	}
}