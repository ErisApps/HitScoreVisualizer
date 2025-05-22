using System;
using BeatSaberMarkupLanguage.MenuButtons;
using Zenject;

namespace HitScoreVisualizer.UI;

internal class MenuButtonManager : IInitializable, IDisposable
{
	private readonly HsvMainFlowCoordinator hsvMainFlowCoordinator;
	private readonly MainFlowCoordinator mainFlowCoordinator;
	private readonly MenuButtons menuButtons;
	private readonly MenuButton hsvMenuButton;

	private const string Title = "Hit Score Visualizer";
	private const string HoverHint = "Change your score visualizer config";

	public MenuButtonManager(
		HsvMainFlowCoordinator hsvMainFlowCoordinator,
		MainFlowCoordinator mainFlowCoordinator,
		MenuButtons menuButtons)
	{
		this.hsvMainFlowCoordinator = hsvMainFlowCoordinator;
		this.mainFlowCoordinator = mainFlowCoordinator;
		this.menuButtons = menuButtons;
		hsvMenuButton = new(Title, HoverHint, OnClick);
	}

	public void Initialize()
	{
		menuButtons.RegisterButton(hsvMenuButton);
	}

	public void Dispose()
	{
		menuButtons.UnregisterButton(hsvMenuButton);
	}

	private void OnClick()
	{
		mainFlowCoordinator.PresentFlowCoordinator(hsvMainFlowCoordinator);
	}
}