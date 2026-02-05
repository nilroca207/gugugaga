using UnityEngine;
using TMPro; // Ensure you have TextMeshPro installed

public class GameUIManager : MonoBehaviour
{
    public GameObject winPanel;
    public TextMeshProUGUI statusText;

    public void ShowEndScreen(string message)
    {
        winPanel.SetActive(true);
        statusText.text = message;
        // Hide the screen after 5 seconds
        Invoke("HideScreen", 5f);
    }

    void HideScreen()
    {
        winPanel.SetActive(false);
    }
}