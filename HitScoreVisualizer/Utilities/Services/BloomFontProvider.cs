using System;
using System.Linq;
using System.Threading;
using HitScoreVisualizer.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace HitScoreVisualizer.Utilities.Services
{
	internal class BloomFontProvider : IDisposable
	{


		private readonly Lazy<TMP_FontAsset> cachedTekoFont;
		private readonly Lazy<TMP_FontAsset> bloomTekoFont;

		public BloomFontProvider()
		{
			var tekoFontAsset = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(x => x.name == "Teko-Medium SDF");
			var bloomFontShader = Resources.FindObjectsOfTypeAll<Shader>().FirstOrDefault(x => x.name == "TextMeshPro/Distance Field");

			if (tekoFontAsset == null)
			{
				throw new("Teko-Medium SDF not found, unable to create HSV fonts. This is likely because of a game update.");
			}
			if (bloomFontShader == null)
			{
				throw new("Bloom font shader not found, unable to create HSV fonts. This is likely because of a game update.");
			}

			cachedTekoFont = new(() => tekoFontAsset.CopyFontAsset(), LazyThreadSafetyMode.ExecutionAndPublication);
			bloomTekoFont = new(() =>
			{
				var bloomTekoFont = tekoFontAsset.CopyFontAsset("Teko-Medium SDF (Bloom)");
				bloomTekoFont.material.shader = bloomFontShader;
				return bloomTekoFont;
			}, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public TMP_FontAsset GetFontForType(HsvFontType hsvFontType)
		{
			return hsvFontType switch
			{
				HsvFontType.Default => cachedTekoFont.Value,
				HsvFontType.Bloom => bloomTekoFont.Value,
				_ => throw new ArgumentOutOfRangeException(nameof(hsvFontType))
			};
		}

		public void Dispose()
		{
			if (cachedTekoFont.IsValueCreated && cachedTekoFont.Value != null)
			{
				Object.Destroy(cachedTekoFont.Value);
			}

			if (bloomTekoFont.IsValueCreated && bloomTekoFont.Value != null)
			{
				Object.Destroy(bloomTekoFont.Value);
			}
		}
	}
}