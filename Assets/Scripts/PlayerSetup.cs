using Unity.Netcode;
using UnityEngine;

public class PlayerSetup : NetworkBehaviour
{
    [SerializeField] private GameObject playerCamera; // Drag your camera child here
    [SerializeField] private AudioListener audioListener;

    public override void OnNetworkSpawn()
    {
        // If this is NOT the player controlled by this specific computer...
        if (!IsOwner)
        {
            // Disable the camera and listener so they don't interfere
            playerCamera.SetActive(false);
            if (audioListener != null) audioListener.enabled = false;
        }
    }
}