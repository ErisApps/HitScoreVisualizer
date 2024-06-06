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
		private readonly SiraLog _siraLog;
		private readonly HSVConfig _hsvConfig;
		private readonly Version _pluginVersion;

		private readonly string _hsvConfigsFolderPath;
		private readonly string _hsvConfigsBackupFolderPath;
		private readonly JsonSerializerSettings _jsonSerializerSettings;

		private readonly Dictionary<Version, Func<Configuration, bool>> _migrationActions;

		private readonly Version _minimumMigratableVersion;
		private readonly Version _maximumMigrationNeededVersion;

		private Configuration? _currentConfig;

		internal string? CurrentConfigPath => _hsvConfig.ConfigFilePath;

		internal ConfigProvider(SiraLog siraLog, HSVConfig hsvConfig, UBinder<Plugin, PluginMetadata> pluginMetadata)
		{
			_siraLog = siraLog;
			_hsvConfig = hsvConfig;
			_pluginVersion = pluginMetadata.Value.HVersion;

			_jsonSerializerSettings = new JsonSerializerSettings
			{
				DefaultValueHandling = DefaultValueHandling.Include,
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.Indented,
				Converters = new List<JsonConverter> { new Vector3Converter() },
				ContractResolver = ShouldNotSerializeContractResolver.Instance
			};
			_hsvConfigsFolderPath = Path.Combine(UnityGame.UserDataPath, nameof(HitScoreVisualizer));
			_hsvConfigsBackupFolderPath = Path.Combine(_hsvConfigsFolderPath, "Backups");

			_migrationActions = new Dictionary<Version, Func<Configuration, bool>>
			{
				{ new Version(2, 0, 0), RunMigration2_0_0 },
				{ new Version(2, 1, 0), RunMigration2_1_0 },
				{ new Version(2, 2, 3), RunMigration2_2_3 },
				{ new Version(3, 2, 0), RunMigration3_2_0 }
			};

			_minimumMigratableVersion = _migrationActions.Keys.Min();
			_maximumMigrationNeededVersion = _migrationActions.Keys.Max();
		}

		public async void Initialize()
		{
			if (CreateHsvConfigsFolderIfYeetedByPlayer())
			{
				await SaveConfig(Path.Combine(_hsvConfigsFolderPath, "HitScoreVisualizerConfig (default).json"), Configuration.Default).ConfigureAwait(false);

				var oldHsvConfigPath = Path.Combine(UnityGame.UserDataPath, "HitScoreVisualizerConfig.json");
				if (File.Exists(oldHsvConfigPath))
				{
					try
					{
						var destinationHsvConfigPath = Path.Combine(_hsvConfigsFolderPath, "HitScoreVisualizerConfig (imported).json");
						File.Move(oldHsvConfigPath, destinationHsvConfigPath);

						_hsvConfig.ConfigFilePath = destinationHsvConfigPath;
					}
					catch (Exception e)
					{
						_siraLog.Warn(e);
					}
				}
			}

			if (_hsvConfig.ConfigFilePath == null)
			{
				return;
			}

			var fullPath = Path.Combine(_hsvConfigsFolderPath, _hsvConfig.ConfigFilePath);
			if (!File.Exists(fullPath))
			{
				_hsvConfig.ConfigFilePath = null;
				return;
			}

			var userConfig = await LoadConfig(_hsvConfig.ConfigFilePath).ConfigureAwait(false);
			if (userConfig == null)
			{
				_siraLog.Warn($"Couldn't load userConfig at {fullPath}");
				return;
			}

			var configFileInfo = new ConfigFileInfo(Path.GetFileNameWithoutExtension(_hsvConfig.ConfigFilePath), _hsvConfig.ConfigFilePath)
			{
				Configuration = userConfig,
				State = GetConfigState(userConfig, Path.GetFileNameWithoutExtension(_hsvConfig.ConfigFilePath), true)
			};

			await SelectUserConfig(configFileInfo).ConfigureAwait(false);
		}

		public Configuration? GetCurrentConfig()
		{
			return _currentConfig;
		}

		internal async Task<IEnumerable<ConfigFileInfo>> ListAvailableConfigs()
		{
			var configFileInfoList = Directory
				.EnumerateFiles(_hsvConfigsFolderPath, "*", SearchOption.AllDirectories)
				.Where(path => !path.StartsWith(_hsvConfigsBackupFolderPath))
				.Select(x => new ConfigFileInfo(Path.GetFileNameWithoutExtension(x), x.Substring(_hsvConfigsFolderPath.Length + 1)))
				.ToList();

			foreach (var configInfo in configFileInfoList)
			{
				configInfo.Configuration = await LoadConfig(Path.Combine(_hsvConfigsFolderPath, configInfo.ConfigPath)).ConfigureAwait(false);
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
				_hsvConfig.ConfigFilePath = null;
				return;
			}

			if (configFileInfo.State == ConfigState.NeedsMigration)
			{
				var existingConfigFullPath = Path.Combine(_hsvConfigsFolderPath, configFileInfo.ConfigPath);
				_siraLog.Notice($"Config at path '{existingConfigFullPath}' requires migration. Starting automagical config migration logic.");

				// Create backups folder if it not exists
				var backupFolderPath = Path.GetDirectoryName(Path.Combine(_hsvConfigsBackupFolderPath, configFileInfo.ConfigPath))!;
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

				_siraLog.Debug($"Backing up config file at '{existingConfigFullPath}' to '{combinedConfigBackupPath}'");
				File.Copy(existingConfigFullPath, combinedConfigBackupPath);

				if (configFileInfo.Configuration!.IsDefaultConfig)
				{
					_siraLog.Warn("Config is marked as default config and will therefore be reset to defaults");
					configFileInfo.Configuration = Configuration.Default;
				}
				else
				{
					_siraLog.Debug("Starting actual config migration logic for config");
					RunMigration(configFileInfo.Configuration!);
				}

				await SaveConfig(configFileInfo.ConfigPath, configFileInfo.Configuration).ConfigureAwait(false);

				_siraLog.Debug($"Config migration finished successfully and updated config is stored to disk at path: '{existingConfigFullPath}'");
			}

			if (Validate(configFileInfo.Configuration!, configFileInfo.ConfigName))
			{
				_currentConfig = configFileInfo.Configuration;
				_hsvConfig.ConfigFilePath = configFileInfo.ConfigPath;
			}
		}

		internal void UnselectUserConfig()
		{
			_currentConfig = null;
			_hsvConfig.ConfigFilePath = null;
		}

		internal void YeetConfig(string relativePath)
		{
			var fullPath = Path.Combine(_hsvConfigsFolderPath, relativePath);
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
				using var streamReader = new StreamReader(Path.Combine(_hsvConfigsFolderPath, relativePath));
				var content = await streamReader.ReadToEndAsync().ConfigureAwait(false);
				return JsonConvert.DeserializeObject<Configuration>(content, _jsonSerializerSettings);
			}
			catch (Exception ex)
			{
				_siraLog.Warn(ex);
				// Expected behaviour when file isn't an actual hsv config file...
				return null;
			}
		}

		private async Task SaveConfig(string relativePath, Configuration configuration)
		{
			CreateHsvConfigsFolderIfYeetedByPlayer(false);

			var fullPath = Path.Combine(_hsvConfigsFolderPath, relativePath);
			var folderPath = Path.GetDirectoryName(fullPath);
			if (folderPath != null && Directory.Exists(folderPath))
			{
				Directory.CreateDirectory(folderPath);
			}

			try
			{
				using var streamWriter = new StreamWriter(fullPath, false);
				var content = JsonConvert.SerializeObject(configuration, Formatting.Indented, _jsonSerializerSettings);
				await streamWriter.WriteAsync(content).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				_siraLog.Error(e);
			}
		}

		private ConfigState GetConfigState(Configuration? configuration, string configName, bool shouldLogWarning = false)
		{
			void LogWarning(string message)
			{
				if (shouldLogWarning)
				{
					_siraLog.Warn(message);
				}
			}

			if (configuration?.Version == null)
			{
				LogWarning($"Config {configName} is not recognized as a valid HSV config file");
				return ConfigState.Broken;
			}

			// Both full version comparison and check on major, minor or patch version inequality in case the mod is versioned with a pre-release id
			if (configuration.Version > _pluginVersion &&
			    (configuration.Version.Major != _pluginVersion.Major || configuration.Version.Minor != _pluginVersion.Minor || configuration.Version.Patch != _pluginVersion.Patch))
			{
				LogWarning($"Config {configName} is made for a newer version of HSV than is currently installed. Targets {configuration.Version} while only {_pluginVersion} is installed");
				return ConfigState.NewerVersion;
			}

			if (configuration.Version < _minimumMigratableVersion)
			{
				LogWarning($"Config {configName} is too old and cannot be migrated. Please manually update said config to a newer version of HSV");
				return ConfigState.Incompatible;
			}

			if (configuration.Version < _maximumMigrationNeededVersion)
			{
				LogWarning($"Config {configName} is is made for an older version of HSV, but can be migrated (safely?). Targets {configuration.Version} while version {_pluginVersion} is installed");
				return ConfigState.NeedsMigration;
			}

			return !Validate(configuration, configName) ? ConfigState.ValidationFailed : ConfigState.Compatible;
		}

		// ReSharper disable once CognitiveComplexity
		private bool Validate(Configuration configuration, string configName)
		{
			if (!configuration.Judgments?.Any() ?? true)
			{
				_siraLog.Warn($"No judgments found for {configName}");
				return false;
			}

			if (!ValidateJudgments(configuration, configName))
			{
				return false;
			}

			// 99 is the max for NumberFormatInfo.NumberDecimalDigits
			if (configuration.TimeDependenceDecimalPrecision < 0 || configuration.TimeDependenceDecimalPrecision > 99)
			{
				_siraLog.Warn($"timeDependencyDecimalPrecision value {configuration.TimeDependenceDecimalPrecision} is outside the range of acceptable values [0, 99]");
				return false;
			}

			if (configuration.TimeDependenceDecimalOffset < 0 || configuration.TimeDependenceDecimalOffset > Math.Log10(float.MaxValue))
			{
				_siraLog.Warn($"timeDependencyDecimalOffset value {configuration.TimeDependenceDecimalOffset} is outside the range of acceptable values [0, {(int) Math.Log10(float.MaxValue)}]");
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
			configuration.Judgments = configuration.Judgments!.OrderByDescending(x => x.Threshold).ToList();
			var prevJudgment = configuration.Judgments.First();
			if (prevJudgment.Fade)
			{
				prevJudgment.Fade = false;
			}

			if (!ValidateJudgmentColor(prevJudgment, configName))
			{
				_siraLog.Warn($"Judgment entry for threshold {prevJudgment.Threshold} has invalid color in {configName}");
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
							_siraLog.Warn($"Judgment entry for threshold {currentJudgment.Threshold} has invalid color in {configName}");
							return false;
						}

						prevJudgment = currentJudgment;
						continue;
					}

					_siraLog.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold} in {configName}");
					return false;
				}
			}

			return true;
		}

		private bool ValidateJudgmentColor(Judgment judgment, string configName)
		{
			if (judgment.Color.Count != 4)
			{
				_siraLog.Warn($"Judgment for threshold {judgment.Threshold} has invalid color in {configName}! Make sure to include exactly 4 numbers for each judgment's color!");
				return false;
			}

			if (judgment.Color.All(x => x >= 0f))
			{
				return true;
			}

			_siraLog.Warn($"Judgment for threshold {judgment.Threshold} has invalid color in {configName}! Make sure to include exactly 4 numbers that are greater or equal than 0 (and preferably smaller or equal than 1) for each judgment's color!");
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

				_siraLog.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold} in {configName}");
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

				_siraLog.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold} in {configName}");
				return false;
			}

			return true;
		}

		private void RunMigration(Configuration userConfig)
		{
			var userConfigVersion = userConfig.Version;
			foreach (var requiredMigration in _migrationActions.Keys.Where(migrationVersion => migrationVersion >= userConfigVersion))
			{
				_migrationActions[requiredMigration](userConfig);
			}

			userConfig.Version = _pluginVersion;
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
			if (configuration.Judgments != null)
			{
				foreach (var j in configuration.Judgments.Where(j => j.Threshold == 110))
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
			if (!Directory.Exists(_hsvConfigsFolderPath))
			{
				if (!calledOnInit)
				{
					_siraLog.Warn("*sigh* Don't yeet the HSV configs folder while the game is running... Recreating it again...");
				}

				Directory.CreateDirectory(_hsvConfigsFolderPath);

				return true;
			}

			return false;
		}
	}
}