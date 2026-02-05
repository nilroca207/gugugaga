using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement; // Add this line!

public class GameFlowManager : NetworkBehaviour
{
    public enum GameState { Lobby, PreRound, Playing, GameOver }

    [Header("Networking State")]
    // Using NetworkVariables so the Client (friend) stays synced with the Host
    public NetworkVariable<GameState> currentState = new NetworkVariable<GameState>(
        GameState.Lobby, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public NetworkVariable<int> countdownTimer = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    [Header("Spawn Settings")]
    public Transform[] personalRooms;
    public Transform[] mapSpawnPoints;
    public Vector3 lobbyPosition;

    public List<NetworkObject> activePlayers = new List<NetworkObject>();

    public void StartGameSequence()
    {
        if (!IsServer) return; 
        if (currentState.Value != GameState.Lobby && currentState.Value != GameState.GameOver) return;

        activePlayers.Clear();

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null)
            {
                activePlayers.Add(client.PlayerObject);
            }
        }

        if (activePlayers.Count > 0)
        {
            StartCoroutine(GameLoop());
        }
    }

    private IEnumerator GameLoop()
    {
        currentState.Value = GameState.PreRound;
        TeleportAllPlayers(personalRooms);

        int remainingTime = 20;
        while (remainingTime > 0)
        {
            countdownTimer.Value = remainingTime;
            yield return new WaitForSeconds(1f);
            remainingTime--;
        }
        countdownTimer.Value = 0;

        currentState.Value = GameState.Playing;
        TeleportAllPlayers(mapSpawnPoints);

        foreach (NetworkObject playerObj in activePlayers)
        {
            if (playerObj.TryGetComponent(out PlayerCombat combat))
            {
                EnableCombatClientRpc(playerObj.NetworkObjectId);
            }
        }
    }

    private void TeleportAllPlayers(Transform[] destinationList)
    {
        for (int i = 0; i < activePlayers.Count; i++)
        {
            NetworkObject playerNetObj = activePlayers[i];
            Transform target = destinationList[i % destinationList.Length];
            TeleportPlayerClientRpc(playerNetObj.NetworkObjectId, target.position, target.rotation);
        }
    }

    [ClientRpc]
    void TeleportPlayerClientRpc(ulong networkObjectId, Vector3 pos, Quaternion rot)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            if (netObj.TryGetComponent(out CharacterController cc)) cc.enabled = false;
            netObj.transform.position = pos;
            netObj.transform.rotation = rot;
            if (cc != null) cc.enabled = true;
        }
    }

    [ClientRpc]
    void EnableCombatClientRpc(ulong networkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(networkObjectId, out NetworkObject netObj))
        {
            if (netObj.TryGetComponent(out PlayerCombat combat)) combat.canShoot = true;
        }
    }
    public void HostShutdown()
    {
        if (IsServer)
        {
            // This forces all clients to disconnect and triggers their popup
            NetworkManager.Singleton.Shutdown();
            SceneManager.LoadScene("MainMenu");
        }
    }
    public void PlayerEliminated(GameObject victim)
    {
        if (!IsServer) return;
        
        NetworkObject victimNetObj = victim.GetComponent<NetworkObject>();
        activePlayers.Remove(victimNetObj);
        TeleportPlayerClientRpc(victimNetObj.NetworkObjectId, lobbyPosition, Quaternion.identity);

        if (activePlayers.Count == 1 && currentState.Value == GameState.Playing)
        {
            DeclareWinner(activePlayers[0].gameObject);
        }
    }

    void DeclareWinner(GameObject winner)
    {
        currentState.Value = GameState.GameOver;
        NetworkObject winnerNetObj = winner.GetComponent<NetworkObject>();
        TeleportPlayerClientRpc(winnerNetObj.NetworkObjectId, lobbyPosition, Quaternion.identity);

        if (winner.TryGetComponent(out PlayerCombat combat)) combat.canShoot = false;
    }
}