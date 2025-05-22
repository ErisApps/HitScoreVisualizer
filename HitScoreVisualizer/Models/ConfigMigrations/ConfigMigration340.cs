using Hive.Versioning;

namespace HitScoreVisualizer.Models.ConfigMigrations;

internal class ConfigMigration340 : IHsvConfigMigration
{
	public Version Version { get; } = new(3, 4, 0);

	public void Migrate(HsvConfigModel config)
	{
		if (config.ChainHeadJudgments is null or [])
		{
			config.ChainHeadJudgments = [ChainHeadJudgment.Default];
		}
	}
}