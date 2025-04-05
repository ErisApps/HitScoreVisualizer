using System;
using BeatSaberMarkupLanguage.MenuButtons;
using Zenject;

namespace HitScoreVisualizer.UI;

internal class MenuButtonManager : IInitializable, IDisposable
{
	private readonly HitScoreFlowCoordinator hitScoreFlowCoordinator;
	private readonly MainFlowCoordinator mainFlowCoordinator;
	private readonly MenuButtons menuButtons;
	private readonly MenuButton hsvMenuButton;

	private const string Title = "Hit Score Visualizer";
	private const string HoverHint = "Change your score visualizer config";

	public MenuButtonManager(
		HitScoreFlowCoordinator hitScoreFlowCoordinator,
		MainFlowCoordinator mainFlowCoordinator,
		MenuButtons menuButtons)
	{
		this.hitScoreFlowCoordinator = hitScoreFlowCoordinator;
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
		mainFlowCoordinator.PresentFlowCoordinator(hitScoreFlowCoordinator);
	}
}