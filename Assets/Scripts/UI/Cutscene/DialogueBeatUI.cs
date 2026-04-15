using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Cutscene;

namespace TCG.UI.Cutscene
{
    /// <summary>
    /// Dialogue box sub-panel for cutscenes.
    ///
    /// Displays a character portrait, name-plate, and body text with a typewriter animation.
    /// A "tap to continue" indicator blinks once the typewriter finishes.
    ///
    /// Inspector setup:
    ///   • <see cref="portrait"/>      — character sprite (hidden when no character assigned)
    ///   • <see cref="portraitFrame"/> — decorative border around the portrait (optional)
    ///   • <see cref="nameText"/>      — character name label
    ///   • <see cref="dialogueText"/>  — body text with typewriter animation
    ///   • <see cref="tapIndicator"/>  — blinking arrow / icon shown when ready to advance
    ///   • <see cref="charDelay"/>     — seconds between each character reveal
    /// </summary>
    public class DialogueBeatUI : MonoBehaviour
    {
        [Header("Portrait")]
        [SerializeField] private Image    portrait;
        [SerializeField] private Image    portraitFrame;

        [Header("Text")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text dialogueText;

        [Header("Tap Indicator")]
        [SerializeField] private GameObject tapIndicator;

        [Header("Typewriter")]
        [SerializeField] [Range(0.01f, 0.15f)]
        private float charDelay = 0.03f;

        // ── State ──────────────────────────────────────────────────────────────────
        private string    _currentText = string.Empty;
        private Coroutine _typewriterCoroutine;

        /// <summary>True while the typewriter animation is still revealing characters.</summary>
        public bool IsTyping { get; private set; }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>Populates the panel from <paramref name="beat"/> and starts the typewriter.</summary>
        public void Show(CutsceneBeat beat)
        {
            SetPortrait(beat.character);

            if (tapIndicator != null) tapIndicator.SetActive(false);

            _currentText = beat.text ?? string.Empty;
            RestartTypewriter(_currentText);
        }

        /// <summary>
        /// Skips the typewriter animation, showing the full text immediately.
        /// The tap-to-continue indicator is shown.
        /// </summary>
        public void CompleteInstantly()
        {
            StopTypewriter();
            IsTyping = false;
            if (dialogueText  != null) dialogueText.text = _currentText;
            if (tapIndicator  != null) tapIndicator.SetActive(true);
        }

        // ── Internal ──────────────────────────────────────────────────────────────

        private void SetPortrait(CharacterProfile character)
        {
            bool hasCharacter = character != null;
            bool hasPortrait  = hasCharacter && character.portrait != null;

            if (portrait != null)
            {
                portrait.gameObject.SetActive(hasPortrait);
                if (hasPortrait) portrait.sprite = character.portrait;
            }

            if (portraitFrame != null)
                portraitFrame.gameObject.SetActive(hasPortrait);

            if (nameText != null)
            {
                nameText.gameObject.SetActive(hasCharacter);
                if (hasCharacter)
                {
                    nameText.text  = character.characterName;
                    nameText.color = character.nameColor;
                }
            }
        }

        private void RestartTypewriter(string text)
        {
            StopTypewriter();
            _typewriterCoroutine = StartCoroutine(TypewriterEffect(text));
        }

        private void StopTypewriter()
        {
            if (_typewriterCoroutine != null)
            {
                StopCoroutine(_typewriterCoroutine);
                _typewriterCoroutine = null;
            }
        }

        private IEnumerator TypewriterEffect(string text)
        {
            IsTyping = true;
            if (dialogueText != null) dialogueText.text = string.Empty;

            for (int i = 0; i < text.Length; i++)
            {
                if (dialogueText != null)
                    dialogueText.text = text.Substring(0, i + 1);

                yield return new WaitForSeconds(charDelay);
            }

            _typewriterCoroutine = null;
            IsTyping = false;
            if (tapIndicator != null) tapIndicator.SetActive(true);
        }
    }
}
