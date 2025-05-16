using System.IO;
using System.Linq;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Models.ConfigMigrations;
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

	public ConfigMigrator(PluginDirectories directories)
	{
		this.directories = directories;

		var versions = migrations.Select(m => m.Version).ToList();
		minimumMigratableVersion = versions.Min();
		maximumMigrationNeededVersion = versions.Max();
	}

	public ConfigState GetConfigState(HsvConfigModel? configuration)
	{
		if (configuration is null)
		{
			return ConfigState.Broken;
		}
		var configVersion = configuration.GetVersion();

		return configVersion.NewerThan(Plugin.Metadata.HVersion) ? ConfigState.NewerVersion
			: configVersion < minimumMigratableVersion ? ConfigState.Incompatible
			: configVersion < maximumMigrationNeededVersion ? ConfigState.NeedsMigration
			: configuration.Validate() ? ConfigState.Compatible : ConfigState.ValidationFailed;
	}

	public ConfigInfo MigrateConfig(ConfigInfo configInfo)
	{
		if (configInfo.Config is null)
		{
			Plugin.Log.Warn($"Can't migrate {configInfo.ConfigName} because there is no HSV config");
			return configInfo;
		}

		// Create a backup file
		var backupName = $"{configInfo.ConfigName} (backup of config made for {configInfo.Config.GetVersion()}{configInfo.File.Extension})";
		var backupPath = FilePathUtils.GetUniqueFilePath(Path.Combine(directories.Backups.FullName, backupName));
		configInfo.File.CopyTo(backupPath);

		foreach (var migration in migrations.Where((m => m.Version >= configInfo.Config.GetVersion())))
		{
			Plugin.Log.Debug($"Running migration {migration.Version} on {configInfo.ConfigName}");
			migration.Migrate(configInfo.Config);
		}

		configInfo.Config.SetVersion(Plugin.Metadata.HVersion);
		return configInfo;
	}
}