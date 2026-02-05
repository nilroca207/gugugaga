using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

public class PropHuntManager : NetworkBehaviour
{
    public static PropHuntManager Instance;
    public enum GameState { Waiting, Playing, Finished }
    public NetworkVariable<GameState> currentState = new NetworkVariable<GameState>(GameState.Waiting);
    public NetworkVariable<float> timer = new NetworkVariable<float>();
    public NetworkVariable<FixedString128Bytes> winMessage = new NetworkVariable<FixedString128Bytes>("");

    public Transform lobbySpawnPoint;

    private void Awake() => Instance = this;

    void Update()
    {
        if (!IsServer) return;
        // Restart logic: If not playing, press E to reset
        if (currentState.Value != GameState.Playing && Input.GetKeyDown(KeyCode.E))
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        StopAllCoroutines();
        currentState.Value = GameState.Playing;
        StartCoroutine(GameFlowRoutine());
    }

    private IEnumerator GameFlowRoutine()
    {
        AssignRoles();
        BalanceAmmo();
        winMessage.Value = "Preparing...";
        yield return StartTimer(15f);
        MoveHidersToMapClientRpc();
        yield return StartTimer(10f);
        ReleaseSeekerClientRpc();
    }

    private void BalanceAmmo()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            // We look for the script. If this client is a Hider, shooter will be null.
            var shooter = client.PlayerObject.GetComponentInChildren<Shoot>(true);
        
            if (shooter != null) 
            {
                // .Value is required because it's now a NetworkVariable
                shooter.bullets.Value = 10; 
                Debug.Log($"Refilled ammo for Seeker: {client.ClientId}");
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EliminatePlayerServerRpc(ulong networkObjectId)
    {
        TeleportPlayerClientRpc(networkObjectId, lobbySpawnPoint.position);
    }

    public void EndGame(string message)
    {
        if (!IsServer) return;
        currentState.Value = GameState.Finished;
        winMessage.Value = message;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            TeleportPlayerClientRpc(client.PlayerObject.NetworkObjectId, lobbySpawnPoint.position);
    }

    private IEnumerator StartTimer(float duration)
    {
        float t = duration;
        while (t > 0) {
            if (currentState.Value != GameState.Playing) yield break;
            t -= Time.deltaTime;
            timer.Value = t;
            yield return null;
        }
    }

    private void AssignRoles()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsList;
        int seekerIndex = Random.Range(0, clients.Count);
        for (int i = 0; i < clients.Count; i++)
        {
            var p = clients[i].PlayerObject.GetComponent<PropHuntPlayer>();
            if (p) p.currentRole.Value = (i == seekerIndex) ? PlayerRole.Seeker : PlayerRole.Hider;
        }
    }

    [ClientRpc]
    private void TeleportPlayerClientRpc(ulong netId, Vector3 pos)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(netId, out var netObj))
        {
            var cc = netObj.GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            netObj.transform.position = pos;
            if (cc) cc.enabled = true;
        }
    }

    [ClientRpc] private void MoveHidersToMapClientRpc() => PropHuntPlayer.LocalPlayer?.OnHiderReadyToHide();
    [ClientRpc] private void ReleaseSeekerClientRpc() => PropHuntPlayer.LocalPlayer?.OnSeekerReleased();
}