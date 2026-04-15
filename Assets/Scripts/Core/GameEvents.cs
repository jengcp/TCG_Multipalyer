using System;
using System.Collections.Generic;
using TCG.Campaign;
using TCG.Characters;
using TCG.Currency;
using TCG.Cutscene;
using TCG.Inventory;
using TCG.Inventory.Deck;
using TCG.Items;
using TCG.Match;
using TCG.Match.Effects;
using TCG.Narrative;
using TCG.Quest;
using TCG.Ranked;
using TCG.Shop;

namespace TCG.Core
{
    /// <summary>
    /// Central static event bus for the entire game.
    /// All systems communicate through here — no direct MonoBehaviour references cross systems.
    /// </summary>
    public static class GameEvents
    {
        // ── Currency ────────────────────────────────────────────────────────────
        public static event Action<CurrencyType, int>       OnCurrencyChanged;
        public static event Action<CurrencyType, int, bool> OnPurchaseAttempted;

        // ── Shop ────────────────────────────────────────────────────────────────
        public static event Action<ShopItemListing> OnItemPurchased;
        public static event Action                  OnShopRefreshed;
        public static event Action<string>          OnShopCategoryChanged;

        // ── Inventory ───────────────────────────────────────────────────────────
        public static event Action<InventoryItem>   OnItemAdded;
        public static event Action<InventoryItem>   OnItemRemoved;
        public static event Action<InventoryItem>   OnItemInspected;
        public static event Action                  OnInventoryOpened;
        public static event Action                  OnInventoryClosed;

        // ── Deck ────────────────────────────────────────────────────────────────
        public static event Action<DeckData>  OnDeckChanged;
        public static event Action<string>    OnDeckDeleted;

        // ── Match — summary events (for Quest tracking) ──────────────────────────
        public static event Action<bool, CardElement, CardClass> OnMatchCompleted;
        public static event Action<CardData>  OnCardPlayed;
        public static event Action<PackData>  OnPackOpened;
        public static event Action            OnDayLogin;
        public static event Action<int>       OnGoldEarned;

        // ── Match — lifecycle ────────────────────────────────────────────────────
        public static event Action<MatchState>  OnMatchStarted;
        public static event Action<int, int>    OnTurnStarted;   // turnNumber, playerIndex
        public static event Action<MatchPhase>  OnPhaseChanged;
        public static event Action<int, int>    OnTurnEnded;     // turnNumber, playerIndex

        // ── Match — battlefield ──────────────────────────────────────────────────
        public static event Action<CardInstance, int, int> OnCardPlacedOnBattlefield; // card, playerIdx, slotIdx
        public static event Action<CardInstance, int>      OnCardReturnedToHand;
        public static event Action<CardInstance, int>      OnCardSentToGraveyard;
        public static event Action<CardInstance, int>      OnCardDrawn;

        // ── Match — combat ───────────────────────────────────────────────────────
        public static event Action<CardInstance, int>       OnAttackDeclared;      // attacker, attackerPlayerIdx
        public static event Action<int, CardInstance, int>  OnDamageDealt;         // dmg, target (null=direct), targetPlayerIdx
        public static event Action<int, int, int>           OnPlayerHealthChanged; // playerIdx, newHealth, delta
        public static event Action<CardInstance, int>       OnCreatureDied;        // card, playerIdx

        // ── Match — end ───────────────────────────────────────────────────────────
        public static event Action<MatchResult, MatchState, MatchRewards> OnMatchEnded;

        // ── Campaign ─────────────────────────────────────────────────────────────
        /// <summary>Fires when a stage match ends and stars have been evaluated.</summary>
        public static event Action<CampaignStageResult> OnCampaignStageCompleted;
        /// <summary>Fires when a previously-locked stage becomes playable.</summary>
        public static event Action<string>               OnCampaignStageUnlocked;  // stageId
        /// <summary>Fires when gemstones are awarded for earning all 3 stars on a stage.</summary>
        public static event Action<int>                  OnGemstoneRewardGranted;

        // ── Gacha ─────────────────────────────────────────────────────────────────
        /// <summary>Fires after a gacha pull completes with the list of cards obtained.</summary>
        public static event Action<List<CardData>>       OnGachaPullCompleted;

        // ── Characters ───────────────────────────────────────────────────────────
        /// <summary>Fires when a character is unlocked (starter or purchased).</summary>
        public static event Action<CharacterData>        OnCharacterUnlocked;

        // ── Quest ────────────────────────────────────────────────────────────────
        public static event Action<QuestProgress> OnQuestCompleted;
        public static event Action<QuestProgress> OnQuestClaimed;
        public static event Action<QuestProgress> OnQuestExpired;
        public static event Action<QuestCategory> OnQuestRotationRefreshed;
        public static event Action<int>           OnXPEarned;

        // ── Ranked ───────────────────────────────────────────────────────────────
        /// <summary>Fires after a ranked match resolves and RP / rank have been updated.</summary>
        public static event Action<RankedMatchOutcome> OnRankedMatchResolved;
        /// <summary>Fires when a season ends. Carries the player's peak tier and gems rewarded.</summary>
        public static event Action<RankTier, int>      OnSeasonEnded;

        // ── Cutscene ─────────────────────────────────────────────────────────────
        /// <summary>Fires when a cutscene begins playing.</summary>
        public static event Action<CutsceneData> OnCutsceneStarted;
        /// <summary>Fires when a cutscene finishes (all beats done or skipped).</summary>
        public static event Action<CutsceneData> OnCutsceneEnded;
        /// <summary>Fires at the start of each individual beat within a cutscene.</summary>
        public static event Action<CutsceneBeat> OnCutsceneBeatStarted;

        // ── Narrative ─────────────────────────────────────────────────────────────
        /// <summary>Fires when a narrative event fires for the first time (or is manually triggered).</summary>
        public static event Action<NarrativeEventData> OnNarrativeEventTriggered;

        // ── Raisers ──────────────────────────────────────────────────────────────

        public static void RaiseCurrencyChanged(CurrencyType type, int newAmount)
            => OnCurrencyChanged?.Invoke(type, newAmount);

        public static void RaisePurchaseAttempted(CurrencyType type, int amount, bool success)
            => OnPurchaseAttempted?.Invoke(type, amount, success);

        public static void RaiseItemPurchased(ShopItemListing listing)
            => OnItemPurchased?.Invoke(listing);

        public static void RaiseShopRefreshed()
            => OnShopRefreshed?.Invoke();

        public static void RaiseShopCategoryChanged(string category)
            => OnShopCategoryChanged?.Invoke(category);

        public static void RaiseItemAdded(InventoryItem item)
            => OnItemAdded?.Invoke(item);

        public static void RaiseItemRemoved(InventoryItem item)
            => OnItemRemoved?.Invoke(item);

        public static void RaiseItemInspected(InventoryItem item)
            => OnItemInspected?.Invoke(item);

        public static void RaiseInventoryOpened()
            => OnInventoryOpened?.Invoke();

        public static void RaiseInventoryClosed()
            => OnInventoryClosed?.Invoke();

        public static void RaiseDeckChanged(DeckData deck)
            => OnDeckChanged?.Invoke(deck);

        public static void RaiseDeckDeleted(string deckId)
            => OnDeckDeleted?.Invoke(deckId);

        public static void RaiseMatchCompleted(bool won, CardElement element, CardClass cardClass)
            => OnMatchCompleted?.Invoke(won, element, cardClass);

        public static void RaiseCardPlayed(CardData card)
            => OnCardPlayed?.Invoke(card);

        public static void RaisePackOpened(PackData pack)
            => OnPackOpened?.Invoke(pack);

        public static void RaiseDayLogin()
            => OnDayLogin?.Invoke();

        public static void RaiseGoldEarned(int amount)
            => OnGoldEarned?.Invoke(amount);

        public static void RaiseMatchStarted(MatchState state)
            => OnMatchStarted?.Invoke(state);

        public static void RaiseTurnStarted(int turn, int playerIndex)
            => OnTurnStarted?.Invoke(turn, playerIndex);

        public static void RaisePhaseChanged(MatchPhase phase)
            => OnPhaseChanged?.Invoke(phase);

        public static void RaiseTurnEnded(int turn, int playerIndex)
            => OnTurnEnded?.Invoke(turn, playerIndex);

        public static void RaiseCardPlacedOnBattlefield(CardInstance card, int playerIndex, int slotIndex)
            => OnCardPlacedOnBattlefield?.Invoke(card, playerIndex, slotIndex);

        public static void RaiseCardReturnedToHand(CardInstance card, int playerIndex)
            => OnCardReturnedToHand?.Invoke(card, playerIndex);

        public static void RaiseCardSentToGraveyard(CardInstance card, int playerIndex)
            => OnCardSentToGraveyard?.Invoke(card, playerIndex);

        public static void RaiseCardDrawn(CardInstance card, int playerIndex)
            => OnCardDrawn?.Invoke(card, playerIndex);

        public static void RaiseAttackDeclared(CardInstance attacker, int attackerPlayerIndex)
            => OnAttackDeclared?.Invoke(attacker, attackerPlayerIndex);

        public static void RaiseDamageDealt(int damage, CardInstance target, int targetPlayerIndex)
            => OnDamageDealt?.Invoke(damage, target, targetPlayerIndex);

        public static void RaisePlayerHealthChanged(int playerIndex, int newHealth, int delta)
            => OnPlayerHealthChanged?.Invoke(playerIndex, newHealth, delta);

        public static void RaiseCreatureDied(CardInstance card, int playerIndex)
            => OnCreatureDied?.Invoke(card, playerIndex);

        public static void RaiseMatchEnded(MatchResult result, MatchState state, MatchRewards rewards)
            => OnMatchEnded?.Invoke(result, state, rewards);

        public static void RaiseCampaignStageCompleted(CampaignStageResult result)
            => OnCampaignStageCompleted?.Invoke(result);

        public static void RaiseCampaignStageUnlocked(string stageId)
            => OnCampaignStageUnlocked?.Invoke(stageId);

        public static void RaiseGemstoneRewardGranted(int amount)
            => OnGemstoneRewardGranted?.Invoke(amount);

        public static void RaiseGachaPullCompleted(List<CardData> cards)
            => OnGachaPullCompleted?.Invoke(cards);

        public static void RaiseCharacterUnlocked(CharacterData character)
            => OnCharacterUnlocked?.Invoke(character);

        public static void RaiseQuestCompleted(QuestProgress quest)
            => OnQuestCompleted?.Invoke(quest);

        public static void RaiseQuestClaimed(QuestProgress quest)
            => OnQuestClaimed?.Invoke(quest);

        public static void RaiseQuestExpired(QuestProgress quest)
            => OnQuestExpired?.Invoke(quest);

        public static void RaiseQuestRotationRefreshed(QuestCategory category)
            => OnQuestRotationRefreshed?.Invoke(category);

        public static void RaiseXPEarned(int amount)
            => OnXPEarned?.Invoke(amount);

        public static void RaiseRankedMatchResolved(RankedMatchOutcome outcome)
            => OnRankedMatchResolved?.Invoke(outcome);

        public static void RaiseSeasonEnded(RankTier peakTier, int gemReward)
            => OnSeasonEnded?.Invoke(peakTier, gemReward);

        public static void RaiseCutsceneStarted(CutsceneData data)
            => OnCutsceneStarted?.Invoke(data);

        public static void RaiseCutsceneEnded(CutsceneData data)
            => OnCutsceneEnded?.Invoke(data);

        public static void RaiseCutsceneBeatStarted(CutsceneBeat beat)
            => OnCutsceneBeatStarted?.Invoke(beat);

        public static void RaiseNarrativeEventTriggered(NarrativeEventData ev)
            => OnNarrativeEventTriggered?.Invoke(ev);
    }
}
