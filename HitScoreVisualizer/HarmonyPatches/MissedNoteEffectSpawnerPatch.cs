using System;
using HitScoreVisualizer.Components;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities;
using SiraUtil.Affinity;

namespace HitScoreVisualizer.HarmonyPatches;

internal class MissedNoteEffectSpawnerPatch : IAffinity
{
	private readonly HsvFlyingEffectSpawner flyingEffectSpawner;
	private readonly HsvConfigModel config;
	private readonly Random random;

	private readonly ArrayPicker<MissDisplay> missPicker = new([]);

	public MissedNoteEffectSpawnerPatch(HsvFlyingEffectSpawner flyingEffectSpawner, HsvConfigModel config, Random random)
	{
		this.flyingEffectSpawner = flyingEffectSpawner;
		this.config = config;
		this.random = random;

		if (config.MissDisplays is null)
		{
			return;
		}

		missPicker = new(config.MissDisplays.ToArray());
	}

	[AffinityPrefix]
	[AffinityPatch(typeof(MissedNoteEffectSpawner), nameof(MissedNoteEffectSpawner.HandleNoteWasMissed))]
	private bool HandleNoteWasMissedPrefix(MissedNoteEffectSpawner __instance, NoteController noteController)
	{
		if (noteController.hidden
		    || noteController.noteData.time + 0.5f < __instance._audioTimeSyncController.songTime
		    || noteController.noteData.colorType == ColorType.None)
		{
			// Do nothing
			return false;
		}

		return !TrySpawnText(missPicker, noteController, __instance._spawnPosZ);
	}

	private bool TrySpawnText(ArrayPicker<MissDisplay> picker, NoteController noteController, float spawnPosZ)
	{
		if (config.RandomizeMissDisplays && picker.TryGetRandomDisplay(random, out var display))
		{
			SpawnText(display, noteController, spawnPosZ);
			return true;
		}

		if (picker.TryGetNextDisplay(out display))
		{
			SpawnText(display, noteController, spawnPosZ);
			return true;
		}

		return false;
	}

	private void SpawnText(MissDisplay display, NoteController noteController, float spawnPosZ)
	{
		var position = noteController.inverseWorldRotation * noteController.noteTransform.position;
		position.z = spawnPosZ;

		flyingEffectSpawner.SpawnText(
			noteController.worldRotation * position,
			noteController.worldRotation,
			noteController.inverseWorldRotation,
			display.Text,
			display.Color);
	}
}