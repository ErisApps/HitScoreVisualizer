using SiraUtil.Affinity;
using UnityEngine;

namespace HitScoreVisualizer.HarmonyPatches;

internal class FlyingScoreSpawnerPatch : IAffinity
{
	[AffinityPrefix]
	[AffinityPriority(1000)]
	[AffinityPatch(typeof(FlyingScoreSpawner), nameof(FlyingScoreSpawner.SpawnFlyingScoreNextFrame))]
	internal bool SpawnFlyingScoreNextFrame(FlyingScoreSpawner __instance, IReadonlyCutScoreBuffer cutScoreBuffer, Color color)
	{
		// The scoring element owns a pooled CutScoreBuffer. If spawning is deferred for a frame,
		// the element can be despawned and its buffer reused for another note before it is read.
		__instance.SpawnFlyingScore(cutScoreBuffer, color);
		return false;
	}
}
