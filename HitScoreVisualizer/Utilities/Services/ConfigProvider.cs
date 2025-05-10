using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using HitScoreVisualizer.Utilities.Json;
using IPA.Loader;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using UnityEngine;
using Zenject;
using Version = Hive.Versioning.Version;

namespace HitScoreVisualizer.Utilities.Services;

public class ConfigProvider : IInitializable
{
	private readonly SiraLog siraLog;
	private readonly HSVConfig hsvConfig;
	private readonly Version pluginVersion;

	private readonly string hsvConfigsFolderPath = Path.Combine(UnityGame.UserDataPath, nameof(HitScoreVisualizer));
	private readonly string hsvConfigsBackupFolderPath;

	private readonly JsonSerializerSettings configSerializerSettings = new()
	{
		DefaultValueHandling = DefaultValueHandling.Include,
		NullValueHandling = NullValueHandling.Ignore,
		Formatting = Formatting.Indented,
		Converters = [ new Vector3Converter() ],
		ContractResolver = new HsvConfigContractResolver()
	};

	private readonly Dictionary<Version, Func<HsvConfigModel, bool>> migrationActions;

	private readonly Version minimumMigratableVersion;
	private readonly Version maximumMigrationNeededVersion;

	internal string? CurrentConfigPath => hsvConfig.ConfigFilePath;

	public HsvConfigModel? CurrentConfig { get; private set; }

	internal ConfigProvider(SiraLog siraLog, HSVConfig hsvConfig, UBinder<Plugin, PluginMetadata> pluginMetadata)
	{
		this.siraLog = siraLog;
		this.hsvConfig = hsvConfig;
		pluginVersion = pluginMetadata.Value.HVersion;

		hsvConfigsBackupFolderPath = Path.Combine(hsvConfigsFolderPath, "Backups");

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

	public async void Initialize()
	{
		if (CreateHsvConfigsFolderIfYeetedByPlayer())
		{
			await SaveConfig(Path.Combine(hsvConfigsFolderPath, "HitScoreVisualizerConfig (default).json"), HsvConfigModel.Default).ConfigureAwait(false);

			var oldHsvConfigPath = Path.Combine(UnityGame.UserDataPath, "HitScoreVisualizerConfig.json");
			if (File.Exists(oldHsvConfigPath))
			{
				try
				{
					var destinationHsvConfigPath = Path.Combine(hsvConfigsFolderPath, "HitScoreVisualizerConfig (imported).json");
					File.Move(oldHsvConfigPath, destinationHsvConfigPath);

					hsvConfig.ConfigFilePath = destinationHsvConfigPath;
				}
				catch (Exception e)
				{
					siraLog.Warn(e);
				}
			}
		}

		if (hsvConfig.ConfigFilePath == null)
		{
			return;
		}

		var fullPath = Path.Combine(hsvConfigsFolderPath, hsvConfig.ConfigFilePath);
		if (!File.Exists(fullPath))
		{
			hsvConfig.ConfigFilePath = null;
			return;
		}

		var userConfig = await LoadConfig(hsvConfig.ConfigFilePath).ConfigureAwait(false);
		if (userConfig == null)
		{
			siraLog.Warn($"Couldn't load userConfig at {fullPath}");
			return;
		}

		var configFileInfo = new ConfigFileInfo(Path.GetFileNameWithoutExtension(hsvConfig.ConfigFilePath), hsvConfig.ConfigFilePath)
		{
			Configuration = userConfig,
			State = GetConfigState(userConfig, Path.GetFileNameWithoutExtension(hsvConfig.ConfigFilePath), true)
		};

		await SelectUserConfig(configFileInfo).ConfigureAwait(false);
	}

	internal async Task<IEnumerable<ConfigFileInfo>> ListAvailableConfigs()
	{
		var configFileInfoList = Directory
			.EnumerateFiles(hsvConfigsFolderPath, "*.json", SearchOption.AllDirectories)
			.Where(path => !path.StartsWith(hsvConfigsBackupFolderPath))
			.Select(x => new ConfigFileInfo(Path.GetFileNameWithoutExtension(x), x.Substring(hsvConfigsFolderPath.Length + 1)))
			.ToList();

		foreach (var configInfo in configFileInfoList)
		{
			configInfo.Configuration = await LoadConfig(Path.Combine(hsvConfigsFolderPath, configInfo.ConfigPath)).ConfigureAwait(false);
			configInfo.State = GetConfigState(configInfo.Configuration, configInfo.ConfigName);
		}

		return configFileInfoList;
	}

	internal static bool ConfigSelectable(ConfigState? state)
	{
		return state switch
		{
			ConfigState.Compatible => true,
			ConfigState.NeedsMigration => true,
			_ => false
		};
	}

	internal async Task SelectUserConfig(ConfigFileInfo configFileInfo)
	{
		// safe-guarding just to be sure
		if (!ConfigSelectable(configFileInfo.State))
		{
			hsvConfig.ConfigFilePath = null;
			return;
		}

		if (configFileInfo.State == ConfigState.NeedsMigration)
		{
			var existingConfigFullPath = Path.Combine(hsvConfigsFolderPath, configFileInfo.ConfigPath);
			siraLog.Notice($"Config at path '{existingConfigFullPath}' requires migration. Starting automagical config migration logic.");

			// Create backups folder if it not exists
			var backupFolderPath = Path.GetDirectoryName(Path.Combine(hsvConfigsBackupFolderPath, configFileInfo.ConfigPath))!;
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

			siraLog.Debug($"Backing up config file at '{existingConfigFullPath}' to '{combinedConfigBackupPath}'");
			File.Copy(existingConfigFullPath, combinedConfigBackupPath);

			if (configFileInfo.Configuration!.IsDefaultConfig)
			{
				siraLog.Warn("Config is marked as default config and will therefore be reset to defaults");
				configFileInfo.Configuration = HsvConfigModel.Default;
			}
			else
			{
				siraLog.Debug("Starting actual config migration logic for config");
				RunMigration(configFileInfo.Configuration!);
			}

			await SaveConfig(configFileInfo.ConfigPath, configFileInfo.Configuration).ConfigureAwait(false);

			siraLog.Debug($"Config migration finished successfully and updated config is stored to disk at path: '{existingConfigFullPath}'");
		}

		if (Validate(configFileInfo.Configuration!, configFileInfo.ConfigName))
		{
			CurrentConfig = configFileInfo.Configuration;
			hsvConfig.ConfigFilePath = configFileInfo.ConfigPath;
		}
	}

	internal void UnselectUserConfig()
	{
		CurrentConfig = null;
		hsvConfig.ConfigFilePath = null;
	}

	internal void YeetConfig(string relativePath)
	{
		var fullPath = Path.Combine(hsvConfigsFolderPath, relativePath);
		if (File.Exists(fullPath))
		{
			File.Delete(fullPath);
		}
	}

	private async Task<HsvConfigModel?> LoadConfig(string relativePath)
	{
		CreateHsvConfigsFolderIfYeetedByPlayer(false);

		try
		{
			using var streamReader = new StreamReader(Path.Combine(hsvConfigsFolderPath, relativePath));
			var content = await streamReader.ReadToEndAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<HsvConfigModel>(content, configSerializerSettings);
		}
		catch (Exception ex)
		{
			siraLog.Warn($"Problem encountered when trying to load a config;\nFile path: {relativePath}\n{ex}");
			// Expected behaviour when file isn't an actual hsv config file...
			return null;
		}
	}

	private async Task SaveConfig(string relativePath, HsvConfigModel configuration)
	{
		CreateHsvConfigsFolderIfYeetedByPlayer(false);

		var fullPath = Path.Combine(hsvConfigsFolderPath, relativePath);
		var folderPath = Path.GetDirectoryName(fullPath);
		if (folderPath != null && Directory.Exists(folderPath))
		{
			Directory.CreateDirectory(folderPath);
		}

		try
		{
			using var streamWriter = new StreamWriter(fullPath, false);
			var content = JsonConvert.SerializeObject(configuration, Formatting.Indented, configSerializerSettings);
			await streamWriter.WriteAsync(content).ConfigureAwait(false);
		}
		catch (Exception e)
		{
			siraLog.Error(e);
		}
	}

	private ConfigState GetConfigState(HsvConfigModel? configuration, string configName, bool shouldLogWarning = false)
	{
		if (configuration is null)
		{
			LogWarning($"Config {configName} is not recognized as a valid HSV config file");
			return ConfigState.Broken;
		}

		var configVersion = configuration.GetVersion();

		// Both full version comparison and check on major, minor or patch version inequality in case the mod is versioned with a pre-release id
		if (configVersion > pluginVersion &&
		    (configVersion.Major != pluginVersion.Major
		     || configVersion.Minor != pluginVersion.Minor
		     || configVersion.Patch != pluginVersion.Patch))
		{
			LogWarning($"Config {configName} is made for a newer version of HSV than is currently installed. Targets {configVersion} while only {pluginVersion} is installed");
			return ConfigState.NewerVersion;
		}

		if (configVersion < minimumMigratableVersion)
		{
			LogWarning($"Config {configName} is too old and cannot be migrated. Please manually update said config to a newer version of HSV");
			return ConfigState.Incompatible;
		}

		if (configVersion < maximumMigrationNeededVersion)
		{
			LogWarning($"Config {configName} is is made for an older version of HSV, but can be migrated (safely?). Targets {configVersion} while version {pluginVersion} is installed");
			return ConfigState.NeedsMigration;
		}

		return Validate(configuration, configName) ? ConfigState.Compatible : ConfigState.ValidationFailed;

		void LogWarning(string message)
		{
			if (shouldLogWarning)
			{
				siraLog.Warn(message);
			}
		}
	}

	// ReSharper disable once CognitiveComplexity
	private bool Validate(HsvConfigModel configuration, string configName)
	{
		if (!configuration.Judgments?.Any() ?? true)
		{
			siraLog.Warn($"No judgments found for {configName}");
			return false;
		}

		if (!ValidateJudgments(configuration, configName))
		{
			return false;
		}

		// 99 is the max for NumberFormatInfo.NumberDecimalDigits
		if (configuration.TimeDependenceDecimalPrecision < 0 || configuration.TimeDependenceDecimalPrecision > 99)
		{
			siraLog.Warn($"timeDependencyDecimalPrecision value {configuration.TimeDependenceDecimalPrecision} is outside the range of acceptable values [0, 99]");
			return false;
		}

		if (configuration.TimeDependenceDecimalOffset < 0 || configuration.TimeDependenceDecimalOffset > Math.Log10(float.MaxValue))
		{
			siraLog.Warn($"timeDependencyDecimalOffset value {configuration.TimeDependenceDecimalOffset} is outside the range of acceptable values [0, {(int) Math.Log10(float.MaxValue)}]");
			return false;
		}

		if (configuration.BeforeCutAngleJudgments != null)
		{
			configuration.BeforeCutAngleJudgments = configuration.BeforeCutAngleJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateJudgmentSegment(configuration.BeforeCutAngleJudgments, configName))
			{
				return false;
			}
		}

		if (configuration.AccuracyJudgments != null)
		{
			configuration.AccuracyJudgments = configuration.AccuracyJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateJudgmentSegment(configuration.AccuracyJudgments, configName))
			{
				return false;
			}
		}

		if (configuration.AfterCutAngleJudgments != null)
		{
			configuration.AfterCutAngleJudgments = configuration.AfterCutAngleJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateJudgmentSegment(configuration.AfterCutAngleJudgments, configName))
			{
				return false;
			}
		}

		if (configuration.TimeDependenceJudgments != null)
		{
			configuration.TimeDependenceJudgments = configuration.TimeDependenceJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateTimeDependenceJudgmentSegment(configuration.TimeDependenceJudgments, configName))
			{
				return false;
			}
		}

		return true;
	}

	// ReSharper disable once CognitiveComplexity
	private bool ValidateJudgments(HsvConfigModel configuration, string configName)
	{
		configuration.Judgments = configuration.Judgments!.OrderByDescending(x => x.Threshold).ToList();
		var prevJudgment = configuration.Judgments[0];
		if (prevJudgment.Fade)
		{
			prevJudgment = new()
			{
				Color = prevJudgment.Color,
				Fade = false,
				Text = prevJudgment.Text,
				Threshold = prevJudgment.Threshold,
			};
		}

		if (!ValidateJudgmentColor(prevJudgment, configName))
		{
			siraLog.Warn($"Judgment entry for threshold {prevJudgment.Threshold} has invalid color in {configName}");
			return false;
		}

		if (configuration.Judgments.Count > 1)
		{
			for (var i = 1; i < configuration.Judgments.Count; i++)
			{
				var currentJudgment = configuration.Judgments[i];
				if (prevJudgment.Threshold != currentJudgment.Threshold)
				{
					if (!ValidateJudgmentColor(currentJudgment, configName))
					{
						siraLog.Warn($"Judgment entry for threshold {currentJudgment.Threshold} has invalid color in {configName}");
						return false;
					}

					prevJudgment = currentJudgment;
					continue;
				}

				siraLog.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold} in {configName}");
				return false;
			}
		}

		return true;
	}

	private bool ValidateJudgmentColor(NormalJudgment judgment, string configName)
	{
		if (judgment.Color.Count != 4)
		{
			siraLog.Warn($"Judgment for threshold {judgment.Threshold} has invalid color in {configName}! Make sure to include exactly 4 numbers for each judgment's color!");
			return false;
		}

		if (judgment.Color.All(x => x >= 0f))
		{
			return true;
		}

		siraLog.Warn($"Judgment for threshold {judgment.Threshold} has invalid color in {configName}! Make sure to include exactly 4 numbers that are greater or equal than 0 (and preferably smaller or equal than 1) for each judgment's color!");
		return false;
	}

	private bool ValidateJudgmentSegment(List<JudgmentSegment> segments, string configName)
	{
		if (segments.Count <= 1)
		{
			return true;
		}

		var prevJudgmentSegment = segments.First();
		for (var i = 1; i < segments.Count; i++)
		{
			var currentJudgment = segments[i];
			if (prevJudgmentSegment.Threshold != currentJudgment.Threshold)
			{
				prevJudgmentSegment = currentJudgment;
				continue;
			}

			siraLog.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold} in {configName}");
			return false;
		}

		return true;
	}

	private bool ValidateTimeDependenceJudgmentSegment(List<TimeDependenceJudgmentSegment> segments, string configName)
	{
		if (segments.Count <= 1)
		{
			return true;
		}

		var prevJudgmentSegment = segments.First();
		for (var i = 1; i < segments.Count; i++)
		{
			var currentJudgment = segments[i];
			if (prevJudgmentSegment.Threshold - currentJudgment.Threshold > double.Epsilon)
			{
				prevJudgmentSegment = currentJudgment;
				continue;
			}

			siraLog.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold} in {configName}");
			return false;
		}

		return true;
	}

	private void RunMigration(HsvConfigModel userConfig)
	{
		foreach (var requiredMigration in migrationActions.Keys.Where(migrationVersion => migrationVersion >= userConfig.GetVersion()))
		{
			migrationActions[requiredMigration](userConfig);
		}

		userConfig.SetVersion(pluginVersion);
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
		if (configuration.Judgments != null)
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
		}

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

	private bool CreateHsvConfigsFolderIfYeetedByPlayer(bool calledOnInit = true)
	{
		if (Directory.Exists(hsvConfigsFolderPath))
		{
			return false;
		}

		if (!calledOnInit)
		{
			siraLog.Warn("*sigh* Don't yeet the HSV configs folder while the game is running... Recreating it again...");
		}

		Directory.CreateDirectory(hsvConfigsFolderPath);

		return true;
	}
}