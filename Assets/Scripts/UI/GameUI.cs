using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TCG.Core;
using TCG.Player;

namespace TCG.UI
{
    /// <summary>
    /// Main HUD — health bars, mana display, phase banner, and action buttons.
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("Player HUD")]
        public TextMeshProUGUI player1HealthText;
        public TextMeshProUGUI player2HealthText;
        public Slider player1HealthBar;
        public Slider player2HealthBar;

        [Header("Mana")]
        public TextMeshProUGUI player1ManaText;
        public TextMeshProUGUI player2ManaText;

        [Header("Phase / Turn")]
        public TextMeshProUGUI phaseBannerText;
        public TextMeshProUGUI activePlayerText;

        [Header("Deck / Graveyard Counters")]
        public TextMeshProUGUI player1DeckCountText;
        public TextMeshProUGUI player2DeckCountText;
        public TextMeshProUGUI player1GraveyardText;
        public TextMeshProUGUI player2GraveyardText;

        [Header("Buttons")]
        public Button endTurnButton;
        public Button attackPlayerButton;
        public Button endMainPhaseButton;
        public Button surrenderButton;

        [Header("Overlays")]
        public GameObject gameOverPanel;
        public TextMeshProUGUI gameOverText;

        private PlayerController _localController;

        private void Start()
        {
            _localController = FindObjectOfType<PlayerController>();

            endTurnButton.onClick.AddListener(() => _localController.EndTurn());
            attackPlayerButton.onClick.AddListener(() => _localController.DeclareAttackOnPlayer());
            surrenderButton.onClick.AddListener(() => _localController.Surrender());
            endMainPhaseButton.onClick.AddListener(
                () => GameManager.Instance.Turns.EndMainPhase());

            GameEvents.OnPlayerDamaged += OnPlayerDamaged;
            GameEvents.OnPlayerHealed += OnPlayerHealed;
            GameEvents.OnManaChanged += OnManaChanged;
            GameEvents.OnPhaseChanged += OnPhaseChanged;
            GameEvents.OnTurnStarted += OnTurnStarted;
            GameEvents.OnCardDrawn += OnDeckChanged;
            GameEvents.OnCardPlayed += OnDeckChanged;
            GameEvents.OnCardDestroyed += OnGraveyardChanged;
            GameEvents.OnGameEnded += OnGameEnded;

            gameOverPanel.SetActive(false);
            RefreshAll();
        }

        private void OnDestroy()
        {
            GameEvents.OnPlayerDamaged -= OnPlayerDamaged;
            GameEvents.OnPlayerHealed -= OnPlayerHealed;
            GameEvents.OnManaChanged -= OnManaChanged;
            GameEvents.OnPhaseChanged -= OnPhaseChanged;
            GameEvents.OnTurnStarted -= OnTurnStarted;
            GameEvents.OnCardDrawn -= OnDeckChanged;
            GameEvents.OnCardPlayed -= OnDeckChanged;
            GameEvents.OnCardDestroyed -= OnGraveyardChanged;
            GameEvents.OnGameEnded -= OnGameEnded;
        }

        // ── Event handlers ─────────────────────────────────────────────────

        private void OnPlayerDamaged(PlayerState p, int _) => RefreshHealth();
        private void OnPlayerHealed(PlayerState p, int _) => RefreshHealth();
        private void OnManaChanged(PlayerState p, int _) => RefreshMana();
        private void OnDeckChanged(TCG.Cards.Card _, PlayerState __) => RefreshDeckCounts();
        private void OnGraveyardChanged(TCG.Cards.Card _, PlayerState __) => RefreshGraveyardCounts();

        private void OnPhaseChanged(GamePhase phase)
        {
            phaseBannerText.text = phase.ToString().Replace("Phase", " Phase");

            bool isMainPhase = phase == GamePhase.MainPhase;
            bool isBattlePhase = phase == GamePhase.BattlePhase;

            endMainPhaseButton.gameObject.SetActive(isMainPhase);
            attackPlayerButton.gameObject.SetActive(isBattlePhase);
            endTurnButton.gameObject.SetActive(isBattlePhase);
        }

        private void OnTurnStarted(PlayerState p)
        {
            activePlayerText.text = $"{p.DisplayName}'s Turn";
            bool isLocal = p == _localController?.State;
            endTurnButton.interactable = isLocal;
            attackPlayerButton.interactable = isLocal;
            endMainPhaseButton.interactable = isLocal;
        }

        private void OnGameEnded(GameResult result)
        {
            gameOverPanel.SetActive(true);
            gameOverText.text = result switch
            {
                GameResult.Player1Win => $"{GameManager.Instance.Player1.DisplayName} Wins!",
                GameResult.Player2Win => $"{GameManager.Instance.Player2.DisplayName} Wins!",
                GameResult.Draw => "Draw!",
                _ => ""
            };
        }

        // ── Refresh helpers ────────────────────────────────────────────────

        public void RefreshBoard()
        {
            RefreshAll();
            FindObjectOfType<HandUI>()?.RefreshAll();
            foreach (var f in FindObjectsOfType<FieldUI>()) f.RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshHealth();
            RefreshMana();
            RefreshDeckCounts();
            RefreshGraveyardCounts();
        }

        private void RefreshHealth()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            int maxHp = PlayerState.StartingHealth;
            player1HealthText.text = $"{gm.Player1.Health}/{maxHp}";
            player2HealthText.text = $"{gm.Player2.Health}/{maxHp}";
            player1HealthBar.value = (float)gm.Player1.Health / maxHp;
            player2HealthBar.value = (float)gm.Player2.Health / maxHp;
        }

        private void RefreshMana()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            player1ManaText.text = $"{gm.Player1.CurrentMana}/{gm.Player1.MaxManaThisTurn}";
            player2ManaText.text = $"{gm.Player2.CurrentMana}/{gm.Player2.MaxManaThisTurn}";
        }

        private void RefreshDeckCounts()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            player1DeckCountText.text = $"Deck: {gm.Player1.Deck.Count}";
            player2DeckCountText.text = $"Deck: {gm.Player2.Deck.Count}";
        }

        private void RefreshGraveyardCounts()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            player1GraveyardText.text = $"GY: {gm.Player1.Graveyard.Count}";
            player2GraveyardText.text = $"GY: {gm.Player2.Graveyard.Count}";
        }
    }
}
