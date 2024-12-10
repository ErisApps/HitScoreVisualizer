using TMPro;
using UnityEngine;

namespace HitScoreVisualizer.Utilities.Extensions;

internal static class TextMeshExtensions
{
	public static TMP_FontAsset CopyFontAsset(this TMP_FontAsset original, string newName = "")
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
}