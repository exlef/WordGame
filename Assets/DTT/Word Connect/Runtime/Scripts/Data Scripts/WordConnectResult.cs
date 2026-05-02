
namespace DTT.WordConnect
{
	/// <summary>
	/// Struct which holds the data after finishing the game.
	/// </summary>
	public struct WordConnectResult
	{
		/// <summary>
		/// The time taken to complete the game in seconds.
		/// </summary>
		public readonly float timeTaken;

		/// <summary>
		/// The score that was achieved by the end of the game from zero to one.
		/// </summary>
		public readonly float finalScore;

		/// <summary>
		/// The score that was displayed to the user at the end of the game.
		/// </summary>
		public readonly float displayScore;

		/// <summary>
		/// Constructor for quick creation of struct and associated information.
		/// </summary>
		/// <param name="timeTaken">The time taken to complete the game in seconds.</param>
		/// <param name="finalScore">The score that was achieved by the end of the game scaled from 0 to 1, 1 being the maximum achievable score.</param>
		/// <param name="displayScore">The score that was achieved by the end of the game without scaling.</param>
		public WordConnectResult(float timeTaken, float finalScore, float displayScore)
		{
			this.timeTaken = timeTaken;
			this.finalScore = finalScore;
			this.displayScore = displayScore;
		}
	}
}
