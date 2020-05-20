using System.IO;
using UnityEditor;
using UnityEngine;

public static class HexSpriteProcessor
{
	[MenuItem("Assets/Hex/Postprocess hex")]
	public static void ProcessHexSprite()
	{
		Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
		
		foreach (Texture2D texture in textures)
			ProcessTexture(texture);
	}

	[MenuItem("Assets/Hex/Postprocess hex", true)]
	public static bool ProcessHexSpriteCheck()
	{
		Texture2D[] textures = Selection.GetFiltered<Texture2D>(SelectionMode.Assets);
		
		return textures != null && textures.Length > 0;
	}

	static void ProcessTexture(Texture2D _Texture)
	{
		string path = AssetDatabase.GetAssetPath(_Texture);
		
		if (string.IsNullOrEmpty(path))
			return;
		
		TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
		
		if (importer == null)
			return;
		
		TextureImporterSettings settings = new TextureImporterSettings();
		importer.ReadTextureSettings(settings);
		
		settings.readable = true;
		importer.SetTextureSettings(settings);
		importer.SaveAndReimport();
		
		int width  = _Texture.width;
		int height = _Texture.height;
		
		for (int x = 0; x < width; x++)
		for (int y = 0; y < height; y++)
		{
			Color color = _Texture.GetPixel(x, y);
			
			if (Mathf.Approximately(color.a, 0))
				continue;
			
			ProcessPixel(_Texture, x, y);
		}
		
		_Texture.Apply();
		
		File.WriteAllBytes(path, _Texture.EncodeToPNG());
		
		settings.readable = false;
		importer.SetTextureSettings(settings);
		importer.SaveAndReimport();
	}

	static void ProcessPixel(Texture2D _Texture, int _X, int _Y)
	{
		int width  = _Texture.width;
		int height = _Texture.height;
		
		if (_X < 0 || _X >= width || _Y < 0 || _Y >= height)
			return;
		
		for (int dx = -1; dx <= 1; dx++)
		for (int dy = -1; dy <= 1; dy++)
		{
			int x = _X + dx;
			int y = _Y + dy;
			
			if (x < 0 || x >= width || y < 0 || y >= height)
				continue;
			
			Color source = _Texture.GetPixel(_X, _Y);
			Color target = _Texture.GetPixel(x, y);
			
			if (target.a > float.Epsilon)
				continue;
			
			target.r = source.r;
			target.g = source.g;
			target.b = source.b;
			
			_Texture.SetPixel(x, y, target);
		}
	}
}
