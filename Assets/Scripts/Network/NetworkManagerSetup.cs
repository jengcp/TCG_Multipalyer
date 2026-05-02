using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace TCG.Network
{
    /// <summary>
    /// Simple lobby entry point. Attach to a NetworkManager GameObject.
    /// Call StartHost() for the server player and StartClient() for the joining player.
    /// </summary>
    public class NetworkManagerSetup : MonoBehaviour
    {
        [Header("Transport")]
        public string serverAddress = "127.0.0.1";
        public ushort port = 7777;

        private void Awake()
        {
            var transport = GetComponent<UnityTransport>();
            if (transport != null)
                transport.SetConnectionData(serverAddress, port);
        }

        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }

        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }

        public void Disconnect()
        {
            NetworkManager.Singleton.Shutdown();
        }
    }
}
