using Hive.Versioning;

namespace HitScoreVisualizer.Models.ConfigMigrations;

internal interface IHsvConfigMigration
{
	public Version Version { get; }
	public void Migrate(HsvConfigModel config);
}