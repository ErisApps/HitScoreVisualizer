using HitScoreVisualizer.Models;
using Hive.Versioning;

namespace HitScoreVisualizer.Utilities.Services;

internal interface IHsvConfigMigration
{
	public Version Version { get; }
	public void Migrate(HsvConfigModel config);
}