using UnityEngine;
using System.Collections.Generic;
using TCG.Cards;
using TCG.Characters;
using TCG.Core;
using TCG.Game;
using TCG.Player;

/// <summary>
/// Central singleton that owns all top-level game systems.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Deck Config")]
    public List<CardData> player1DeckConfig = new List<CardData>();
    public List<CardData> player2DeckConfig = new List<CardData>();

    [Header("Character Config (optional)")]
    public CharacterData player1CharacterData;
    public CharacterData player2CharacterData;

    [Header("Prefabs")]
    public TCG.Cards.Card cardPrefab;
    public TCG.Cards.Card CardPrefab => cardPrefab;

    // Systems
    public TurnManager Turns { get; private set; }
    public CombatManager Combat { get; private set; }
    public BattleManager Battle { get; private set; }

    // Players
    public PlayerState Player1 { get; private set; }
    public PlayerState Player2 { get; private set; }

    public GamePhase CurrentPhase => Turns?.CurrentPhase ?? GamePhase.NotStarted;
    public PlayerState ActivePlayer => Turns?.ActivePlayer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => StartGame("Player1", "Player2");

    private void OnDestroy()
    {
        if (Instance == this)
        {
            GameEvents.ClearAll();
            Instance = null;
        }
    }

    // ── Game lifecycle ─────────────────────────────────────────────────────

    public void StartGame(string p1Name, string p2Name)
    {
        StatusEffectProcessor.Reset();

        Player1 = new PlayerState("p1", p1Name, isLocal: true, isPlayer1: true);
        Player2 = new PlayerState("p2", p2Name, isLocal: false, isPlayer1: false);

        InitCharacter(Player1, player1CharacterData);
        InitCharacter(Player2, player2CharacterData);

        BuildDeck(Player1, player1DeckConfig);
        BuildDeck(Player2, player2DeckConfig);

        Player1.Deck.Shuffle();
        Player2.Deck.Shuffle();

        Player1.DrawStartingHand();
        Player2.DrawStartingHand();

        Combat = new CombatManager();
        Battle = new BattleManager();
        Turns = new TurnManager();
        Turns.Initialize(Player1, Player2, Combat);

        GameEvents.OnPlayerDamaged += CheckWinCondition;
        GameEvents.OnGameEnded += OnGameEnded;

        Turns.StartFirstTurn();
    }

    private void InitCharacter(PlayerState player, CharacterData data)
    {
        if (data == null) return;
        player.AssignCharacter(new CharacterState(data, player));
    }

    private void BuildDeck(PlayerState player, List<CardData> config)
    {
        foreach (var data in config)
        {
            var cardGO = Instantiate(cardPrefab, transform);
            cardGO.Initialize(data, player);
            player.Deck.AddCard(cardGO);
        }
    }

    // ── Win condition ──────────────────────────────────────────────────────

    private void CheckWinCondition(PlayerState damaged, int _)
    {
        if (!Player1.IsAlive) DeclareResult(GameResult.Player2Win);
        else if (!Player2.IsAlive) DeclareResult(GameResult.Player1Win);
    }

    public void DeclareResult(GameResult result)
    {
        Turns.SetGameOver();
        GameEvents.GameEnded(result);
    }

    private void OnGameEnded(GameResult result)
    {
        GameEvents.OnPlayerDamaged -= CheckWinCondition;
        string winner = result switch
        {
            GameResult.Player1Win => Player1.DisplayName,
            GameResult.Player2Win => Player2.DisplayName,
            _ => "Nobody"
        };
        Debug.Log($"Game Over! Winner: {winner}");
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    public PlayerState GetOpponent(PlayerState player) =>
        player == Player1 ? Player2 : Player1;
}
