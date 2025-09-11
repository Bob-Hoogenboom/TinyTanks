using Mirror;
using TMPro;
using UnityEngine;

namespace Menu
{
    public class HostJoinMenu : MonoBehaviour
    {
        private NetworkManager _netManager;
        public TMP_InputField ipInput;
        public TMP_InputField portInput;

        private void Awake()
        {
            _netManager = FindObjectOfType<NetworkManager>();
        }

        public void HostButton()
        {
            //#TODO does the host need to fill in their own IP and port? or should this be done automatic?
            _netManager.StartHost();
        }

        public void JoinButton()
        {
            UpdateNetwork();
            _netManager.StartClient();
        }

        private void UpdateNetwork()
        {
            // set the IP
            _netManager.networkAddress = ipInput.text;

            // set the port if transport supports it
            if (Transport.active is PortTransport portTransport)
            {
                if (ushort.TryParse(portInput.text, out ushort port))
                {
                    portTransport.Port = port;
                }
                else
                {
                    Debug.LogWarning("Invalid port entered. Using default.");
                }
            }
        }
    }
}
