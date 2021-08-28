using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using HitScoreVisualizer.Helpers;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Services;
using IPA.Utilities;
using TMPro;
using UnityEngine;
using Zenject;

namespace HitScoreVisualizer.Harmony_Patches
{
	// Roughly based on SiraLocalizer' MissedEffectSpawnerSwapper prefix patch
	// Basically "upgrades" the existing FlyingScoreEffect with a custom one
	// https://github.com/Auros/SiraLocalizer/blob/main/SiraLocalizer/HarmonyPatches/MissedEffectSpawnerSwapper.cs

	// Will most likely be rewritten once SiraUtil 3 becomes available
	[HarmonyPatch(typeof(EffectPoolsManualInstaller), nameof(EffectPoolsManualInstaller.ManualInstallBindings))]
	internal class FlyingScoreEffectPatch
	{
		private static readonly MethodInfo MemoryPoolBinderMethod = typeof(DiContainer).GetMethods()
			.First(x => x.Name == nameof(DiContainer.BindMemoryPool) && x.IsGenericMethod && x.GetGenericArguments().Length == 2)
			.MakeGenericMethod(typeof(FlyingScoreEffect), typeof(FlyingScoreEffect.Pool));

		private static readonly MethodInfo MemoryPoolBinderReplacement = SymbolExtensions.GetMethodInfo(() => MemoryPoolBinderStub(null!));


		private static readonly MethodInfo WithInitialSizeMethod = typeof(MemoryPoolInitialSizeMaxSizeBinder<FlyingScoreEffect>)
			.GetMethod(nameof(MemoryPoolInitialSizeMaxSizeBinder<FlyingScoreEffect>.WithInitialSize), new[]
			{
				typeof(int)
			})!;

		private static readonly MethodInfo WithInitialSizeReplacement = SymbolExtensions.GetMethodInfo(() => WithInitialSizeStub(null!, 0));

		// ReSharper disable once SuggestBaseTypeForParameter
		// ReSharper disable InconsistentNaming
		[HarmonyPrefix]
		internal static void Prefix(FlyingScoreEffect ____flyingScoreEffectPrefab, DiContainer container)
		{
			var gameObject = ____flyingScoreEffectPrefab.gameObject;

			// we can't destroy original FlyingScoreEffect since it kills the reference given through [SerializeField]
			var flyingScoreEffect = gameObject.GetComponent<FlyingScoreEffect>();
			flyingScoreEffect.enabled = false;

			var text = Accessors.TextAccessor(ref ____flyingScoreEffectPrefab);

			var hsvScoreEffect = gameObject.GetComponent<HsvFlyingScoreEffect>();

			if (!hsvScoreEffect)
			{
				var hsvFlyingScoreEffect = gameObject.AddComponent<HsvFlyingScoreEffect>();

				// Serialized fields aren't filled in correctly in our own custom override, so copying over the values using FieldAccessors
				var flyingObjectEffect = (FlyingObjectEffect) flyingScoreEffect;
				FieldAccessor<FlyingObjectEffect, AnimationCurve>.Set(hsvFlyingScoreEffect, "_moveAnimationCurve", Accessors.MoveAnimationCurveAccessor(ref flyingObjectEffect));
				FieldAccessor<FlyingObjectEffect, float>.Set(hsvFlyingScoreEffect, "_shakeFrequency", Accessors.ShakeFrequencyAccessor(ref flyingObjectEffect));
				FieldAccessor<FlyingObjectEffect, float>.Set(hsvFlyingScoreEffect, "_shakeStrength", Accessors.ShakeStrengthAccessor(ref flyingObjectEffect));
				FieldAccessor<FlyingObjectEffect, AnimationCurve>.Set(hsvFlyingScoreEffect, "_shakeStrengthAnimationCurve", Accessors.ShakeStrengthAnimationCurveAccessor(ref flyingObjectEffect));

				FieldAccessor<FlyingScoreEffect, TextMeshPro>.Set(hsvFlyingScoreEffect, "_text", Accessors.TextAccessor(ref flyingScoreEffect));
				FieldAccessor<FlyingScoreEffect, AnimationCurve>.Set(hsvFlyingScoreEffect, "_fadeAnimationCurve", Accessors.FadeAnimationCurveAccessor(ref flyingScoreEffect));
				FieldAccessor<FlyingScoreEffect, SpriteRenderer>.Set(hsvFlyingScoreEffect, "_maxCutDistanceScoreIndicator", Accessors.SpriteRendererAccessor(ref flyingScoreEffect));
			}

			// Once the HSV stuff is done, we reconfigure the HSV prefab font.
			container.Resolve<BloomFontProvider>().ConfigureFont(ref text);
		}

		[HarmonyTranspiler]
		internal static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (var codeInstruction in instructions)
			{
				if (codeInstruction.opcode != OpCodes.Callvirt)
				{
					yield return codeInstruction;
					continue;
				}

				if (codeInstruction.Calls(MemoryPoolBinderMethod))
				{
					yield return new CodeInstruction(OpCodes.Callvirt, MemoryPoolBinderReplacement);
				}
				else if (codeInstruction.Calls(WithInitialSizeMethod))
				{
					yield return new CodeInstruction(OpCodes.Callvirt, WithInitialSizeReplacement);
				}
				else
				{
					yield return codeInstruction;
				}
			}
		}

		// ReSharper disable once UnusedMethodReturnValue.Local
		private static MemoryPoolIdInitialSizeMaxSizeBinder<HsvFlyingScoreEffect> MemoryPoolBinderStub(DiContainer contract)
		{
			return contract.BindMemoryPool<HsvFlyingScoreEffect, FlyingScoreEffect.Pool>();
		}

		// ReSharper disable once SuggestBaseTypeForParameter
		// ReSharper disable once UnusedMethodReturnValue.Local
		private static MemoryPoolMaxSizeBinder<HsvFlyingScoreEffect> WithInitialSizeStub(MemoryPoolIdInitialSizeMaxSizeBinder<HsvFlyingScoreEffect> contract, int size)
		{
			return contract.WithInitialSize(size);
		}
	}
}