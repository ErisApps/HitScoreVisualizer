using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using SiraUtil.Affinity;
using UnityEngine;

namespace HitScoreVisualizer.HarmonyPatches;

internal class FlyingScoreEffectPatch : IAffinity
{
	private readonly HsvConfigModel config;

	private FlyingScoreEffectPatch(HsvConfigModel config)
	{
		this.config = config;
	}

	// When the flying score effect spawns, InitAndPresent is called
	// When the post swing score changes - as the saber moves - HandleCutScoreBufferDidChange is called
	// When the post swing score stops changing - HandleCutScoreBufferDidFinish is called

	[AffinityPrefix]
	[AffinityPriority(1000)]
	[AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.InitAndPresent))]
	internal bool InitAndPresent(ref FlyingScoreEffect __instance, IReadonlyCutScoreBuffer cutScoreBuffer, float duration, Vector3 targetPos)
	{
		var judgmentDetails = new JudgmentDetails(cutScoreBuffer)
		{
			AfterCutScore = config.AssumeMaxPostSwing ? cutScoreBuffer.noteScoreDefinition.maxAfterCutScore : cutScoreBuffer.afterCutScore,
		};

		var (text, color) = config.Judge(in judgmentDetails);
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

		var judgmentDetails = new JudgmentDetails(cutScoreBuffer);
		var (text, color) = config.Judge(in judgmentDetails);
		__instance._text.text = text;
		__instance._color = color;

		return false;
	}

	[AffinityPrefix]
	[AffinityPatch(typeof(FlyingScoreEffect), nameof(FlyingScoreEffect.HandleCutScoreBufferDidFinish))]
	internal void HandleCutScoreBufferDidFinish(FlyingScoreEffect __instance, CutScoreBuffer cutScoreBuffer)
	{
		var judgmentDetails = new JudgmentDetails(cutScoreBuffer);
		var (text, color) = config.Judge(in judgmentDetails);
		__instance._text.text = text;
		__instance._color = color;
	}
}