namespace HitScoreVisualizer.Utilities.Extensions;

internal static class NoteExtensions
{
	public static bool IsBomb(this NoteController noteController)
	{
		return noteController.noteData.colorType == ColorType.None;
	}

	public static bool IsBadCut(this NoteCutInfo noteCutInfo)
	{
		return !noteCutInfo.allIsOK;
	}
}