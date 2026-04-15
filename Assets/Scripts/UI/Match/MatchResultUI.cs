using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using TCG.Match;
using TCG.Navigation;

namespace TCG.UI.Match
{
    /// <summary>
    /// Victory / Defeat / Draw end screen.
    /// Receives result data from <see cref="MatchUI"/> and shows gold earned.
    /// "Continue" button loads the MainMenu scene.
    /// </summary>
    public class MatchResultUI : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private TMP_Text   resultText;
        [SerializeField] private TMP_Text   goldEarnedText;
        [SerializeField] private Button     continueButton;

        [SerializeField] private string mainMenuSceneName = "MainMenu";

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (panel != null) panel.SetActive(false);
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinueClicked);
        }

        private void OnDestroy()
        {
            if (continueButton != null)
                continueButton.onClick.RemoveListener(OnContinueClicked);
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        public void Show(MatchResult result, MatchRewards rewards)
        {
            if (panel != null) panel.SetActive(true);

            if (resultText != null)
                resultText.text = result switch
                {
                    MatchResult.Victory => "Victory!",
                    MatchResult.Defeat  => "Defeat",
                    _                   => "Draw"
                };

            if (goldEarnedText != null)
                goldEarnedText.text = $"+{rewards.GoldEarned} Gold";
        }

        // ── Handler ────────────────────────────────────────────────────────────────

        private void OnContinueClicked()
        {
            // Prefer in-scene panel navigation; fall back to scene loading if not available.
            if (PanelNavigator.Instance != null)
            {
                PanelNavigator.Instance.ShowHome();
            }
            else if (!string.IsNullOrEmpty(mainMenuSceneName))
            {
                SceneManager.LoadScene(mainMenuSceneName);
            }
        }
    }
}
