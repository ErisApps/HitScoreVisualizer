# HitScoreVisualizer

A fairly simple mod that allows you to heavily customize the appearance of hit scores.


## Manual Installation

This mod requires BSIPA, BeatSaberMarkupLanguage, and SiraUtil already installed for it to load. You should install them for your game version using your mod installer.

The installation is fairly simple:

1. Grab the latest plugin release from BeatMods/ModAssistant (once it's available) or from the [releases page](https://github.com/ErisApps/HitScoreVisualizer/releases) (once there is actually a
   release)
2. Drop the .dll file in the "Plugins" folder of your Beat Saber installation.
3. Boot it up (or reboot)


## Usage

Having it all installed, you might wonder how you actually use this version compared to the original one. The first important thing that you'll need to know, is that it now has support for multiple
configs which means that you won't have to restart your game anymore in order to change a config.

On the first run, it will create a new folder `UserData\HitScoreVisualizer` (and a default config for those who didn't have one yet).
In that folder, you can drop all your HSV config files. It doesn't even matter if you create new folders in that folder, HSV will still be able to find them all. One remark about this though... while
you can have config files with the same name when using folder structures, **I still strongly advise you to use unique config filenames** because, despite HSV being able to handle them just fine, config
files will appear in-game with only their filename, and you might end up with several files in the list that have the same one.

Having dropped your config files in the folder doesn't mean you're ready to go yet as you still need to select it in-game.
It's pretty straight-forward to do so, but follow the steps below if you really want to be sure.

1) You'll need to start up Beat Saber (you don't have to restart it if it was still running though)
2) Select HitScoreVisualizer in the mod list on the left side.
3) Push the 'SELECT' button
4) Profit I guess?

As you might have noticed, each config has a text below its name that describes it current state. You can find all the possible config states in the table below.

| State (internal) | Text shown in-game                                 | Description                                                                                                                                      |
|------------------|----------------------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------|
| Broken           | Invalid config. Not selectable...                  | Means that the file cannot be loaded, it's either not a valid HSV config file or not a config file at all.                                       |
| Incompatible     | Config is too old. Targets version {Version}       | The config is too old and therefore can't be migrated to the newer format automagically.                                                         |
| ValidationFailed | Validation failed, please check the file again.    | You made an error in the config itself, you can check the logs to see what might be wrong (will be logged as a warning).                         |
| NeedsMigration   | Config made for HSV {Version}. Migration possible. | The config is valid, but still needs a migration to the newer format, this can be done automagically though.                                     |
| Compatible       | OK - {Version}                                     | The config file is just fine, nothing special needs to happen. Shows the version for which it was originally made.                               |
| NewerVersion     | Config is too new. Targets version {Version}       | The config file is made for a newer version of HSV and therefore won't be allowed to be loaded in as it's correct workings cannot be guaranteed. |

### Config migration remark
If one of your configs needed migration upon selection. The old config will automagically be backed up to the folder `UserData\HitScoreVisualizer\Backups` (will be created if it doesn't exist) and the migrated config will be saved to the original file instead.
However, all configs placed in that folder will **NOT** be loaded by HSV. This behavior is by-design and is intended as a safety measure, to prevent accidental migration issues with backups.
If you want revert to a backup, move one of the files back to the main folder (or another folder in the main folder if you're using a folder structure.)


## How-To Config? (aka config explained)

When first running the game, it will create a default config which can be found at `UserData\HitScoreVisualizer\HitScoreVisualizerConfig-default.json`.<br>
You can use that file as a starting point in case you want to customize it. Just make sure to set `isDefaultConfig` to `false`.

| Property name(s)                             | Explanation / Info                                                                                                                                                                                                                                                                                                                                                                                                                                   | Example or possible values                                                |
|----------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------|
| majorVersion<br>minorVersion<br>patchVersion | Indicates the version of HSV that the config is made for specifically.                                                                                                                                                                                                                                                                                                                                                                               | N/A                                                                       |
| isDefaultConfig                              | Indicates whether this file is a default config file. This also means that it will be overwritten by the built-in default config if migration is required.                                                                                                                                                                                                                                                                                           | true or false                                                             |
| displayMode                                  | - "format": Allows you to use format tokens in the Judgment texts. (More info later on)<br>- "numeric": Displays only the note score. (Eg: 115)<br>- "textOnly": Displays only the Judgment text. (Eg: Purrfect)<br>- "scoreOnTop": Displays both (numeric score above Judgment text).<br>- Anything else or empty: Displays both (Judgment text above numeric score).<br>- If set to "directions", displays judgement text and off-direction arrow. | "format"<br>"numeric"<br>"textOnly"<br>"scoreOnTop"<br>"directions"<br>"" |
| fixedPosition                                | The coordinate object that defines the fixed location where the hit scores have to be shown.<br>Either leave this out or set as `null` to fully disable.                                                                                                                                                                                                                                                                                             | {<br>"x": 0.0,<br>"y": 3.0,<br>	"z": -2.0<br>}<br><br>null                |
| targetPositionOffset                         | The coordinate object that indicates how much the hitscore fade animation target position has to be offset.<br>Note: If a fixed position is defined in the config, that one will take priority over this one and this will be fully ignored.<br>Either leave this out or set as `null` to fully disable.                                                                                                                                             | {<br>"x": 0.0,<br>"y": 3.0,<br>	"z": -2.0<br>}<br><br>null                |
| doIntermediateUpdates                        | When enabled, Judgments will be updated multiple times. This will make score popups more accurate during a brief period before the note's score is finalized, at some cost of performance.                                                                                                                                                                                                                                                           | true or false                                                             |
| assumeMaxPostSwing                           | When enabled, the first score number will show the maximum possible post-swing score for that note cut. This can be preferable when doIntermediateUpdates is false, your swings are very slow, and you don't want to automatically use lower score thresholds as the game calculates your swing.                                                                                                                                                     | true or false                                                             |
| timeDependencyDecimalPrecision               | The number of decimal places to show the time dependence to.<br> **Must be between 0 and 99, inclusive**                                                                                                                                                                                                                                                                                                                                             | ints                                                                      |
| timeDependencyDecimalOffset                  | Which power of 10 to multiply the time dependence by (time dependence is from 0 - 1).<br> **Must be between 0 and 38, inclusive**                                                                                                                                                                                                                                                                                                                    | ints                                                                      |
| judgments                                    | The list of Judgments that can be used to customize the Judgments in general.                                                                                                                                                                                                                                                                                                                                                                        | Uses judgment objects.<br>More info below.                                |
| chainHeadJudgments                           | Same as judgments, but for chain notes                                                                                                                                                                                                                                                                                                                                                                                                               | Identical to judgments                                                    |
| chainLinkDisplay                             | Allows you to customize the text which is shown when you hit a segment of a chain. Text works the same as judgments.                                                                                                                                                                                                                                                                                                                                 | { <br> "text": "%s", <br> "color": [0.6, 0.9, 0.6, 1] <br> }              |
| beforeCutAngleJudgments                      | The list that can be used to customize the Judgments for the part of the swing before cutting the block (score is from 0 - 70).<br>Format token: %B<br>* Can only be used when `displayMode` is set to `"format"`                                                                                                                                                                                                                                    | Uses JudgmentSegments objects.<br>More info below.                        |
| accuracyJudgments                            | The list that can be used to customize the Judgments for the accuracy of the cut. How close was the cut to the center of the block? (score is from 0 - 15).<br>Format token: %C<br>* Can only be used when `displayMode` is set to `"format"`                                                                                                                                                                                                        | Uses JudgmentSegments objects.<br>More info below.                        |
| afterCutAngleJudgments                       | The list that can be used to customize the Judgments for the part of the swing after cutting the block (score is from 0 - 30).<br>Format token: %A<br>* Can only be used when `displayMode` is set to `"format"`                                                                                                                                                                                                                                     | Uses JudgmentSegments.<br>More info below.                                |
| timeDependencyJudgments                      | The list that can be used to customize the Judgments for the time dependence (value is from 0 - 1).<br>Format token: %T<br>* Can only be used when `displayMode` is set to `"format"`                                                                                                                                                                                                                                                                | Uses TimeDependenceJudgmentSegments.<br>More info below.                  |


### Important info
- The `text` property of Judgment, JudgmentSegment, and TimeDependenceJudgmentSegment all have support for [TextMeshPro formatting!](http://digitalnativestudios.com/textmeshpro/docs/rich-text/).
- The order of Judgments and JudgmentSegments in the list doesn't matter anymore from version 3.0.0 and onwards. However, it's still advised to keep a descending order if you plan on targetting older versions of HSV as well.
- To prevent unexpected issues down the road, the validation will also check within each list whether there are Judgments or JudgmentSegments that share a threshold. Validation will mark the config is failed if when this is detected.


### Judgments explanation

| Property name(s) | Explanation / Info                                                                                                                                                                                                                                                                                                                                                                                               | Example or possible values |
|------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------|
| threshold        | The threshold that defines whether this Judgment will be used for a given score. The Judgment will be used if it is the one with the highest threshold that's either equal or smaller than the score the given hit score. It can also be omitted when it's the Judgment for the lowest scores.                                                                                                                   | 110                        |
| text             | The text to display (if Judgment text is enabled). This can contain the formatting tokens (explained later on) if `displayMode` is set to `"format"`                                                                                                                                                                                                                                                             | "%BFantastic%A%n%s"        |
| color            | An array that specifies the color. Consists of 4 floating numbers ranging between (inclusive) 0 and 1. Red, Green, Blue and Glow.                                                                                                                                                                                                                                                                                | [0, 0.5, 1, 0.75]          |
| fade             | If true, the text color will be interpolated between this Judgment's color and the Judgment with the next highest threshold based on how close to the next threshold it is.<br>Remark: If your config still targets older, but compatible versions, then don't specify it on the Judgment with the highest threshold or the game and plugin will be burned to a crisp. This is mitigated from 3.0.0 and onwards. | true or false              |


### JudgmentSegments explanation

| Property name(s) | Explanation / Info                                                                                                                                                                                                                                                                                                   | Example or possible values |
|------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------|
| threshold        | The threshold that defines whether this JudgmentSegment will be used for a given score. The JudgmentSegment will be used if it is the one with the highest threshold that's either equal or smaller than the score the given part score. It can also be omitted when it's the JudgmentSegment for the lowest scores. | 30                         |
| text             | The text to display.<br>Remark: Format tokens can't be used in this text... or better, they can be used, but won't be replaced with the actual value.                                                                                                                                                                | "+++"                      |


### TimeDependenceJudgmentSegments explanation

| Property name(s) | Explanation / Info                                                                                                                                                                                                                                                                                                                                                            | Example or possible values |
|------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------|
| threshold        | The threshold that defines whether this TimeDependenceJudgmentSegment will be used for a given time dependence. The TimeDependenceJudgmentSegment will be used if it is the one with the highest threshold that's either equal or smaller than the given time dependence. It can also be omitted when it's the TimeDependenceJudgmentSegment for the lowest time dependences. | floats                     |
| text             | The text to display.<br>Remark: Format tokens can't be used in this text... or better, they can be used, but won't be replaced with the actual value.                                                                                                                                                                                                                         | "+++"                      |


### Format tokens

| Token          | Explanation / Info                                                                                                                                                                                                                                                                                                                                              |
|----------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| %b             | The score contributed by the swing before cutting the block.                                                                                                                                                                                                                                                                                                    |
| %c             | The score contributed by the accuracy of the cut.                                                                                                                                                                                                                                                                                                               |
| %a             | The score contributed by the part of the swing after cutting the block.                                                                                                                                                                                                                                                                                         |
| %t             | The time dependence of the swing. This value indicates how dependent the accuracy part of the score is upon *when* you hit the block, measured from 0 - 1. A value of 0 indicates a completely time independent swing, while a value of 1 indicates that the accuracy part of the score would vary greatly if the block was hit even slightly earlier or later. |
| %B, %C, %A, %T | Uses the Judgment text that matches the threshold as specified in either `beforeCutAngleJudgments`, `accuracyJudgments`, `afterCutAngleJudgments`, or `timeDependencyJudgments` (depending on the used token).                                                                                                                                                  |
| %d             | An arrow that points towards the center line of the note relative to the cut line.                                                                                                                                                                                                                                                                              |
| %s             | The total score of the cut.                                 ~~~~                                                                                                                                                                                                                                                                                                    |
| %p             | A number representing the percentage of the maximum total score.                                                                                                                                                                                                                                                                                                |
| %%             | A literal percent symbol.                                                                                                                                                                                                                                                                                                                                       |
| %n             | A newline.                                                                                                                                                                                                                                                                                                                                                      |


## Developers

To build this project you will need to create a `HitScoreVisualizer/HitScoreVisualizer.csproj.user` file specifying where the game is located.
This can be done using either of the following options:

1) Create the file using the [BSMT Visual Studio extension](https://github.com/Zingabopp/BeatSaberModdingTools).
2) Manually create the file, copy the xml below into the file and change the path to your accomodate for your installation. The file shows up as a missing file in the solution explorer, so it should be easy to spot.

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
	<PropertyGroup>
		<!-- Change this path if necessary. Make sure it ends with a backslash. -->
		<BeatSaberDir>D:\Program Files (x86)\Steam\steamapps\common\Beat Saber\</BeatSaberDir>
	</PropertyGroup>
</Project>
```

## Changelog
x.x.x: Probably created and such, no clue here...<br>
2.0.2: Bug fixes and performance improvements. If you've been noticing lag, try disabling the fade option on all Judgments in the config.<br>
2.1.0: Added display mode "format" (see below).<br>
2.1.2: Actually bump version number this time; minor fixes to default config<br>
2.1.3: Minor bug fix<br>
x.x.x: Probably a few more changes and such, no clue here either...<br>
3.0.0: Big rewrite! Support for changing configs at runtime, better validation, ...

## Useful links

[HSV Config Creator by @MoreOwO](https://github.com/MoreOwO/HSV-Config-Creator/releases/latest): A program that helps you create configs for HSV.

## Credits
Credit where credit is due:
 - [@artemiswkearney](https://github.com/artemiswkearney) for writing the original mod
 - @AntRazor for the mod idea/request and default config input in the original mod
 - @wulkanat for the default config input in the original mod
 - Everyone in #pc-mod-dev (and the rest of the server) for the love and support. ❤
