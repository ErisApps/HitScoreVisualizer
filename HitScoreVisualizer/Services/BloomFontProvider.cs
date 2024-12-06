using System;
using System.Linq;
using System.Threading;
using HitScoreVisualizer.Utilities;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HitScoreVisualizer.Services
{
	internal class BloomFontProvider : IDisposable
	{
		private readonly Lazy<TMP_FontAsset> cachedTekoFont;
		private readonly Lazy<TMP_FontAsset> bloomTekoFont;

		public BloomFontProvider()
		{
			var tekoFontAsset = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(x => x.name.Contains("Teko-Medium SDF"));

			cachedTekoFont = new(() => CopyFontAsset(tekoFontAsset), LazyThreadSafetyMode.ExecutionAndPublication);
			bloomTekoFont = new(() =>
			{
				var bloomTekoFont = CopyFontAsset(tekoFontAsset, "Teko-Medium SDF (Bloom)");
				bloomTekoFont.material.shader = Resources.FindObjectsOfTypeAll<Shader>().First(x => x.name == "TextMeshPro/Distance Field ZFix");

				return bloomTekoFont;
			}, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public TMP_FontAsset BloomFont => bloomTekoFont.Value;

		public TMP_FontAsset DefaultFont => cachedTekoFont.Value;

		private static TMP_FontAsset CopyFontAsset(TMP_FontAsset original, string newName = "")
		{
			if (string.IsNullOrEmpty(newName))
			{
				newName = original.name;
			}

			var newFontAsset = Object.Instantiate(original);

			var texture = original.atlasTexture;

			var newTexture = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount, true) { name = $"{newName} Atlas" };
			Graphics.CopyTexture(texture, newTexture);

			var material = new Material(original.material) { name = $"{newName} Atlas Material" };
			material.SetTexture(MaterialProperties.MainTex, newTexture);

			newFontAsset.m_AtlasTexture = newTexture;
			newFontAsset.name = newName;
			newFontAsset.atlasTextures = [newTexture];
			newFontAsset.material = material;

			return newFontAsset;
		}

		public void Dispose()
		{
			if (bloomTekoFont.IsValueCreated)
			{
				Object.Destroy(bloomTekoFont.Value);
			}
		}
	}
}