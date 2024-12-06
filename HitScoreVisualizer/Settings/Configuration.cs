using System;
using System.Collections.Generic;
using System.ComponentModel;
using HitScoreVisualizer.Utilities.Json;
using Newtonsoft.Json;
using UnityEngine;
using Version = Hive.Versioning.Version;

namespace HitScoreVisualizer.Settings
{
	public class Configuration
	{
		[JsonIgnore]
		internal static Configuration Default { get; } = new Configuration
		{
			Version = Plugin.Metadata.HVersion,
			IsDefaultConfig = true,
			DisplayMode = "format",
			DoIntermediateUpdates = true,
			AssumeMaxPostSwing = false,
			TimeDependenceDecimalPrecision = 1,
			TimeDependenceDecimalOffset = 2,
			NormalJudgments =
			[
				new(115, "%BFantastic%A%n%s", [1.0f, 1.0f, 1.0f, 1.0f]),
				new(101, "<size=80%>%BExcellent%A</size>%n%s", [0.0f, 1.0f, 0.0f, 1.0f]),
				new(90, "<size=80%>%BGreat%A</size>%n%s", [1.0f, 0.980392158f, 0.0f, 1.0f]),
				new(80, "<size=80%>%BGood%A</size>%n%s", [1.0f, 0.6f, 0.0f, 1.0f], true),
				new(60, "<size=80%>%BDecent%A</size>%n%s", [1.0f, 0.0f, 0.0f, 1.0f], true),
				new(0, "<size=80%>%BWay Off%A</size>%n%s", [0.5f, 0.0f, 0.0f, 1.0f], true)
			],
			ChainHeadJudgments =
			[
				new(115, "%BFantastic%n%s", [1.0f, 1.0f, 1.0f, 1.0f]),
				new(101, "<size=80%>%BExcellent</size>%n%s", [0.0f, 1.0f, 0.0f, 1.0f]),
				new(90, "<size=80%>%BGreat</size>%n%s", [1.0f, 0.980392158f, 0.0f, 1.0f]),
				new(80, "<size=80%>%BGood</size>%n%s", [1.0f, 0.6f, 0.0f, 1.0f], true),
				new(60, "<size=80%>%BDecent</size>%n%s", [1.0f, 0.0f, 0.0f, 1.0f], true),
				new(0, "<size=80%>%BWay Off</size>%n%s", [0.5f, 0.0f, 0.0f, 1.0f], true)
			],
			BeforeCutAngleJudgments =
			[
				new(70, " + "),
				new(0, " ")
			],
			AccuracyJudgments =
			[
				new(15, " + "),
				new(0, " ")
			],
			AfterCutAngleJudgments =
			[
				new(30, " + "),
				new(0, " ")
			]
		};

		[JsonProperty("majorVersion", DefaultValueHandling = DefaultValueHandling.Include)]
		public ulong MajorVersion { get; private set; } = Plugin.Metadata.HVersion.Major;

		[JsonProperty("minorVersion", DefaultValueHandling = DefaultValueHandling.Include)]
		public ulong MinorVersion { get; private set; } = Plugin.Metadata.HVersion.Minor;

		[JsonProperty("patchVersion", DefaultValueHandling = DefaultValueHandling.Include)]
		public ulong PatchVersion { get; private set; } = Plugin.Metadata.HVersion.Patch;

		[JsonIgnore]
		internal Version Version
		{
			get => new(MajorVersion, MinorVersion, PatchVersion);
			set
			{
				MajorVersion = value.Major;
				MinorVersion = value.Minor;
				PatchVersion = value.Patch;
			}
		}

		[JsonProperty("isDefaultConfig")]
		public bool IsDefaultConfig { get; internal set; }

		[JsonProperty("displayMode")]
		[DefaultValue("")]
		public string DisplayMode { get; internal set; } = string.Empty;

		[JsonProperty("useFixedPos")]
		[DefaultValue(false)]
		[ShouldNotSerialize]
		[Obsolete("Obsolete since 3.2.0. Either use the FixedPosition property or leave as null instead.")]
		public bool UseFixedPos { get; internal set; }

		[JsonProperty("fixedPosX")]
		[DefaultValue(0f)]
		[ShouldNotSerialize]
		[Obsolete("Obsolete since 3.2.0. Use the FixedPosition property instead.")]
		public float FixedPosX { get; internal set; }

		[JsonProperty("fixedPosY")]
		[DefaultValue(0f)]
		[ShouldNotSerialize]
		[Obsolete("Obsolete since 3.2.0. Use the FixedPosition property instead.")]
		public float FixedPosY { get; internal set; }

		[JsonProperty("fixedPosZ")]
		[DefaultValue(0f)]
		[ShouldNotSerialize]
		[Obsolete("Obsolete since 3.2.0. Use the FixedPosition property instead.")]
		public float FixedPosZ { get; internal set; }

		[JsonProperty("fixedPosition", NullValueHandling = NullValueHandling.Ignore)]
		public Vector3? FixedPosition { get; set; }

		[JsonProperty("targetPositionOffset", NullValueHandling = NullValueHandling.Ignore)]
		public Vector3? TargetPositionOffset { get; set; }

		[JsonProperty("doIntermediateUpdates")]
		public bool DoIntermediateUpdates { get; internal set; }

		[JsonProperty("assumeMaxPostSwing")]
		public bool AssumeMaxPostSwing { get; internal set; }

		[JsonProperty("timeDependencyDecimalPrecision")]
		[DefaultValue(1)]
		public int TimeDependenceDecimalPrecision { get; internal set; }

		[JsonProperty("timeDependencyDecimalOffset")]
		[DefaultValue(2)]
		public int TimeDependenceDecimalOffset { get; internal set; }

		[JsonProperty("judgments")]
		public List<NormalJudgment>? NormalJudgments { get; internal set; }

		[JsonProperty("chainHeadJudgments")]
		public List<ChainHeadJudgment>? ChainHeadJudgments { get; internal set; }

		[JsonProperty("chainLinkDisplay")]
		public ChainLinkDisplay? ChainLinkDisplay { get; internal set; }

		[JsonProperty("beforeCutAngleJudgments")]
		public List<JudgmentSegment>? BeforeCutAngleJudgments { get; internal set; }

		[JsonProperty("accuracyJudgments")]
		public List<JudgmentSegment>? AccuracyJudgments { get; internal set; }

		[JsonProperty("afterCutAngleJudgments")]
		public List<JudgmentSegment>? AfterCutAngleJudgments { get; internal set; }

		[JsonProperty("timeDependencyJudgments")]
		public List<TimeDependenceJudgmentSegment>? TimeDependenceJudgments { get; internal set; }
	}
}