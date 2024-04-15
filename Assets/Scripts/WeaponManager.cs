using Unity.Netcode;
using UnityEngine;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField]
    private Transform weaponHolder;
    [SerializeField]
    public string localGunLayerName = "Gun";
    [SerializeField]
    private Weapon primaryWeapon;
    private Weapon currentWeapon;
    private WeaponGraphics currentGraphics;
    public GameObject weaponIns;


    public override void OnNetworkSpawn()
    {
        EquipWeapon(primaryWeapon);
    }

    public void InstantiateGun(Weapon _weapon)
    {
        weaponIns = Instantiate(_weapon.graphics, weaponHolder.position, weaponHolder.rotation);
        weaponIns.transform.SetParent(weaponHolder);
    }

    private void EquipWeapon(Weapon _weapon)
    {
        InstantiateGun(_weapon);
        currentWeapon = _weapon;
        weaponIns.SetActive(true);

        currentGraphics = weaponIns.GetComponent<WeaponGraphics>();
        if(currentGraphics == null)
        {
            Debug.LogError("No Graphics on the weapon: " + weaponIns.name);
        }

        if (IsOwner)
        {
            RecursiveLayerAssignment(weaponIns, LayerMask.NameToLayer(localGunLayerName));
        }
    }

    public Weapon GetWeapon()
    {
        return currentWeapon;
    }

    public WeaponGraphics GetGraphics()
    {
        return currentGraphics;
    }

    public void RecursiveLayerAssignment(GameObject obj, int newlayer)
    {
        obj.layer = newlayer;

        foreach(Transform child in obj.transform)
        {
            RecursiveLayerAssignment(child.gameObject, newlayer);
        }
    }
}
