using Hive.Versioning;

namespace HitScoreVisualizer.Utilities.Extensions;

internal static class HiveVersionExtensions
{
	/// <summary>
	/// Checks if a version is newer than and not equal to another version.
	/// </summary>
	public static bool NewerThan(this Version a, Version b)
	{
		return a > b && !a.ValueEquals(b);
	}

	/// <summary>
	/// Compares two versions by their major, minor, and patch values, ignoring any potential version suffixes.
	/// </summary>
	/// <returns>True if the major, minor, and patch versions are equal</returns>
	public static bool ValueEquals(this Version a, Version b)
	{
		return a.Major == b.Major && a.Minor == b.Minor && a.Patch == b.Patch;
	}
}