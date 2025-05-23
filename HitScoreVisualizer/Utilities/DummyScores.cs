using UnityEngine;

namespace HitScoreVisualizer.Utilities;

internal static class DummyScores
{
	private static SaberMovementData DummySaberMovementData { get; } = new();

	private static NoteCutInfo? dummyNormalNote;
	private static NoteCutInfo? dummyChainNote;
	private static NoteCutInfo? dummyChainLink;

	private static NoteData DummyNormalNoteData { get; } = NoteData.CreateBasicNoteData(0, 0, 0, 0, 0, 0, 0);
	private static NoteData DummyChainNoteData { get; } = new(0, 0, 0, 0, 0, 0, NoteData.GameplayType.BurstSliderHead, NoteData.ScoringType.ChainHead, 0, 0, 0, 0, 0, 0, 0, 0);
	private static NoteData DummyChainLinkData { get; } = new(0, 0, 0, 0, 0, 0, NoteData.GameplayType.BurstSliderElement, NoteData.ScoringType.ChainLink, 0, 0, 0, 0, 0, 0, 0, 0);

	public static NoteCutInfo Normal => dummyNormalNote ??= CreateInfo(DummyNormalNoteData);
	public static NoteCutInfo ChainHead => dummyChainNote ??= CreateInfo(DummyChainNoteData);
	public static NoteCutInfo ChainLink => dummyChainLink ??= CreateInfo(DummyChainLinkData);

	private static NoteCutInfo CreateInfo(NoteData noteData)
	{
		return new(noteData,
			true, true, true, false, 0, Vector3.zero, 0, 0, 0, Vector3.zero, Vector3.zero, 0, 0, Quaternion.identity, Quaternion.identity, Quaternion.identity, Vector3.zero,
			DummySaberMovementData);
	}
}