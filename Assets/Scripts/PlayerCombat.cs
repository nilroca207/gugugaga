using UnityEngine;
using Unity.Netcode; // Change to Mirror if necessary

public class PlayerCombat : NetworkBehaviour
{
    public bool canShoot = false;
    public float fireRate = 1.5f;
    private float nextFire;
    public AudioSource myAudioSource;
    public AudioClip actionClip;

    [Header("Settings")]
    public Vector3 lobbyPosition = new Vector3(0, 5, 0); // Set your actual lobby coordinates

    void Update()
    {
        // Only the local player should control their own shooting
        if (!IsOwner || !canShoot) return;

        if (Input.GetMouseButtonDown(0) && Time.time > nextFire)
        {
            nextFire = Time.time + fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        // Shoot from the camera center for better accuracy
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            if (hit.collider.CompareTag("Player"))
            {
                // We hit someone! Tell the server which NetworkObject we hit
                var targetNetworkObject = hit.collider.GetComponent<NetworkObject>();
                if (targetNetworkObject != null)
                {
                    RequestKillServerRpc(targetNetworkObject.NetworkObjectId);
                }
            }
            else if (hit.collider.CompareTag("NPC"))
            {
                RevealPlayerLocation();
            }
        }
    }

    [ServerRpc]
    void RequestKillServerRpc(ulong targetId)
    {
        // The Server finds the victim based on their ID
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out NetworkObject victim))
        {
            // 1. Move the victim back to the lobby
            victim.transform.position = lobbyPosition;

            // 2. Disable their combat so they can't shoot from the lobby
            var victimCombat = victim.GetComponent<PlayerCombat>();
            if (victimCombat != null) victimCombat.canShoot = false;

            // 3. Tell the Game Manager someone was eliminated to check for a winner
            GameFlowManager manager = Object.FindFirstObjectByType<GameFlowManager>();
            if (manager != null)
            {
                manager.PlayerEliminated(victim.gameObject);
            }
        }
    }

    void RevealPlayerLocation()
    {
        // Logic for missing goes here (e.g., play a loud sound or show a red icon)
        myAudioSource.PlayOneShot(actionClip);
    }
}