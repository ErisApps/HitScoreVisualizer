using System.Collections.Generic;
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
	public CustomCellListTableData? customListTableData;

	[UIValue("available-configs")]
	internal List<object> AvailableConfigs { get; } = [];

	[UIValue("loading-available-configs")]
	internal bool LoadingConfigs { get; private set; }

	[UIValue("has-loaded-available-configs")]
	internal bool HasLoadedConfigs => !LoadingConfigs;

	[UIValue("is-valid-config-selected")]
	internal bool ConfigPickable =>
		pluginConfig.SelectedConfig?.ConfigSelectable() ?? false;

	[UIValue("has-config-loaded")]
	internal bool HasConfigCurrently =>
		!string.IsNullOrWhiteSpace(pluginConfig.ConfigFilePath);

	[UIValue("config-loaded-text")]
	internal string LoadedConfigText =>
		$"Currently Loaded Config<size=90%> : {(HasConfigCurrently ? Path.GetFileNameWithoutExtension(pluginConfig.ConfigFilePath) : "None")}";

	[UIValue("is-config-yeetable")]
	internal bool CanConfigGetYeeted =>
		pluginConfig.SelectedConfig != null && pluginConfig.SelectedConfig.ConfigPath != pluginConfig.ConfigFilePath;

	[UIAction("config-Selected")]
	internal void Select(TableView _, object obj)
	{
		pluginConfig.SelectedConfig = (ConfigFileInfo)obj;
		NotifyPropertyChanged(nameof(ConfigPickable));
		NotifyPropertyChanged(nameof(CanConfigGetYeeted));
	}

	[UIAction("reload-list")]
	internal async void RefreshList()
	{
		await LoadInternal().ConfigureAwait(false);
	}

	[UIAction("pick-config")]
	internal async void PickConfig()
	{
		if (ConfigPickable && pluginConfig.SelectedConfig != null)
		{
			await configLoader.SelectUserConfig(pluginConfig.SelectedConfig).ConfigureAwait(false);
			await LoadInternal().ConfigureAwait(false);
		}
	}

	[UIAction("unpick-config")]
	internal void UnpickConfig()
	{
		if (HasConfigCurrently)
		{
			UnityMainThreadTaskScheduler.Factory.StartNew(() =>
			{
				if (customListTableData == null)
				{
					Plugin.Log.Warn($"{nameof(customListTableData)} is null.");
					return;
				}

				customListTableData.TableView.ClearSelection();
			});

			configLoader.UnselectUserConfig();
			NotifyPropertyChanged(nameof(HasConfigCurrently));
			NotifyPropertyChanged(nameof(LoadedConfigText));
		}
	}

	[UIAction("yeet-config")]
	internal async void YeetConfig()
	{
		if (!CanConfigGetYeeted)
		{
			return;
		}

		configLoader.YeetConfig(pluginConfig.SelectedConfig!.ConfigPath);
		await LoadInternal().ConfigureAwait(false);

		NotifyPropertyChanged(nameof(CanConfigGetYeeted));
	}

	protected override async void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
	{
		base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);

		await LoadInternal().ConfigureAwait(false);
	}

	protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
	{
		base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);

		AvailableConfigs.Clear();

		pluginConfig.SelectedConfig = null;
	}

	private async Task LoadInternal()
	{
		if (customListTableData == null)
		{
			Plugin.Log.Warn($"{nameof(customListTableData)} is null.");
			return;
		}

		if (AvailableConfigs.Count > 0)
		{
			AvailableConfigs.Clear();
		}

		LoadingConfigs = true;
		NotifyPropertyChanged(nameof(LoadingConfigs));
		NotifyPropertyChanged(nameof(HasLoadedConfigs));

		var intermediateConfigs = (await configLoader.ListAvailableConfigs())
			.OrderByDescending(x => x.State)
			.ThenBy(x => x.ConfigName)
			.ToList();
		AvailableConfigs.AddRange(intermediateConfigs);

		var currentConfigIndex = intermediateConfigs.FindIndex(x => x.ConfigPath == pluginConfig.ConfigFilePath);

		await UnityMainThreadTaskScheduler.Factory.StartNew(() =>
		{
			customListTableData.TableView.ReloadData();
			customListTableData.TableView.ScrollToCellWithIdx(0, TableView.ScrollPositionType.Beginning, false);
			if (currentConfigIndex >= 0)
			{
				customListTableData.TableView.SelectCellWithIdx(currentConfigIndex, true);
			}

			LoadingConfigs = false;
			NotifyPropertyChanged(nameof(LoadingConfigs));
			NotifyPropertyChanged(nameof(HasLoadedConfigs));
			NotifyPropertyChanged(nameof(HasConfigCurrently));
			NotifyPropertyChanged(nameof(LoadedConfigText));
		}).ConfigureAwait(false);
	}
}