using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Components;
using BeatSaberMarkupLanguage.ViewControllers;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Services;
using HMUI;
using IPA.Utilities.Async;
using SiraUtil.Logging;
using Zenject;

namespace HitScoreVisualizer.UI;

[HotReload(RelativePathToLayout = @"Views\ConfigSelector.bsml")]
[ViewDefinition("HitScoreVisualizer.UI.Views.ConfigSelector.bsml")]
internal class ConfigSelectorViewController : BSMLAutomaticViewController
{
	private SiraLog siraLog = null!;
	private ConfigProvider configProvider = null!;

	private ConfigFileInfo? selectedConfigFileInfo;

	[Inject]
	internal void Construct(SiraLog siraLog, ConfigProvider configProvider)
	{
		this.siraLog = siraLog;
		this.configProvider = configProvider;
	}

	[UIComponent("configs-list")]
	public CustomCellListTableData? customListTableData;

	[UIValue("available-configs")]
	internal List<object> AvailableConfigs { get; } = [];

	[UIValue("loading-available-configs")]
	internal bool LoadingConfigs { get; private set; }

	[UIValue("has-loaded-available-configs")]
	internal bool HasLoadedConfigs => !LoadingConfigs;

	[UIValue("is-valid-config-selected")]
	internal bool CanConfigGetSelected => selectedConfigFileInfo?.ConfigPath != configProvider.CurrentConfigPath && ConfigProvider.ConfigSelectable(selectedConfigFileInfo?.State);

	[UIValue("has-config-loaded")]
	internal bool HasConfigCurrently => !string.IsNullOrWhiteSpace(configProvider.CurrentConfigPath);

	[UIValue("config-loaded-text")]
	internal string LoadedConfigText => $"Currently Loaded Config<size=90%> : {(HasConfigCurrently ? Path.GetFileNameWithoutExtension(configProvider.CurrentConfigPath) : "None")}";

	[UIValue("is-config-yeetable")]
	internal bool CanConfigGetYeeted => selectedConfigFileInfo?.ConfigPath != null && selectedConfigFileInfo.ConfigPath != configProvider.CurrentConfigPath;

	[UIAction("config-Selected")]
	internal void Select(TableView _, object @object)
	{
		selectedConfigFileInfo = (ConfigFileInfo)@object;
		NotifyPropertyChanged(nameof(CanConfigGetSelected));
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
		if (CanConfigGetSelected && selectedConfigFileInfo != null)
		{
			await configProvider.SelectUserConfig(selectedConfigFileInfo).ConfigureAwait(false);
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
					siraLog.Warn($"{nameof(customListTableData)} is null.");
					return;
				}

				customListTableData.TableView.ClearSelection();
			});

			configProvider.UnselectUserConfig();
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

		configProvider.YeetConfig(selectedConfigFileInfo!.ConfigPath);
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

		selectedConfigFileInfo = null;
	}

	private async Task LoadInternal()
	{
		if (customListTableData == null)
		{
			siraLog.Warn($"{nameof(customListTableData)} is null.");
			return;
		}

		if (AvailableConfigs.Count > 0)
		{
			AvailableConfigs.Clear();
		}

		LoadingConfigs = true;
		NotifyPropertyChanged(nameof(LoadingConfigs));
		NotifyPropertyChanged(nameof(HasLoadedConfigs));

		var intermediateConfigs = (await configProvider.ListAvailableConfigs())
			.OrderByDescending(x => x.State)
			.ThenBy(x => x.ConfigName)
			.ToList();
		AvailableConfigs.AddRange(intermediateConfigs);

		var currentConfigIndex = intermediateConfigs.FindIndex(x => x.ConfigPath == configProvider.CurrentConfigPath);

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