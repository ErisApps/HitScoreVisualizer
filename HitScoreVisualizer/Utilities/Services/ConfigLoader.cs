using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using HitScoreVisualizer.Utilities.Json;
using IPA.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Zenject;

namespace HitScoreVisualizer.Utilities.Services;

public class ConfigLoader : IInitializable
{
	private readonly PluginConfig pluginConfig;
	private readonly ConfigMigrator configMigrator;
	private readonly PluginDirectories directories;

	private readonly JsonSerializerSettings configSerializerSettings = new()
	{
		DefaultValueHandling = DefaultValueHandling.Include,
		NullValueHandling = NullValueHandling.Ignore,
		Formatting = Formatting.Indented,
		Converters = [ new Vector3Converter(), new StringEnumConverter(), new ColorArrayConverter() ],
		ContractResolver = new HsvConfigContractResolver()
	};

	internal ConfigLoader(PluginConfig pluginConfig, ConfigMigrator configMigrator, PluginDirectories directories)
	{
		this.pluginConfig = pluginConfig;
		this.configMigrator = configMigrator;
		this.directories = directories;
	}

	public event Action<HsvConfigModel?>? ConfigChanged;

	public async void Initialize()
	{
		try
		{
			await CreateDefaultConfig();
			await LoadSelectedConfig();
		}
		catch (Exception ex)
		{
			Plugin.Log.Error($"Problem encountered while initializing loader:\n {ex}");
		}
	}

	internal async Task<ConfigInfo[]> LoadAllHsvConfigs()
	{
		var createFileTasks = directories.Configs
			.EnumerateFiles("*.json", SearchOption.AllDirectories)
			.Where(file => !file.FullName.StartsWith(directories.Backups.FullName))
			.Select(GetConfigInfo);

		return await Task.WhenAll(createFileTasks);
	}

	internal async Task<bool> TrySelectConfig(ConfigInfo? configInfo)
	{
		if (configInfo is not { State: ConfigState.Compatible or ConfigState.NeedsMigration })
		{
			Plugin.Log.Warn("Tried selecting a config that is not compatible with the current version of the plugin");
			pluginConfig.ConfigFilePath = null;
			ConfigChanged?.Invoke(null);
			return false;
		}

		if (configInfo.State is ConfigState.NeedsMigration)
		{
			Plugin.Log.Warn("Selected a config that needs migration");
			configInfo = configMigrator.MigrateConfig(configInfo);
			await SaveConfig(configInfo);
		}

		if (configInfo is { Config: not null, State: ConfigState.Compatible })
		{
			Plugin.Log.Info($"Selecting config {configInfo.ConfigName}");
			pluginConfig.SelectedConfig = configInfo;
			pluginConfig.ConfigFilePath = configInfo.File.FullName.Substring(directories.Configs.FullName.Length + 1);
			ConfigChanged?.Invoke(configInfo.Config);
		}

		return pluginConfig.ConfigFilePath != null;
	}

	private async Task<ConfigInfo> GetConfigInfo(FileInfo file)
	{
		var config = await TryLoadConfig(file);
		var version = config?.GetVersion() ?? Plugin.Metadata.HVersion;
		var state = configMigrator.GetConfigState(config);
		var description = state.GetConfigDescription(version);
		return new(file, description, state)
		{
			Config = config
		};
	}

	private async Task<HsvConfigModel?> TryLoadConfig(FileInfo file)
	{
		try
		{
			using var streamReader = file.OpenText();
			var content = await streamReader.ReadToEndAsync();
			return JsonConvert.DeserializeObject<HsvConfigModel>(content, configSerializerSettings);
		}
		catch (Exception ex)
		{
			Plugin.Log.Warn($"Problem encountered when trying to load {file.Name}\n{ex}");
			return null;
		}
	}

	private async Task SaveConfig(ConfigInfo config)
	{
		try
		{
			if (config.Config is null)
			{
				return;
			}

			Plugin.Log.Info($"Saving config {config.ConfigName}");
			await using var streamWriter = config.File.CreateText();
			var content = JsonConvert.SerializeObject(config.Config, Formatting.Indented, configSerializerSettings);
			await streamWriter.WriteAsync(content);
		}
		catch (Exception e)
		{
			Plugin.Log.Error(e);
		}
	}

	private async Task CreateDefaultConfig()
	{
		const string defaultConfigName = "HitScoreVisualizerConfig (default).json";
		var defaultConfigPath = Path.Combine(directories.Configs.FullName, defaultConfigName);
		var defaultConfigFile = new FileInfo(defaultConfigPath);
		if (!defaultConfigFile.Exists)
		{
			var defaultConfigDescription = ConfigState.Compatible.GetConfigDescription(Plugin.Metadata.HVersion);
			await SaveConfig(new(defaultConfigFile, defaultConfigDescription, ConfigState.Compatible)
			{
				Config = HsvConfigModel.Default
			});
		}

		var legacyConfigPath = Path.Combine(UnityGame.UserDataPath, "HitScoreVisualizerConfig.json");
		var legacyConfigFile = new FileInfo(legacyConfigPath);
		if (legacyConfigFile.Exists)
		{
			var destinationHsvConfigPath = Path.Combine(directories.Configs.FullName, "HitScoreVisualizerConfig (imported).json");
			legacyConfigFile.MoveTo(destinationHsvConfigPath);
		}
	}

	private async Task LoadSelectedConfig()
	{
		if (pluginConfig.ConfigFilePath == null)
		{
			return;
		}

		var fullPath = Path.Combine(directories.Configs.FullName, pluginConfig.ConfigFilePath);
		var fileInfo = new FileInfo(fullPath);
		if (!fileInfo.Exists)
		{
			Plugin.Log.Warn("Selected config file was not found; resetting to default.");
			pluginConfig.ConfigFilePath = null;
			return;
		}

		var selectedConfig = await GetConfigInfo(fileInfo);
		if (selectedConfig.Config is null)
		{
			Plugin.Log.Warn("Problem encountered when trying to load selected config; resetting to default.");
			pluginConfig.ConfigFilePath = null;
			return;
		}

		if (selectedConfig.State.HasWarning())
		{
			Plugin.Log.Warn(selectedConfig.State.GetWarningMessage(selectedConfig.ConfigName, selectedConfig.Config.GetVersion()));
		}

		await TrySelectConfig(selectedConfig);
	}
}