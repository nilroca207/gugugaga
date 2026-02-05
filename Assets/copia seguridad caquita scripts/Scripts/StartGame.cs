using Unity.Netcode;
using UnityEngine;

public class PropTransform : NetworkBehaviour
{
    [Header("Settings")]
    [SerializeField] private float reachDistance = 5f;
    [SerializeField] private KeyCode transformKey = KeyCode.E;
    [SerializeField] private LayerMask propLayer; // Set this to a "Prop" layer in Unity

    [Header("References")]
    [SerializeField] private MeshFilter playerMeshFilter;
    [SerializeField] private MeshRenderer playerMeshRenderer;

    void Update()
{
    if (!IsOwner) return;

    // 1. Try to get the component
    var playerScript = GetComponent<PropHuntPlayer>();

    // 2. If it's still null, try looking in the parent (common if this is on a child object)
    if (playerScript == null) {
        playerScript = GetComponentInParent<PropHuntPlayer>();
    }

    // 3. Null check: If we still can't find it, stop here so we don't crash
    if (playerScript == null) {
        Debug.LogWarning("PropTransform cannot find PropHuntPlayer on this object or its parents!");
        return; 
    }

    // 4. Now it's safe to check the role
    if (playerScript.currentRole.Value != PlayerRole.Hider) return;

    if (Input.GetKeyDown(transformKey))
    {
        TryTransform();
    }
}

    private void TryTransform()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, reachDistance, propLayer))
        {
            MeshFilter targetMesh = hit.collider.GetComponent<MeshFilter>();
            MeshRenderer targetRenderer = hit.collider.GetComponent<MeshRenderer>();

            if (targetMesh != null && targetRenderer != null)
            {
                // Tell the server to change our appearance for everyone
                RequestTransformServerRpc(hit.collider.gameObject.name); 
                
                // Update locally for instant feedback
                UpdateVisuals(targetMesh.sharedMesh, targetRenderer.sharedMaterials);
            }
        }
    }

    [ServerRpc]
    private void RequestTransformServerRpc(string objectName)
    {
        // In a real game, you'd find the mesh by ID or Name
        // For now, we'll use a ClientRpc to update everyone's view
        UpdateVisualsClientRpc(objectName);
    }

    [ClientRpc]
    private void UpdateVisualsClientRpc(string objectName)
    {
        if (IsOwner) return; // Already updated locally

        GameObject target = GameObject.Find(objectName);
        if (target != null)
        {
            MeshFilter mf = target.GetComponent<MeshFilter>();
            MeshRenderer mr = target.GetComponent<MeshRenderer>();
            UpdateVisuals(mf.sharedMesh, mr.sharedMaterials);
        }
    }

    private void UpdateVisuals(Mesh mesh, Material[] materials)
    {
        playerMeshFilter.mesh = mesh;
        playerMeshRenderer.materials = materials;
    }
}