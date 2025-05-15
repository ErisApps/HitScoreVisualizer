namespace HitScoreVisualizer.Models.ConfigValidations;

internal interface IConfigValidation
{
	bool IsValid(HsvConfigModel config);
}