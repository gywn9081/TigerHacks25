using UnityEngine;
using Unity.Netcode;

public class NetworkSetup1 : MonoBehaviour
{
    private void OnGUI()
    {
        if (GUILayout.Button("Host"))
        {
            NetworkManager.Singleton.StartHost();
        }
        if (GUILayout.Button("Client"))
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}

