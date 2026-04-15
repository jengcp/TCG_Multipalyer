using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TCG.Campaign;
using TCG.Characters;
using TCG.Core;
using TCG.Cutscene;
using TCG.Items;
using TCG.Ranked;
using TCG.Save;

namespace TCG.Narrative
{
    /// <summary>
    /// Singleton that drives the overarching game narrative.
    ///
    /// Attach to a persistent GameObject. Assign <see cref="config"/> in the Inspector.
    ///
    /// The manager listens to <see cref="GameEvents"/> and fires the first unseen
    /// <see cref="NarrativeEventData"/> whose trigger condition is satisfied.
    /// When the event has a cutscene it is queued and played via <see cref="CutsceneManager"/>;
    /// if a cutscene is already playing, the new one waits until it finishes.
    ///
    /// Story Log access:
    /// <code>
    ///   var entries = NarrativeManager.Instance.GetUnlockedEntries();
    /// </code>
    /// </summary>
    public class NarrativeManager : MonoBehaviour
    {
        public static NarrativeManager Instance { get; private set; }

        [SerializeField] private NarrativeConfig config;

        // ── Runtime ────────────────────────────────────────────────────────────────
        private NarrativeSaveData       _saveData;
        private HashSet<string>         _seenIds = new();
        private Queue<NarrativeEventData> _cutsceneQueue = new();
        private bool                    _processingQueue;

        // ── Unity ──────────────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadSave();

            GameEvents.OnCampaignStageCompleted  += OnCampaignStageCompleted;
            GameEvents.OnRankedMatchResolved     += OnRankedMatchResolved;
            GameEvents.OnCharacterUnlocked       += OnCharacterUnlocked;
            GameEvents.OnGachaPullCompleted      += OnGachaPullCompleted;
            GameEvents.OnDayLogin                += OnDayLogin;

            // Fire OnGameStart events the very first time the game is played.
            TryFireEvents(t => t.type == NarrativeTriggerType.OnGameStart);
        }

        private void OnDestroy()
        {
            GameEvents.OnCampaignStageCompleted  -= OnCampaignStageCompleted;
            GameEvents.OnRankedMatchResolved     -= OnRankedMatchResolved;
            GameEvents.OnCharacterUnlocked       -= OnCharacterUnlocked;
            GameEvents.OnGachaPullCompleted      -= OnGachaPullCompleted;
            GameEvents.OnDayLogin                -= OnDayLogin;
        }

        // ── Public API ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns all narrative events that have been seen, in config order.
        /// Used by the Story Log UI to display unlocked entries.
        /// </summary>
        public IReadOnlyList<NarrativeEventData> GetUnlockedEntries()
        {
            var result = new List<NarrativeEventData>();
            if (config == null) return result;

            foreach (var ev in config.events)
            {
                if (ev != null && ev.addToLog && _seenIds.Contains(ev.eventId))
                    result.Add(ev);
            }
            return result;
        }

        /// <summary>
        /// Replays the cutscene attached to a previously-seen narrative event.
        /// Does not re-fire the event or alter save data.
        /// </summary>
        public void ReplayCutscene(string eventId)
        {
            if (config == null) return;
            foreach (var ev in config.events)
            {
                if (ev != null && ev.eventId == eventId && ev.cutscene != null)
                {
                    EnqueueCutscene(ev);
                    return;
                }
            }
        }

        /// <summary>
        /// Manually fires a <see cref="NarrativeTriggerType.Manual"/> event by ID.
        /// Safe to call even if the event has already been seen (no-op in that case).
        /// </summary>
        public void TriggerManual(string eventId)
        {
            if (config == null) return;
            foreach (var ev in config.events)
            {
                if (ev != null && ev.eventId == eventId
                    && ev.trigger.type == NarrativeTriggerType.Manual)
                {
                    FireEvent(ev);
                    return;
                }
            }
        }

        // ── GameEvent handlers ─────────────────────────────────────────────────────

        private void OnCampaignStageCompleted(CampaignStageResult result)
        {
            TryFireEvents(t =>
            {
                if (t.type == NarrativeTriggerType.OnCampaignStageCompleted)
                {
                    if (!string.IsNullOrEmpty(t.stageId) && t.stageId != result.StageId) return false;
                    if (t.minStars > 0 && result.StarsEarned < t.minStars) return false;
                    return result.MatchWon;
                }
                if (t.type == NarrativeTriggerType.OnCampaignStageFullyStarred)
                {
                    if (!string.IsNullOrEmpty(t.stageId) && t.stageId != result.StageId) return false;
                    return result.StarsEarned >= 3 && result.PreviousStars < 3;
                }
                return false;
            });
        }

        private void OnRankedMatchResolved(RankedMatchOutcome outcome)
        {
            if (outcome.MatchResult == TCG.Match.MatchResult.Victory)
                TryFireEvents(t => t.type == NarrativeTriggerType.OnRankedWin);

            if (outcome.Promoted)
                TryFireEvents(t =>
                    t.type == NarrativeTriggerType.OnRankedPromotion &&
                    outcome.NewTier >= t.minRankTier);
        }

        private void OnCharacterUnlocked(CharacterData character)
        {
            TryFireEvents(t =>
                t.type == NarrativeTriggerType.OnCharacterUnlocked &&
                (string.IsNullOrEmpty(t.characterId) || t.characterId == character.characterId));
        }

        private void OnGachaPullCompleted(System.Collections.Generic.List<CardData> _)
        {
            TryFireEvents(t => t.type == NarrativeTriggerType.OnGachaPull);
        }

        private void OnDayLogin()
        {
            TryFireEvents(t => t.type == NarrativeTriggerType.OnDayLogin);
        }

        // ── Core firing logic ──────────────────────────────────────────────────────

        /// <summary>
        /// Scans config.events in order and fires the first unseen event whose trigger passes
        /// <paramref name="predicate"/>. Only one event fires per call.
        /// </summary>
        private void TryFireEvents(Func<NarrativeTrigger, bool> predicate)
        {
            if (config == null) return;

            foreach (var ev in config.events)
            {
                if (ev == null || string.IsNullOrEmpty(ev.eventId)) continue;
                if (_seenIds.Contains(ev.eventId)) continue;
                if (!predicate(ev.trigger)) continue;

                FireEvent(ev);
                return; // one event per trigger check
            }
        }

        private void FireEvent(NarrativeEventData ev)
        {
            if (_seenIds.Contains(ev.eventId)) return;

            _seenIds.Add(ev.eventId);
            _saveData.seenEventIds.Add(ev.eventId);
            SaveProgress();

            GameEvents.RaiseNarrativeEventTriggered(ev);

            if (ev.cutscene != null)
                EnqueueCutscene(ev);
        }

        // ── Cutscene queue ─────────────────────────────────────────────────────────

        private void EnqueueCutscene(NarrativeEventData ev)
        {
            _cutsceneQueue.Enqueue(ev);
            if (!_processingQueue)
                StartCoroutine(ProcessCutsceneQueue());
        }

        private IEnumerator ProcessCutsceneQueue()
        {
            _processingQueue = true;

            while (_cutsceneQueue.Count > 0)
            {
                // Wait if CutsceneManager is already playing something else
                yield return new WaitUntil(() =>
                    CutsceneManager.Instance == null || !CutsceneManager.Instance.IsPlaying);

                if (_cutsceneQueue.Count == 0) break;

                var next = _cutsceneQueue.Dequeue();
                if (CutsceneManager.Instance != null && next.cutscene != null)
                    CutsceneManager.Instance.Play(next.cutscene);

                // Give CutsceneManager one frame to set IsPlaying = true before we check again
                yield return null;
            }

            _processingQueue = false;
        }

        // ── Persistence ────────────────────────────────────────────────────────────

        private void LoadSave()
        {
            var save = SaveSystem.Load();
            _saveData = save?.narrative ?? new NarrativeSaveData();

            _seenIds.Clear();
            foreach (var id in _saveData.seenEventIds)
                _seenIds.Add(id);
        }

        private void SaveProgress()
        {
            var save = SaveSystem.Load() ?? new PlayerSaveData();
            save.narrative = _saveData;
            SaveSystem.Save(save);
        }
    }
}
