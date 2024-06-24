## Changelog

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