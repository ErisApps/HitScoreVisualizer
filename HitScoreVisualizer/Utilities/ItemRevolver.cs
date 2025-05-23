using System.Collections.Generic;
using System.Linq;

namespace HitScoreVisualizer.Utilities;

internal class ItemRevolver<T>(params IEnumerable<T> items)
{
	private readonly T[] arr = items.ToArray();
	private int idx;

	public T? Current => arr is not [] ? arr[idx] : default;

	public T? AdvanceNext()
	{
		if (arr is [])
		{
			return default;
		}
		idx = idx < arr.Length - 1 ? idx + 1 : 0;
		return arr[idx];
	}

	public T? AdvancePrevious()
	{
		if (arr is [])
		{
			return default;
		}
		idx = idx > 0 ? idx - 1 : arr.Length - 1;
		return arr[idx];
	}
}