using System.Collections;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
public class PlayerAttributes : NetworkBehaviour
{
    [SerializeField]
    private Weapon weaponScript;
    [SerializeField]
    private WeaponManager weaponManager;
    [SerializeField]
    private int maxHealth;
    [SerializeField]
    private Transform[] respawnPoints;
    [SerializeField]
    private Behaviour[] componentsToDisableOnDeath;
    [SerializeField]
    private GameObject playerModel;
    [SerializeField]
    private GameObject UIPrefab;
    private GameObject UIInstance;
    private bool[] defaultValuesBeforeDeath;
    private int deathMessage = 0;
    private NetworkVariable<int> currentHealth = new NetworkVariable<int>(0);
    private NetworkVariable<bool> isDead = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);



    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(int _damage)
    {
        if(isDead.Value){return;}

        currentHealth.Value -= _damage;

        if(currentHealth.Value > 0)
        {
            Debug.Log(transform.name + " now has " + currentHealth.Value + " health");
        }
        else
        {
            Debug.Log(transform.name + " now has 0 health");
        }

        if(currentHealth.Value<=0)
        {
            isDead.Value = true;
            DieClientRpc();
            StartCoroutine(RespawnDelay(3));
        }
    }

    private IEnumerator RespawnDelay(int i)
    {
        yield return new WaitForSeconds(i);
        RespawnClientRpc();
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        playerModel.SetActive(false);
        Destroy(weaponManager.weaponIns);

        if(IsOwner)
        {
            Destroy(UIInstance);
            Debug.Log("crosshair removed");
        }
        
        for(int i=0; i<componentsToDisableOnDeath.Length; i++)
        {
            componentsToDisableOnDeath[i].enabled = false;
        }

        if(deathMessage==0)
        {
            Debug.Log(transform.name + " is Dead");
            deathMessage++;
        }
    }

    [ClientRpc]
    private void RespawnClientRpc()
    {
        SetDefaultsServerRpc();
        playerModel.SetActive(true);
        weaponManager.InstantiateGun(weaponScript);
        
        if(IsOwner)
        {
            weaponManager.RecursiveLayerAssignment(weaponManager.weaponIns, LayerMask.NameToLayer(weaponManager.localGunLayerName));
            UIInstance = Instantiate(UIPrefab);
            UIInstance.name = UIPrefab.name;
        }

        int i = Random.Range(0, respawnPoints.Length);
        transform.position = respawnPoints[i].transform.position;
        transform.rotation = respawnPoints[i].transform.rotation;

        for(int j=0; j<defaultValuesBeforeDeath.Length; j++)
        {
            componentsToDisableOnDeath[j].enabled = defaultValuesBeforeDeath[j];
        }

        deathMessage = 0;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetDefaultsServerRpc()
    {
        isDead.Value = false;
        currentHealth.Value = maxHealth;
    }

    public override void OnNetworkSpawn()
    {
        SetDefaultsServerRpc();
        if(IsOwner)
        {
            UIInstance = Instantiate(UIPrefab);
            UIInstance.name = UIPrefab.name;

            StaminaUI staminaUI = UIInstance.GetComponent<StaminaUI>();
            if(staminaUI == null)
            {
                Debug.LogError("No staminaUI prefab on staminaUI.");
            }
            staminaUI.SetController(GetComponent<PlayerController>());
        }

        defaultValuesBeforeDeath = new bool[componentsToDisableOnDeath.Length];
        for(int i=0; i<componentsToDisableOnDeath.Length; i++)
        {
            defaultValuesBeforeDeath[i] = componentsToDisableOnDeath[i].enabled;
        }
        
        currentHealth.Value = Mathf.Clamp(currentHealth.Value, 0, maxHealth);
    }
}
