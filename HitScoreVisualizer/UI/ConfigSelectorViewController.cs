using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using HitScoreVisualizer.Utilities.Services;
using HMUI;
using IPA.Utilities.Async;
using Zenject;

namespace HitScoreVisualizer.UI;

[HotReload(RelativePathToLayout = @"Views\ConfigSelector.bsml")]
[ViewDefinition("HitScoreVisualizer.UI.Views.ConfigSelector.bsml")]
internal class ConfigSelectorViewController : BSMLAutomaticViewController
{
	[Inject] private readonly PluginConfig pluginConfig = null!;
	[Inject] private readonly ConfigLoader configLoader = null!;

	[UIComponent("configs-list")]
	private readonly CustomCellListTableData configsList = null!;

	public bool LoadingConfigs { get; private set; }

	public bool HasLoadedConfigs => !LoadingConfigs;

	public bool ConfigPickable =>
		pluginConfig.SelectedConfig is { State: ConfigState.Compatible or ConfigState.NeedsMigration };

	public bool HasConfigCurrently =>
		!string.IsNullOrWhiteSpace(pluginConfig.ConfigFilePath);

	public string LoadedConfigText =>
		$"Currently Loaded Config<size=90%> : {(HasConfigCurrently ? Path.GetFileNameWithoutExtension(pluginConfig.ConfigFilePath) : "None")}";

	public bool ConfigYeetable => pluginConfig.SelectedConfig is { Config: not null };

	public void ConfigSelected(TableView tableView, object obj)
	{
		pluginConfig.SelectedConfig = (ConfigInfo)obj;
		NotifyPropertyChanged(nameof(ConfigPickable));
		NotifyPropertyChanged(nameof(ConfigYeetable));
	}

	public async void RefreshList()
	{
		try
		{
			await RefreshListInternal();
		}
		catch (Exception ex)
		{
			Plugin.Log.Error($"Encountered a problem while refreshing config list: {ex}");
		}
	}

	public async void PickConfig()
	{
		try
		{
			if (await configLoader.TrySelectConfig(pluginConfig.SelectedConfig))
			{
				await RefreshListInternal();
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.Error($"Encountered a problem while picking selected config: {ex}");
		}
	}

	public void UnpickConfig()
	{
		if (!HasConfigCurrently)
		{
			return;
		}

		configsList.TableView.ClearSelection();
		pluginConfig.ConfigFilePath = null;
		pluginConfig.SelectedConfig = null;

		NotifyPropertyChanged(nameof(HasConfigCurrently));
		NotifyPropertyChanged(nameof(LoadedConfigText));
	}

	public async void YeetConfig()
	{
		try
		{
			if (!ConfigYeetable)
			{
				return;
			}

			pluginConfig.SelectedConfig?.Yeet();
			await RefreshListInternal();

			NotifyPropertyChanged(nameof(ConfigYeetable));
		}
		catch (Exception ex)
		{
			Plugin.Log.Error($"Encountered a problem while deleting selected config: {ex}");
		}
	}

	protected override async void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
	{
		try
		{
			base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
			await RefreshListInternal();
		}
		catch (Exception ex)
		{
			Plugin.Log.Error($"Encountered a problem while deactivating {nameof(ConfigSelectorViewController)}: {ex}");
		}
	}

	protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
	{
		base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
		configsList.Data.Clear();
		pluginConfig.SelectedConfig = null;
	}

	private async Task RefreshListInternal()
	{
		LoadingConfigs = true;

		NotifyPropertyChanged(nameof(LoadingConfigs));
		NotifyPropertyChanged(nameof(HasLoadedConfigs));

		var intermediateConfigs = (await configLoader.LoadAllHsvConfigs())
			.OrderByDescending(x => x.State)
			.ThenBy(x => x.ConfigName)
			.ToList();
		var currentConfigIndex = intermediateConfigs.FindIndex(configInfo => configInfo.File.FullName == pluginConfig.SelectedConfig?.File.FullName);

		configsList.Data = intermediateConfigs;

		await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
		{
			configsList.TableView.ReloadData();
			configsList.TableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
			if (currentConfigIndex >= 0)
			{
				configsList.TableView.SelectCellWithIdx(currentConfigIndex, true);
			}

			LoadingConfigs = false;
			NotifyPropertyChanged(nameof(LoadingConfigs));
			NotifyPropertyChanged(nameof(HasLoadedConfigs));
			NotifyPropertyChanged(nameof(HasConfigCurrently));
			NotifyPropertyChanged(nameof(LoadedConfigText));
		});
	}
}