using System;
using System.Reflection;
using HarmonyLib;
using SiraUtil.Logging;
using Zenject;

namespace HitScoreVisualizer.HarmonyPatches;

internal class HarmonyPatchManager : IInitializable, IDisposable
{
	private readonly Harmony harmony = new(Plugin.Metadata.Id);
	private readonly Assembly executingAssembly = Assembly.GetExecutingAssembly();

	public void Initialize()
	{
		try
		{
			harmony.PatchAll(executingAssembly);
		}
		catch (Exception e)
		{
			Plugin.Log.Error(e);
		}
	}

	public void Dispose()
	{
		harmony.UnpatchSelf();
	}
}