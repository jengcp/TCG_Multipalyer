using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TCG.Characters;
using TCG.Core;
using TCG.Player;

namespace TCG.UI
{
    /// <summary>
    /// HUD panel for one player's character: portrait, HP, energy bar, and ability buttons.
    /// </summary>
    public class CharacterUI : MonoBehaviour
    {
        [Header("Character Panel")]
        public Image portrait;
        public TextMeshProUGUI characterNameText;
        public bool isLocalPlayer;

        [Header("Health")]
        public Slider healthBar;
        public TextMeshProUGUI healthText;

        [Header("Energy")]
        public Slider energyBar;
        public TextMeshProUGUI energyText;

        [Header("Ability Buttons (up to 3)")]
        public List<AbilityButtonUI> abilityButtons = new List<AbilityButtonUI>();

        [Header("Dead Overlay")]
        public GameObject deadOverlay;

        private CharacterState _character;
        private PlayerController _playerController;

        private void Start()
        {
            GameEvents.OnCharacterDamaged += OnCharacterDamaged;
            GameEvents.OnCharacterHealed += OnCharacterHealed;
            GameEvents.OnCharacterDied += OnCharacterDied;
            GameEvents.OnEnergyChanged += OnEnergyChanged;
            GameEvents.OnAbilityUsed += OnAbilityUsed;
            GameEvents.OnAbilityCooldownTicked += OnCooldownTicked;
            GameEvents.OnTurnStarted += OnTurnStarted;
        }

        private void OnDestroy()
        {
            GameEvents.OnCharacterDamaged -= OnCharacterDamaged;
            GameEvents.OnCharacterHealed -= OnCharacterHealed;
            GameEvents.OnCharacterDied -= OnCharacterDied;
            GameEvents.OnEnergyChanged -= OnEnergyChanged;
            GameEvents.OnAbilityUsed -= OnAbilityUsed;
            GameEvents.OnAbilityCooldownTicked -= OnCooldownTicked;
            GameEvents.OnTurnStarted -= OnTurnStarted;
        }

        public void Bind(PlayerController controller)
        {
            _playerController = controller;
            _character = controller.State.Character;

            if (_character == null)
            {
                gameObject.SetActive(false);
                return;
            }

            portrait.sprite = _character.Data.portrait;
            characterNameText.text = _character.Data.characterName;
            deadOverlay.SetActive(false);

            BindAbilityButtons();
            RefreshAll();
        }

        private void BindAbilityButtons()
        {
            for (int i = 0; i < abilityButtons.Count; i++)
            {
                if (i >= _character.Abilities.Count)
                {
                    abilityButtons[i].gameObject.SetActive(false);
                    continue;
                }

                int capturedIndex = i;
                var ability = _character.Abilities[i];
                abilityButtons[i].gameObject.SetActive(true);
                abilityButtons[i].Bind(ability, () => OnAbilityButtonClicked(capturedIndex));
            }
        }

        private void OnAbilityButtonClicked(int index)
        {
            if (!isLocalPlayer) return;
            if (GameManager.Instance.ActivePlayer != _playerController.State) return;

            // For abilities that require a target, the targeting flow would be handled
            // by a separate targeting overlay. For non-targeted abilities we fire immediately.
            var ability = _character.Abilities[index];
            bool needsTarget = ability.targetType != TargetType.None
                && ability.targetType != TargetType.Self
                && ability.targetType != TargetType.Opponent
                && ability.targetType != TargetType.AllFriendlyCreatures
                && ability.targetType != TargetType.AllEnemyCreatures
                && ability.targetType != TargetType.AllCreatures;

            if (needsTarget)
                FindObjectOfType<GameUI>()?.BeginTargeting(index);
            else
                _playerController.State.UseCharacterAbility(index);
        }

        // ── Event handlers ─────────────────────────────────────────────────

        private void OnCharacterDamaged(CharacterState c, int _) { if (c == _character) RefreshHealth(); }
        private void OnCharacterHealed(CharacterState c, int _) { if (c == _character) RefreshHealth(); }

        private void OnCharacterDied(CharacterState c)
        {
            if (c != _character) return;
            deadOverlay.SetActive(true);
            RefreshAbilityButtons();
        }

        private void OnEnergyChanged(CharacterState c, int _) { if (c == _character) RefreshEnergy(); }

        private void OnAbilityUsed(CharacterState c, int idx)
        {
            if (c == _character) RefreshAbilityButton(idx);
        }

        private void OnCooldownTicked(CharacterState c, int idx, int turnsLeft)
        {
            if (c == _character) RefreshAbilityButton(idx);
        }

        private void OnTurnStarted(PlayerState p)
        {
            if (p == _playerController?.State) RefreshAbilityButtons();
        }

        // ── Refresh ────────────────────────────────────────────────────────

        private void RefreshAll()
        {
            RefreshHealth();
            RefreshEnergy();
            RefreshAbilityButtons();
        }

        private void RefreshHealth()
        {
            if (_character == null) return;
            float t = (float)_character.CurrentHealth / _character.MaxHealth;
            healthBar.value = t;
            healthText.text = $"{_character.CurrentHealth}/{_character.MaxHealth}";
        }

        private void RefreshEnergy()
        {
            if (_character == null) return;
            float t = _character.MaxEnergy > 0
                ? (float)_character.CurrentEnergy / _character.MaxEnergy
                : 0f;
            energyBar.value = t;
            energyText.text = $"{_character.CurrentEnergy}/{_character.MaxEnergy}";
        }

        private void RefreshAbilityButtons()
        {
            for (int i = 0; i < abilityButtons.Count; i++)
                RefreshAbilityButton(i);
        }

        private void RefreshAbilityButton(int index)
        {
            if (index >= abilityButtons.Count) return;
            if (_character == null || index >= _character.Abilities.Count) return;

            var btn = abilityButtons[index];
            var state = _character.GetAbilityState(index);
            int cdLeft = _character.GetCooldownRemaining(index);
            btn.Refresh(state, cdLeft, _character.CurrentEnergy);
        }
    }

    // ── Nested helper for a single ability button ──────────────────────────

    [System.Serializable]
    public class AbilityButtonUI
    {
        public GameObject gameObject;
        public Button button;
        public Image icon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI costText;
        public TextMeshProUGUI cooldownText;
        public Image cooldownOverlay;

        private CharacterAbilityData _ability;

        public void Bind(CharacterAbilityData ability, System.Action onClick)
        {
            _ability = ability;
            icon.sprite = ability.icon;
            nameText.text = ability.abilityName;
            costText.text = $"{ability.energyCost}E";
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick());
        }

        public void Refresh(AbilityState state, int cooldownRemaining, int currentEnergy)
        {
            if (_ability == null) return;

            bool ready = state == AbilityState.Ready;
            button.interactable = ready;

            bool onCd = state == AbilityState.OnCooldown;
            cooldownOverlay.gameObject.SetActive(onCd);
            cooldownText.gameObject.SetActive(onCd);
            if (onCd) cooldownText.text = cooldownRemaining.ToString();

            costText.color = currentEnergy >= _ability.energyCost
                ? Color.white
                : Color.red;
        }
    }
}
