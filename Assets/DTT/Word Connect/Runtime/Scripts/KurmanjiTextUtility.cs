using System.Globalization;
using System.Text;

namespace DTT.WordConnect
{
    /// <summary>
    /// Centralizes text normalization for Kurmanji word matching and display.
    /// </summary>
    public static class KurmanjiTextUtility
    {
        /// <summary>
        /// Normalizes a word for storage and comparison.
        /// </summary>
        public static string NormalizeWord(string value) => string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().Normalize(NormalizationForm.FormC).ToLowerInvariant();

        /// <summary>
        /// Normalizes text for display without changing casing.
        /// </summary>
        public static string NormalizeDisplayText(string value) => string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Normalize(NormalizationForm.FormC);

        /// <summary>
        /// Uppercases text for UI display.
        /// </summary>
        public static string ToDisplayUpper(string value) => NormalizeDisplayText(value).ToUpperInvariant();

        /// <summary>
        /// Checks if two words are equal after Kurmanji-safe normalization.
        /// </summary>
        public static bool WordsEqual(string first, string second) => string.Compare(
            NormalizeWord(first),
            NormalizeWord(second),
            false,
            CultureInfo.InvariantCulture) == 0;
    }
}
