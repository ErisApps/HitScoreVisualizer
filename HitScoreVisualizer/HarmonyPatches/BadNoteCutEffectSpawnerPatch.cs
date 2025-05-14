using HitScoreVisualizer.Components;
using HitScoreVisualizer.Utilities.Extensions;
using SiraUtil.Affinity;

namespace HitScoreVisualizer.HarmonyPatches;

internal class BadNoteCutEffectSpawnerPatch : IAffinity
{
	private readonly HsvFlyingEffectSpawner flyingEffectSpawner;

	public BadNoteCutEffectSpawnerPatch(HsvFlyingEffectSpawner flyingEffectSpawner)
	{
		this.flyingEffectSpawner = flyingEffectSpawner;
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
			SpawnText("BOMB'D", in noteCutInfo);
		}
		else if (noteCutInfo.IsBadCut())
		{
			SpawnText(noteCutInfo.saberTypeOK ? "WRONG DIRECTION" : "WRONG COLOR", in noteCutInfo);
		}

		// Cancel the original implementation
		return false;

		void SpawnText(string text, in NoteCutInfo noteCutInfo)
		{
			flyingEffectSpawner.SpawnText(noteCutInfo.cutPoint, noteController.worldRotation, noteController.inverseWorldRotation, text);
		}
	}
}