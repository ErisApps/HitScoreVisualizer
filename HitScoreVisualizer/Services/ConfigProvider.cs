using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HitScoreVisualizer.Helpers.Json;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Settings;
using IPA.Loader;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Logging;
using SiraUtil.Zenject;
using UnityEngine;
using Zenject;
using Version = Hive.Versioning.Version;

namespace HitScoreVisualizer.Services
{
	public class ConfigProvider : IInitializable
	{
		private readonly SiraLog siraLog;
		private readonly HSVConfig hsvConfig;
		private readonly Version pluginVersion;

		private readonly string hsvConfigsFolderPath;
		private readonly string hsvConfigsBackupFolderPath;
		private readonly JsonSerializerSettings jsonSerializerSettings;

		private readonly Dictionary<Version, Func<Configuration, bool>> migrationActions;

		private readonly Version minimumMigratableVersion;
		private readonly Version maximumMigrationNeededVersion;

		internal string? CurrentConfigPath => hsvConfig.ConfigFilePath;

		public Configuration? CurrentConfig { get; private set; }

		internal ConfigProvider(SiraLog siraLog, HSVConfig hsvConfig, UBinder<Plugin, PluginMetadata> pluginMetadata)
		{
			this.siraLog = siraLog;
			this.hsvConfig = hsvConfig;
			pluginVersion = pluginMetadata.Value.HVersion;

			jsonSerializerSettings = new JsonSerializerSettings
			{
				DefaultValueHandling = DefaultValueHandling.Include,
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.Indented,
				Converters = new List<JsonConverter> { new Vector3Converter() },
				ContractResolver = ShouldNotSerializeContractResolver.Instance
			};
			hsvConfigsFolderPath = Path.Combine(UnityGame.UserDataPath, nameof(HitScoreVisualizer));
			hsvConfigsBackupFolderPath = Path.Combine(hsvConfigsFolderPath, "Backups");

			migrationActions = new Dictionary<Version, Func<Configuration, bool>>
			{
				{ new Version(2, 0, 0), RunMigration2_0_0 },
				{ new Version(2, 1, 0), RunMigration2_1_0 },
				{ new Version(2, 2, 3), RunMigration2_2_3 },
				{ new Version(3, 2, 0), RunMigration3_2_0 }
			};

			minimumMigratableVersion = migrationActions.Keys.Min();
			maximumMigrationNeededVersion = migrationActions.Keys.Max();
		}

		public async void Initialize()
		{
			if (CreateHsvConfigsFolderIfYeetedByPlayer())
			{
				await SaveConfig(Path.Combine(hsvConfigsFolderPath, "HitScoreVisualizerConfig (default).json"), Configuration.Default).ConfigureAwait(false);

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
				.EnumerateFiles(hsvConfigsFolderPath, "*", SearchOption.AllDirectories)
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

				var newFileName = $"{Path.GetFileNameWithoutExtension(existingConfigFullPath)} (backup of config made for {configFileInfo.Configuration!.Version})";
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
					configFileInfo.Configuration = Configuration.Default;
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

		private async Task<Configuration?> LoadConfig(string relativePath)
		{
			CreateHsvConfigsFolderIfYeetedByPlayer(false);

			try
			{
				using var streamReader = new StreamReader(Path.Combine(hsvConfigsFolderPath, relativePath));
				var content = await streamReader.ReadToEndAsync().ConfigureAwait(false);
				return JsonConvert.DeserializeObject<Configuration>(content, jsonSerializerSettings);
			}
			catch (Exception ex)
			{
				siraLog.Warn(ex);
				// Expected behaviour when file isn't an actual hsv config file...
				return null;
			}
		}

		private async Task SaveConfig(string relativePath, Configuration configuration)
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
				var content = JsonConvert.SerializeObject(configuration, Formatting.Indented, jsonSerializerSettings);
				await streamWriter.WriteAsync(content).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				siraLog.Error(e);
			}
		}

		private ConfigState GetConfigState(Configuration? configuration, string configName, bool shouldLogWarning = false)
		{
			void LogWarning(string message)
			{
				if (shouldLogWarning)
				{
					siraLog.Warn(message);
				}
			}

			if (configuration?.Version == null)
			{
				LogWarning($"Config {configName} is not recognized as a valid HSV config file");
				return ConfigState.Broken;
			}

			// Both full version comparison and check on major, minor or patch version inequality in case the mod is versioned with a pre-release id
			if (configuration.Version > pluginVersion &&
			    (configuration.Version.Major != pluginVersion.Major || configuration.Version.Minor != pluginVersion.Minor || configuration.Version.Patch != pluginVersion.Patch))
			{
				LogWarning($"Config {configName} is made for a newer version of HSV than is currently installed. Targets {configuration.Version} while only {pluginVersion} is installed");
				return ConfigState.NewerVersion;
			}

			if (configuration.Version < minimumMigratableVersion)
			{
				LogWarning($"Config {configName} is too old and cannot be migrated. Please manually update said config to a newer version of HSV");
				return ConfigState.Incompatible;
			}

			if (configuration.Version < maximumMigrationNeededVersion)
			{
				LogWarning($"Config {configName} is is made for an older version of HSV, but can be migrated (safely?). Targets {configuration.Version} while version {pluginVersion} is installed");
				return ConfigState.NeedsMigration;
			}

			return !Validate(configuration, configName) ? ConfigState.ValidationFailed : ConfigState.Compatible;
		}

		// ReSharper disable once CognitiveComplexity
		private bool Validate(Configuration configuration, string configName)
		{
			if (!configuration.NormalJudgments?.Any() ?? true)
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
		private bool ValidateJudgments(Configuration configuration, string configName)
		{
			configuration.NormalJudgments = configuration.NormalJudgments!.OrderByDescending(x => x.Threshold).ToList();
			var prevJudgment = configuration.NormalJudgments.First();
			if (prevJudgment.Fade)
			{
				prevJudgment.Fade = false;
			}

			if (!ValidateJudgmentColor(prevJudgment, configName))
			{
				siraLog.Warn($"Judgment entry for threshold {prevJudgment.Threshold} has invalid color in {configName}");
				return false;
			}

			if (configuration.NormalJudgments.Count > 1)
			{
				for (var i = 1; i < configuration.NormalJudgments.Count; i++)
				{
					var currentJudgment = configuration.NormalJudgments[i];
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

		private void RunMigration(Configuration userConfig)
		{
			var userConfigVersion = userConfig.Version;
			foreach (var requiredMigration in migrationActions.Keys.Where(migrationVersion => migrationVersion >= userConfigVersion))
			{
				migrationActions[requiredMigration](userConfig);
			}

			userConfig.Version = pluginVersion;
		}

		private static bool RunMigration2_0_0(Configuration configuration)
		{
			configuration.BeforeCutAngleJudgments = new List<JudgmentSegment> { JudgmentSegment.Default };
			configuration.AccuracyJudgments = new List<JudgmentSegment> { JudgmentSegment.Default };
			configuration.AfterCutAngleJudgments = new List<JudgmentSegment> { JudgmentSegment.Default };

			return true;
		}

		private static bool RunMigration2_1_0(Configuration configuration)
		{
			if (configuration.NormalJudgments != null)
			{
				foreach (var j in configuration.NormalJudgments.Where(j => j.Threshold == 110))
				{
					j.Threshold = 115;
				}
			}

			if (configuration.AccuracyJudgments != null)
			{
				foreach (var aj in configuration.AccuracyJudgments.Where(aj => aj.Threshold == 10))
				{
					aj.Threshold = 15;
				}
			}

			return true;
		}

		private static bool RunMigration2_2_3(Configuration configuration)
		{
			configuration.DoIntermediateUpdates = true;

			return true;
		}

		private static bool RunMigration3_2_0(Configuration configuration)
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
			if (!Directory.Exists(hsvConfigsFolderPath))
			{
				if (!calledOnInit)
				{
					siraLog.Warn("*sigh* Don't yeet the HSV configs folder while the game is running... Recreating it again...");
				}

				Directory.CreateDirectory(hsvConfigsFolderPath);

				return true;
			}

			return false;
		}
	}
}