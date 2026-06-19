using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DTT.MinigameBase.LevelSelect;
using DTT.WordConnect;
using DTT.WordConnect.Demo;
using DTT.WordConnect.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WordGame.Editor
{
    /// <summary>
    /// Imports spreadsheet-friendly CSV level data into Word Connect configuration assets.
    /// </summary>
    public static class WordConnectLevelCsvImporter
    {
        private const string DefaultCsvPath = "Assets/WordGame/LevelImport/levels.csv";
        private const string GeneratedLevelsFolder = "Assets/DTT/Word Connect/Game/Game Data/Generated";
        private const string DefaultLevelDatabasePath = "Assets/DTT/Word Connect/Game/Game Data/Levels.asset";
        private const string DefaultGameScenePath = "Assets/DTT/Word Connect/Game/Scenes/Demo - Portrait.unity";
        private const int GenerationAttemptLimit = 250;

        [MenuItem("WordGame/Levels/Import Default CSV")]
        public static void ImportDefaultCsv()
        {
            ImportCsv(DefaultCsvPath);
        }

        [MenuItem("WordGame/Levels/Import CSV...")]
        private static void ImportCsvWithPicker()
        {
            string selectedPath = EditorUtility.OpenFilePanelWithFilters(
                "Import Word Connect Levels",
                Application.dataPath,
                new[] { "CSV files", "csv", "All files", "*" });

            if (string.IsNullOrEmpty(selectedPath))
                return;

            ImportCsv(selectedPath);
        }

        [MenuItem("WordGame/Levels/Recreate CSV Template")]
        private static void RecreateCsvTemplate()
        {
            EnsureAssetFolder("Assets/WordGame/LevelImport");
            File.WriteAllText(DefaultCsvPath, GetTemplateCsv(), Encoding.UTF8);
            AssetDatabase.ImportAsset(DefaultCsvPath);
            Debug.Log($"Created CSV template at {DefaultCsvPath}");
        }

        private static void ImportCsv(string csvPath)
        {
            ImportReport report = new ImportReport();

            if (!File.Exists(csvPath))
            {
                EditorUtility.DisplayDialog("Level Import Failed", $"Could not find CSV file:\n{csvPath}", "OK");
                return;
            }

            if (!TryReadLevelDefinitions(csvPath, report, out List<LevelDefinition> levels))
            {
                ShowReport("Level Import Failed", report);
                return;
            }

            if (!TryGenerateLevels(levels, report, out List<GeneratedLevel> generatedLevels))
            {
                ShowReport("Level Import Failed", report);
                return;
            }

            EnsureAssetFolder(GeneratedLevelsFolder);

            List<WordConnectConfigurationData> assets = new List<WordConnectConfigurationData>();
            foreach (GeneratedLevel level in generatedLevels)
            {
                WordConnectConfigurationData asset = CreateOrUpdateLevelAsset(level, report);
                assets.Add(asset);
            }

            UpdateLevelDatabase(assets.Count, report);
            UpdateGameSceneReferences(assets, report);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            ShowReport("Level Import Complete", report);
        }

        private static bool TryReadLevelDefinitions(string csvPath, ImportReport report, out List<LevelDefinition> levels)
        {
            levels = new List<LevelDefinition>();
            string csvText = File.ReadAllText(csvPath);
            List<CsvRecord> records;

            try
            {
                records = ParseCsv(csvText);
            }
            catch (FormatException exception)
            {
                report.Errors.Add(exception.Message);
                return false;
            }

            if (records.Count == 0)
            {
                report.Errors.Add("CSV is empty.");
                return false;
            }

            Dictionary<string, int> header = BuildHeaderIndex(records[0]);
            if (!RequireColumn(header, "level", report) ||
                !RequireColumn(header, "word", report) ||
                !RequireColumn(header, "hint", report))
                return false;

            Dictionary<int, LevelDefinition> definitionsByLevel = new Dictionary<int, LevelDefinition>();
            for (int i = 1; i < records.Count; i++)
            {
                CsvRecord record = records[i];
                if (record.Fields.All(string.IsNullOrWhiteSpace))
                    continue;

                string levelText = GetField(record, header["level"]);
                string wordText = GetField(record, header["word"]);
                string hintText = GetField(record, header["hint"]);

                if (!int.TryParse(levelText, out int levelNumber) || levelNumber <= 0)
                {
                    report.Errors.Add($"Line {record.LineNumber}: level must be a positive number.");
                    continue;
                }

                string normalizedWord = KurmanjiTextUtility.NormalizeWord(wordText);
                if (string.IsNullOrEmpty(normalizedWord))
                {
                    report.Errors.Add($"Line {record.LineNumber}: word is empty.");
                    continue;
                }

                if (!definitionsByLevel.TryGetValue(levelNumber, out LevelDefinition definition))
                {
                    definition = new LevelDefinition(levelNumber);
                    definitionsByLevel.Add(levelNumber, definition);
                }

                if (definition.Words.Contains(normalizedWord))
                {
                    report.Warnings.Add($"Line {record.LineNumber}: duplicate word '{normalizedWord}' in level {levelNumber}; keeping the first row.");
                    continue;
                }

                definition.Words.Add(normalizedWord);
                definition.Pairs.Add(new WordHintPair(normalizedWord, hintText));
            }

            if (report.Errors.Count > 0)
                return false;

            levels = definitionsByLevel.Values.OrderBy(level => level.LevelNumber).ToList();
            if (levels.Count == 0)
            {
                report.Errors.Add("CSV does not contain any level rows.");
                return false;
            }

            int expectedLevel = 1;
            foreach (LevelDefinition definition in levels)
            {
                if (definition.LevelNumber != expectedLevel)
                {
                    report.Errors.Add($"Missing level {expectedLevel}. CSV levels must be continuous from 1 to {levels.Last().LevelNumber}.");
                    return false;
                }

                expectedLevel++;
            }

            return true;
        }

        private static bool TryGenerateLevels(List<LevelDefinition> levels, ImportReport report, out List<GeneratedLevel> generatedLevels)
        {
            generatedLevels = new List<GeneratedLevel>();

            foreach (LevelDefinition level in levels)
            {
                if (level.Pairs.Count == 0)
                {
                    report.Errors.Add($"Level {level.LevelNumber}: no valid words.");
                    continue;
                }

                if (!TryGenerateFullLayout(level, out List<WordVector> wordVectors, out char[,] crosswordLetters, out string failureReason))
                {
                    report.Errors.Add($"Level {level.LevelNumber}: {failureReason}");
                    continue;
                }

                generatedLevels.Add(new GeneratedLevel(level, wordVectors, crosswordLetters));
            }

            return report.Errors.Count == 0;
        }

        private static bool TryGenerateFullLayout(LevelDefinition level, out List<WordVector> wordVectors, out char[,] crosswordLetters, out string failureReason)
        {
            wordVectors = null;
            crosswordLetters = null;
            failureReason = string.Empty;

            HashSet<string> requiredWords = new HashSet<string>(level.Words);
            HashSet<string> closestPlacedWords = new HashSet<string>();

            for (int attempt = 0; attempt < GenerationAttemptLimit; attempt++)
            {
                UnityEngine.Random.InitState((level.LevelNumber * 73856093) ^ attempt);
                List<WordVector> candidateVectors = AbstractedCrosswordAlgorithm.GetVectorCrosswordLayout(level.Pairs, false);
                HashSet<string> placedWords = new HashSet<string>(candidateVectors.Select(vector => vector.Word));

                if (placedWords.Count > closestPlacedWords.Count)
                    closestPlacedWords = placedWords;

                if (!requiredWords.SetEquals(placedWords))
                    continue;

                wordVectors = candidateVectors;
                crosswordLetters = BuildCrosswordLetters(candidateVectors);
                return true;
            }

            List<string> missingWords = requiredWords.Except(closestPlacedWords).OrderBy(word => word).ToList();
            failureReason = $"could not place every word after {GenerationAttemptLimit} attempts. Missing: {string.Join(", ", missingWords)}";
            return false;
        }

        private static char[,] BuildCrosswordLetters(List<WordVector> wordVectors)
        {
            List<Vector2Int> occupiedPositions = wordVectors.SelectMany(vector => vector.OccupiedPositions).ToList();
            int xLowerBound = occupiedPositions.Min(position => position.x);
            int xUpperBound = occupiedPositions.Max(position => position.x);
            int yLowerBound = occupiedPositions.Min(position => position.y);
            int yUpperBound = occupiedPositions.Max(position => position.y);

            Vector2Int offset = new Vector2Int(
                xLowerBound < 0 ? -xLowerBound : 0,
                yLowerBound < 0 ? -yLowerBound : 0);

            Vector2Int dimensions = new Vector2Int(
                xUpperBound + Mathf.Abs(xLowerBound) + 1,
                yUpperBound + Mathf.Abs(yLowerBound) + 1);

            char[,] grid = AbstractedCrosswordAlgorithm.GetEmptyCharacterLayout(dimensions);

            foreach (WordVector wordVector in wordVectors)
            {
                for (int i = 0; i < wordVector.OccupiedPositions.Count; i++)
                {
                    Vector2Int position = wordVector.OccupiedPositions[i] + offset;
                    grid[position.x, position.y] = wordVector.Word[i];
                }

                foreach (Vector2Int influencedPosition in wordVector.InfluencedPositions)
                {
                    Vector2Int position = influencedPosition + offset;
                    if (position.x < 0 || position.x >= dimensions.x || position.y < 0 || position.y >= dimensions.y)
                        continue;

                    if (grid[position.x, position.y] == '\0')
                        grid[position.x, position.y] = '$';
                }
            }

            return grid;
        }

        private static WordConnectConfigurationData CreateOrUpdateLevelAsset(GeneratedLevel generatedLevel, ImportReport report)
        {
            string assetPath = $"{GeneratedLevelsFolder}/Level {generatedLevel.Level.LevelNumber:000}.asset";
            WordConnectConfigurationData asset = AssetDatabase.LoadAssetAtPath<WordConnectConfigurationData>(assetPath);

            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<WordConnectConfigurationData>();
                AssetDatabase.CreateAsset(asset, assetPath);
                report.CreatedLevels++;
            }
            else
            {
                report.UpdatedLevels++;
            }

            SerializedObject serializedAsset = new SerializedObject(asset);
            serializedAsset.FindProperty("_gameName").stringValue = $"Level {generatedLevel.Level.LevelNumber}";

            SerializedProperty wordHintPairs = serializedAsset.FindProperty("_wordHintPairs");
            wordHintPairs.arraySize = generatedLevel.Level.Pairs.Count;
            for (int i = 0; i < generatedLevel.Level.Pairs.Count; i++)
            {
                WordHintPair pair = generatedLevel.Level.Pairs[i];
                SerializedProperty element = wordHintPairs.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("word").stringValue = pair.Word;
                element.FindPropertyRelative("hint").stringValue = pair.Hint;
            }

            serializedAsset.ApplyModifiedPropertiesWithoutUndo();

            asset.SetWordVectors(generatedLevel.WordVectors);
            asset.SetCrosswordLetters(generatedLevel.CrosswordLetters);
            EditorUtility.SetDirty(asset);

            return asset;
        }

        private static void UpdateLevelDatabase(int levelCount, ImportReport report)
        {
            LevelDatabase database = AssetDatabase.LoadAssetAtPath<LevelDatabase>(DefaultLevelDatabasePath);
            if (database == null)
            {
                report.Warnings.Add($"Could not find level database at {DefaultLevelDatabasePath}; generated assets were still created.");
                return;
            }

            SerializedObject serializedDatabase = new SerializedObject(database);
            serializedDatabase.FindProperty("_levelCount").intValue = levelCount;

            SerializedProperty data = serializedDatabase.FindProperty("_data");
            int previousSize = data.arraySize;
            data.arraySize = levelCount;

            for (int i = 0; i < levelCount; i++)
            {
                SerializedProperty element = data.GetArrayElementAtIndex(i);
                element.FindPropertyRelative("levelNumber").intValue = i + 1;

                if (i >= previousSize)
                {
                    element.FindPropertyRelative("score").floatValue = 0f;
                    element.FindPropertyRelative("locked").boolValue = i > 0;
                }
            }

            serializedDatabase.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(database);
        }

        private static void UpdateGameSceneReferences(List<WordConnectConfigurationData> assets, ImportReport report)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DefaultGameScenePath) == null)
            {
                report.Warnings.Add($"Could not find game scene at {DefaultGameScenePath}; update the level config array manually.");
                return;
            }

            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                report.Warnings.Add("Skipped scene reference update because current scene changes were not saved.");
                return;
            }

            SceneSetup[] previousSetup = EditorSceneManager.GetSceneManagerSetup();
            Scene scene = EditorSceneManager.OpenScene(DefaultGameScenePath, OpenSceneMode.Single);

            try
            {
                WordConnectLevelSelectHandler levelSelectHandler = FindObjectInScene<WordConnectLevelSelectHandler>(scene);
                if (levelSelectHandler == null)
                {
                    report.Warnings.Add("Could not find WordConnectLevelSelectHandler in the game scene; update the level config array manually.");
                    return;
                }

                SerializedObject serializedHandler = new SerializedObject(levelSelectHandler);
                SerializedProperty configurations = serializedHandler.FindProperty("_wordConnectConfigurations");
                configurations.arraySize = assets.Count;

                for (int i = 0; i < assets.Count; i++)
                    configurations.GetArrayElementAtIndex(i).objectReferenceValue = assets[i];

                serializedHandler.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(levelSelectHandler);

                WordConnectGridManager gridManager = FindObjectInScene<WordConnectGridManager>(scene);
                if (gridManager != null && assets.Count > 0)
                {
                    SerializedObject serializedGridManager = new SerializedObject(gridManager);
                    serializedGridManager.FindProperty("_gameData").objectReferenceValue = assets[0];
                    serializedGridManager.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(gridManager);
                }

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
            finally
            {
                if (!Application.isBatchMode && previousSetup.Length > 0)
                    EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }
        }

        private static T FindObjectInScene<T>(Scene scene) where T : Component
        {
            foreach (GameObject root in scene.GetRootGameObjects())
            {
                T component = root.GetComponentInChildren<T>(true);
                if (component != null)
                    return component;
            }

            return null;
        }

        private static List<CsvRecord> ParseCsv(string text)
        {
            List<CsvRecord> records = new List<CsvRecord>();
            List<string> fields = new List<string>();
            StringBuilder field = new StringBuilder();
            bool inQuotes = false;
            int lineNumber = 1;
            int recordLineNumber = 1;

            for (int i = 0; i < text.Length; i++)
            {
                char current = text[i];

                if (inQuotes)
                {
                    if (current == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(current);
                    }

                    continue;
                }

                if (current == '"')
                {
                    inQuotes = true;
                    continue;
                }

                if (current == ',')
                {
                    fields.Add(field.ToString().Trim());
                    field.Clear();
                    continue;
                }

                if (current == '\r' || current == '\n')
                {
                    fields.Add(field.ToString().Trim());
                    field.Clear();
                    records.Add(new CsvRecord(recordLineNumber, fields));
                    fields = new List<string>();

                    if (current == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                        i++;

                    lineNumber++;
                    recordLineNumber = lineNumber;
                    continue;
                }

                field.Append(current);
            }

            if (inQuotes)
                throw new FormatException("CSV has an unterminated quoted value.");

            if (field.Length > 0 || fields.Count > 0)
            {
                fields.Add(field.ToString().Trim());
                records.Add(new CsvRecord(recordLineNumber, fields));
            }

            return records;
        }

        private static Dictionary<string, int> BuildHeaderIndex(CsvRecord headerRecord)
        {
            Dictionary<string, int> header = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headerRecord.Fields.Count; i++)
            {
                string column = headerRecord.Fields[i].Trim().TrimStart('\uFEFF');
                if (!header.ContainsKey(column))
                    header.Add(column, i);
            }

            return header;
        }

        private static bool RequireColumn(Dictionary<string, int> header, string columnName, ImportReport report)
        {
            if (header.ContainsKey(columnName))
                return true;

            report.Errors.Add($"CSV is missing required column '{columnName}'. Expected header: level,word,hint");
            return false;
        }

        private static string GetField(CsvRecord record, int index)
        {
            return index < record.Fields.Count ? record.Fields[index] : string.Empty;
        }

        private static void EnsureAssetFolder(string folderPath)
        {
            string[] parts = folderPath.Split('/');
            string current = parts[0];

            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);

                current = next;
            }
        }

        private static void ShowReport(string title, ImportReport report)
        {
            StringBuilder message = new StringBuilder();
            if (report.Errors.Count == 0)
            {
                message.AppendLine($"Created levels: {report.CreatedLevels}");
                message.AppendLine($"Updated levels: {report.UpdatedLevels}");
            }

            AppendSection(message, "Errors", report.Errors);
            AppendSection(message, "Warnings", report.Warnings);

            string reportText = message.ToString().Trim();
            Debug.Log(reportText);

            if (!Application.isBatchMode)
                EditorUtility.DisplayDialog(title, reportText, "OK");
        }

        private static void AppendSection(StringBuilder message, string title, List<string> items)
        {
            if (items.Count == 0)
                return;

            if (message.Length > 0)
                message.AppendLine();

            message.AppendLine($"{title}:");
            foreach (string item in items)
                message.AppendLine($"- {item}");
        }

        private static string GetTemplateCsv()
        {
            return "level,word,hint\n" +
                   "1,nava,Hundirê tiştekî.\n" +
                   "1,nav,Peyva ku mirov pê tê nasîn.\n" +
                   "1,nan,Xwarina rojane.\n" +
                   "1,av,Tişta ku em vedixwin.\n";
        }

        private readonly struct CsvRecord
        {
            public readonly int LineNumber;
            public readonly List<string> Fields;

            public CsvRecord(int lineNumber, List<string> fields)
            {
                LineNumber = lineNumber;
                Fields = fields;
            }
        }

        private sealed class LevelDefinition
        {
            public readonly int LevelNumber;
            public readonly List<string> Words = new List<string>();
            public readonly List<WordHintPair> Pairs = new List<WordHintPair>();

            public LevelDefinition(int levelNumber)
            {
                LevelNumber = levelNumber;
            }
        }

        private sealed class GeneratedLevel
        {
            public readonly LevelDefinition Level;
            public readonly List<WordVector> WordVectors;
            public readonly char[,] CrosswordLetters;

            public GeneratedLevel(LevelDefinition level, List<WordVector> wordVectors, char[,] crosswordLetters)
            {
                Level = level;
                WordVectors = wordVectors;
                CrosswordLetters = crosswordLetters;
            }
        }

        private sealed class ImportReport
        {
            public int CreatedLevels;
            public int UpdatedLevels;
            public readonly List<string> Errors = new List<string>();
            public readonly List<string> Warnings = new List<string>();
        }
    }
}
