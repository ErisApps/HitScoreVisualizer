using System.Collections.Generic;
using UnityEngine;

namespace HitScoreVisualizer.Utilities.Extensions;

public static class ColorExtensions
{
	public static Color ToColor(this List<float> rgba)
	{
		return new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
	}
}