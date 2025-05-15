using System.Linq;
using HitScoreVisualizer.Components;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using SiraUtil.Affinity;
using UnityEngine;
using Random = System.Random;

namespace HitScoreVisualizer.HarmonyPatches;

internal class BadNoteCutEffectSpawnerPatch : IAffinity
{
	private readonly HsvFlyingEffectSpawner flyingEffectSpawner;
	private readonly PluginConfig pluginConfig;
	private readonly Random random;

	public BadNoteCutEffectSpawnerPatch(HsvFlyingEffectSpawner flyingEffectSpawner, PluginConfig pluginConfig, Random random)
	{
		this.flyingEffectSpawner = flyingEffectSpawner;
		this.pluginConfig = pluginConfig;
		this.random = random;
	}

	[AffinityPrefix]
	[AffinityPatch(typeof(BadNoteCutEffectSpawner), nameof(BadNoteCutEffectSpawner.HandleNoteWasCut))]
	private bool HandleNoteWasCutPrefix(MissedNoteEffectSpawner __instance, NoteController noteController, in NoteCutInfo noteCutInfo)
	{
		var badCutDisplays = pluginConfig.SelectedConfig?.Config?.BadCutDisplays;
		if (badCutDisplays is null or [])
		{
			return true;
		}

		var bombs = badCutDisplays.Where(x => x.Type is BadCutDisplayType.Bomb or BadCutDisplayType.All).ToList();
		var wrongDirections = badCutDisplays.Where(x => x.Type is BadCutDisplayType.WrongDirection or BadCutDisplayType.All).ToList();
		var wrongColors = badCutDisplays.Where(x => x.Type is BadCutDisplayType.WrongColor or BadCutDisplayType.All).ToList();

		if (noteController.noteData.time + 0.5f < __instance._audioTimeSyncController.songTime)
		{
			// Do nothing
			return false;
		}

		if (noteController.IsBomb())
		{
			if (bombs is [])
			{
				return true;
			}

			var display = bombs[random.Next(bombs.Count)];
			SpawnText(display.Text, display.Color.ToColor(), in noteCutInfo);
		}
		else if (noteCutInfo.IsBadCut())
		{
			if (noteCutInfo.saberTypeOK)
			{
				if (wrongDirections is [])
				{
					return true;
				}

				var display = wrongDirections[random.Next(wrongDirections.Count)];
				SpawnText(display.Text, display.Color.ToColor(), in noteCutInfo);
			}
			else
			{
				if (wrongColors is [])
				{
					return true;
				}

				var display = wrongColors[random.Next(wrongColors.Count)];
				SpawnText(display.Text, display.Color.ToColor(), in noteCutInfo);
			}
		}

		// Cancel the original implementation
		return false;

		void SpawnText(string text, Color? color, in NoteCutInfo noteCutInfo)
		{
			flyingEffectSpawner.SpawnText(noteCutInfo.cutPoint, noteController.worldRotation, noteController.inverseWorldRotation, text, color);
		}
	}
}