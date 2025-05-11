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
		pluginConfig.SelectedConfig?.ConfigSelectable() ?? false;

	public bool HasConfigCurrently =>
		!string.IsNullOrWhiteSpace(pluginConfig.ConfigFilePath);

	public string LoadedConfigText =>
		$"Currently Loaded Config<size=90%> : {(HasConfigCurrently ? Path.GetFileNameWithoutExtension(pluginConfig.ConfigFilePath) : "None")}";

	public bool ConfigYeetable =>
		pluginConfig.SelectedConfig != null && pluginConfig.SelectedConfig.ConfigPath != pluginConfig.ConfigFilePath;

	public void ConfigSelected(TableView _, object obj)
	{
		pluginConfig.SelectedConfig = (ConfigFileInfo)obj;
		NotifyPropertyChanged(nameof(ConfigPickable));
		NotifyPropertyChanged(nameof(ConfigYeetable));
	}

	public async void RefreshList()
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

	public void UnpickConfig()
	{
		if (HasConfigCurrently)
		{
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