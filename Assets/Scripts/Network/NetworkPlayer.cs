using UnityEngine;
using Unity.Netcode;
using TCG.Core;

namespace TCG.Network
{
    /// <summary>
    /// Per-player NetworkBehaviour. Each connected client owns one.
    /// Validates that only the owning client can send action RPCs.
    /// </summary>
    public class NetworkPlayer : NetworkBehaviour
    {
        public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(
            default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<int> PlayerIndex = new NetworkVariable<int>(
            -1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        public override void OnNetworkSpawn()
        {
            if (IsOwner)
                PlayerName.Value = $"Player_{OwnerClientId}";
        }

        // ── Action wrappers called by local PlayerController ──────────────

        public void PlayCard(int cardIndex, int targetIndex = -1)
        {
            if (!IsOwner) return;
            NetworkGameManager.Instance.PlayCardServerRpc(cardIndex, targetIndex);
        }

        public void DeclareAttack(int attackerIndex, int targetIndex, bool targetIsPlayer)
        {
            if (!IsOwner) return;
            NetworkGameManager.Instance.DeclareAttackServerRpc(attackerIndex, targetIndex, targetIsPlayer);
        }

        public void EndTurn()
        {
            if (!IsOwner) return;
            NetworkGameManager.Instance.EndTurnServerRpc();
        }

        public void Surrender()
        {
            if (!IsOwner) return;
            NetworkGameManager.Instance.SurrenderServerRpc();
        }
    }
}
