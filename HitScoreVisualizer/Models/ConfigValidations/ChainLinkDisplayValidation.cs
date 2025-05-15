using System.Linq;

namespace HitScoreVisualizer.Models.ConfigValidations;

internal class ChainLinkDisplayValidation : IConfigValidation
{
	public bool IsValid(HsvConfigModel config)
	{
		var display = config.ChainLinkDisplay;
		if (display is null)
		{
			return true;
		}

		var color = display.Color;
		if (color.Count != 4)
		{
			Plugin.Log.Warn("Chain link display color is invalid; make sure to include exactly 4 numbers");
			return false;
		}

		if (!color.All(x => x >= 0f))
		{
			Plugin.Log.Warn("Chain link display color is invalid; all numbers must be positive and preferably less than or equal to 1");
			return false;
		}

		return true;
	}
}