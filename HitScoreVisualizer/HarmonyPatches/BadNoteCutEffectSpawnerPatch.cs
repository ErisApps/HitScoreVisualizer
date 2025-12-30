using System;
using System.Linq;
using HitScoreVisualizer.Components;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities;
using HitScoreVisualizer.Utilities.Extensions;
using SiraUtil.Affinity;

namespace HitScoreVisualizer.HarmonyPatches;

internal class BadNoteCutEffectSpawnerPatch : IAffinity
{
	private readonly HsvFlyingEffectSpawner flyingEffectSpawner;
	private readonly HsvConfigModel config;
	private readonly Random random = new();

	private readonly ArrayPicker<BadCutDisplay> wrongDirectionPicker = new([]);
	private readonly ArrayPicker<BadCutDisplay> wrongColorPicker = new([]);
	private readonly ArrayPicker<BadCutDisplay> bombPicker = new([]);

	public BadNoteCutEffectSpawnerPatch(HsvFlyingEffectSpawner flyingEffectSpawner, HsvConfigModel config)
	{
		this.flyingEffectSpawner = flyingEffectSpawner;
		this.config = config;

		if (config.BadCutDisplays is null or [])
		{
			return;
		}

		wrongDirectionPicker = new(config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.WrongDirection or BadCutDisplayType.All).ToArray());
		wrongColorPicker = new(config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.WrongColor or BadCutDisplayType.All).ToArray());
		bombPicker = new(config.BadCutDisplays.Where(x => x.Type is BadCutDisplayType.Bomb or BadCutDisplayType.All).ToArray());
	}

	[AffinityPrefix]
	[AffinityPriority(1000)]
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
			return !TrySpawnText(bombPicker, noteController, in noteCutInfo);
		}

		if (!noteCutInfo.IsBadCut())
		{
			// Not a bomb or a bad cut, do nothing
			return false;
		}

		return !TrySpawnText(noteCutInfo.saberTypeOK ? wrongDirectionPicker : wrongColorPicker, noteController, in noteCutInfo);
	}

	private bool TrySpawnText(ArrayPicker<BadCutDisplay> picker, NoteController noteController, in NoteCutInfo noteCutInfo)
	{
		if (config.RandomizeBadCutDisplays && picker.TryGetRandom(random, out var display))
		{
			SpawnText(display, noteController, in noteCutInfo);
			return true;
		}

		if (picker.TryGetNext(out display))
		{
			SpawnText(display, noteController, in noteCutInfo);
			return true;
		}

		return false;
	}

	private void SpawnText(BadCutDisplay display, NoteController noteController, in NoteCutInfo noteCutInfo)
	{
		flyingEffectSpawner.SpawnText(
			noteCutInfo.cutPoint,
			noteController.worldRotation,
			noteController.inverseWorldRotation,
			display.Text,
			display.Color);
	}
}