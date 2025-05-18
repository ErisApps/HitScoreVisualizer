using System;
using System.Diagnostics.CodeAnalysis;

namespace HitScoreVisualizer.Utilities;

internal class ArrayPicker<T> where T : class
{
	private readonly T[] array;

	public ArrayPicker(T[] array)
	{
		this.array = array;
	}

	private int currentItemIndex;

	public bool TryGetNextDisplay([NotNullWhen(true)] out T? item)
	{
		if (array is [])
		{
			item = null;
			return false;
		}

		item = array[currentItemIndex];
		currentItemIndex++;
		currentItemIndex %= array.Length;

		return true;
	}

	public bool TryGetRandomDisplay(Random random, [NotNullWhen(true)] out T? item)
	{
		if (array is [])
		{
			item = null;
			return false;
		}

		item = array[random.Next(0, array.Length)];
		return true;
	}
}