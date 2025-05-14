using Hive.Versioning;

namespace HitScoreVisualizer.Models.ConfigMigrations;

internal class ConfigMigration223 : IHsvConfigMigration
{
	public Version Version { get; } = new(2, 2, 3);
	public void Migrate(HsvConfigModel config)
	{
		config.DoIntermediateUpdates = true;
	}
}