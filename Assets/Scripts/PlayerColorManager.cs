using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerColorManager : NetworkBehaviour
{
    private static List<Color> availableColors = new List<Color>
    {
        Color.black,
        Color.blue,
        Color.green,
        Color.yellow,
        Color.magenta
    };

    private NetworkVariable<Color> networkedColor = new NetworkVariable<Color>(Color.clear, 
        NetworkVariableReadPermission.Everyone, 
        NetworkVariableWritePermission.Server);

    private Renderer headRenderer;
    private Renderer legsRenderer;
    private Renderer torsoRenderer;
    private bool isApplicationQuitting = false;
    private bool hasDespawned = false;


    private void Start()
    {
        headRenderer = transform.Find("SK_Soldier_Head").GetComponent<Renderer>();
        legsRenderer = transform.Find("SK_Soldier_Legs").GetComponent<Renderer>();
        torsoRenderer = transform.Find("SK_Soldier_Torso").GetComponent<Renderer>();

        if (!headRenderer || !legsRenderer || !torsoRenderer) 
        {
            Debug.LogError("Renderers not found on player.");
        }

        networkedColor.OnValueChanged += OnColorChanged;
    }

    private void OnColorChanged(Color oldValue, Color newValue)
    {
        SetPlayerColor(newValue);
    }

    private void SetPlayerColor(Color color)
    {
        if (headRenderer != null) headRenderer.material.color = color;
        if (legsRenderer != null) legsRenderer.material.color = color;
        if (torsoRenderer != null) torsoRenderer.material.color = color;
    }

    [ServerRpc]
    public void RequestColorServerRpc()
    {
        if (availableColors.Count > 0)
        {
            Color assignedColor = availableColors[0];
            availableColors.RemoveAt(0);
            networkedColor.Value = assignedColor;
        }
        else
        {
            networkedColor.Value = Color.gray;
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit called in PlayerColorManager");
        isApplicationQuitting = true;
    }

    public override void OnNetworkDespawn()
    {
        if (isApplicationQuitting || hasDespawned) 
        {
            return;
        }

        if (IsServer)
        {
            if (availableColors.Count <= 5)
            {
                availableColors.Add(networkedColor.Value);
            }
            networkedColor.Value = Color.gray;
        }

        hasDespawned = true;
    }
}
