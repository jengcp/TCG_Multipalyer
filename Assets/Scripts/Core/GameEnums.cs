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
        Graveyard,
        Exile
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
}
