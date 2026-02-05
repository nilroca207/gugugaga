using UnityEngine;
using Unity.Netcode;

public class gun : MonoBehaviour
{
    public float rayDistance = 100f;
    public LayerMask layerMask;
    public int playerCount;
    public int deadCount;
    public Transform targetSpawnPoint;
    void Start()
    {
        deadCount=0;

    }
    // Update is called once per frame
    void Update()
    {
        int playerCount = NetworkManager.Singleton.ConnectedClients.Count;
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
        if (deadCount == playerCount -1)
        {
            TeleportClientsClientRpc(targetSpawnPoint.position, targetSpawnPoint.rotation);
        }

    }
    

    [ClientRpc]
    private void TeleportClientsClientRpc(Vector3 newPos, Quaternion newRot)
    {
        // Get the local player object for this specific client
        GameObject localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject.gameObject;

        if (localPlayer != null)
        {
            // 1. MUST disable CharacterController first! 
            // If you don't, it will "fight" the teleport and snap you back.
            CharacterController cc = localPlayer.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            // 2. Move the player
            localPlayer.transform.position = newPos;
            localPlayer.transform.rotation = newRot;

            // 3. Re-enable the controller
            if (cc != null) cc.enabled = true;
            
            Debug.Log("Teleport complete for: " + localPlayer.name);
        }
    }
    void Shoot()
    {
        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out hit, rayDistance, layerMask))
        {
            if (hit.collider.CompareTag("Player"))
            {
                Debug.Log("epstein");
                hit.transform.position = new Vector3(0,0,0);
                deadCount+=1;
                //check if all hiders dead
                // If all players dead,
            }
            else if(hit.collider.CompareTag("NPC")){
                Debug.Log("WOMP WOMP");
                Destroy(hit.collider.gameObject);
            }
        }
    }
    
}
