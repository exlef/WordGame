using DTT.MinigameBase.LevelSelect;
using UnityEngine;

namespace DTT.WordConnect.Editor
{

    /// <summary>
    /// Level select handler that allows for easy management of word connect level transitioning.
    /// </summary>
    public class WordConnectLevelSelectHandler : LevelSelectHandler<WordConnectConfigurationData, WordConnectResult, WordConnectManager>
    {
        /// <summary>
        /// The WordConnect configurations that are associated to the different levels.
        /// </summary>
        [SerializeField]
        private WordConnectConfigurationData[] _wordConnectConfigurations;

        /// <summary>
        /// Calculates the score of a given Word Connect Result.
        /// </summary>
        /// <param name="result">The result data of the word connect level.</param>
        /// <returns>The calculated score for the level.</returns>
        protected override float CalculateScore(WordConnectResult result) => Mathf.Clamp01(Mathf.Round(result.finalScore * 3) / 3);

        /// <summary>
        /// Gets a configuration file based on a given level number from the level select screen.
        /// </summary>
        /// <param name="levelNumber">The level being fetched.</param>
        /// <returns>The word connect configuration that was fetched.</returns>
        protected override WordConnectConfigurationData GetConfig(int levelNumber)
        {
            WordConnectManager.Instance.SetCurrentLevelNumber(levelNumber);
            return _wordConnectConfigurations[(levelNumber - 1) % _wordConnectConfigurations.Length];
        }
    }
}
