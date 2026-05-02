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

        // Player stats
        public NetworkVariable<int> Player1Health = new NetworkVariable<int>(30,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Player2Health = new NetworkVariable<int>(30,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Player1Mana = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Player2Mana = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Character energy
        public NetworkVariable<int> Player1Energy = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Player2Energy = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Character health
        public NetworkVariable<int> Player1CharacterHealth = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Player2CharacterHealth = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Ability cooldowns — packed as 3-element arrays (max 3 abilities per character)
        // Encoded as a single int: cooldown0 | (cooldown1 << 8) | (cooldown2 << 16)
        public NetworkVariable<int> Player1AbilityCooldowns = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
        public NetworkVariable<int> Player2AbilityCooldowns = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // Phase / turn
        public NetworkVariable<GamePhase> CurrentPhase = new NetworkVariable<GamePhase>(
            GamePhase.NotStarted, NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);
        public NetworkVariable<int> ActivePlayerIndex = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private Dictionary<ulong, int> _clientPlayerMap = new Dictionary<ulong, int>();

        public override void OnNetworkSpawn()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;

            if (IsServer)
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            GameEvents.OnPlayerDamaged += SyncPlayerHealth;
            GameEvents.OnManaChanged += SyncPlayerMana;
            GameEvents.OnPhaseChanged += SyncPhase;
            GameEvents.OnTurnStarted += SyncActivePlayer;
            GameEvents.OnEnergyChanged += SyncEnergy;
            GameEvents.OnCharacterDamaged += SyncCharacterHealth;
            GameEvents.OnAbilityCooldownTicked += SyncAbilityCooldowns;
        }

        public override void OnNetworkDespawn()
        {
            if (Instance == this) Instance = null;

            GameEvents.OnPlayerDamaged -= SyncPlayerHealth;
            GameEvents.OnManaChanged -= SyncPlayerMana;
            GameEvents.OnPhaseChanged -= SyncPhase;
            GameEvents.OnTurnStarted -= SyncActivePlayer;
            GameEvents.OnEnergyChanged -= SyncEnergy;
            GameEvents.OnCharacterDamaged -= SyncCharacterHealth;
            GameEvents.OnAbilityCooldownTicked -= SyncAbilityCooldowns;

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
            GameManager.Instance.StartGame("Player 1", "Player 2");
            NotifyGameStartedClientRpc();
        }

        [ClientRpc]
        private void NotifyGameStartedClientRpc()
        {
            Debug.Log("[Client] Game has started.");
        }

        // ── Player actions ─────────────────────────────────────────────────

        [ServerRpc(RequireOwnership = false)]
        public void PlayCardServerRpc(int cardIndex, int targetIndex,
            ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            var player = GetPlayer(rpcParams);
            var hand = player.Hand.Cards;
            if (cardIndex < 0 || cardIndex >= hand.Count) return;

            var card = hand[cardIndex];
            TCG.Cards.Card target = null;
            if (targetIndex >= 0)
            {
                var opp = GameManager.Instance.GetOpponent(player);
                var field = opp.Field.Creatures;
                if (targetIndex < field.Count) target = field[targetIndex];
            }

            player.PlayCard(card, target);
            SyncBoardStateClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void DeclareAttackServerRpc(int attackerIndex, int targetIndex, bool targetIsPlayer,
            ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            var player = GetPlayer(rpcParams);
            var opponent = GameManager.Instance.GetOpponent(player);

            var attackers = player.Field.Creatures;
            if (attackerIndex < 0 || attackerIndex >= attackers.Count) return;
            var attacker = attackers[attackerIndex];

            if (targetIsPlayer)
                GameManager.Instance.Battle.ResolvePlayerAttack(attacker, opponent);
            else
            {
                var targets = opponent.Field.Creatures;
                if (targetIndex < 0 || targetIndex >= targets.Count) return;
                GameManager.Instance.Battle.ResolveCombat(attacker, targets[targetIndex]);
            }

            SyncBoardStateClientRpc();
        }

        /// <summary>
        /// Client requests to use their character's ability at abilityIndex.
        /// targetCardIndex = -1 means no creature target; -2 means target is opponent player.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void UseCharacterAbilityServerRpc(int abilityIndex, int targetCardIndex,
            bool targetIsOpponentField, ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            var player = GetPlayer(rpcParams);

            TCG.Cards.Card targetCard = null;
            if (targetCardIndex >= 0)
            {
                var fieldToSearch = targetIsOpponentField
                    ? GameManager.Instance.GetOpponent(player).Field.Creatures
                    : player.Field.Creatures;
                if (targetCardIndex < fieldToSearch.Count)
                    targetCard = fieldToSearch[targetCardIndex];
            }

            player.UseCharacterAbility(abilityIndex, targetCard);
            SyncBoardStateClientRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        public void EndTurnServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            var player = GetPlayer(rpcParams);
            if (GameManager.Instance.ActivePlayer == player)
                GameManager.Instance.Turns.EndCurrentTurn();
        }

        [ServerRpc(RequireOwnership = false)]
        public void SurrenderServerRpc(ServerRpcParams rpcParams = default)
        {
            if (!IsServer) return;
            int idx = GetPlayerIndex(rpcParams.Receive.SenderClientId);
            var result = idx == 0 ? GameResult.Player2Win : GameResult.Player1Win;
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

        private void SyncEnergy(TCG.Characters.CharacterState character, int energy)
        {
            if (!IsServer) return;
            if (character.Owner == GameManager.Instance.Player1)
                Player1Energy.Value = energy;
            else
                Player2Energy.Value = energy;
        }

        private void SyncCharacterHealth(TCG.Characters.CharacterState character, int _)
        {
            if (!IsServer) return;
            if (character.Owner == GameManager.Instance.Player1)
                Player1CharacterHealth.Value = character.CurrentHealth;
            else
                Player2CharacterHealth.Value = character.CurrentHealth;
        }

        private void SyncAbilityCooldowns(TCG.Characters.CharacterState character, int _, int __)
        {
            if (!IsServer) return;
            int packed = PackCooldowns(character);
            if (character.Owner == GameManager.Instance.Player1)
                Player1AbilityCooldowns.Value = packed;
            else
                Player2AbilityCooldowns.Value = packed;
        }

        private static int PackCooldowns(TCG.Characters.CharacterState c)
        {
            int packed = 0;
            for (int i = 0; i < Mathf.Min(c.Abilities.Count, 3); i++)
                packed |= (c.GetCooldownRemaining(i) & 0xFF) << (i * 8);
            return packed;
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
            FindObjectOfType<TCG.UI.GameUI>()?.RefreshBoard();
        }

        // ── Utilities ─────────────────────────────────────────────────────

        private TCG.Player.PlayerState GetPlayer(ServerRpcParams rpcParams)
        {
            int idx = GetPlayerIndex(rpcParams.Receive.SenderClientId);
            return idx == 0 ? GameManager.Instance.Player1 : GameManager.Instance.Player2;
        }

        private int GetPlayerIndex(ulong clientId)
        {
            return _clientPlayerMap.TryGetValue(clientId, out var idx) ? idx : 0;
        }
    }
}
