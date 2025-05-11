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
using Zenject;

namespace HitScoreVisualizer.Utilities.Services;

public class ConfigProvider : IInitializable
{
	private readonly HSVConfig hsvConfig;
	private readonly ConfigMigrator configMigrator;
	private readonly PluginDirectories directories;

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

	internal ConfigProvider(HSVConfig hsvConfig, ConfigMigrator configMigrator, PluginDirectories directories)
	{
		this.hsvConfig = hsvConfig;
		this.configMigrator = configMigrator;
		this.directories = directories;
	}

	public async void Initialize()
	{
		var defaultConfigPath = Path.Combine(directories.Configs.FullName, "HitScoreVisualizerConfig (default).json");
		var defaultConfigFile = new FileInfo(defaultConfigPath);

		if (!defaultConfigFile.Exists)
		{
			await SaveConfig(new("HitScoreVisualizerConfig (default).json", defaultConfigPath)
			{
				Configuration = HsvConfigModel.Default
			});
		}

		var legacyConfigPath = Path.Combine(UnityGame.UserDataPath, "HitScoreVisualizerConfig.json");
		var legacyConfigFile = new FileInfo(legacyConfigPath);

		if (legacyConfigFile.Exists)
		{
			var destinationHsvConfigPath = Path.Combine(directories.Configs.FullName, "HitScoreVisualizerConfig (imported).json");
			File.Move(legacyConfigPath, destinationHsvConfigPath);

			hsvConfig.ConfigFilePath = destinationHsvConfigPath;
		}

		if (hsvConfig.ConfigFilePath == null)
		{
			return;
		}

		var fullPath = Path.Combine(directories.Configs.FullName, hsvConfig.ConfigFilePath);
		if (!File.Exists(fullPath))
		{
			hsvConfig.ConfigFilePath = null;
			return;
		}

		var userConfig = await LoadConfig(hsvConfig.ConfigFilePath).ConfigureAwait(false);
		if (userConfig == null)
		{
			Plugin.Log.Warn($"Couldn't load userConfig at {fullPath}");
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

	internal async Task<ConfigFileInfo[]> ListAvailableConfigs()
	{
		var createFileTasks = directories.Configs
			.EnumerateFiles("*.json", SearchOption.AllDirectories)
			.Where(file => !file.FullName.StartsWith(directories.Backups.FullName))
			.Select(CreateConfigFileInfo);

		return await Task.WhenAll(createFileTasks);

		async Task<ConfigFileInfo> CreateConfigFileInfo(FileInfo file)
		{
			var config = await LoadConfig(Path.Combine(directories.Configs.FullName, file.Name));
			var configName = Path.GetFileNameWithoutExtension(file.Name);
			return new(configName, file.FullName.Substring(directories.Configs.FullName.Length + 1))
			{
				Configuration = config,
				State = configMigrator.GetConfigState(config, configName)
			};
		}
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
			configFileInfo = configMigrator.MigrateConfig(configFileInfo);
			await SaveConfig(configFileInfo);
			Plugin.Log.Debug($"Config migration finished successfully and updated config is stored to disk at path: '{configFileInfo.ConfigPath}'");
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
		var fullPath = Path.Combine(directories.Configs.FullName, relativePath);
		if (File.Exists(fullPath))
		{
			File.Delete(fullPath);
		}
	}

	private async Task<HsvConfigModel?> LoadConfig(string relativePath)
	{
		try
		{
			using var streamReader = new StreamReader(Path.Combine(directories.Configs.FullName, relativePath));
			var content = await streamReader.ReadToEndAsync().ConfigureAwait(false);
			return JsonConvert.DeserializeObject<HsvConfigModel>(content, configSerializerSettings);
		}
		catch (Exception ex)
		{
			Plugin.Log.Warn($"Problem encountered when trying to load a config;\nFile path: {relativePath}\n{ex}");
			// Expected behaviour when file isn't an actual hsv config file...
			return null;
		}
	}

	private async Task SaveConfig(ConfigFileInfo configFile)
	{
		try
		{
			if (configFile.Configuration is null)
			{
				return;
			}

			await using var streamWriter = new StreamWriter(configFile.ConfigPath, false);

			var content = JsonConvert.SerializeObject(configFile.Configuration, Formatting.Indented, configSerializerSettings);
			await streamWriter.WriteAsync(content);
		}
		catch (Exception e)
		{
			Plugin.Log.Error(e);
		}
	}
}