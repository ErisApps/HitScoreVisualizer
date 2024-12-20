## Changelog

### 3.5.1
- Added a setting to allow hit score text to be shown while "No Texts and Huds" is enabled
- Fixed bloom font showing over notes and environment objects
- Fixed "assumeMaxPostSwing" not adding the full post swing score to the total score, which was causing lower judgment thresholds to be used
- Moved settings to the left screen and tweaked some interface elements' scale

### 3.5.0
- Added a toggle in configs that will decide, when the note is first cut, if the score display will show the max possible after c~~~~ut score
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