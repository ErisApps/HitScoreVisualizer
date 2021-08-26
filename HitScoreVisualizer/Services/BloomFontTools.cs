using System.Linq;
using TMPro;
using UnityEngine;

namespace HitScoreVisualizer.Services
{
	public static class BloomFontTools
	{
		public static TMP_FontAsset? cachedTekoFont;
		public static TMP_FontAsset? bloomTekoFont;
		private static Shader? distanceFieldShader;

		public static void Setup()
		{
			cachedTekoFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(x => x.name.Contains("Teko-Medium SDF"));
			distanceFieldShader = Resources.FindObjectsOfTypeAll<Shader>().First(x => x.name.Contains("TextMeshPro/Distance Field"));

			bloomTekoFont = TMP_FontAsset.CreateFontAsset(cachedTekoFont.sourceFontFile);
			bloomTekoFont.name = "Teko-Medium SDF (Bloom)";
			bloomTekoFont.material.shader = distanceFieldShader;
		}

		public static TMP_FontAsset BloomFont()
		{
			Setup();
			return bloomTekoFont!;
		}
	}
}
