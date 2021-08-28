using System;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;

namespace HitScoreVisualizer.Services
{
	internal class BloomFontProvider : IDisposable
	{
		private readonly Lazy<TMP_FontAsset> _cachedTekoFont;
		private readonly Lazy<TMP_FontAsset> _bloomTekoFont;

		public BloomFontProvider()
		{
			_cachedTekoFont = new Lazy<TMP_FontAsset>(() => Resources.FindObjectsOfTypeAll<TMP_FontAsset>().First(x => x.name.Contains("Teko-Medium SDF")),
				LazyThreadSafetyMode.ExecutionAndPublication);

			_bloomTekoFont = new Lazy<TMP_FontAsset>(() =>
			{
				var distanceFieldShader = Resources.FindObjectsOfTypeAll<Shader>().First(x => x.name.Contains("TextMeshPro/Distance Field"));
				var bloomTekoFont = TMP_FontAsset.CreateFontAsset(_cachedTekoFont.Value.sourceFontFile);
				bloomTekoFont.name = "Teko-Medium SDF (Bloom)";
				bloomTekoFont.material.shader = distanceFieldShader;

				return bloomTekoFont;
			}, LazyThreadSafetyMode.ExecutionAndPublication);
		}

		public void ConfigureOriginalFont(ref TextMeshPro text)
		{
			text.font = _cachedTekoFont.Value;
		}

		public void ConfigureBloomFont(ref TextMeshPro text)
		{
			text.font = _bloomTekoFont.Value;
		}

		public void Dispose()
		{
			if (_bloomTekoFont.IsValueCreated)
			{
				UnityEngine.Object.Destroy(_bloomTekoFont.Value);
			}
		}
	}
}