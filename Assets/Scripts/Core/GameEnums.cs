namespace TCG.Core
{
    public enum CardType
    {
        Creature,
        Spell,
        Artifact,
        Trap
    }

    public enum CardRarity
    {
        Common,
        Uncommon,
        Rare,
        Legendary
    }

    public enum CardElement
    {
        Neutral,
        Fire,
        Water,
        Earth,
        Wind,
        Light,
        Dark
    }

    public enum EffectType
    {
        DealDamage,
        Heal,
        DrawCards,
        BuffAttack,
        BuffDefense,
        DestroyCreature,
        ReturnToHand,
        AddMana,
        ApplyPoison,
        Shield,
        Silence
    }

    public enum TargetType
    {
        None,
        Self,
        Opponent,
        FriendlyCreature,
        EnemyCreature,
        AnyCreature,
        AllFriendlyCreatures,
        AllEnemyCreatures,
        AllCreatures
    }

    public enum GamePhase
    {
        NotStarted,
        DrawPhase,
        MainPhase,
        BattlePhase,
        EndPhase,
        GameOver
    }

    public enum PlayerAction
    {
        PlayCard,
        Attack,
        ActivateAbility,
        EndTurn,
        Surrender
    }

    public enum GameZone
    {
        Deck,
        Hand,
        Field,
        ArtifactZone,
        TrapZone,
        Graveyard,
        Exile
    }

    // ── Combat sub-phases ─────────────────────────────────────────────────

    public enum BattleSubPhase
    {
        Idle,
        DeclareAttackers,   // active player picks attackers
        DeclareBlockers,    // defending player assigns blockers
        ResolveCombat       // server resolves all assignments
    }

    // ── Trap triggers ─────────────────────────────────────────────────────

    public enum TrapTrigger
    {
        OnCreatureAttacks,
        OnCreaturePlayed,
        OnSpellPlayed,
        OnPlayerDamaged,
        OnTurnStart,
        OnCreatureDies
    }

    // ── Fatigue / special draw results ────────────────────────────────────

    public enum DrawResult
    {
        Success,
        HandFull,
        FatigueDamage   // deck was empty — player took damage instead
    }

    public enum GameResult
    {
        None,
        Player1Win,
        Player2Win,
        Draw
    }

    public enum StatusEffect
    {
        None,
        Poisoned,
        Silenced,
        Shielded,
        Exhausted
    }

    // ── Character system ───────────────────────────────────────────────────

    public enum CharacterEffectType
    {
        DealDamageToPlayer,
        DealDamageToAllCreatures,
        HealPlayer,
        HealCharacter,
        DrawCards,
        BuffAllFriendlyCreatures,
        SummonToken,
        StealCreature,
        ResurrectCreature,
        AddEnergy,
        DrainOpponentEnergy,
        DoubleManaThisTurn,
        ShieldPlayerOnce,
        DestroyAllCreatures,
        GiveCreatureKeyword
    }

    public enum AbilityState
    {
        Ready,
        OnCooldown,
        NotEnoughEnergy,
        Disabled
    }

    public enum CharacterKeyword
    {
        None,
        Rapid,        // Can use abilities twice per turn
        Resilient,    // Immune to first lethal damage each game
        Energized,    // Gains +1 extra energy per turn
        Arcane,       // Spells cost 1 less mana while character is alive
        Warlord        // Friendly creatures enter with +1 ATK
    }
}
