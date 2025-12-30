## Changelog

### 3.7.2

- Updated for Beat Saber 1.42.0
- Added support for config files ending in '.hsv' and '.hsvconfig'; it's now possible to use files that are compatible with both this mod, and MBF for quest-standalone.

### 3.7.1

- Fixed an issue where configs that didn't specify a version would break the plugin as it would assume the config was up-to-date even when it needed migration
- Added an error message explaining that configs should specify a version

### 3.7.0

- Added a config previewer menu to the right of the config selector menu
- Added two new automatic config migrations:
  - configs that are older than v3.4.0 will automatically have "chainHeadJudgments" added to account for the rule stating configs must have at least one judgment
  - configs that are older than v3.6.0 will automatically have their "judgments", "chainHeadJudgments", and "timeDependenceJudgments" thresholds automatically sorted in descending order to account for the rule stating thresholds must be ordered
- Potentially slightly reduced the performance cost of using HitScoreVisualizer during gameplay

### 3.6.1

- Added a button to open the config folder in the file explorer
- Fixed judgment thresholds being falsely invalidated as having "duplicate thresholds"

### 3.6.0

- Thresholds now must be sorted correctly in descending order in order for configs to be compatible
- Removed `isDefaultConfig` - you don't need to update your configs, it will just be ignored
- Added customization for miss and bad cut effects with custom text

```json
"badCutDisplays": [
	{
		"text": "A Cut Of The Bad Variety",
		"color": [1.0, 0.7, 0.7, 1.0]
	}
],
"missDisplays": [
	{
		"text": "You Should Work On Your Aim",
		"color": [0.6, 1.0, 0.7, 1.0]
	}
]
```

- Miss and bad cut effects will be randomized by default unless you specify `randomizeBadCutDisplays` and `randomizeMissDisplays`

```json
"randomizeBadCutDisplays": false,
"randomizeMissDisplays": false
```

- For bad cuts, you can specify which types of bad cuts the display will show for. The options are `WrongDirection`, `WrongColor`, and `Bomb`. Not specifying a type will make the display show for all
types of bad cuts

```json
"badCutDisplays": [
	{
		"text": "<size=600%>BOOM",
		"type": "Bomb",
		"color": [1.0, 1.0, 1.0, 1.0]
	}
]
```

- HitScoreVisualizer will recreate the default config file each time the game is loaded
- Updated the default config to be more like the default config for the HitScoreVisualizer quest mod

### 3.5.3
- Added spaces to the main menu button

### 3.5.2
- Fixed bloom font causing black screen when Enhancements was not installed

### 3.5.1
- Added a setting to allow hit score text to be shown while "No Texts and Huds" is enabled
- Fixed bloom font showing over notes and environment objects
- Fixed "assumeMaxPostSwing" not adding the full post swing score to the total score, which was causing lower judgment thresholds to be used
- Moved settings to the left screen and tweaked some interface elements' scale

### 3.5.0
- Added a toggle in configs that will decide, when the note is first cut, if the score display will show the max possible after cut score
```json
"assumeMaxPostSwing": true
```
Note: it is recommended to set `doIntermediateUpdates` to `false` in order for this to be noticeable. This setting is generally only useful if you're swinging extremely slow, like on "poodle"
maps, for instance.
- Added the `%d` token which, when used in `format` display mode, will be replaced with an arrow that points in the direction relative to the cut line towards the center line of the note
```json
"judgments": [
    { "threshold": 115, "text": "%d%n%s", "color": [1, 1, 1, 1] },
]
```

### 3.4.0
- Added a button to toggle score effect's italics
- Added judgments for chain head notes. These work the way that normal judgments work but are only applied when a chain is cut. Example below
```json
"chainHeadJudgments":[
  { "threshold": 85, "text": "<size=250%><b>•", "color": [1, 1, 1, 1] },
  { "threshold": 78, "text": "<size=120%>%B%c", "color": [1, 1, 1, 1] },
  { "threshold": 0, "text": "<size=120%><alpha=#88>%B%c", "color": [1, 1, 1, 1] }
]
```
- Added configuration of chain segment (aka. chain links) display. Example below
```json
"chainLinkDisplay": {
  "text": "<alpha=#66>%s",
  "color": [1, 1, 1, 1]
}
```
- The mod will now only attempt to load `.json` files in the configs folder
- Some minor optimizations

### 3.3.8
- Fixed an error causing hitscore bloom breaking the game on level load

### 3.3.7
- Fixed arc notes inconsistently showing hit score