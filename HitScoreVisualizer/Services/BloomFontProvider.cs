using System;
using System.Linq;
using System.Threading;
using HitScoreVisualizer.Settings;
using TMPro;
using UnityEngine;

namespace HitScoreVisualizer.Services
{
	internal class BloomFontProvider : IDisposable
	{
		private readonly Lazy<TMP_FontAsset> cachedTekoFont;
		private readonly Lazy<TMP_FontAsset> bloomTekoFont;

		public BloomFontProvider()
		{
			var tekoFontAsset = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(x => x.name.Contains("Teko-Medium SDF"));

			cachedTekoFont = new Lazy<TMP_FontAsset>(() => CopyFontAsset(tekoFontAsset), LazyThreadSafetyMode.ExecutionAndPublication);
			bloomTekoFont = new Lazy<TMP_FontAsset>(() =>
			{
				var distanceFieldShader = Resources.FindObjectsOfTypeAll<Shader>().First(x => x.name.Contains("TextMeshPro/Distance Field"));
				var bloomTekoFont = CopyFontAsset(tekoFontAsset, "Teko-Medium SDF (Bloom)");
				bloomTekoFont.material.shader = distanceFieldShader;

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

			var newFontAsset = GameObject.Instantiate(original);

			var texture = original.atlasTexture;

			var newTexture = new Texture2D(texture.width, texture.height, texture.format, texture.mipmapCount, true) { name = $"{newName} Atlas" };
			Graphics.CopyTexture(texture, newTexture);

			var material = new Material(original.material) { name = $"{newName} Atlas Material" };
			material.SetTexture("_MainTex", newTexture);

			newFontAsset.m_AtlasTexture = newTexture;
			newFontAsset.name = newName;
			newFontAsset.atlasTextures = new[] { newTexture };
			newFontAsset.material = material;

			return newFontAsset;
		}

		public void Dispose()
		{
			if (bloomTekoFont.IsValueCreated)
			{
				UnityEngine.Object.Destroy(bloomTekoFont.Value);
			}
		}
	}
}