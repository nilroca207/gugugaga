using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Netcode; // Added for Shutdown

public class MainMenuUI : MonoBehaviour
{
    public RelayManager relayManager;

    [Header("UI Panels")]
    public GameObject startMenuPanel;  // Host/Join screen
    public GameObject gameInfoPanel;   // Join Code text
    public GameObject inGameMenuPanel; // The Tab/Esc "Leave" screen
    public GameObject disconnectPopUp; // "Host Left" popup

    [Header("UI Elements")]
    public TMP_InputField joinInputField;
    public TextMeshProUGUI codeDisplayText;

    void Start()
    {
        // Set initial state
        ResetUIToMain();
        
        // Listen for disconnects (e.g., Host leaves)
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleDisconnect;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
        {
            Debug.Log("Tab/Esc Pressed!"); // If this doesn't show up, the script isn't on an active object

        // Simplified check: If the start menu is hidden, we must be in a game!
            if (!startMenuPanel.activeSelf)
            {
                ToggleInGameMenu();
            }
            else
            {
                Debug.Log("Still in Start Menu - Tab Menu disabled.");
            }
        }
    }

    public void ToggleInGameMenu()
    {
        if (inGameMenuPanel == null) return;

        bool isNowActive = !inGameMenuPanel.activeSelf;
        inGameMenuPanel.SetActive(isNowActive);

        if (isNowActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
        // If you have a first-person game, lock it. Otherwise, keep it visible.
        Cursor.lockState = CursorLockMode.Locked; 
        Cursor.visible = false;
        }
    }

    public async void OnHostClicked()
    {
        string code = await relayManager.StartRelay();
        
        if (codeDisplayText != null)
            codeDisplayText.text = "JOIN CODE: " + code;
        
        startMenuPanel.SetActive(false);
        gameInfoPanel.SetActive(true);
    }

    public async void OnJoinClicked()
    {
        string codeToJoin = joinInputField.text;

        if (!string.IsNullOrEmpty(codeToJoin))
        {
            await relayManager.JoinRelay(codeToJoin);
            
            if (codeDisplayText != null)
                codeDisplayText.text = "CONNECTED TO HOST";

            startMenuPanel.SetActive(false);
            gameInfoPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Please enter a join code first!");
        }
    }

    // THIS IS THE BUTTON FUNCTION
    public void LeaveRelayLobby()
    {
        Debug.Log("Shutting down Relay...");
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
        ResetUIToMain();
    }

    public void ResetUIToMain()
    {
        // Close game UI
        if (inGameMenuPanel != null) inGameMenuPanel.SetActive(false);
        if (gameInfoPanel != null) gameInfoPanel.SetActive(false);
        if (disconnectPopUp != null) disconnectPopUp.SetActive(false);

        // Show start menu
        if (startMenuPanel != null) startMenuPanel.SetActive(true);

        // Fix Cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleDisconnect(ulong clientId)
    {
        // If we are a client and the host shuts down the relay
        if (NetworkManager.Singleton != null && !NetworkManager.Singleton.IsServer)
        {
            if (disconnectPopUp != null) disconnectPopUp.SetActive(true);
            ResetUIToMain();
        }
    }
}