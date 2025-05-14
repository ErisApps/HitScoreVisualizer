using System;
using System.Collections.Generic;
using HitScoreVisualizer.Models;
using UnityEngine;

namespace HitScoreVisualizer.Utilities.Extensions;

internal static class DirectionConversion
{
	private static readonly float Angle = Mathf.Sqrt(2) / 2;

	private static readonly Dictionary<Direction, Vector3> NormalsMap = new()
	{
		{ Direction.Down, new(0, -1, 0) },
		{ Direction.DownLeft, new(-Angle, -Angle, 0) },
		{ Direction.Left, new(-1, 0, 0) },
		{ Direction.UpLeft, new(-Angle, Angle, 0)}
	};

	public static Direction CalculateOffDirection(this NoteCutInfo noteCutInfo)
	{
		var direction = GetClosestOffDirection(noteCutInfo.cutNormal);
		var directionAsInt = (int)direction;
		return
			direction == Direction.None ? direction
			: Vector3.Dot(NormalsMap[direction], noteCutInfo.notePosition - noteCutInfo.cutPoint) > 0 ? direction
			: directionAsInt < 4 ? (Direction)(directionAsInt + 4)
			: (Direction)(directionAsInt - 4);
	}

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

	private static Direction GetClosestOffDirection(Vector3 cutNormal)
	{
		var closestDot = Mathf.NegativeInfinity;
		var result = Direction.None;

		foreach (var (direction, normal) in NormalsMap)
		{
			var dot = Vector3.Dot(cutNormal, normal);
			var dotValue = Math.Abs(dot);
			if (!(dotValue > closestDot))
			{
				continue;
			}
			closestDot = dot;
			result = direction;
		}

		return result;
	}
}