using HitScoreVisualizer.Services;
using SiraUtil.Affinity;
using UnityEngine;

namespace HitScoreVisualizer.HarmonyPatches
{
	internal class FlyingScoreEffectPatch : IAffinity
	{
		private readonly JudgmentService _judgmentService;
		private readonly ConfigProvider _configProvider;

		public FlyingScoreEffectPatch(JudgmentService judgmentService, ConfigProvider configProvider)
		{
			_judgmentService = judgmentService;
			_configProvider = configProvider;
		}

		[AffinityPrefix]
		[AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.InitAndPresent))]
		internal bool InitAndPresent(ref FlyingScoreEffect __instance, IReadonlyCutScoreBuffer cutScoreBuffer, float duration, Vector3 targetPos, Color color)
		{
			var configuration = _configProvider.GetCurrentConfig();
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

			if (configuration == null || noteCutInfo.noteData.gameplayType is not NoteData.GameplayType.Normal)
			{
				__instance._text.text = cutScoreBuffer.cutScore.ToString();
				__instance._maxCutDistanceScoreIndicator.enabled = cutScoreBuffer.centerDistanceCutScore == cutScoreBuffer.noteScoreDefinition.maxCenterDistanceCutScore;
				__instance._colorAMultiplier = (double) cutScoreBuffer.cutScore > (double) cutScoreBuffer.maxPossibleCutScore * 0.9f ? 1f : 0.3f;
			}
			else
			{
				__instance._maxCutDistanceScoreIndicator.enabled = false;

				// Apply judgments a total of twice - once when the effect is created, once when it finishes.
				Judge(__instance, (CutScoreBuffer) cutScoreBuffer, 30);
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
			var configuration = _configProvider.GetCurrentConfig();
			if (configuration == null || cutScoreBuffer.noteCutInfo.noteData.gameplayType is not NoteData.GameplayType.Normal)
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
			var configuration = _configProvider.GetCurrentConfig();
			if (configuration != null && cutScoreBuffer.noteCutInfo.noteData.gameplayType is NoteData.GameplayType.Normal)
			{
				Judge(__instance, cutScoreBuffer);
			}
		}

		private void Judge(FlyingScoreEffect flyingScoreEffect, CutScoreBuffer cutScoreBuffer, int? assumedAfterCutScore = null)
		{
			var before = cutScoreBuffer.beforeCutScore;
			var after = assumedAfterCutScore ?? cutScoreBuffer.afterCutScore;
			var accuracy = cutScoreBuffer.centerDistanceCutScore;
			var total = before + after + accuracy;
			var timeDependence = Mathf.Abs(cutScoreBuffer.noteCutInfo.cutNormal.z);
			_judgmentService.Judge(cutScoreBuffer.noteScoreDefinition, ref flyingScoreEffect._text, ref flyingScoreEffect._color, total, before, after, accuracy, timeDependence);
		}
	}
}
