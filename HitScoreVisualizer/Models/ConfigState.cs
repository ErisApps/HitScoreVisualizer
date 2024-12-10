namespace HitScoreVisualizer.Models;

internal enum ConfigState
{
	Broken,
	Incompatible,
	ValidationFailed,
	NeedsMigration,
	Compatible,
	NewerVersion
}