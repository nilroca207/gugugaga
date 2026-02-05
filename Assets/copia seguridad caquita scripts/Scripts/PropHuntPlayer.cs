using Unity.Netcode;
using UnityEngine;

public class PropHuntPlayer : NetworkBehaviour
{
    public static PropHuntPlayer LocalPlayer;
    public NetworkVariable<PlayerRole> currentRole = new NetworkVariable<PlayerRole>();

    [Header("Visuals/Skins")]
    [SerializeField] private GameObject hiderSkin; 
    [SerializeField] private GameObject seekerSkin;
    
    [Header("Equipment")]
    [SerializeField] private GameObject weaponPrefab; // Child object with the Shoot script
    [SerializeField] private Transform weaponHoldPoint;

    [Header("Spawn Points (Tags)")]
    [SerializeField] private string prepRoomTag = "PrepRoom";
    [SerializeField] private string gameMapTag = "GameMap";

    public override void OnNetworkSpawn()
    {
        if (IsOwner) LocalPlayer = this;
        
        currentRole.OnValueChanged += HandleRoleChanged;
        UpdateVisuals(currentRole.Value);
    }

    private void HandleRoleChanged(PlayerRole oldRole, PlayerRole newRole)
    {
        UpdateVisuals(newRole);

        if (IsOwner)
        {
            // Hide weapon immediately on role change/reset
            if (weaponPrefab != null) weaponPrefab.SetActive(false);

            if (newRole == PlayerRole.Hider)
            {
                TeleportPlayer(prepRoomTag);
            }
        }
    }

    private void UpdateVisuals(PlayerRole role)
    {
        if(hiderSkin) hiderSkin.SetActive(role == PlayerRole.Hider);
        if(seekerSkin) seekerSkin.SetActive(role == PlayerRole.Seeker);
        
        if (role != PlayerRole.Seeker && weaponPrefab != null)
        {
            weaponPrefab.SetActive(false);
        }
    }

    public void OnHiderReadyToHide()
    {
        if (IsOwner && currentRole.Value == PlayerRole.Hider)
        {
            TeleportPlayer(gameMapTag);
        }
    }

    public void OnSeekerReleased()
    {
        if (IsOwner && currentRole.Value == PlayerRole.Seeker)
        {
            TeleportPlayer(gameMapTag);
            if (weaponPrefab != null) weaponPrefab.SetActive(true);
        }
    }

    private void TeleportPlayer(string tag)
    {
        GameObject target = GameObject.FindWithTag(tag);
        if (target != null)
        {
            var cc = GetComponent<CharacterController>();
            if (cc) cc.enabled = false;
            transform.position = target.transform.position;
            if (cc) cc.enabled = true;
        }
    }
}

public enum PlayerRole { Unassigned, Hider, Seeker }