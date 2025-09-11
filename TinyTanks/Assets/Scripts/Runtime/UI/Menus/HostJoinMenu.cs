using Mirror;
using UnityEngine;
using UnityEngine.UI;

namespace Menu
{
    public class HostJoinMenu : MonoBehaviour
    {
        private NetworkManager _netManager;
        public InputField ipInput;

        private void Awake()
        {
            _netManager = GetComponent<NetworkManager>();
        }

        public void HostButton()
        {
            _netManager.StartHost();
        }

        public void JoinButton()
        {

        }
    }
}
