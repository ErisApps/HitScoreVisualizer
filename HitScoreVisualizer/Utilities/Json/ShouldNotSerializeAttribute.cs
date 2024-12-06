using System;

namespace HitScoreVisualizer.Helpers.Json
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	internal sealed class ShouldNotSerializeAttribute : Attribute
	{
	}
}