using System;
using System.Collections.Generic;
using System.Linq;
using HitScoreVisualizer.Models;
using HitScoreVisualizer.Utilities.Extensions;
using UnityEngine;
using Version = Hive.Versioning.Version;

namespace HitScoreVisualizer.Utilities.Services;

internal class ConfigMigrator
{
	private readonly Dictionary<Version, Func<HsvConfigModel, bool>> migrationActions;
	private readonly Version minimumMigratableVersion;
	private readonly Version maximumMigrationNeededVersion;

	private Version PluginVersion => Plugin.Metadata.HVersion;

	public ConfigMigrator()
	{
		migrationActions = new()
		{
			{ new(2, 0, 0), RunMigration2_0_0 },
			{ new(2, 1, 0), RunMigration2_1_0 },
			{ new(2, 2, 3), RunMigration2_2_3 },
			{ new(3, 2, 0), RunMigration3_2_0 }
		};

		minimumMigratableVersion = migrationActions.Keys.Min();
		maximumMigrationNeededVersion = migrationActions.Keys.Max();
	}

	public ConfigState GetConfigState(HsvConfigModel? configuration, string configName)
	{
		if (configuration is null)
		{
			return ConfigState.Broken;
		}
		var configVersion = configuration.GetVersion();

		return configVersion.NewerThan(PluginVersion) ? ConfigState.NewerVersion
			: configVersion < minimumMigratableVersion ? ConfigState.Incompatible
			: configVersion < maximumMigrationNeededVersion ? ConfigState.NeedsMigration
			: Validate(configuration, configName) ? ConfigState.Compatible : ConfigState.ValidationFailed;
	}

	public void RunMigration(HsvConfigModel userConfig)
	{
		foreach (var requiredMigration in migrationActions.Keys.Where(migrationVersion =>
			         migrationVersion >= userConfig.GetVersion()))
		{
			migrationActions[requiredMigration](userConfig);
		}

		userConfig.SetVersion(PluginVersion);
	}

	private static bool RunMigration2_0_0(HsvConfigModel configuration)
	{
		configuration.BeforeCutAngleJudgments = [JudgmentSegment.Default];
		configuration.AccuracyJudgments = [JudgmentSegment.Default];
		configuration.AfterCutAngleJudgments = [JudgmentSegment.Default];

		return true;
	}

	private static bool RunMigration2_1_0(HsvConfigModel configuration)
	{
		configuration.Judgments = configuration.Judgments
			.Where(j => j.Threshold == 110)
			.Select(j => new NormalJudgment
			{
				Threshold = 115,
				Text = j.Text,
				Color = j.Color,
				Fade = j.Fade
			}).ToList();

		if (configuration.AccuracyJudgments != null)
		{
			configuration.AccuracyJudgments = configuration.AccuracyJudgments
				.Where(aj => aj.Threshold == 10)
				.Select(s => new JudgmentSegment
				{
					Threshold = 15,
					Text = s.Text,
				}).ToList();
		}

		return true;
	}

	private static bool RunMigration2_2_3(HsvConfigModel configuration)
	{
		configuration.DoIntermediateUpdates = true;

		return true;
	}

	private static bool RunMigration3_2_0(HsvConfigModel configuration)
	{
#pragma warning disable 618
		if (configuration.UseFixedPos)
		{
			configuration.FixedPosition = new Vector3(configuration.FixedPosX, configuration.FixedPosY, configuration.FixedPosZ);
		}
#pragma warning restore 618

		return true;
	}

	// Validation

	// ReSharper disable once CognitiveComplexity
	public bool Validate(HsvConfigModel configuration, string configName)
	{
		if (!configuration.Judgments.Any())
		{
			Plugin.Log.Warn($"No judgments found for {configName}");
			return false;
		}

		if (!ValidateJudgments(configuration, configName))
		{
			return false;
		}

		// 99 is the max for NumberFormatInfo.NumberDecimalDigits
		if (configuration.TimeDependenceDecimalPrecision < 0 || configuration.TimeDependenceDecimalPrecision > 99)
		{
			Plugin.Log.Warn($"timeDependencyDecimalPrecision value {configuration.TimeDependenceDecimalPrecision} is outside the range of acceptable values [0, 99]");
			return false;
		}

		if (configuration.TimeDependenceDecimalOffset < 0 || configuration.TimeDependenceDecimalOffset > Math.Log10(float.MaxValue))
		{
			Plugin.Log.Warn($"timeDependencyDecimalOffset value {configuration.TimeDependenceDecimalOffset} is outside the range of acceptable values [0, {(int) Math.Log10(float.MaxValue)}]");
			return false;
		}

		if (configuration.BeforeCutAngleJudgments != null)
		{
			configuration.BeforeCutAngleJudgments = configuration.BeforeCutAngleJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateJudgmentSegment(configuration.BeforeCutAngleJudgments, configName))
			{
				return false;
			}
		}

		if (configuration.AccuracyJudgments != null)
		{
			configuration.AccuracyJudgments = configuration.AccuracyJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateJudgmentSegment(configuration.AccuracyJudgments, configName))
			{
				return false;
			}
		}

		if (configuration.AfterCutAngleJudgments != null)
		{
			configuration.AfterCutAngleJudgments = configuration.AfterCutAngleJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateJudgmentSegment(configuration.AfterCutAngleJudgments, configName))
			{
				return false;
			}
		}

		if (configuration.TimeDependenceJudgments != null)
		{
			configuration.TimeDependenceJudgments = configuration.TimeDependenceJudgments.OrderByDescending(x => x.Threshold).ToList();
			if (!ValidateTimeDependenceJudgmentSegment(configuration.TimeDependenceJudgments, configName))
			{
				return false;
			}
		}

		return true;
	}

	// ReSharper disable once CognitiveComplexity
	private bool ValidateJudgments(HsvConfigModel configuration, string configName)
	{
		configuration.Judgments = configuration.Judgments.OrderByDescending(x => x.Threshold).ToList();
		var prevJudgment = configuration.Judgments[0];
		if (prevJudgment.Fade)
		{
			prevJudgment = new()
			{
				Color = prevJudgment.Color,
				Fade = false,
				Text = prevJudgment.Text,
				Threshold = prevJudgment.Threshold,
			};
		}

		if (!ValidateJudgmentColor(prevJudgment, configName))
		{
			Plugin.Log.Warn($"Judgment entry for threshold {prevJudgment.Threshold} has invalid color in {configName}");
			return false;
		}

		if (configuration.Judgments.Count > 1)
		{
			for (var i = 1; i < configuration.Judgments.Count; i++)
			{
				var currentJudgment = configuration.Judgments[i];
				if (prevJudgment.Threshold != currentJudgment.Threshold)
				{
					if (!ValidateJudgmentColor(currentJudgment, configName))
					{
						Plugin.Log.Warn($"Judgment entry for threshold {currentJudgment.Threshold} has invalid color in {configName}");
						return false;
					}

					prevJudgment = currentJudgment;
					continue;
				}

				Plugin.Log.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold} in {configName}");
				return false;
			}
		}

		return true;
	}

	private bool ValidateJudgmentColor(NormalJudgment judgment, string configName)
	{
		if (judgment.Color.Count != 4)
		{
			Plugin.Log.Warn($"Judgment for threshold {judgment.Threshold} has invalid color in {configName}! Make sure to include exactly 4 numbers for each judgment's color!");
			return false;
		}

		if (judgment.Color.All(x => x >= 0f))
		{
			return true;
		}

		Plugin.Log.Warn($"Judgment for threshold {judgment.Threshold} has invalid color in {configName}! Make sure to include exactly 4 numbers that are greater or equal than 0 (and preferably smaller or equal than 1) for each judgment's color!");
		return false;
	}

	private bool ValidateJudgmentSegment(List<JudgmentSegment> segments, string configName)
	{
		if (segments.Count <= 1)
		{
			return true;
		}

		var prevJudgmentSegment = segments.First();
		for (var i = 1; i < segments.Count; i++)
		{
			var currentJudgment = segments[i];
			if (prevJudgmentSegment.Threshold != currentJudgment.Threshold)
			{
				prevJudgmentSegment = currentJudgment;
				continue;
			}

			Plugin.Log.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold} in {configName}");
			return false;
		}

		return true;
	}

	private bool ValidateTimeDependenceJudgmentSegment(List<TimeDependenceJudgmentSegment> segments, string configName)
	{
		if (segments.Count <= 1)
		{
			return true;
		}

		var prevJudgmentSegment = segments.First();
		for (var i = 1; i < segments.Count; i++)
		{
			var currentJudgment = segments[i];
			if (prevJudgmentSegment.Threshold - currentJudgment.Threshold > double.Epsilon)
			{
				prevJudgmentSegment = currentJudgment;
				continue;
			}

			Plugin.Log.Warn($"Duplicate entry found for threshold {currentJudgment.Threshold} in {configName}");
			return false;
		}

		return true;
	}
}