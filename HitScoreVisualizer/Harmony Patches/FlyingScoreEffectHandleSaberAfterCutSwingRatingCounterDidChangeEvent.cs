using HarmonyLib;
using HitScoreVisualizer.Services;
using UnityEngine;

namespace HitScoreVisualizer.Harmony_Patches
{
	[HarmonyPatch(typeof(FlyingScoreEffect))]
	[HarmonyPatch("HandleSaberSwingRatingCounterDidChangeEvent", MethodType.Normal)]
	internal class FlyingScoreEffectHandleSaberAfterCutSwingRatingCounterDidChangeEvent
	{
// ReSharper disable InconsistentNaming
		internal static bool Prefix(FlyingScoreEffect __instance, NoteCutInfo ____noteCutInfo)
// ReSharper enable InconsistentNaming
		{
			if (ConfigProvider.CurrentConfig == null)
			{
				return true;
			}

			if (ConfigProvider.CurrentConfig.DoIntermediateUpdates)
			{
				ScoreModel.RawScoreWithoutMultiplier(____noteCutInfo, out var before, out var after, out var accuracy);
				var total = before + after + accuracy;
				var timeDependence = Mathf.Abs(____noteCutInfo.cutNormal.z);
				JudgmentService.Judge(__instance, total, before, after, accuracy, timeDependence);
			}

			return false;
		}
	}
}