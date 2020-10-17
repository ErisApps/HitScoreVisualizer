using HarmonyLib;
using HitScoreVisualizer.Services;
using IPA.Utilities;
using UnityEngine;

namespace HitScoreVisualizer.Harmony_Patches
{
	[HarmonyPatch(typeof(FlyingScoreEffect))]
	[HarmonyPatch("InitAndPresent", MethodType.Normal)]
	internal class FlyingScoreEffectInitAndPresent
	{
		private static FlyingScoreEffect _currentEffect = null!;

// ReSharper disable InconsistentNaming
		internal static void Prefix(FlyingScoreEffect __instance, ref Vector3 targetPos)
// ReSharper restore InconsistentNaming
		{
			if (ConfigProvider.CurrentConfig?.UseFixedPos ?? false)
			{
				var transform = __instance.transform;

				// Set current and target position to the desired fixed position
				transform.position = ConfigProvider.CurrentConfig.FixedPos;
				targetPos = transform.position;

				// If there's an existing judgment effect, clear that first
				if (_currentEffect != null)
				{
					// Remove it gracefully by setting its duration to 0
					_currentEffect.SetField("_duration", 0f);

					// We don't need to clear currentEffect when it disappears, because we'll be setting it to the new effect anyway
					_currentEffect.didFinishEvent -= HandleEffectDidFinish;
				}

				// Save the existing effect to clear if a new one spawns
				_currentEffect = __instance;

				// In case it despawns before the next note is hit, don't try to clear it
				_currentEffect.didFinishEvent += HandleEffectDidFinish;
			}
		}

// ReSharper disable InconsistentNaming
		internal static void Postfix(FlyingScoreEffect __instance, NoteCutInfo noteCutInfo)
// ReSharper restore InconsistentNaming
		{
			if (ConfigProvider.CurrentConfig == null)
			{
				return;
			}

			void Judge(ISaberSwingRatingCounter counter)
			{
				ScoreModel.RawScoreWithoutMultiplier(noteCutInfo, out var before, out var after, out var accuracy);
				var total = before + after + accuracy;
				var timeDependence = Mathf.Abs(noteCutInfo.cutNormal.z);
				JudgmentService.Judge(__instance, total, before, after, accuracy, timeDependence);

				// If the counter is finished, remove our event from it
				counter.didFinishEvent -= Judge;
			}

			// Apply judgments a total of twice - once when the effect is created, once when it finishes.
			Judge(noteCutInfo.swingRatingCounter);
			noteCutInfo.swingRatingCounter.didFinishEvent += Judge;
		}

		private static void HandleEffectDidFinish(FlyingObjectEffect effect)
		{
			effect.didFinishEvent -= HandleEffectDidFinish;
			if (_currentEffect == effect)
			{
				_currentEffect = null!;
			}
		}
	}
}