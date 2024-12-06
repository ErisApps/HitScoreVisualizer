using System;

namespace HitScoreVisualizer.Utilities.Json
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	internal sealed class ShouldNotSerializeAttribute : Attribute
	{
	}
}