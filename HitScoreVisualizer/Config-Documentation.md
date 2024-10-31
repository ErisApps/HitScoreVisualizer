## HitScoreVisualizer config documentation

Below is a series of descriptions that relate to each value in a HitScoreVisualizer json config

### "majorVersion" "minorVersion" "patchVersion"
If the version number (excluding patch version) of the config is higher than that of the plugin, the config will not be loaded. If the version number of the config is lower than that of the plugin, the file will be automatically converted. Conversion is not guaranteed to occur, or be accurate, across major versions

### "isDefaultConfig"
If this is true, the config will be overwritten with the plugin' default settings after an update rather than being converted

### "displayMode"
- If not set, by default, it will display judgment text above score.
- If set to "scoreOnTop", it is like default but with score above.
- If set to "format", displays the judgment text, with the following format specifiers allowed:
  - %b: The score contributed by the part of the swing before cutting the block
  - %c: The score contributed by the accuracy of the cut
  - %a: The score contributed by the part of the swing after cutting the block
  - %t: The time dependence of the swing
  - %B, %C, %A, %T: As above, except using the appropriate judgment from that part of the swing (as configured for "beforeCutAngleJudgments", "accuracyJudgments", "afterCutAngleJudgments", or "timeDependencyJudgments")
  - %d: An arrow that points towards the center line of the note relative to the cut line
  - %s: The total score for the cut
  - %p: The percent out of 115 you achieved with your swing's score
  - %%: A literal percent symbol[README.md](../README.md)
  - %n: A newline
- If set to "textOnly", displays only the judgment text.
- If set to "numeric", displays only the note score.
- If set to "directions", displays judgement text and off-direction arrow.

### "fixedPosition"
If not null, judgments will appear and stay at rather than moving as normal, this will take priority over TargetPositionOffset. Additionally, the previous judgment will disappear when a new one is created (so there won't be overlap)

### "targetPositionOffset"
Will offset the target position of the hitscore fade animation. If a fixed position is defined in the config, that one will take priority over this one and this will be fully ignored

### "timeDependencyDecimalPrecision"
Number of decimal places to show time dependence to

### "timeDependencyDecimalOffset"
Which power of 10 to multiply the time dependence by

### "doIntermediateUpdates"
Disabling this only updates the score number once the note is cut and once the post swing score is done calculating; this slightly improves performance

### "assumeMaxPostSwing"
When the note is first cut, should the post swing score show the max rating before it finishes calculating

### "judgments"
Order from highest threshold to lowest; the first matching judgment will be applied

### "chainHeadJudgments"
Same as normal judgments but for burst sliders aka. chain notes

### "chainLinkDisplay"
Text displayed for burst slider segments

### "beforeCutAngleJudgments"
Judgments for the part of the swing before cutting the block (score is from 0-70)

### "accuracyJudgments"
Judgments for the accuracy of the cut (how close to the center of the block the cut was, score is from 0-15)

### "afterCutAngleJudgments"
Judgments for the part of the swing after cutting the block (score is from 0-30)

### "timeDependencyJudgments"
Judgments for time dependence (score is from 0-1)


### Example Config
```json
{
  "majorVersion": 3,
  "minorVersion": 4,
  "patchVersion": 0,
  "isDefaultConfig": false,
  "displayMode": "format",
  "fixedPosition": null,
  "targetPositionOffset": null,
  "timeDependencyDecimalPrecision": 1,
  "timeDependencyDecimalOffset": 2,
  "doIntermediateUpdates": true,
  "assumeMaxPostSwing": false,
  "judgments": [
    { "threshold": 115, "text": "<size=250%><b>•", "color": [1, 1, 1, 1] },
    { "threshold": 108, "text": "<size=120%>%B%c%A", "color": [1, 1, 1, 1] },
    { "threshold": 0, "text": "<size=120%><alpha=#88>%B%c%A", "color": [1, 1, 1, 1] }
  ],
  "chainHeadJudgments":[
    { "threshold": 85, "text": "<size=250%><b>•", "color": [1, 1, 1, 1] },
    { "threshold": 78, "text": "<size=120%>%B%c", "color": [1, 1, 1, 1] },
    { "threshold": 0, "text": "<size=120%><alpha=#88>%B%c", "color": [1, 1, 1, 1] }
  ],
  "chainLinkDisplay": {
    "text": "<alpha=#66>•",
    "color": [1, 1, 1, 1]
  },
  "beforeCutAngleJudgments": [
    { "threshold": 70, "text": " " },
    { "threshold": 63, "text": "-" },
    { "threshold": 56, "text": "<color=#FFCC44>-</color>" },
    { "threshold": 0, "text": "<color=#FF0000>-</color>" }
  ],
  "accuracyJudgments": [],
  "afterCutAngleJudgments": [
    { "threshold": 30, "text": " " },
    { "threshold": 27, "text": "-" },
    { "threshold": 24, "text": "<color=#FFCC44>-</color>" },
    { "threshold": 0, "text": "<color=#FF0000>-</color>" }
  ],
  "timeDependencyJudgments": []
}
```