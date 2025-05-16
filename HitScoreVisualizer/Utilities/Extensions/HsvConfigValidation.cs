
using System.Linq;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Models.ConfigValidations;

namespace HitScoreVisualizer.Utilities.Extensions;

internal static class HsvConfigValidation
{
	private static IConfigValidation[] Validations { get; } =
	[
		new JudgmentsValidation(config => config.Judgments),
		new JudgmentsValidation(config => config.ChainHeadJudgments),
		new ChainLinkDisplayValidation(),
		new JudgmentSegmentsValidation(config => config.BeforeCutAngleJudgments),
		new JudgmentSegmentsValidation(config => config.AccuracyJudgments),
		new JudgmentSegmentsValidation(config => config.AfterCutAngleJudgments),
		new TimeDependenceDecimalValidation(),
		new TimeDependenceJudgmentsValidation(),
		new BadCutDisplaysValidation(),
		new MissDisplaysValidation()
	];

	public static bool Validate(this HsvConfigModel configuration)
	{
		return Validations.All(validation => validation.IsValid(configuration));
	}
}