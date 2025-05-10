using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using HitScoreVisualizer.Utilities.Json;
using IPA.Utilities;
using Newtonsoft.Json;
using SiraUtil.Logging;
using Zenject;

namespace HitScoreVisualizer.Utilities.Services;

public class ConfigProvider : IInitializable
{
	private readonly SiraLog siraLog;
	private readonly HSVConfig hsvConfig;
	private readonly ConfigMigrator configMigrator;

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

	internal string? CurrentConfigPath => hsvConfig.ConfigFilePath;

	public HsvConfigModel? CurrentConfig { get; private set; }

	internal ConfigProvider(SiraLog siraLog, HSVConfig hsvConfig, ConfigMigrator configMigrator)
	{
		this.siraLog = siraLog;
		this.hsvConfig = hsvConfig;
		this.configMigrator = configMigrator;

		hsvConfigsBackupFolderPath = Path.Combine(hsvConfigsFolderPath, "Backups");
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

		var configName = Path.GetFileNameWithoutExtension(hsvConfig.ConfigFilePath);
		var configFileInfo = new ConfigFileInfo(Path.GetFileNameWithoutExtension(hsvConfig.ConfigFilePath), hsvConfig.ConfigFilePath)
		{
			Configuration = userConfig,
			State = configMigrator.GetConfigState(userConfig, configName)
		};

		if (configFileInfo.State.HasWarning())
		{
			Plugin.Log.Warn(configFileInfo.State.GetWarningMessage(configName, userConfig.GetVersion()));
		}

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
			configInfo.State = configMigrator.GetConfigState(configInfo.Configuration, configInfo.ConfigName);
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
				configMigrator.RunMigration(configFileInfo.Configuration!);
			}

			await SaveConfig(configFileInfo.ConfigPath, configFileInfo.Configuration).ConfigureAwait(false);

			siraLog.Debug($"Config migration finished successfully and updated config is stored to disk at path: '{existingConfigFullPath}'");
		}

		if (configFileInfo.Configuration!.Validate(configFileInfo.ConfigName))
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