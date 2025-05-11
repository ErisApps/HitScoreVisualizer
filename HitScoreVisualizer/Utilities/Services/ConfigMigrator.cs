using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using UnityEngine;
using Version = Hive.Versioning.Version;

namespace HitScoreVisualizer.Utilities.Services;

internal class ConfigMigrator
{
	private readonly PluginDirectories directories;

	private readonly Dictionary<Version, Func<HsvConfigModel, bool>> migrationActions;
	private readonly Version minimumMigratableVersion;
	private readonly Version maximumMigrationNeededVersion;

	private Version PluginVersion => Plugin.Metadata.HVersion;

	public ConfigMigrator(PluginDirectories directories)
	{
		this.directories = directories;

		migrationActions = new()
		{
			{ new(2, 0, 0), RunMigration2_0_0 },
			{ new(2, 1, 0), RunMigration2_1_0 },
			{ new(2, 2, 3), RunMigration2_2_3 },
			{ new(3, 2, 0), RunMigration3_2_0 }
		};

		minimumMigratableVersion = migrationActions.Keys.Min();
		maximumMigrationNeededVersion = migrationActions.Keys.Max();
	}

	public ConfigState GetConfigState(HsvConfigModel? configuration, string configName)
	{
		if (configuration is null)
		{
			return ConfigState.Broken;
		}
		var configVersion = configuration.GetVersion();

		return configVersion.NewerThan(PluginVersion) ? ConfigState.NewerVersion
			: configVersion < minimumMigratableVersion ? ConfigState.Incompatible
			: configVersion < maximumMigrationNeededVersion ? ConfigState.NeedsMigration
			: configuration.Validate(configName) ? ConfigState.Compatible : ConfigState.ValidationFailed;
	}

	public ConfigFileInfo MigrateConfig(ConfigFileInfo configFileInfo)
	{
		var existingConfigFullPath = Path.Combine(directories.Configs.FullName, configFileInfo.ConfigPath);
		Plugin.Log.Notice($"Config at path '{existingConfigFullPath}' requires migration. Starting automagical config migration logic.");

		// Create backups folder if it not exists
		var backupFolderPath = Path.GetDirectoryName(Path.Combine(directories.Backups.FullName, configFileInfo.ConfigPath))!;
		Directory.CreateDirectory(backupFolderPath);

		var newFileName = $"{Path.GetFileNameWithoutExtension(existingConfigFullPath)} (backup of config made for {configFileInfo.Configuration!.GetVersion()})";
		var fileExtension = Path.GetExtension(existingConfigFullPath);
		var combinedConfigBackupPath = Path.Combine(backupFolderPath, newFileName + fileExtension);

		if (File.Exists(combinedConfigBackupPath))
		{
			var existingFileCount = Directory.EnumerateFiles(backupFolderPath).Count(filePath => Path.GetFileNameWithoutExtension(filePath).StartsWith(newFileName));
			newFileName += $" ({(++existingFileCount).ToString()})";
			combinedConfigBackupPath = Path.Combine(backupFolderPath, newFileName + fileExtension);
		}

		Plugin.Log.Debug($"Backing up config file at '{existingConfigFullPath}' to '{combinedConfigBackupPath}'");
		File.Copy(existingConfigFullPath, combinedConfigBackupPath);

		if (configFileInfo.Configuration!.IsDefaultConfig)
		{
			Plugin.Log.Warn("Config is marked as default config and will therefore be reset to defaults");
			configFileInfo.Configuration = HsvConfigModel.Default;
		}
		else
		{
			Plugin.Log.Debug("Starting actual config migration logic for config");
			RunMigration(configFileInfo.Configuration!);
		}

		return configFileInfo;
	}

	public void RunMigration(HsvConfigModel userConfig)
	{
		foreach (var requiredMigration in migrationActions.Keys.Where(migrationVersion =>
			         migrationVersion >= userConfig.GetVersion()))
		{
			migrationActions[requiredMigration](userConfig);
		}

		userConfig.SetVersion(PluginVersion);
	}

	private static bool RunMigration2_0_0(HsvConfigModel configuration)
	{
		configuration.BeforeCutAngleJudgments = [JudgmentSegment.Default];
		configuration.AccuracyJudgments = [JudgmentSegment.Default];
		configuration.AfterCutAngleJudgments = [JudgmentSegment.Default];

		return true;
	}

	private static bool RunMigration2_1_0(HsvConfigModel configuration)
	{
		configuration.Judgments = configuration.Judgments
			.Where(j => j.Threshold == 110)
			.Select(j => new NormalJudgment
			{
				Threshold = 115,
				Text = j.Text,
				Color = j.Color,
				Fade = j.Fade
			}).ToList();

		if (configuration.AccuracyJudgments != null)
		{
			configuration.AccuracyJudgments = configuration.AccuracyJudgments
				.Where(aj => aj.Threshold == 10)
				.Select(s => new JudgmentSegment
				{
					Threshold = 15,
					Text = s.Text,
				}).ToList();
		}

		return true;
	}

	private static bool RunMigration2_2_3(HsvConfigModel configuration)
	{
		configuration.DoIntermediateUpdates = true;

		return true;
	}

	private static bool RunMigration3_2_0(HsvConfigModel configuration)
	{
#pragma warning disable 618
		if (configuration.UseFixedPos)
		{
			configuration.FixedPosition = new Vector3(configuration.FixedPosX, configuration.FixedPosY, configuration.FixedPosZ);
		}
#pragma warning restore 618

		return true;
	}
}