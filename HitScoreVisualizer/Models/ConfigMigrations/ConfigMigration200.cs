using HitScoreVisualizer.Models;
using Hive.Versioning;

namespace HitScoreVisualizer.Utilities.Services;

internal class ConfigMigration200 : IHsvConfigMigration
{
	public Version Version { get; } = new(2, 0, 0);
	public void Migrate(HsvConfigModel config)
	{
		config.BeforeCutAngleJudgments = [JudgmentSegment.Default];
		config.AccuracyJudgments = [JudgmentSegment.Default];
		config.AfterCutAngleJudgments = [JudgmentSegment.Default];
	}
}