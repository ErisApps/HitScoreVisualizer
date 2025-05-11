using System.Linq;
using HitScoreVisualizer.Models;
using Hive.Versioning;

namespace HitScoreVisualizer.Utilities.Services;

internal class ConfigMigration210 : IHsvConfigMigration
{
	public Version Version { get; } = new(2, 1, 0);
	public void Migrate(HsvConfigModel config)
	{
		config.Judgments = config.Judgments
			.Where(j => j.Threshold == 110)
			.Select(j => new NormalJudgment
			{
				Threshold = 115,
				Text = j.Text,
				Color = j.Color,
				Fade = j.Fade
			}).ToList();

		if (config.AccuracyJudgments != null)
		{
			config.AccuracyJudgments = config.AccuracyJudgments
				.Where(aj => aj.Threshold == 10)
				.Select(s => new JudgmentSegment
				{
					Threshold = 15,
					Text = s.Text,
				}).ToList();
		}
	}
}