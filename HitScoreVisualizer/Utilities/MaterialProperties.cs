using UnityEngine;

namespace HitScoreVisualizer.Utilities;

internal class MaterialProperties
{
	public static int MainTex { get; } = Shader.PropertyToID("_MainTex");
}