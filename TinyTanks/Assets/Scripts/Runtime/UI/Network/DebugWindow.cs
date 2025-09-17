using Mirror;
using UnityEngine;

/// <summary>
/// Paste this with the NetworkManager in one object and 
/// when you press the F1 Key you toggle this window only when connected
/// if you are not connected you will get a warning text
/// </summary>
[RequireComponent(typeof(NetworkManager))]
public class DebugWindow : MonoBehaviour
{
    public int offsetX;
    public int offsetY;

    private NetworkManager _netManager;
    private bool _showStatus = false;
    private bool _showWarning;
    private float _warningHideTime;


    private void Awake()
    {
        _netManager = FindObjectOfType<NetworkManager>();
    }

    private void OnGUI()
    {
        int width = 300;
        GUILayout.BeginArea(new Rect(10 + offsetX, 40 + offsetY, width, 9999));

        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.F1)
        {
            if (!NetworkClient.isConnected && !NetworkServer.active)
            {
                _showWarning = true;
                _warningHideTime = Time.time + 5f;
            }
            else
            {
                _showStatus = !_showStatus;
            }
        }

        //timer for the warning text
        if (_showWarning && Time.time > _warningHideTime)
        {
            _showWarning = false;
        }

        if (_showStatus)
        {
            StatusScreen();
        }

        if (_showWarning)
        {
            StatusWarning();
        }

        GUILayout.EndArea();
    }


    private void StatusWarning()
    {
        GUILayout.Label("<color=yellow>Not connected to server</color>");
    }

    private void StatusScreen()
    {
        // host mode
        // display separately because this always confused people:
        //   Server: ...
        //   Client: ...
        if (NetworkServer.active && NetworkClient.active)
        {
            // host mode
            GUILayout.Label($"<b>Host</b>: running via {Transport.active}");
        }
        else if (NetworkServer.active)
        {
            // server only
            GUILayout.Label($"<b>Server</b>: running via {Transport.active}");
        }
        else if (NetworkClient.isConnected)
        {
            // client only
            GUILayout.Label($"<b>Client</b>: connected to {_netManager.networkAddress} via {Transport.active}");
        }
    }
}
