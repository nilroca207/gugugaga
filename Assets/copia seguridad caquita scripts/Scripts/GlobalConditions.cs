using UnityEngine;

public class GlobalConditions : MonoBehaviour
{
    public bool SeekerWon;
    public bool HidersWon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(SeekerWon){
            Debug.Log("Tge seeker has won!");
            SeekerWon = false;
        }
        else if(HidersWon){
            Debug.Log("Hiders have won!");
            HidersWon = false;
        }
    }
}
