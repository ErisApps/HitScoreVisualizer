using System.Linq;
using Hive.Versioning;

namespace HitScoreVisualizer.Models.ConfigMigrations;

internal class ConfigMigration360 : IHsvConfigMigration
{
	public Version Version { get; } = new(3, 6, 0);

	public void Migrate(HsvConfigModel config)
	{
		config.Judgments = config.Judgments.OrderByDescending(x => x.Threshold).ToList();
		config.ChainHeadJudgments = config.ChainHeadJudgments.OrderByDescending(x => x.Threshold).ToList();
	}
}