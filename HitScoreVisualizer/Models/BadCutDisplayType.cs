namespace HitScoreVisualizer.Models;

public enum BadCutDisplayType
{
	/// <summary>
	/// This display will be used for any kind of bad cut
	/// </summary>
	All,

	/// <summary>
	/// This display will be used when the bad cut is caused by the note being cut in the wrong direction by the correct color saber
	/// </summary>
	WrongDirection,

	/// <summary>
	/// This display will be used when the bad cut is caused by the note being cut by the wrong color saber
	/// </summary>
	WrongColor,

	/// <summary>
	/// This display will be used by bad cuts caused by hitting bombs
	/// </summary>
	Bomb
}