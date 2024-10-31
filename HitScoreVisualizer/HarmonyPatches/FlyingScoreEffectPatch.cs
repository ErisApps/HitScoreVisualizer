using HitScoreVisualizer.Services;
using SiraUtil.Affinity;
using UnityEngine;

namespace HitScoreVisualizer.HarmonyPatches
{
	internal class FlyingScoreEffectPatch : IAffinity
	{
		private readonly JudgmentService judgmentService;
		private readonly ConfigProvider configProvider;

		private FlyingScoreEffectPatch(JudgmentService judgmentService, ConfigProvider configProvider)
		{
			this.judgmentService = judgmentService;
			this.configProvider = configProvider;
		}

		// When the flying score effect spawns, InitAndPresent is called
		// When the post swing score changes - as the saber moves - HandleCutScoreBufferDidChange is called
		// When the post swing score stops changing - HandleCutScoreBufferDidFinish is called

		[AffinityPrefix]
		[AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.InitAndPresent))]
		internal bool InitAndPresent(ref FlyingScoreEffect __instance, IReadonlyCutScoreBuffer cutScoreBuffer, float duration, Vector3 targetPos)
		{
			var configuration = configProvider.CurrentConfig;

			if (configuration == null)
			{
				// Run original implementation
				return true;
			}

			var (text, color) = judgmentService.Judge(cutScoreBuffer, configuration.AssumeMaxPostSwing);
			__instance._text.text = text;
			__instance._color = color;
			__instance._cutScoreBuffer = cutScoreBuffer;
			__instance._maxCutDistanceScoreIndicator.enabled = false;
			__instance._colorAMultiplier = 1f;

			if (!cutScoreBuffer.isFinished)
			{
				cutScoreBuffer.RegisterDidChangeReceiver(__instance);
				cutScoreBuffer.RegisterDidFinishReceiver(__instance);
				__instance._registeredToCallbacks = true;
			}

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

			__instance.InitAndPresent(duration, targetPos, cutScoreBuffer.noteCutInfo.worldRotation, false);

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

			if (!configuration.DoIntermediateUpdates)
			{
				return false;
			}

			var (text, color) = judgmentService.Judge(cutScoreBuffer, false);
			__instance._text.text = text;
			__instance._color = color;

			return false;
		}

		[AffinityPrefix]
		[AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.HandleCutScoreBufferDidFinish))]
		internal void HandleCutScoreBufferDidFinish(FlyingScoreEffect __instance, CutScoreBuffer cutScoreBuffer)
		{
			if (configProvider.CurrentConfig == null)
			{
				return;
			}

			var (text, color) = judgmentService.Judge(cutScoreBuffer, false);
			__instance._text.text = text;
			__instance._color = color;
		}
	}
}
