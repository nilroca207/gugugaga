using UnityEngine;
using TMPro; 
using Unity.Netcode;

public class PropHuntUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI statusText;
    private PropHuntManager gameManager;

    void Update()
    {
        if (gameManager == null)
        {
            gameManager = Object.FindAnyObjectByType<PropHuntManager>();
            return;
        }

        var state = gameManager.currentState.Value;

        if (state == PropHuntManager.GameState.Waiting)
        {
            timerText.text = "--";
            statusText.text = NetworkManager.Singleton.IsServer ? "HOST: PRESS E TO START" : "WAITING FOR HOST...";
            statusText.color = Color.white;
        }
        else if (state == PropHuntManager.GameState.Finished)
        {
            statusText.text = gameManager.winMessage.Value.ToString();
            statusText.color = Color.yellow;
        }
        else // Playing
        {
            float currentTime = gameManager.timer.Value;
            timerText.text = currentTime > 0 ? Mathf.CeilToInt(currentTime).ToString() : "";

            var localPlayer = PropHuntPlayer.LocalPlayer;
            if (localPlayer == null) return;

            if (localPlayer.currentRole.Value == PlayerRole.Hider)
            {
                statusText.text = currentTime > 0 ? "HIDE!" : "SURVIVE!";
                statusText.color = Color.green;
            }
            else
            {
                statusText.text = currentTime > 0 ? "WAITING..." : "HUNT!";
                statusText.color = Color.red;
            }
        }
    }
}