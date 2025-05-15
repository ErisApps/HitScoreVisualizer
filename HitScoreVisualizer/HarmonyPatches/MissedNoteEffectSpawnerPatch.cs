using HitScoreVisualizer.Components;
using SiraUtil.Affinity;

namespace HitScoreVisualizer.HarmonyPatches;

internal class MissedNoteEffectSpawnerPatch : IAffinity
{
	private readonly HsvFlyingEffectSpawner flyingEffectSpawner;

	public MissedNoteEffectSpawnerPatch(HsvFlyingEffectSpawner flyingEffectSpawner)
	{
		this.flyingEffectSpawner = flyingEffectSpawner;
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

		var position = noteController.inverseWorldRotation * noteController.noteTransform.position;
		position.z = __instance._spawnPosZ;

		flyingEffectSpawner.SpawnText(
			noteController.worldRotation * position,
			noteController.worldRotation,
			noteController.inverseWorldRotation,
			"MISSED",
			null);

		// Cancel the original implementation
		return false;
	}
}