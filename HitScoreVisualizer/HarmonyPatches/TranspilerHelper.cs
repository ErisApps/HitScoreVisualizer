using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;

namespace HitScoreVisualizer.HarmonyPatches;

public static class TranspilerHelper
{
	public static CodeMatcher LogInstructionsFromPosition(this CodeMatcher matcher, int count) =>
		matcher.LogInstructions(matcher.Instructions().Skip(matcher.Pos).Take(count));

	public static CodeMatcher LogInstructions(this CodeMatcher matcher, int count, int offset) =>
		matcher.LogInstructions(matcher.Instructions().Skip(offset).Take(count));

	public static CodeMatcher LogInstructions(this CodeMatcher matcher) =>
		matcher.LogInstructions(matcher.Instructions());

	public static CodeMatcher GetOperand(this CodeMatcher matcher, out object operand)
	{
		operand = matcher.Operand;
		return matcher;
	}

	public static CodeMatcher GetOpcode(this CodeMatcher matcher, out object opcode)
	{
		opcode = matcher.Opcode;
		return matcher;
	}

	private static CodeMatcher LogInstructions(this CodeMatcher matcher, IEnumerable<CodeInstruction> instructions)
	{
		var stringBuilder = new StringBuilder();

		foreach (var instruction in instructions)
		{
			stringBuilder.AppendLine(instruction.ToString());
		}

		Plugin.Log.Notice(stringBuilder.ToString());
		return matcher;
	}
}