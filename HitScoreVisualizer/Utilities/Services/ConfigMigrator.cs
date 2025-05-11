using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using Version = Hive.Versioning.Version;

namespace HitScoreVisualizer.Utilities.Services;

internal class ConfigMigrator
{
	private readonly PluginDirectories directories;

	private readonly IHsvConfigMigration[] migrations = [
		new ConfigMigration200(),
		new ConfigMigration210(),
		new ConfigMigration223(),
		new ConfigMigration320()
	];

	private readonly Version minimumMigratableVersion;
	private readonly Version maximumMigrationNeededVersion;

	private Version PluginVersion => Plugin.Metadata.HVersion;

	public ConfigMigrator(PluginDirectories directories)
	{
		this.directories = directories;

		var versions = migrations.Select(m => m.Version).ToList();
		minimumMigratableVersion = versions.Min();
		maximumMigrationNeededVersion = versions.Max();
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

		if (!configFileInfo.Configuration!.IsDefaultConfig)
		{
			Plugin.Log.Debug($"Starting config migration for {configFileInfo.ConfigName}");

			var config = configFileInfo.Configuration;
			foreach (var migration in migrations.Where((m => m.Version >= config.GetVersion())))
			{
				migration.Migrate(config);
			}

			config.SetVersion(PluginVersion);
		}
		else
		{
			Plugin.Log.Warn("Config is marked as default config and will therefore be reset to defaults");
			configFileInfo.Configuration = HsvConfigModel.Default;
		}

		return configFileInfo;
	}
}