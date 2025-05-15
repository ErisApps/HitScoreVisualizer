using System.Linq;

namespace HitScoreVisualizer.Models.ConfigValidations;

internal class BadCutDisplaysValidation : IConfigValidation
{
	public bool IsValid(HsvConfigModel config)
	{
		var displays = config.BadCutDisplays;
		if (displays is null)
		{
			return true;
		}

		var colors = displays.Select(x => x.Color).ToList();
		if (colors.Any(color => color.Count != 4))
		{
			Plugin.Log.Warn("Bad cut display color is invalid; make sure to include exactly 4 numbers");
			return false;
		}

		if (colors.Any(color => !color.All(x => x >= 0f)))
		{
			Plugin.Log.Warn("Bad cut display color is invalid; all numbers must be positive and preferably less than or equal to 1");
			return false;
		}

		return true;
	}
}