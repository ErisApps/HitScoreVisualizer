namespace HitScoreVisualizer.Models;

internal readonly ref struct JudgmentDetails
{
	public int BeforeCutScore { get; init; }
	public int CenterCutScore { get; init; }
	public int AfterCutScore { get; init; }
	public int TotalCutScore { get; init; }
	public int MaxPossibleScore { get; init; }
	public NoteCutInfo CutInfo { get; init; }

	public JudgmentDetails(IReadonlyCutScoreBuffer cutScoreBuffer)
	{
		BeforeCutScore = cutScoreBuffer.beforeCutScore;
		CenterCutScore = cutScoreBuffer.centerDistanceCutScore;
		AfterCutScore = cutScoreBuffer.afterCutScore;
		TotalCutScore = cutScoreBuffer.cutScore;
		MaxPossibleScore = cutScoreBuffer.noteScoreDefinition.maxCutScore;
		CutInfo = cutScoreBuffer.noteCutInfo;
	}
}