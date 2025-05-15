using System;
using System.Linq;
using HitScoreVisualizer.Components;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using SiraUtil.Affinity;

namespace HitScoreVisualizer.HarmonyPatches;

internal class BadNoteCutEffectSpawnerPatch : IAffinity
{
	private readonly HsvFlyingEffectSpawner flyingEffectSpawner;
	private readonly Random random;

	private readonly BadCutDisplay[] wrongDirectionDisplays = [];
	private readonly BadCutDisplay[] wrongColorDisplays = [];
	private readonly BadCutDisplay[] bombDisplays = [];

	public BadNoteCutEffectSpawnerPatch(HsvFlyingEffectSpawner flyingEffectSpawner, HsvConfigModel config, Random random)
	{
		this.flyingEffectSpawner = flyingEffectSpawner;
		this.random = random;

		if (config.BadCutDisplays is null or [])
		{
			return;
		}

		wrongDirectionDisplays = config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.WrongDirection or BadCutDisplayType.All).ToArray();
		wrongColorDisplays = config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.WrongColor or BadCutDisplayType.All).ToArray();
		bombDisplays = config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.Bomb or BadCutDisplayType.All).ToArray();
	}

	[AffinityPrefix]
	[AffinityPatch(typeof(BadNoteCutEffectSpawner), nameof(BadNoteCutEffectSpawner.HandleNoteWasCut))]
	private bool HandleNoteWasCutPrefix(MissedNoteEffectSpawner __instance, NoteController noteController, in NoteCutInfo noteCutInfo)
	{
		if (noteController.noteData.time + 0.5f < __instance._audioTimeSyncController.songTime)
		{
			// Do nothing
			return false;
		}

		if (noteController.IsBomb())
		{
			return !TrySpawnRandomText(bombDisplays, in noteCutInfo, noteController);
		}

		if (!noteCutInfo.IsBadCut())
		{
			// Not a bomb or a bad cut, do nothing
			return false;
		}

		if (!noteCutInfo.saberTypeOK)
		{
			return !TrySpawnRandomText(wrongColorDisplays, in noteCutInfo, noteController);
		}

		return !TrySpawnRandomText(wrongDirectionDisplays, in noteCutInfo, noteController);

		// Cancel the original implementation
	}


	private bool TrySpawnRandomText(BadCutDisplay[] displays, in NoteCutInfo noteCutInfo, NoteController noteController)
	{
		if (displays is [])
		{
			return false;
		}

		var display = displays[random.Next(0, displays.Length)];

		flyingEffectSpawner.SpawnText(
			noteCutInfo.cutPoint,
			noteController.worldRotation,
			noteController.inverseWorldRotation,
			display.Text,
			display.Color.ToColor());

		return true;
	}
}