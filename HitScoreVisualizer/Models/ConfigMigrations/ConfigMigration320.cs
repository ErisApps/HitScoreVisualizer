using Hive.Versioning;
using UnityEngine;

namespace HitScoreVisualizer.Models.ConfigMigrations;

internal class ConfigMigration320 : IHsvConfigMigration
{
	public Version Version { get; } = new(3, 2, 0);
	public void Migrate(HsvConfigModel config)
	{
#pragma warning disable 618
		if (config.UseFixedPos)
		{
			config.FixedPosition = new Vector3(config.FixedPosX, config.FixedPosY, config.FixedPosZ);
		}
#pragma warning restore 618
	}
}