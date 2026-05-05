
namespace DTT.WordConnect
{
	/// <summary>
	/// Struct which holds the data after finishing the game.
	/// </summary>
		public struct WordConnectResult
		{
			/// <summary>
			/// The score that was achieved by the end of the game from zero to one.
			/// </summary>
			public readonly float finalScore;

			/// <summary>
			/// The number of stars earned by the player.
			/// </summary>
			public readonly int stars;

			/// <summary>
			/// The score cost of all assists the player used.
			/// </summary>
			public readonly int hintPointsUsed;

			/// <summary>
			/// The sum of all letters in the placed level words.
			/// </summary>
			public readonly int totalLevelLetters;

			/// <summary>
			/// Constructor for quick creation of struct and associated information.
			/// </summary>
			/// <param name="stars">The amount of stars earned by the player.</param>
			/// <param name="hintPointsUsed">The score cost of all assists the player used.</param>
			/// <param name="totalLevelLetters">The sum of all letters in the placed level words.</param>
			public WordConnectResult(int stars, int hintPointsUsed, int totalLevelLetters)
			{
				this.stars = stars;
				this.hintPointsUsed = hintPointsUsed;
				this.totalLevelLetters = totalLevelLetters;
				finalScore = stars / 3f;
			}
		}
	}
