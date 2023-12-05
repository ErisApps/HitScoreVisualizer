using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using SiraUtil.Affinity;

namespace HitScoreVisualizer.HarmonyPatches
{
	public class GameplayCoreInstallerPatch : IAffinity
	{
		[AffinityTranspiler]
		[AffinityPatch(typeof(GameplayCoreInstaller), nameof(GameplayCoreInstaller.InstallBindings))]
		internal IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			var instructionList = new List<CodeInstruction>(instructions);

			for (var i = 0; i < instructionList.Count - 2; i++)
			{
				// Look for the getter of noTextAndHuds
				if (IsGetterForNoTextAndHuds(instructionList[i]) && IsGetterForZenMode(instructionList[i + 3]) && IsBindNoteCutScoreSpawner(instructionList[i + 7]))
				{
					instructionList[i].opcode = OpCodes.Nop;
					instructionList[i + 1].opcode = OpCodes.Nop;
					instructionList[i + 2].opcode = OpCodes.Nop;

					// Skip two additional instructions
					i += 2;
				}
			}

			return instructionList.AsEnumerable();
		}

		// Check if the instruction is the getter for get_noTextsAndHuds
		private static bool IsGetterForNoTextAndHuds(CodeInstruction instruction)
		{
			return instruction.operand is MethodInfo methodInfo &&
				   methodInfo.Name == "get_noTextsAndHuds";
		}

		// Check if the instruction is the getter for zenMode
		private static bool IsGetterForZenMode(CodeInstruction instruction)
		{
			return instruction.opcode == OpCodes.Callvirt &&
				   instruction.operand is MethodInfo methodInfo &&
				   methodInfo.Name == "get_zenMode";
		}

		// Check if the instruction is the Bind<NoteCutScoreSpawner>() operation
		private static bool IsBindNoteCutScoreSpawner(CodeInstruction instruction)
		{
			return instruction.opcode == OpCodes.Callvirt &&
				   instruction.operand is MethodInfo methodInfo &&
				   methodInfo.DeclaringType == typeof(Zenject.DiContainer) &&
				   methodInfo.Name == "Bind" &&
				   methodInfo.GetGenericArguments().FirstOrDefault() == typeof(NoteCutScoreSpawner);
		}
	}
}
