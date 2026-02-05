using UnityEngine;
using Unity.Netcode;

public class NetworkStartUI : MonoBehaviour
{
    void OnGUI()
    {
        // 1. Check if the Singleton is null first
        if (NetworkManager.Singleton == null)
        {
            Debug.LogWarning("NetworkManager not found in scene!");
            return;
        }

        float w = 200f, h = 40f;
        float x = 10f, y = 10f;

        // 2. Now it's safe to check IsClient and IsServer
        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUI.Button(new Rect(x, y, w, h), "Host")) 
                NetworkManager.Singleton.StartHost();

            if (GUI.Button(new Rect(x, y + h + 10, w, h), "Client")) 
                NetworkManager.Singleton.StartClient();

            if (GUI.Button(new Rect(x, y + 2 * (h + 10), w, h), "Server")) 
                NetworkManager.Singleton.StartServer();
        }
    }
}