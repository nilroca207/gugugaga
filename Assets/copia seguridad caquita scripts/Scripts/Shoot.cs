using Unity.Netcode;
using UnityEngine;

public class Shoot : NetworkBehaviour 
{
    public float range = 100f;
    public Camera fpsCam;
    
    // Change 'int' to 'NetworkVariable' so the Manager can see/set it
    public NetworkVariable<int> bullets = new NetworkVariable<int>(10);

    void Update()
    {
        // Only the player who physically owns this Seeker object can click to fire
        if (!IsOwner) return;

        if (Input.GetButtonDown("Fire1") && bullets.Value > 0)
        {
            ShootServerRpc(); // Tell the server we want to fire
        }
    }

    [ServerRpc]
    void ShootServerRpc()
    {
        // Server-side check: prevent firing if empty
        if (bullets.Value <= 0) return;
        
        bullets.Value--;

        RaycastHit hit;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, range))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // Find the NetworkObject of the Hider we hit
                var targetNetObj = hit.collider.GetComponent<NetworkObject>();
                if (targetNetObj != null)
                {
                    // Call the elimination logic you already have in your Manager
                    PropHuntManager.Instance.EliminatePlayerServerRpc(targetNetObj.NetworkObjectId);
                    
                    // Optional: Reward seeker with a bullet for a successful hit
                    bullets.Value++; 
                }
            }
        }
    }
}