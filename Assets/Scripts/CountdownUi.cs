using UnityEngine;
using TMPro;
using Unity.Netcode;

public class CountdownUI : NetworkBehaviour
{
    public TextMeshProUGUI timerText;
    private GameFlowManager gameManager;

    public override void OnNetworkSpawn()
    {
        // Initial search for the manager
        gameManager = Object.FindFirstObjectByType<GameFlowManager>();
    }

    void Update()
    {
        // If the manager hasn't spawned yet, keep trying to find it
        if (gameManager == null) 
        {
            gameManager = Object.FindFirstObjectByType<GameFlowManager>();
            return;
        }

        // Read the synced values from the GameFlowManager
        int currentTime = gameManager.countdownTimer.Value;
        GameFlowManager.GameState state = gameManager.currentState.Value;

        // UI Logic based on the synced state
        if (state == GameFlowManager.GameState.PreRound && currentTime > 0)
        {
            timerText.enabled = true;
            timerText.text = "MATCH STARTING IN: " + currentTime;
        }
        else if (state == GameFlowManager.GameState.GameOver)
        {
            timerText.enabled = true;
            timerText.text = "MATCH OVER!";
        }
        else
        {
            timerText.enabled = false;
        }
    }
}