using HitScoreVisualizer.Models;

namespace HitScoreVisualizer.Extensions;

internal static class DirectionConversion
{
	public static string ToFormattedDirection(this Direction direction)
	{
		return direction switch
		{
			Direction.Up => "\u2191",
			Direction.UpRight => "\u2197",
			Direction.Right => "\u2192",
			Direction.DownRight => "\u2198",
			Direction.Down => "\u2193",
			Direction.DownLeft => "\u2199",
			Direction.Left => "\u2190",
			Direction.UpLeft => "\u2196",
			_ => string.Empty
		};
	}
}