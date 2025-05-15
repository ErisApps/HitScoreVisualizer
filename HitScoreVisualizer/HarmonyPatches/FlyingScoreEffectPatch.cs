using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Services;
using SiraUtil.Affinity;
using UnityEngine;

namespace HitScoreVisualizer.HarmonyPatches;

internal class FlyingScoreEffectPatch : IAffinity
{
	private readonly JudgmentService judgmentService;
	private readonly HsvConfigModel config;

	private FlyingScoreEffectPatch(JudgmentService judgmentService, HsvConfigModel config)
	{
		this.judgmentService = judgmentService;
		this.config = config;
	}

	// When the flying score effect spawns, InitAndPresent is called
	// When the post swing score changes - as the saber moves - HandleCutScoreBufferDidChange is called
	// When the post swing score stops changing - HandleCutScoreBufferDidFinish is called

	[AffinityPrefix]
	[AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.InitAndPresent))]
	internal bool InitAndPresent(ref FlyingScoreEffect __instance, IReadonlyCutScoreBuffer cutScoreBuffer, float duration, Vector3 targetPos)
	{
		var (text, color) = judgmentService.Judge(cutScoreBuffer, config.AssumeMaxPostSwing);
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

		if (config.FixedPosition != null)
		{
			// Set current and target position to the desired fixed position
			targetPos = config.FixedPosition.Value;
			__instance.transform.position = targetPos;
		}
		else if (config.TargetPositionOffset != null)
		{
			targetPos += config.TargetPositionOffset.Value;
		}

		__instance.InitAndPresent(duration, targetPos, cutScoreBuffer.noteCutInfo.worldRotation, false);

		return false;
	}

	[AffinityPrefix]
	[AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.HandleCutScoreBufferDidChange))]
	internal bool HandleCutScoreBufferDidChange(FlyingScoreEffect __instance, CutScoreBuffer cutScoreBuffer)
	{
		if (!config.DoIntermediateUpdates)
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
		var (text, color) = judgmentService.Judge(cutScoreBuffer, false);
		__instance._text.text = text;
		__instance._color = color;
	}
}