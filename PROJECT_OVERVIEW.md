# WordGame Project Overview

This file captures the current working map of the Unity project so future changes can start from shared context.

## Project Summary

- Unity project version: `2022.3.72f1`
- Product name: `WordGame`
- Current repository branch at time of review: `main`
- Main package: DTT Word Connect, version `1.0.2`
- Current language target: Kurmanji Kurdish using standard Latin/Hawar orthography
- Default runtime font: `Assets/Fonts/Patrick_Hand/PatrickHand-Regular.ttf`
- Runtime font override asset: `Assets/WordGame/GameConfig.asset`
- Main enabled build scenes:
  - `Assets/DTT/Minigame Base/Assets/Scenes/DTT Level Select.unity`
  - `Assets/DTT/Word Connect/Demo/Scenes/Demo - Portrait.unity`

The project is primarily a Unity Asset Store style package import. Most source code and assets live under `Assets/DTT`, with the Word Connect gameplay package depending on DTT Minigame Base, Runtime Utilities, Editor Utilities, Publishing Tools, Minigame Advertisements, and DTTween.

## Kurmanji Migration Notes

- Player-facing legacy `UnityEngine.UI.Text` references in the Word Connect demo scenes/prefabs and Minigame Base UI scenes/prefabs are pointed at Patrick Hand Regular.
- `GameConfig.asset` can override player-facing legacy `UnityEngine.UI.Text` fonts when play mode starts.
- Kurmanji text comparison is centralized in `Assets/DTT/Word Connect/Runtime/Scripts/KurmanjiTextUtility.cs`.
- Runtime word matching normalizes to Unicode Form C and invariant lowercase before comparison.
- Runtime display uppercasing uses invariant Unicode casing so `ç ê î ş û` display consistently as `Ç Ê Î Ş Û`.
- The five demo `Level X.asset` configs still use the existing ScriptableObject architecture, but their dictionaries, word assets, word vectors, and flattened editor previews now contain Kurmanji starter content.
- Starter words and clues are prototype-quality and should be reviewed by a fluent Kurmanji speaker before release.

## Main Runtime Flow

`WordConnectManager` is the gameplay hub:

- File: `Assets/DTT/Word Connect/Runtime/Scripts/WordConnectManager.cs`
- Owns the active `WordConnectConfigurationData`
- Builds and updates `WordConnectState`
- Computes available input letters
- Handles letter selection and word submission
- Handles letter, word, and descriptive hints
- Tracks timer, score, streak bonus, pause state, and finish state
- Emits lifecycle events such as `Initialized`, `BuildGame`, `Started`, `StateUpdated`, `Paused`, `Continued`, `Finish`, and `Cleanup`

The broad sequence is:

1. A level is selected from Minigame Base level select.
2. `WordConnectLevelSelectHandler` chooses a `WordConnectConfigurationData` asset.
3. `WordConnectManager.StartGame` initializes state, hints, words, available letters, and events.
4. `WordConnectGridManager` receives `BuildGame`, loads the saved layout from the config, and creates grid tiles.
5. UI scripts subscribe to manager events and repaint from `WordConnectState`.
6. Letter input is gathered by `LetterInputUI` and `InputHandler`.
7. On pointer release, `SubmitWord` validates the current sequence.
8. When all words are found or fully hinted, `ForceFinish` emits a `WordConnectResult`.

## Important Runtime Files

- `Assets/DTT/Word Connect/Runtime/Scripts/WordConnectManager.cs`
  - Core gameplay state transitions, scoring, input validation, hints, completion.

- `Assets/DTT/Word Connect/Runtime/Scripts/LetterInput.cs`
  - Small value struct for an input letter and unique id.

- `Assets/DTT/Word Connect/Runtime/Scripts/ColorTheme.cs`
  - ScriptableObject color profile used by the demo UI.

- `Assets/DTT/Word Connect/Runtime/Scripts/KurmanjiTextUtility.cs`
  - Normalizes Kurmanji words and display text for matching, hints, grid letters, and input letters.

- `Assets/WordGame/Scripts/GameConfig.cs`
  - Project-level ScriptableObject for simple game-wide settings. It currently exposes one global font.

- `Assets/WordGame/Scripts/GameConfigFontApplier.cs`
  - One-shot play-mode font applier for legacy `Text` components under a scene/prefab root.

- `Assets/DTT/Word Connect/Runtime/Scripts/Data Scripts/WordConnectState.cs`
  - Mutable per-game state: current input, found words, hint balances, hint options, score, streak.

- `Assets/DTT/Word Connect/Runtime/Scripts/Data Scripts/WordConnectConfigurationData.cs`
  - ScriptableObject level config: game name, color theme, hint counts, dictionary, score settings, word vectors, serialized grid preview.

- `Assets/DTT/Word Connect/Runtime/Scripts/Data Scripts/WordHintDictionary.cs`
  - ScriptableObject collection of `WordHintPair` assets.

- `Assets/DTT/Word Connect/Runtime/Scripts/Data Scripts/WordHintPair.cs`
  - ScriptableObject holding one word and its descriptive hint.

- `Assets/DTT/Word Connect/Runtime/Scripts/Data Scripts/WordVector.cs`
  - Serializable word placement data: origin, orientation, occupied positions, influenced positions.

- `Assets/DTT/Word Connect/Runtime/Scripts/Grid Related/AbstractedCrosswordAlgorithm.cs`
  - Editor/runtime helper that generates valid crossword word vector layouts from a dictionary.

## Demo UI Layer

The demo UI is event-driven and mostly reads from `WordConnectState`.

- `Assets/DTT/Word Connect/Demo/Scripts/WordConnectGridManager.cs`
  - Loads saved layout, generates grid tiles, builds preview grids, saves layouts back into config assets in the editor.

- `Assets/DTT/Word Connect/Demo/Scripts/LetterInputUI.cs`
  - Creates circular letter buttons, handles drag-over selection, draws connecting lines, shuffles letters.

- `Assets/DTT/Word Connect/Demo/Scripts/InputHandler.cs`
  - Wraps legacy and new input systems for pointer state and pointer position.

- `Assets/DTT/Word Connect/Demo/Scripts/WordTile.cs`
  - ScriptableObject wrapper for a word's letter tiles. Reacts to found and hinted states.

- `Assets/DTT/Word Connect/Demo/Scripts/LetterTile.cs`
  - Visual state and animations for one grid tile.

- `Assets/DTT/Word Connect/Demo/Scripts/LetterInputButton.cs`
  - Visual state and animations for one letter input button.

- `Assets/DTT/Word Connect/Demo/Scripts/StateUpdateUI.cs`
  - Base class for UI elements that subscribe to `StateUpdated` and `Started`.

- `Assets/DTT/Word Connect/Demo/Scripts/ScoreUI.cs`
  - Displays current base plus streak score.

- `Assets/DTT/Word Connect/Demo/Scripts/TimeUI.cs`
  - Displays stopwatch time from `WordConnectManager.GameTimer`.

- `Assets/DTT/Word Connect/Demo/Scripts/WordsLeftUI.cs`
  - Displays remaining words count.

- `Assets/DTT/Word Connect/Demo/Scripts/WordHintUI.cs`
  - Updates and enables/disables word hint UI.

- `Assets/DTT/Word Connect/Demo/Scripts/LetterHintUI.cs`
  - Updates and enables/disables letter hint UI.

- `Assets/DTT/Word Connect/Demo/Scripts/DescriptiveHintButtonUI.cs`
  - Updates and enables/disables descriptive hint UI.

- `Assets/DTT/Word Connect/Demo/Scripts/HintDescriptionUI.cs`
  - Displays the active descriptive hint overlay and highlights the hinted word.

- `Assets/DTT/Word Connect/Demo/Scripts/ResultsUI.cs`
  - Displays pause or finish results.

- `Assets/DTT/Word Connect/Demo/Scripts/ColorThemeController.cs`
  - Applies `ColorTheme` values to UI components and prefabs.

## Level Data And Assets

Demo level assets live here:

- `Assets/DTT/Word Connect/Demo/Prefabs/Game Data/Levels.asset`
  - Minigame Base level database.

- `Assets/DTT/Word Connect/Demo/Prefabs/Game Data/Level 1/Level 1.asset`
- `Assets/DTT/Word Connect/Demo/Prefabs/Game Data/Level 2/Level 2.asset`
- `Assets/DTT/Word Connect/Demo/Prefabs/Game Data/Level 3/Level 3.asset`
- `Assets/DTT/Word Connect/Demo/Prefabs/Game Data/Level 4/Level 4.asset`
- `Assets/DTT/Word Connect/Demo/Prefabs/Game Data/Level 5/Level 5.asset`
  - Word Connect config assets.

Each level folder also contains:

- A `Word Dictionary.asset`
- Multiple word assets such as `easy.asset`, `plan.asset`, `west.asset`, etc.
- Sometimes a `Color Theme.asset`

Important demo prefabs:

- `Assets/DTT/Word Connect/Demo/Prefabs/Grid/Letter Tile.prefab`
- `Assets/DTT/Word Connect/Demo/Prefabs/Grid/Blocked Tile.prefab`
- `Assets/DTT/Word Connect/Demo/Prefabs/Input System/Letter Input Button.prefab`
- `Assets/DTT/Word Connect/Demo/Prefabs/Input System/Line.prefab`

## Level Select And Saving

Minigame Base handles level select, score persistence, and unlocks.

- `Assets/DTT/Minigame Base/Runtime/Level Select/LevelSelectHandler.cs`
  - Loads the level select scene additively.
  - Starts the minigame when a level is selected.
  - Saves score and unlocks the next level on finish.

- `Assets/DTT/Minigame Base/Runtime/Level Select/LevelDatabase.cs`
  - ScriptableObject level data plus JSON persistence in `Application.persistentDataPath`.
  - Uses browser local storage for WebGL builds.

- `Assets/DTT/Word Connect/Demo/Scripts/WordConnectLevelSelectHandler.cs`
  - Maps level numbers to `WordConnectConfigurationData` assets.
  - Converts `WordConnectResult.finalScore` into a 0 to 3 star score.

## Editor Tooling

- `Assets/DTT/Word Connect/Editor/Scripts/WordConnectGridManagerEditor.cs`
  - Custom inspector for generating, previewing, saving, and loading crossword layouts.

- `Assets/DTT/Word Connect/Editor/Scripts/WordConnectConfigurationDataEditor.cs`
  - Custom inspector for level configs, score settings, hints, dictionary, and layout preview.

- `Assets/DTT/Word Connect/Editor/Scripts/ConfigurationPropertyCache.cs`
- `Assets/DTT/Word Connect/Editor/Scripts/GridPropertyCache.cs`
  - Serialized property helpers for custom inspectors.

## Modification Guide

Use these files as the likely starting points:

- Change gameplay rules or validation:
  - `Assets/DTT/Word Connect/Runtime/Scripts/WordConnectManager.cs`

- Change scoring or time bonus:
  - `Assets/DTT/Word Connect/Runtime/Scripts/WordConnectManager.cs`
  - `Assets/DTT/Word Connect/Runtime/Scripts/Data Scripts/WordConnectConfigurationData.cs`
  - Individual `Level X.asset` config assets

- Change hint behavior:
  - `Assets/DTT/Word Connect/Runtime/Scripts/WordConnectManager.cs`
  - `Assets/DTT/Word Connect/Runtime/Scripts/Data Scripts/WordConnectState.cs`
  - Hint UI scripts in `Assets/DTT/Word Connect/Demo/Scripts`

- Change letter input behavior:
  - `Assets/DTT/Word Connect/Demo/Scripts/LetterInputUI.cs`
  - `Assets/DTT/Word Connect/Demo/Scripts/InputHandler.cs`
  - `Assets/DTT/Word Connect/Runtime/Scripts/WordConnectManager.cs`

- Change crossword layout generation:
  - `Assets/DTT/Word Connect/Runtime/Scripts/Grid Related/AbstractedCrosswordAlgorithm.cs`
  - `Assets/DTT/Word Connect/Runtime/Scripts/Data Scripts/WordVector.cs`
  - `Assets/DTT/Word Connect/Demo/Scripts/WordConnectGridManager.cs`

- Change visual tile states:
  - `Assets/DTT/Word Connect/Demo/Scripts/LetterTile.cs`
  - `Assets/DTT/Word Connect/Demo/Scripts/WordTile.cs`
  - `Assets/DTT/Word Connect/Demo/Scripts/ColorThemeController.cs`
  - `Color Theme.asset` files

- Change the global test font:
  - Edit `Assets/WordGame/GameConfig.asset`
  - Assign `regularFont` and `boldFont` to Patrick Hand, Baloo2 static fonts, or another imported legacy `Font`

- Change level progression:
  - `Assets/DTT/Minigame Base/Runtime/Level Select`
  - `Assets/DTT/Word Connect/Demo/Scripts/WordConnectLevelSelectHandler.cs`
  - `Assets/DTT/Word Connect/Demo/Prefabs/Game Data/Levels.asset`

- Change result or pause UI:
  - `Assets/DTT/Word Connect/Demo/Scripts/ResultsUI.cs`
  - `Assets/DTT/Minigame Base/Runtime/GameUI.cs`
  - `Assets/DTT/Minigame Base/Runtime/GamePopupUI.cs`

## Known Notes And Risks

These are pre-existing items noticed during review. They are not necessarily blocking, but worth remembering:

- `ColorThemeController` subscribes and unsubscribes using lambda expressions, so the unsubscribe will not remove the original delegate.
- `LevelSelect.OnDisable` uses `+=` instead of `-=` for `_pagination.PageChanged`.
- `MobileInput.GetPointerPosition` returns `Input.mousePosition`, not the current touch position.
- `WordsLeftUI` counts only correctly added words, not completely hinted words.
- There are no automated tests in the repository at the time of review.

## Verification Notes

This overview was first produced by reading the project files directly. After the Kurmanji migration, Unity `2022.3.72f1` was run in batch mode and exited successfully with no C# compiler errors in `/tmp/wordgame-unity.log`.
