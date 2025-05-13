using System.IO;

namespace HitScoreVisualizer.Utilities;

internal static class FilePathUtils
{
	public static string GetUniqueFilePath(string fullPath)
	{
		var ret = fullPath;
		var name = Path.GetFileName(fullPath);
		var extension = Path.GetExtension(fullPath);
		var count = 2;
		while (File.Exists(ret))
		{
			ret = $"{name} ({count}){extension}";
			count++;
		}
		return ret;
	}
}