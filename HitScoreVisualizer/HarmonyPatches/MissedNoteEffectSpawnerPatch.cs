using System;
using HitScoreVisualizer.Components;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using SiraUtil.Affinity;

namespace HitScoreVisualizer.HarmonyPatches;

internal class MissedNoteEffectSpawnerPatch : IAffinity
{
	private readonly HsvFlyingEffectSpawner flyingEffectSpawner;
	private readonly Random random;

	private readonly MissDisplay[] missDisplays = [];

	public MissedNoteEffectSpawnerPatch(HsvFlyingEffectSpawner flyingEffectSpawner, HsvConfigModel config, Random random)
	{
		this.flyingEffectSpawner = flyingEffectSpawner;
		this.random = random;

		if (config.MissDisplays is null)
		{
			return;
		}

		missDisplays = config.MissDisplays.ToArray();
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

		return !TrySpawnRandomText(missDisplays, noteController, __instance._spawnPosZ);
	}

	private bool TrySpawnRandomText(MissDisplay[] displays, NoteController noteController, float spawnPosZ)
	{
		if (displays is [])
		{
			return false;
		}

		var position = noteController.inverseWorldRotation * noteController.noteTransform.position;
		position.z = spawnPosZ;

		var display = displays[random.Next(0, displays.Length)];

		flyingEffectSpawner.SpawnText(
			noteController.worldRotation * position,
			noteController.worldRotation,
			noteController.inverseWorldRotation,
			display.Text,
			display.Color.ToColor());

		return true;
	}
}