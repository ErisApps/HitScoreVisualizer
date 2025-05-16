# HitScoreVisualizer

A mod for Beat Saber (PC) that allows you to heavily customize the appearance of "flying note cut effects", sometimes called hit scores.

## Manual Installation

> [!IMPORTANT]
> In addition to BSIPA, you must have [SiraUtil](https://github.com/Auros/SiraUtil) and [BeatSaberMarkupLanguage](https://github.com/monkeymanboy/BeatSaberMarkupLanguage) installed for this mod to
> load. Install them using your mod manager i.e. [BSManager](https://bsmg.wiki/pc-modding.html#bsmanager).

Place the contents of the unzipped folder from the latest [release](https://github.com/ErisApps/HitScoreVisualizer/releases/latest) into your Beat Saber installation folder. If you need more
information regarding manual installation of mods [this wiki page](https://bsmg.wiki/pc-modding.html#manual-installation) will help. For further help with installing mods, join the
[Beat Saber Modding Group](https://discord.gg/beatsabermods) discord server.

Older versions of HitScoreVisualizer are not supported. If you find issues using an older version then I won't be able to help.

## Usage

Having it all installed, you might wonder how you actually use this version compared to the original one. The first important thing that you'll need to know, is that it now has support for multiple
configs which means that you won't have to restart your game anymore in order to change a config.

On the first run, it will create a new folder `UserData\HitScoreVisualizer` (and a default config for those who didn't have one yet).
In that folder, you can drop all your HSV config files. It doesn't even matter if you create new folders in that folder, HSV will still be able to find them all.

One remark about this though... while you can have config files with the same name when using folder structures, **I still strongly advise you to use unique config filenames** because, despite HSV
being able to handle them just fine, config files will appear in-game with only their filename, and you might end up with several files in the list that have the same one.

Having dropped your config files in the folder doesn't mean you're ready to go yet as you still need to select it in-game.
It's pretty straight-forward to do so, but follow the steps below if you really want to be sure.

1) You'll need to start up Beat Saber (you don't have to restart it if it was still running though)
2) Select "Hit Score Visualizer" in the mod list on the left side of the main menu
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

If one of your configs needed migration upon selection. The old config will automagically be backed up to the folder `UserData\HitScoreVisualizer\Backups` (will be created if it doesn't exist) and the
migrated config will be saved to the original file instead.

However, all configs placed in that folder will **NOT** be loaded by HSV. This behavior is by-design and is intended as a safety measure, to prevent accidental migration issues with backups.
If you want revert to a backup, move one of the files back to the main folder (or another folder in the main folder if you're using a folder structure.)

## How-To Config? (aka config explained)

When first running the game, it will create a default config which can be found at

- `UserData\HitScoreVisualizer\HitScoreVisualizerConfig-default.json`.

You can use that file as a starting point in case you want to customize it.

For an explanation of what each config property is and an example config click [here](./HitScoreVisualizer/Config-Documentation.md) to go to the config doc.

### Important info

- Any `text` property in the config supports [TextMeshPro formatting](http://digitalnativestudios.com/textmeshpro/docs/rich-text/)
- The order of thresholds must be in descending order
- There must not be any duplicate thresholds

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

| Property name(s) | Explanation / Info                                                                                                                                                                                                                                                                                                                                                             | Example or possible values |
|------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------|
| threshold        | The threshold that defines whether this TimeDependenceJudgmentSegment will be used for a given time dependence. The TimeDependenceJudgmentSegment will be used if it is the one with the highest threshold that's either equal or smaller than the given time dependence. It can also be omitted when it's the TimeDependenceJudgmentSegment for the lowest time dependencies. | floats                     |
| text             | The text to display.<br>Remark: Format tokens can't be used in this text... or better, they can be used, but won't be replaced with the actual value.                                                                                                                                                                                                                          | "+++"                      |


### Format tokens

| Token          | Explanation / Info                                                                                                                                                                                                                                                                                                                                              |
|----------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| %b             | The score contributed by the swing before cutting the block.                                                                                                                                                                                                                                                                                                    |
| %c             | The score contributed by the accuracy of the cut.                                                                                                                                                                                                                                                                                                               |
| %a             | The score contributed by the part of the swing after cutting the block.                                                                                                                                                                                                                                                                                         |
| %t             | The time dependence of the swing. This value indicates how dependent the accuracy part of the score is upon *when* you hit the block, measured from 0 - 1. A value of 0 indicates a completely time independent swing, while a value of 1 indicates that the accuracy part of the score would vary greatly if the block was hit even slightly earlier or later. |
| %B, %C, %A, %T | Uses the Judgment text that matches the threshold as specified in either `beforeCutAngleJudgments`, `accuracyJudgments`, `afterCutAngleJudgments`, or `timeDependencyJudgments` (depending on the used token).                                                                                                                                                  |
| %d             | An arrow that points towards the center line of the note relative to the cut line.                                                                                                                                                                                                                                                                              |
| %s             | The total score of the cut.                                                                                                                                                                                                                                                                                                                                     |
| %p             | A number representing the percentage of the maximum total score.                                                                                                                                                                                                                                                                                                |
| %%             | A literal percent symbol.                                                                                                                                                                                                                                                                                                                                       |
| %n             | A newline.                                                                                                                                                                                                                                                                                                                                                      |

### BadCutDisplay

| Property name(s) | Explanation / Info                                                                                                                | Example or possible values |
|------------------|-----------------------------------------------------------------------------------------------------------------------------------|----------------------------|
| text             | The text to display on a bad cut.                                                                                                 | "Miss but different"       |
| type             | What type of bad cut to show the display for. If left out, this will be shown on all kinds of bad cut.                            | "WrongDirection"           |
| color            | An array that specifies the color. Consists of 4 floating numbers ranging between (inclusive) 0 and 1. Red, Green, Blue and Glow. | [0, 0.5, 1, 0.75]          |

### MissDisplay

| Property name(s) | Explanation / Info                                                                                                                | Example or possible values |
|------------------|-----------------------------------------------------------------------------------------------------------------------------------|----------------------------|
| text             | The text to display on a miss.                                                                                                    | "MOSS"                     |
| color            | An array that specifies the color. Consists of 4 floating numbers ranging between (inclusive) 0 and 1. Red, Green, Blue and Glow. | [0, 0.5, 1, 0.75]          |

## Changelog

Click [here](./changelog.md) to visit the changelog.

## Useful links

[HSV Config Creator by @MoreOwO](https://github.com/MoreOwO/HSV-Config-Creator/releases/latest): A program that helps you create configs for HSV. Warning: this may not always be up-to-date.

## Credits

Credit where credit is due:
 - [@artemiswkearney](https://github.com/artemiswkearney) for writing the original mod
 - @AntRazor for the mod idea/request and default config input in the original mod
 - @wulkanat for the default config input in the original mod
 - Everyone in #pc-mod-dev (and the rest of the server) for the love and support. ❤
