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
	private readonly CustomCellListTableData configsList = null!;

	[UIValue("available-configs")]
	internal List<object> AvailableConfigs { get; } = [];
	public bool LoadingConfigs { get; private set; }

	[UIValue("loading-available-configs")]
	internal bool LoadingConfigs { get; private set; }

	[UIValue("has-loaded-available-configs")]
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

	internal void Select(TableView _, object obj)
	{
		pluginConfig.SelectedConfig = (ConfigFileInfo)obj;
		NotifyPropertyChanged(nameof(ConfigPickable));
		NotifyPropertyChanged(nameof(CanConfigGetYeeted));
	}

	{
		await LoadInternal().ConfigureAwait(false);
	}

	public async void PickConfig()
	{
		if (ConfigPickable && pluginConfig.SelectedConfig != null)
		{
			await configLoader.SelectUserConfig(pluginConfig.SelectedConfig).ConfigureAwait(false);
			await LoadInternal().ConfigureAwait(false);
		}
	}

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
			configsList.TableView.ClearSelection();
			configLoader.UnselectUserConfig();

			NotifyPropertyChanged(nameof(HasConfigCurrently));
			NotifyPropertyChanged(nameof(LoadedConfigText));
		}
	}

	public async void YeetConfig()
	{
		if (ConfigYeetable)
		{
			configLoader.YeetConfig(pluginConfig.SelectedConfig!.ConfigPath);
			await LoadInternal().ConfigureAwait(false);

			NotifyPropertyChanged(nameof(ConfigYeetable));
		}
	}

	protected override async void DidActivate(bool firstActivation, bool addedToHierarchy, bool screenSystemEnabling)
	{
		base.DidActivate(firstActivation, addedToHierarchy, screenSystemEnabling);
		await LoadInternal().ConfigureAwait(false);
	}

	protected override void DidDeactivate(bool removedFromHierarchy, bool screenSystemDisabling)
	{
		base.DidDeactivate(removedFromHierarchy, screenSystemDisabling);
		configsList.Data.Clear();
		pluginConfig.SelectedConfig = null;
	}

	private async Task LoadInternal()
	{
		LoadingConfigs = true;

		NotifyPropertyChanged(nameof(LoadingConfigs));
		NotifyPropertyChanged(nameof(HasLoadedConfigs));

		var intermediateConfigs = (await configLoader.ListAvailableConfigs())
			.OrderByDescending(x => x.State)
			.ThenBy(x => x.ConfigName)
			.ToList();
		var currentConfigIndex = intermediateConfigs.FindIndex(x => x.ConfigPath == pluginConfig.ConfigFilePath);

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