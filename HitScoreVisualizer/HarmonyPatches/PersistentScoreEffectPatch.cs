using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace HitScoreVisualizer.HarmonyPatches;

[HarmonyPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
internal class PersistentScoreEffectPatch
{
	private static readonly MethodInfo GetZenMode = AccessTools.PropertyGetter(typeof(GameplayModifiers), nameof(GameplayModifiers.zenMode));
	private static readonly MethodInfo GetNoTextsAndHuds = AccessTools.PropertyGetter(typeof(PlayerSpecificSettings), nameof(PlayerSpecificSettings.noTextsAndHuds));

	/*
	 * Changes:
	 *	- if (!playerSpecificSettings.noTextsAndHuds && !gameplayModifiers.zenMode)
	 *	+ if (!gameplayModifiers.zenMode && (!playerSpecificSettings.noTextsAndHuds || overrideNoTextsAndHuds))
	 *	  {
	 * 	  	  Container.Bind<NoteCutScoreSpawner>().FromComponentInNewPrefab(_noteCutScoreSpawnerPrefab).AsSingle().NonLazy();
	 *	  }
	 *
	 * Description: Binds the NoteCutScoreSpawner when "No Texts And Huds" is enabled, and the "Override No Texts And Huds"
	 * setting in HitScoreVisualizer is enabled
	 */

	public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) => new CodeMatcher(instructions)
		.MatchStartForward(
			new CodeMatch(OpCodes.Ldloc_S),
			new CodeMatch(OpCodes.Callvirt, GetNoTextsAndHuds),
			new CodeMatch(),
			new CodeMatch(),
			new CodeMatch(OpCodes.Callvirt, GetZenMode),
			new CodeMatch(OpCodes.Brtrue)
		)
		.ThrowIfInvalid("Couldn't find match for if (!playerSpecificSettings.noTextsAndHuds && !gameplayModifiers.zenMode)")
		// remove everything in the if statement but the last "Brtrue"; we will use that to check our delegate result and end the if statement
		.RemoveInstructions(5)
		.Insert(new List<CodeInstruction>
		{
			new(OpCodes.Ldloc_S, 4), // load PlayerSpecificSettings
			new(OpCodes.Ldloc_S, 5), // load GameplayModifiers
			Transpilers.EmitDelegate<Func<PlayerSpecificSettings, GameplayModifiers, bool>>(
				((playerSpecificSettings, gameplayModifiers) =>
					!gameplayModifiers.zenMode && (!playerSpecificSettings.noTextsAndHuds || Plugin.Config.OverrideNoTextsAndHuds)))
		})
		.MatchForward(useEnd: false,
			new CodeMatch(OpCodes.Brtrue)
		)
		// replace the "Brtrue" with "Brfalse" because we're using a single bool check instead of &&
		.SetOpcodeAndAdvance(OpCodes.Brfalse)
		.InstructionEnumeration();
}