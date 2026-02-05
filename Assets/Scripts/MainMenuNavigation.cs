using UnityEngine;
using Unity.Netcode;

public class MainMenuNavigation : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;    // Your Host/Join screen
    public GameObject inGameMenuPanel;  // The panel that shows up when you press Tab/Esc
    public GameObject disconnectPopUp;  // "Host has left" message

    private void Update()
    {
        // Toggle the "Leave" menu with Tab or Escape
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            // Only allow if we are actually connected (Server or Client)
            if (NetworkManager.Singleton.IsListening)
            {
                ToggleInGameMenu();
            }
        }
    }

    public void ToggleInGameMenu()
    {
        bool isNowActive = !inGameMenuPanel.activeSelf;
        inGameMenuPanel.SetActive(isNowActive);

        // Control cursor so you can actually click the "Leave" button
        Cursor.lockState = isNowActive ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = isNowActive;
    }

    // THIS IS THE BIG RELAY DISCONNECT
    public void LeaveRelayLobby()
    {
        if (NetworkManager.Singleton != null)
        {
            // 1. Tell the Relay server we are leaving
            // 2. Stop the local NetworkManager
            // 3. This triggers OnClientDisconnectCallback for others if we are the Host
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Relay Connection Shut Down.");
        }

        ResetUIToMain();
    }

    public void ResetUIToMain()
    {
        // Close all in-game overlays
        inGameMenuPanel.SetActive(false);
        if (disconnectPopUp != null) disconnectPopUp.SetActive(false);

        // Show the initial Host/Join screen
        mainMenuPanel.SetActive(true);

        // Reset Cursor for the menu
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            // Set up the listener for when the connection is lost
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleRelayDisconnect;
        }
    }

    private void HandleRelayDisconnect(ulong clientId)
    {
        // If we aren't the one who initiated the shutdown, 
        // it means the Host closed the Relay or we lost connection.
        if (!NetworkManager.Singleton.IsServer && NetworkManager.Singleton.LocalClientId == clientId)
        {
            if (disconnectPopUp != null) disconnectPopUp.SetActive(true);
            ResetUIToMain();
        }
    }
}