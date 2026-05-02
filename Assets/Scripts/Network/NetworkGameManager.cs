using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using TCG.Core;

namespace TCG.Network
{
    /// <summary>
    /// Synchronises game state across the network using Unity Netcode for GameObjects.
    /// The server is authoritative; clients send requests via ServerRpc.
    /// </summary>
    public class NetworkGameManager : NetworkBehaviour
    {
        public static NetworkGameManager Instance { get; private set; }

        // NetworkVariables for UI binding
        public NetworkVariable<int> Player1Health = new NetworkVariable<int>(30,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Player2Health = new NetworkVariable<int>(30,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Player1Mana = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Player2Mana = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<GamePhase> CurrentPhase = new NetworkVariable<GamePhase>(
            GamePhase.NotStarted, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        public NetworkVariable<int> ActivePlayerIndex = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Maps NetworkObject client IDs to player indices (0 or 1)
        private Dictionary<ulong, int> _clientPlayerMap = new Dictionary<ulong, int>();

        public override void OnNetworkSpawn()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            if (IsServer)
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            // Mirror server-side game events to NetworkVariables
            GameEvents.OnPlayerDamaged += SyncPlayerHealth;
            GameEvents.OnManaChanged += SyncPlayerMana;
            GameEvents.OnPhaseChanged += SyncPhase;
            GameEvents.OnTurnStarted += SyncActivePlayer;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;

            GameEvents.OnPlayerDamaged -= SyncPlayerHealth;
            GameEvents.OnManaChanged -= SyncPlayerMana;
            GameEvents.OnPhaseChanged -= SyncPhase;
            GameEvents.OnTurnStarted -= SyncActivePlayer;

            if (IsServer)
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        // ── Connection ────────────────────────────────────────────────────

        private void OnClientConnected(ulong clientId)
        {
            if (_clientPlayerMap.Count == 0)
                _clientPlayerMap[clientId] = 0;
            else if (_clientPlayerMap.Count == 1)
            {
                _clientPlayerMap[clientId] = 1;
                StartGameServerRpc();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        private void StartGameServerRpc()
        {
            // Server starts the game — GameManager.StartGame is authoritative
            GameManager.Instance.StartGame("Player 1", "Player 2");
            NotifyGameStartedClientRpc();
        }

        [ClientRpc]
        private void NotifyGameStartedClientRpc()
        {
            Debug.Log("[Client] Game has started.");
        }

        // ── Player actions → Server ────────────────────────────────────────

        [ServerRpc(RequireOwnership = false)]
        public void PlayCardServerRpc(int cardIndex, int targetIndex, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            int playerIdx = GetPlayerIndex(rpcParams.Receive.SenderClientId);
            var player = playerIdx == 0 ? GameManager.Instance.Player1 : GameManager.Instance.Player2;

            var hand = player.Hand.Cards;
            if (cardIndex < 0 || cardIndex >= hand.Count) return;

            var card = hand[cardIndex];
            TCG.Cards.Card target = null;

            if (targetIndex >= 0)
            {
                var opponent = GameManager.Instance.GetOpponent(player);
                var field = opponent.Field.Creatures;
                if (targetIndex < field.Count) target = field[targetIndex];
            }

            player.PlayCard(card, target);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeclareAttackServerRpc(int attackerIndex, int targetIndex, bool targetIsPlayer,
            ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            int playerIdx = GetPlayerIndex(rpcParams.Receive.SenderClientId);
            var player = playerIdx == 0 ? GameManager.Instance.Player1 : GameManager.Instance.Player2;
            var opponent = GameManager.Instance.GetOpponent(player);

            var attackers = player.Field.Creatures;
            if (attackerIndex < 0 || attackerIndex >= attackers.Count) return;
            var attacker = attackers[attackerIndex];

            if (targetIsPlayer)
            {
                GameManager.Instance.Battle.ResolvePlayerAttack(attacker, opponent);
            }
            else
            {
                var targets = opponent.Field.Creatures;
                if (targetIndex < 0 || targetIndex >= targets.Count) return;
                GameManager.Instance.Battle.ResolveCombat(attacker, targets[targetIndex]);
            }

            SyncBoardStateClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void EndTurnServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            int playerIdx = GetPlayerIndex(rpcParams.Receive.SenderClientId);
            var player = playerIdx == 0 ? GameManager.Instance.Player1 : GameManager.Instance.Player2;

            if (GameManager.Instance.ActivePlayer == player)
                GameManager.Instance.Turns.EndCurrentTurn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SurrenderServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            int playerIdx = GetPlayerIndex(rpcParams.Receive.SenderClientId);
            var result = playerIdx == 0 ? GameResult.Player2Win : GameResult.Player1Win;
            GameManager.Instance.DeclareResult(result);
        }

        // ── Sync helpers ──────────────────────────────────────────────────

        private void SyncPlayerHealth(TCG.Player.PlayerState player, int _)
        {
            if (!IsServer) return;
            if (player == GameManager.Instance.Player1)
                Player1Health.Value = player.Health;
            else
                Player2Health.Value = player.Health;
        }

        private void SyncPlayerMana(TCG.Player.PlayerState player, int mana)
        {
            if (!IsServer) return;
            if (player == GameManager.Instance.Player1)
                Player1Mana.Value = mana;
            else
                Player2Mana.Value = mana;
        }

        private void SyncPhase(GamePhase phase)
        {
            if (!IsServer) return;
            CurrentPhase.Value = phase;
        }

        private void SyncActivePlayer(TCG.Player.PlayerState player)
        {
            if (!IsServer) return;
            ActivePlayerIndex.Value = player == GameManager.Instance.Player1 ? 0 : 1;
        }

        [ClientRpc]
        private void SyncBoardStateClientRpc()
        {
            // Clients re-render their UI from NetworkVariables + local queries
            FindObjectOfType<TCG.UI.GameUI>()?.RefreshBoard();
        }

        private int GetPlayerIndex(ulong clientId)
        {
            return _clientPlayerMap.TryGetValue(clientId, out var idx) ? idx : 0;
        }
    }
}
