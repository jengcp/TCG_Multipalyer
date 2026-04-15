using System;
using System.Collections.Generic;
using UnityEngine;
using TCG.Core;
using TCG.Currency;
using TCG.Inventory.Deck;
using TCG.Match;
using TCG.Save;

namespace TCG.Ranked
{
    /// <summary>
    /// Singleton that owns the entire ranked ladder:
    ///   • <see cref="StartRankedMatch"/> — kicks off a match against an AI opponent near the player's rank
    ///   • <see cref="GetCurrentRank"/> / <see cref="GetStats"/> — read-only rank state for UI
    ///   • <see cref="GetLeaderboard"/> — sorted list merging simulated + local player entries
    ///   • <see cref="EndSeason"/> — distributes season rewards and soft-resets RP
    ///
    /// RP rules:
    ///   Win  = +25 RP   Loss = −20 RP (floor 0 in Bronze III)   Draw = −5 RP
    ///   Auto-promote at 100 RP; auto-demote at 0 RP on a loss (one demotion shield per promotion).
    ///   Master has no division cap — RP accumulates indefinitely.
    /// </summary>
    public class RankedManager : MonoBehaviour
    {
        public static RankedManager Instance { get; private set; }

        // ── Inspector ──────────────────────────────────────────────────────────────
        [SerializeField] private SeasonData           _currentSeason;
        [SerializeField] private LocalPlayerController _localController;
        [SerializeField] private AIPlayerController   _aiController;

        // ── Public Properties ─────────────────────────────────────────────────────
        /// <summary>The currently active season configuration (read-only).</summary>
        public SeasonData CurrentSeason => _currentSeason;

        // ── State ──────────────────────────────────────────────────────────────────
        private bool _pendingRanked;

        // ── Constants ─────────────────────────────────────────────────────────────
        private const int RpPerDivision = 100;
        private const int RpOnWin       =  25;
        private const int RpLossOnLoss  =  20;
        private const int RpLossOnDraw  =   5;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnEnable()  => GameEvents.OnMatchEnded += HandleMatchEnded;
        private void OnDisable() => GameEvents.OnMatchEnded -= HandleMatchEnded;

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Starts a ranked match using <paramref name="playerDeck"/> against an AI opponent
        /// selected from <see cref="SeasonData.aiOpponents"/> closest to the player's current rank.
        /// </summary>
        public void StartRankedMatch(DeckData playerDeck)
        {
            if (_currentSeason == null)
            {
                Debug.LogWarning("[RankedManager] No SeasonData assigned — cannot start ranked match.");
                return;
            }

            if (_localController == null || _aiController == null)
            {
                Debug.LogWarning("[RankedManager] Controllers not assigned — cannot start ranked match.");
                return;
            }

            var (currentTier, _, _) = GetCurrentRank();
            var opponent = PickOpponent(currentTier);
            if (opponent == null)
            {
                Debug.LogWarning("[RankedManager] No AI opponents configured in SeasonData.");
                return;
            }

            var aiDeck = BuildDeck(opponent);

            _pendingRanked = true;
            MatchManager.Instance.StartMatch(playerDeck, aiDeck, _localController, _aiController);
        }

        /// <summary>Returns the player's current (tier, division, rp) from save.</summary>
        public (RankTier tier, RankDivision division, int rp) GetCurrentRank()
        {
            var save = LoadRanked();
            return ((RankTier)save.tier, (RankDivision)save.division, save.rp);
        }

        /// <summary>Returns (wins, losses, draws) for the current season.</summary>
        public (int wins, int losses, int draws) GetStats()
        {
            var save = LoadRanked();
            return (save.wins, save.losses, save.draws);
        }

        /// <summary>Win-rate as a 0–1 float. Returns 0 if no games played.</summary>
        public float GetWinRate()
        {
            var (wins, losses, draws) = GetStats();
            int total = wins + losses + draws;
            return total > 0 ? (float)wins / total : 0f;
        }

        /// <summary>
        /// Returns the leaderboard sorted by <see cref="LeaderboardEntry.AbsoluteRP"/> descending.
        /// Combines <see cref="SeasonData.leaderboardEntries"/> with the local player's current rank.
        /// </summary>
        public List<LeaderboardEntry> GetLeaderboard()
        {
            var entries = new List<LeaderboardEntry>();

            // Simulated players
            if (_currentSeason != null)
            {
                foreach (var sim in _currentSeason.leaderboardEntries)
                {
                    entries.Add(new LeaderboardEntry
                    {
                        PlayerName   = sim.playerName,
                        Tier         = sim.tier,
                        Division     = sim.division,
                        RP           = sim.rp,
                        Wins         = sim.wins,
                        Losses       = sim.losses,
                        IsLocalPlayer = false,
                    });
                }
            }

            // Local player
            var save = LoadRanked();
            var (wins, losses, _) = GetStats();
            entries.Add(new LeaderboardEntry
            {
                PlayerName    = "You",
                Tier          = (RankTier)save.tier,
                Division      = (RankDivision)save.division,
                RP            = save.rp,
                Wins          = wins,
                Losses        = losses,
                IsLocalPlayer = true,
            });

            entries.Sort((a, b) => b.AbsoluteRP.CompareTo(a.AbsoluteRP));
            return entries;
        }

        /// <summary>
        /// Closes the season: grants rewards for peak rank, soft-resets RP to 50, fires <see cref="GameEvents.OnSeasonEnded"/>.
        /// </summary>
        public void EndSeason()
        {
            if (_currentSeason == null) return;

            var save      = LoadRanked();
            var peakTier  = (RankTier)save.peakTier;
            int gemReward = 0;

            // Find the highest matching reward
            SeasonRewardEntry best = null;
            foreach (var entry in _currentSeason.seasonRewards)
            {
                if (peakTier >= entry.minimumRank)
                {
                    if (best == null || entry.minimumRank > best.minimumRank)
                        best = entry;
                }
            }

            if (best != null)
            {
                if (best.goldReward > 0)
                    CurrencyManager.Instance.Add(CurrencyType.Gold, best.goldReward);
                if (best.gemReward > 0)
                {
                    CurrencyManager.Instance.Add(CurrencyType.Gems, best.gemReward);
                    gemReward = best.gemReward;
                }
            }

            // Soft reset: keep tier, reset RP to 50
            save.rp              = 50;
            save.hasDemotionShield = false;
            save.currentSeasonId = _currentSeason.seasonId;
            PersistRanked(save);

            GameEvents.RaiseSeasonEnded(peakTier, gemReward);
        }

        // ── Match Result Handler ───────────────────────────────────────────────────

        private void HandleMatchEnded(MatchResult result, MatchState state, MatchRewards rewards)
        {
            if (!_pendingRanked) return;
            _pendingRanked = false;

            var save = LoadRanked();

            // Tally stats
            switch (result)
            {
                case MatchResult.Victory: save.wins++;   break;
                case MatchResult.Defeat:  save.losses++; break;
                case MatchResult.Draw:    save.draws++;  break;
            }

            var outcome = ApplyRpChange(save, result);

            PersistRanked(save);
            GameEvents.RaiseRankedMatchResolved(outcome);
        }

        // ── RP / Rank Logic ───────────────────────────────────────────────────────

        private RankedMatchOutcome ApplyRpChange(RankedSaveData save, MatchResult result)
        {
            int delta = result switch
            {
                MatchResult.Victory => +RpOnWin,
                MatchResult.Defeat  => -RpLossOnLoss,
                MatchResult.Draw    => -RpLossOnDraw,
                _                   => 0,
            };

            bool promoted = false;
            bool demoted  = false;

            save.rp += delta;

            // Master has no cap
            if ((RankTier)save.tier == RankTier.Master)
            {
                save.rp = Math.Max(0, save.rp);
            }
            else if (save.rp >= RpPerDivision)
            {
                // Promote
                save.rp = 0;
                AdvanceDivision(save);
                save.hasDemotionShield = true;
                promoted = true;
            }
            else if (save.rp < 0)
            {
                // At floor of Bronze III — clamp to 0
                if ((RankTier)save.tier == RankTier.Bronze && (RankDivision)save.division == RankDivision.DivIII)
                {
                    save.rp = 0;
                }
                else if (save.hasDemotionShield)
                {
                    // Shield absorbs this demotion
                    save.rp = 0;
                    save.hasDemotionShield = false;
                }
                else
                {
                    // Demote
                    save.rp = RpPerDivision - 1;
                    RetreatDivision(save);
                    demoted = true;
                }
            }

            // Update peak (tracked by tier+division only, not RP)
            int newRankScore  = save.tier * 300 +
                                (save.division == (int)RankDivision.None ? 0 : save.division * 100);
            int peakRankScore = save.peakTier * 300 +
                                (save.peakDivision == (int)RankDivision.None ? 0 : save.peakDivision * 100);
            if (newRankScore > peakRankScore)
            {
                save.peakTier     = save.tier;
                save.peakDivision = save.division;
            }

            return new RankedMatchOutcome
            {
                RpDelta     = delta,
                NewRP       = save.rp,
                NewTier     = (RankTier)save.tier,
                NewDivision = (RankDivision)save.division,
                Promoted    = promoted,
                Demoted     = demoted,
                MatchResult = result,
            };
        }

        /// <summary>Advances tier/division by one step (DivIII → DivII → DivI → next tier DivIII).</summary>
        private static void AdvanceDivision(RankedSaveData save)
        {
            var tier = (RankTier)save.tier;
            var div  = (RankDivision)save.division;

            if (tier == RankTier.Master) return;  // no further advance

            if (div == RankDivision.DivI)
            {
                // Move to next tier
                save.tier     = save.tier + 1;
                save.division = (int)((RankTier)(save.tier) == RankTier.Master
                    ? RankDivision.None
                    : RankDivision.DivIII);
            }
            else
            {
                save.division++;
            }
        }

        /// <summary>Retreats tier/division by one step (DivIII → previous tier DivI).</summary>
        private static void RetreatDivision(RankedSaveData save)
        {
            var tier = (RankTier)save.tier;
            var div  = (RankDivision)save.division;

            if (tier == RankTier.Bronze && div == RankDivision.DivIII) return; // absolute floor

            if (div == RankDivision.DivIII || div == RankDivision.None)
            {
                // Move to previous tier, DivI
                save.tier     = save.tier - 1;
                save.division = (int)RankDivision.DivI;
            }
            else
            {
                save.division--;
            }
        }

        // ── Opponent Selection ─────────────────────────────────────────────────────

        private RankedAiOpponent PickOpponent(RankTier playerTier)
        {
            if (_currentSeason.aiOpponents == null || _currentSeason.aiOpponents.Length == 0)
                return null;

            // Find opponent whose approximate rank is closest to the player's tier
            RankedAiOpponent best      = _currentSeason.aiOpponents[0];
            int              bestDelta = int.MaxValue;

            foreach (var opp in _currentSeason.aiOpponents)
            {
                int delta = Mathf.Abs((int)opp.approximateRank - (int)playerTier);
                if (delta < bestDelta)
                {
                    bestDelta = delta;
                    best      = opp;
                }
            }

            return best;
        }

        private static DeckData BuildDeck(RankedAiOpponent opponent)
        {
            var deck = new DeckData(opponent.opponentName + "'s Deck");

            if (opponent.deckEntries != null)
            {
                foreach (var entry in opponent.deckEntries)
                {
                    if (entry?.card == null) continue;
                    for (int i = 0; i < entry.count; i++)
                        deck.TryAddCard(entry.card);
                }
            }

            return deck;
        }

        // ── Persistence ───────────────────────────────────────────────────────────

        private static RankedSaveData LoadRanked()
        {
            var save = SaveSystem.Load();
            save.ranked ??= new RankedSaveData();
            return save.ranked;
        }

        private static void PersistRanked(RankedSaveData ranked)
        {
            var save   = SaveSystem.Load() ?? new PlayerSaveData();
            save.ranked = ranked;
            SaveSystem.Save(save);
        }
    }
}
