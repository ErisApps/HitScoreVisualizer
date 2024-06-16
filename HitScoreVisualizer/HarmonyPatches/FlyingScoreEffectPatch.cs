using HitScoreVisualizer.Services;
using SiraUtil.Affinity;
using UnityEngine;

namespace HitScoreVisualizer.HarmonyPatches
{
	internal class FlyingScoreEffectPatch(JudgmentService judgmentService, ConfigProvider configProvider) : IAffinity
	{
		private readonly JudgmentService judgmentService = judgmentService;
		private readonly ConfigProvider configProvider = configProvider;

		[AffinityPrefix]
		[AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.InitAndPresent))]
		internal bool InitAndPresent(ref FlyingScoreEffect __instance, IReadonlyCutScoreBuffer cutScoreBuffer, float duration, Vector3 targetPos, Color color)
		{
			var configuration = configProvider.CurrentConfig;
			var noteCutInfo = cutScoreBuffer.noteCutInfo;

			if (configuration != null)
			{
				if (configuration.FixedPosition != null)
				{
					// Set current and target position to the desired fixed position
					targetPos = configuration.FixedPosition.Value;
					__instance.transform.position = targetPos;
				}
				else if (configuration.TargetPositionOffset != null)
				{
					targetPos += configuration.TargetPositionOffset.Value;
				}
			}

			__instance._color = color;
			__instance._cutScoreBuffer = cutScoreBuffer;
			if (!cutScoreBuffer.isFinished)
			{
				cutScoreBuffer.RegisterDidChangeReceiver(__instance);
				cutScoreBuffer.RegisterDidFinishReceiver(__instance);
				__instance._registeredToCallbacks = true;
			}

			if (configuration == null)
			{
				__instance._text.text = cutScoreBuffer.cutScore.ToString();
				__instance._maxCutDistanceScoreIndicator.enabled = cutScoreBuffer.centerDistanceCutScore == cutScoreBuffer.noteScoreDefinition.maxCenterDistanceCutScore;
				__instance._colorAMultiplier = (double) cutScoreBuffer.cutScore > (double) cutScoreBuffer.maxPossibleCutScore * 0.9f ? 1f : 0.3f;
			}
			else
			{
				__instance._maxCutDistanceScoreIndicator.enabled = false;

				// Apply judgments a total of twice - once when the effect is created, once when it finishes.
				Judge(__instance, (CutScoreBuffer)cutScoreBuffer);
			}

			__instance.InitAndPresent(duration, targetPos, noteCutInfo.worldRotation, false);

			return false;
		}

		[AffinityPrefix]
		[AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.ManualUpdate))]
		internal bool ManualUpdate(FlyingScoreEffect __instance, float t)
		{
			var color = __instance._color.ColorWithAlpha(__instance._fadeAnimationCurve.Evaluate(t));
			__instance._text.color = color;
			__instance._maxCutDistanceScoreIndicator.color = color;

			return false;
		}

		[AffinityPrefix]
		[AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.HandleCutScoreBufferDidChange))]
		internal bool HandleCutScoreBufferDidChange(FlyingScoreEffect __instance, CutScoreBuffer cutScoreBuffer)
		{
			var configuration = configProvider.CurrentConfig;
			if (configuration == null)
			{
				// Run original implementation
				return true;
			}

			if (configuration.DoIntermediateUpdates)
			{
				Judge(__instance, cutScoreBuffer);
			}

			return false;
		}

		[AffinityPrefix]
		[AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.HandleCutScoreBufferDidFinish))]
		internal void HandleCutScoreBufferDidFinish(FlyingScoreEffect __instance, CutScoreBuffer cutScoreBuffer)
		{
			if (configProvider.CurrentConfig != null)
			{
				Judge(__instance, cutScoreBuffer);
			}
		}

		private void Judge(FlyingScoreEffect flyingScoreEffect, IReadonlyCutScoreBuffer cutScoreBuffer)
		{
			(flyingScoreEffect._text.text, flyingScoreEffect._color) = judgmentService.Judge(cutScoreBuffer);
		}
	}
}
