using UnityEngine;
using Unity.Netcode; // Use Mirror if applicable

public class HostActions : NetworkBehaviour
{
    void Update()
    {
        // 1. Check if we are actually pressing the key
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E was pressed!");

            // 2. Check if this specific player object is the Server/Host
            if (IsServer) 
            {
                Debug.Log("I am the Server. Looking for GameFlowManager...");
                
                GameFlowManager manager = Object.FindFirstObjectByType<GameFlowManager>();
                
                if (manager != null)
                {
                    Debug.Log("Manager found! Current State: " + manager.currentState);
                    manager.StartGameSequence();
                }
                else
                {
                    Debug.LogError("CRITICAL: No GameFlowManager exists in the scene!");
                }
            }
            else 
            {
                Debug.Log("E pressed, but I am a Client, not the Server/Host.");
            }
        }
    }
}