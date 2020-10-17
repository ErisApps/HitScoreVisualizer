using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using UnityEngine;
using Version = SemVer.Version;

namespace HitScoreVisualizer.Settings
{
	internal class Configuration
	{
		[JsonIgnore]
		internal static Configuration Default { get; } = new Configuration
		{
			Version = Plugin.Version,
			IsDefaultConfig = true,
			DisplayMode = "format",
			UseFixedPos = false,
			DoIntermediateUpdates = true,
			Judgments = new List<Judgment>
			{
				new Judgment {Threshold = 115, Text = "%BFantastic%A%n%s", Color = new List<float> {1.0f, 1.0f, 1.0f, 1.0f}},
				new Judgment {Threshold = 101, Text = "<size = 80%>%BExcellent%A</size>%n%s", Color = new List<float> {0.0f, 1.0f, 0.0f, 1.0f}},
				new Judgment {Threshold = 90, Text = "<size = 80%>%BGreat%A</size>%n%s", Color = new List<float> {1.0f, 0.980392158f, 0.0f, 1.0f}},
				new Judgment {Threshold = 80, Text = "<size = 80%>%BGood%A</size>%n%s", Color = new List<float> {1.0f, 0.6f, 0.0f, 1.0f}, Fade = true},
				new Judgment {Threshold = 60, Text = "<size = 80%>%BDecent%A</size>%n%s", Color = new List<float> {1.0f, 0.0f, 0.0f, 1.0f}, Fade = true},
				new Judgment {Text = "<size = 80%>%BWay Off%A</size>%n%s", Color = new List<float> {0.5f, 0.0f, 0.0f, 1.0f}, Fade = true}
			},
			BeforeCutAngleJudgments = new List<JudgmentSegment> {new JudgmentSegment {Threshold = 70, Text = "+"}, new JudgmentSegment {Text = " "}},
			AccuracyJudgments = new List<JudgmentSegment> {new JudgmentSegment {Threshold = 15, Text = " + "}, new JudgmentSegment {Text = " "}},
			AfterCutAngleJudgments = new List<JudgmentSegment> {new JudgmentSegment {Threshold = 30, Text = " + "}, new JudgmentSegment {Text = " "}}
		};

		// If the version number (excluding patch version) of the config is higher than that of the plugin,
		// the config will not be loaded. If the version number of the config is lower than that of the
		// plugin, the file will be automatically converted. Conversion is not guaranteed to occur, or be

		// accurate, across major versions.
		[JsonProperty("majorVersion", DefaultValueHandling = DefaultValueHandling.Include)]
		public int MajorVersion { get; private set; } = Plugin.Version.Major;

		[JsonProperty("minorVersion", DefaultValueHandling = DefaultValueHandling.Include)]
		public int MinorVersion { get; private set; } = Plugin.Version.Minor;

		[JsonProperty("patchVersion", DefaultValueHandling = DefaultValueHandling.Include)]
		public int PatchVersion { get; private set; } = Plugin.Version.Patch;

		[JsonIgnore]
		internal Version Version
		{
			get => new Version(MajorVersion, MinorVersion, PatchVersion);
			set
			{
				MajorVersion = value.Major;
				MinorVersion = value.Minor;
				PatchVersion = value.Patch;
			}
		}

		// If this is true, the config will be overwritten with the plugin' default settings after an
		// update rather than being converted.
		[JsonProperty("isDefaultConfig")]
		public bool IsDefaultConfig { get; set; }

		// If set to "format", displays the judgment text, with the following format specifiers allowed:
		// - %b: The score contributed by the part of the swing before cutting the block.
		// - %c: The score contributed by the accuracy of the cut.
		// - %a: The score contributed by the part of the swing after cutting the block.
		// - %t: The time dependence of the swing
		// - %B, %C, %A, %T: As above, except using the appropriate judgment from that part of the swing (as configured for "beforeCutAngleJudgments", "accuracyJudgments", "afterCutAngleJudgments", or "timeDependencyJudgments").
		// - %s: The total score for the cut.
		// - %p: The percent out of 115 you achieved with your swing's score
		// - %%: A literal percent symbol.
		// - %n: A newline.
		//
		// If set to "numeric", displays only the note score.
		// If set to "textOnly", displays only the judgment text.
		// If set to "scoreOnTop", displays both (numeric score above judgment text).
		// Otherwise, displays both (judgment text above numeric score).
		[JsonProperty("displayMode")]
		[DefaultValue("")]
		public string DisplayMode { get; set; } = string.Empty;

		// If enabled, judgments will appear and stay at (fixedPosX, fixedPosY, fixedPosZ) rather than moving as normal.
		// Additionally, the previous judgment will disappear when a new one is created (so there won't be overlap).
		[JsonProperty("useFixedPos")]
		[DefaultValue(false)]
		public bool UseFixedPos { get; set; }

		[JsonProperty("fixedPosX")]
		[DefaultValue(0f)]
		public float FixedPosX { get; set; }

		[JsonProperty("fixedPosY")]
		[DefaultValue(0f)]
		public float FixedPosY { get; set; }

		[JsonProperty("fixedPosZ")]
		[DefaultValue(0f)]
		public float FixedPosZ { get; set; }

		// Only call this when this was validated beforehand
		[JsonIgnore]
		internal Vector3 FixedPos => new Vector3(FixedPosX, FixedPosY, FixedPosZ);

		// If enabled, judgments will be updated more frequently. This will make score popups more accurate during a brief period before the note's score is finalized, at some cost of performance.
		[JsonProperty("doIntermediateUpdates")]
		public bool DoIntermediateUpdates { get; set; }

		// Number of decimal places to show time dependence to
		[JsonProperty("timeDependencyDecimalPrecision")]
		[DefaultValue(1)]
		public int timeDependenceDecimalPrecision { get; set; }

		// Which power of 10 to multiply the time dependence by
		[JsonProperty("timeDependencyDecimalOffset")]
		[DefaultValue(2)]
		public int timeDependenceDecimalOffset { get; set; }

		// Order from highest threshold to lowest; the first matching judgment will be applied
		[JsonProperty("judgments")]
		public List<Judgment>? Judgments { get; set; }

		// Judgments for the part of the swing before cutting the block (score is from 0-70).
		// Format specifier: %B
		[JsonProperty("beforeCutAngleJudgments")]
		public List<JudgmentSegment>? BeforeCutAngleJudgments { get; set; }


		// Judgments for the accuracy of the cut (how close to the center of the block the cut was, score is from 0-15).
		// Format specifier: %C
		[JsonProperty("accuracyJudgments")]
		public List<JudgmentSegment>? AccuracyJudgments { get; set; }

		// Judgments for the part of the swing after cutting the block (score is from 0-30).
		// Format specifier: %A
		[JsonProperty("afterCutAngleJudgments")]
		public List<JudgmentSegment>? AfterCutAngleJudgments { get; set; }

		// Judgments for time dependence (score is from 0-1).
		// Format specifier: %T
		[JsonProperty("timeDependencyJudgments")]
		public List<TimeDependenceJudgmentSegment>? TimeDependenceJudgments { get; set; }
	}
}