using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TCG.Core;
using TCG.Currency;
using TCG.Inventory;
using TCG.Inventory.Deck;
using TCG.Items;
using TCG.Match;
using TCG.Save;

namespace TCG.Campaign
{
    /// <summary>
    /// Singleton that manages the campaign map: tracking stage progress, starting matches,
    /// evaluating stars, granting card/gemstone rewards, and unlocking subsequent stages.
    ///
    /// Attach to a persistent GameObject. Assign <see cref="campaignData"/> in the Inspector.
    /// </summary>
    public class CampaignManager : MonoBehaviour
    {
        public static CampaignManager Instance { get; private set; }

        // ── Inspector ──────────────────────────────────────────────────────────────

        [SerializeField] private CampaignData campaignData;

        [Header("Scene Controllers (assign in Match/Campaign scene)")]
        [SerializeField] private LocalPlayerController localController;
        [SerializeField] private AIPlayerController    aiController;

        // ── Runtime ────────────────────────────────────────────────────────────────

        private CampaignSaveData    _saveData;
        private CampaignStageData   _activeStage;
        private DeckData            _playerDeck;
        private int                 _localCreatureDeaths; // tracked per match for LoseNoCreatures

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadProgress();
            InitializeStageUnlocks();
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the progress entry for a stage, or null if not yet encountered.
        /// </summary>
        public StageSaveEntry GetStageEntry(string stageId)
            => _saveData.stages.Find(e => e.stageId == stageId);

        /// <summary>Returns the status of a stage (Locked / Available / Completed).</summary>
        public StageStatus GetStageStatus(string stageId)
        {
            var entry = GetStageEntry(stageId);
            if (entry == null || !entry.isUnlocked) return StageStatus.Locked;
            return entry.starsEarned > 0 ? StageStatus.Completed : StageStatus.Available;
        }

        /// <summary>Stars earned on a stage (0–3).</summary>
        public int GetStars(string stageId)
            => GetStageEntry(stageId)?.starsEarned ?? 0;

        /// <summary>
        /// Sets the deck the local player will use for campaign matches.
        /// Call this from the campaign map UI before starting a stage.
        /// </summary>
        public void SetPlayerDeck(DeckData deck) => _playerDeck = deck;

        /// <summary>
        /// Builds an AI deck from the stage data and launches a match via MatchManager.
        /// Subscribes to match-end and creature-death events for star evaluation.
        /// </summary>
        public void StartStage(CampaignStageData stage)
        {
            if (stage == null) { Debug.LogWarning("[CampaignManager] StartStage called with null stage."); return; }
            if (_playerDeck == null) { Debug.LogWarning("[CampaignManager] No player deck set. Call SetPlayerDeck first."); return; }

            _activeStage         = stage;
            _localCreatureDeaths = 0;

            GameEvents.OnMatchEnded    += OnMatchEnded;
            GameEvents.OnCreatureDied  += OnCreatureDied;

            var aiDeck = BuildAIDeck(stage);
            MatchManager.Instance.StartMatch(_playerDeck, aiDeck, localController, aiController);
        }

        // ── Private helpers ────────────────────────────────────────────────────────

        private void OnMatchEnded(MatchResult result, MatchState state, MatchRewards rewards)
        {
            GameEvents.OnMatchEnded   -= OnMatchEnded;
            GameEvents.OnCreatureDied -= OnCreatureDied;

            if (_activeStage == null) return;

            var stage   = _activeStage;
            _activeStage = null;

            bool won = result == MatchResult.Victory;

            var entry = EnsureEntry(stage.stageId);
            int prevStars = entry.starsEarned;

            int newStars = won ? EvaluateStars(stage, state) : 0;
            int finalStars = Mathf.Max(prevStars, newStars);

            // Grant newly earned star rewards
            var newCardRewards = new List<CardData>();
            int gemsEarned     = 0;
            bool fullStarBonus = false;

            if (newStars > prevStars)
            {
                for (int i = prevStars; i < newStars; i++)
                {
                    if (stage.starCardRewards != null && i < stage.starCardRewards.Length
                        && stage.starCardRewards[i] != null)
                    {
                        var card = stage.starCardRewards[i];
                        PlayerInventory.Instance?.TryAddItem(card, 1);
                        newCardRewards.Add(card);
                    }
                }

                // Full-star gemstone bonus (only once)
                if (finalStars >= 3 && prevStars < 3)
                {
                    gemsEarned     = stage.gemstoneOnFullStars;
                    fullStarBonus  = true;
                    CurrencyManager.Instance?.Add(CurrencyType.Gems, gemsEarned);
                    GameEvents.RaiseGemstoneRewardGranted(gemsEarned);
                }
            }

            entry.starsEarned = finalStars;
            if (won) entry.isUnlocked = true;

            SaveProgress();
            UnlockAdjacentStages(stage);

            var stageResult = new CampaignStageResult
            {
                StageId             = stage.stageId,
                StarsEarned         = finalStars,
                PreviousStars       = prevStars,
                NewCardRewards      = newCardRewards.ToArray(),
                GemsEarned          = gemsEarned,
                FullStarBonusEarned = fullStarBonus,
                MatchWon            = won
            };

            GameEvents.RaiseCampaignStageCompleted(stageResult);
        }

        private void OnCreatureDied(TCG.Match.CardInstance card, int playerIndex)
        {
            // Track deaths only for local player's creatures (playerIndex == 0)
            if (playerIndex == 0) _localCreatureDeaths++;
        }

        // ── Star evaluation ────────────────────────────────────────────────────────

        private int EvaluateStars(CampaignStageData stage, MatchState state)
        {
            if (stage.starCriteria == null || stage.starCriteria.Length == 0) return 1; // default: 1 star for win

            int count = 0;
            foreach (var criteria in stage.starCriteria)
            {
                if (MeetsCriteria(criteria, state)) count++;
            }
            return Mathf.Min(count, 3);
        }

        private bool MeetsCriteria(StarCriteriaData criteria, MatchState state)
        {
            return criteria.type switch
            {
                StarCriteriaType.WinMatch        => true, // already filtered to wins above
                StarCriteriaType.WinWithinTurns  => state.TurnNumber <= criteria.threshold,
                StarCriteriaType.KeepHealthAbove => state.Players[0].CurrentHealth >= criteria.threshold,
                StarCriteriaType.LoseNoCreatures => _localCreatureDeaths == 0,
                _                                => false
            };
        }

        // ── Stage unlock logic ─────────────────────────────────────────────────────

        private void InitializeStageUnlocks()
        {
            if (campaignData == null) return;

            bool changed = false;
            foreach (var chapter in campaignData.chapters)
            {
                foreach (var stage in chapter.stages)
                {
                    var entry = EnsureEntry(stage.stageId);
                    if (!entry.isUnlocked && ArePrerequisitesMet(stage))
                    {
                        entry.isUnlocked = true;
                        changed = true;
                    }
                }
            }

            if (changed) SaveProgress();
        }

        private void UnlockAdjacentStages(CampaignStageData completedStage)
        {
            if (campaignData == null) return;

            bool changed = false;
            foreach (var chapter in campaignData.chapters)
            {
                foreach (var stage in chapter.stages)
                {
                    var entry = EnsureEntry(stage.stageId);
                    if (!entry.isUnlocked && ArePrerequisitesMet(stage))
                    {
                        entry.isUnlocked = true;
                        GameEvents.RaiseCampaignStageUnlocked(stage.stageId);
                        changed = true;
                    }
                }
            }

            if (changed) SaveProgress();
        }

        private bool ArePrerequisitesMet(CampaignStageData stage)
        {
            if (stage.prerequisites == null || stage.prerequisites.Length == 0)
                return true; // first stage in chapter is always unlocked

            foreach (var prereq in stage.prerequisites)
            {
                if (prereq == null) continue;
                var e = GetStageEntry(prereq.stageId);
                if (e == null || e.starsEarned < 1) return false; // must have at least 1 star
            }
            return true;
        }

        // ── AI deck building ───────────────────────────────────────────────────────

        private DeckData BuildAIDeck(CampaignStageData stage)
        {
            var deck = new DeckData("Enemy Deck");

            if (stage.aiDeckEntries != null)
            {
                foreach (var entry in stage.aiDeckEntries)
                {
                    if (entry.card == null) continue;
                    for (int i = 0; i < entry.count; i++)
                        deck.TryAddCard(entry.card);
                }
            }

            // Pad to minimum size by repeating first card
            if (deck.TotalCards < stage.minAiDeckSize && stage.aiDeckEntries?.Length > 0)
            {
                var filler = stage.aiDeckEntries[0].card;
                while (deck.TotalCards < stage.minAiDeckSize)
                    if (!deck.TryAddCard(filler)) break; // respect max copies
            }

            return deck;
        }

        // ── Persistence ────────────────────────────────────────────────────────────

        private void LoadProgress()
        {
            var save = SaveSystem.Load();
            _saveData = save?.campaign ?? new CampaignSaveData();
        }

        private void SaveProgress()
        {
            var save = SaveSystem.Load() ?? new PlayerSaveData();
            save.campaign = _saveData;
            SaveSystem.Save(save);
        }

        private StageSaveEntry EnsureEntry(string stageId)
        {
            var entry = _saveData.stages.Find(e => e.stageId == stageId);
            if (entry == null)
            {
                entry = new StageSaveEntry { stageId = stageId };
                _saveData.stages.Add(entry);
            }
            return entry;
        }
    }
}
