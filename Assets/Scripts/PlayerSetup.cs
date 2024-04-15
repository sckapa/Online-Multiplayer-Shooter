using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerAttributes))]
public class PlayerSetup : NetworkBehaviour
{
    [SerializeField]
    private Behaviour[] componentsToDisable;
    [SerializeField]
    string remoteLayerName = "RemotePlayer";
    [SerializeField]
    private GameObject playerModel;
    [SerializeField]
    private WeaponManager weaponManager;
    
    void Start()
    {
        AssignLayersToPlayers();
        DisableComponents();
    }

    public override void OnNetworkSpawn()
    {
        PlayerAttributes player = GetComponent<PlayerAttributes>();
        GameManager.RegisterPlayer(OwnerClientId.ToString(), player);
    }

    private void DisableComponents ()
    {
        if(!IsLocalPlayer)
        {
            for(int i=0; i < componentsToDisable.Length; i++)
            {
                componentsToDisable[i].enabled = false;
            }
        }
    }

    private void AssignLayersToPlayers()
    {
        if(!IsOwner)
        {
            weaponManager.RecursiveLayerAssignment(playerModel, LayerMask.NameToLayer(remoteLayerName));
        }
    }
}
