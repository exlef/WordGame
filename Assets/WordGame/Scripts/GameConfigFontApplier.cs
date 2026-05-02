using UnityEngine;
using UnityEngine.UI;

namespace WordGame
{
    public class GameConfigFontApplier : MonoBehaviour
    {
        [SerializeField]
        private GameConfig gameConfig;

        private void Awake() => Apply();

        public void Apply()
        {
            if (gameConfig == null)
                return;

            Text[] textComponents = GetComponentsInChildren<Text>(true);
            foreach (Text textComponent in textComponents)
            {
                Font resolvedFont = gameConfig.ResolveFont(textComponent.fontStyle);
                if (resolvedFont == null)
                    continue;

                textComponent.font = resolvedFont;
                textComponent.fontStyle = gameConfig.ResolveFontStyle(textComponent.fontStyle);
            }
        }
    }
}
