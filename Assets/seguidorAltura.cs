using UnityEngine;

public class seguidorAltura : MonoBehaviour
{
    public Transform Player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.position = new Vector3(Player.position.x, Player.position.y, Player.position.z);
    }
}
