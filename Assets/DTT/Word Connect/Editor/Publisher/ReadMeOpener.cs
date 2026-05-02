#if UNITY_EDITOR

using DTT.PublishingTools;
using UnityEditor;

namespace DTT.PackageTemplate.Editor
{
    /// <summary>
    /// Class that handles opening the editor window for the Wordconnect package.
    /// </summary>
    internal static class ReadMeOpener
    {
        /// <summary>
        /// Opens the readme for this package.
        /// </summary>
        [MenuItem("Tools/DTT/Word Connect/ReadMe")]
        private static void OpenReadMe() => DTTEditorConfig.OpenReadMe("dtt.wordconnect");
    }
}
#endif