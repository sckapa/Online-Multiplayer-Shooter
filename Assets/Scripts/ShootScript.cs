using Unity.Netcode;
using UnityEngine;

public class ShootScript : NetworkBehaviour
{
    [SerializeField]
    private LayerMask mask;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private Weapon currentWeapon;
    [SerializeField]
    private WeaponManager weaponManager;

    public void Start()
    {
        if(cam == null)
        {
            Debug.Log("ShootScript : No camera bound");
        }
    }

    public void Update()
    {
        currentWeapon = weaponManager.GetWeapon();

        if(IsOwner)
        {
            if(currentWeapon.fireRate <= 0f)
            {
                    if(Input.GetButtonDown("Fire1"))
                    {
                        Shoot();
                    }
            }
            else
            {
                    if(Input.GetButtonDown("Fire1"))
                    {
                        InvokeRepeating("Shoot", 0f, 1f/currentWeapon.fireRate);
                    }
                    else if(Input.GetButtonUp("Fire1"))
                    {
                        CancelInvoke("Shoot");
                    }
            }
        }
    }

    [ServerRpc]
    void OnShootServerRpc()
    {
        CallShootOnClientsClientRpc();
    }

    [ClientRpc]
    void CallShootOnClientsClientRpc()
    {
        weaponManager.GetGraphics().muzzleFlash.Play();
    }

    [ServerRpc]
    void OnHitServerRpc(Vector3 _pos, Vector3 _normal)
    {
        CallHitOnClientsClientRpc(_pos,_normal);
    }

    [ClientRpc]
    void CallHitOnClientsClientRpc(Vector3 _pos, Vector3 _normal)
    {
        GameObject hitIns = Instantiate(weaponManager.GetGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
        Destroy(hitIns, 2f);
    }

    private void Shoot()
    {
        RaycastHit hit;
        OnShootServerRpc();
        if(Physics.Raycast(cam.transform.position, cam.transform.forward, out hit, currentWeapon.Range, mask))
        { 
            OnHitServerRpc(hit.point, hit.normal);
            if (hit.collider.name == "Head")
            {
                PlayerShot(hit.transform.name, currentWeapon.Damage);
            }

            if (hit.collider.name == "Chest")
            {
                PlayerShot(hit.transform.name, currentWeapon.Damage);
            }

            if (hit.collider.name == "Legs")
            {
                PlayerShot(hit.transform.name, currentWeapon.Damage);
            }

            if (hit.collider.name == "Feet")
            {
                PlayerShot(hit.transform.name, currentWeapon.Damage);
            }

            if (hit.collider.name == "Arms")
            {
                PlayerShot(hit.transform.name, currentWeapon.Damage);
            }

            if (hit.collider.name == "Underwear")
            {
                PlayerShot(hit.transform.name, currentWeapon.Damage);
            }
        }
    }


    private void PlayerShot(string _playerHit, int _damage)
    {
        Debug.Log(_playerHit + " has been shot");

        PlayerAttributes player = GameManager.GetPlayer(_playerHit);
        player.TakeDamageServerRpc(_damage);
    }
}
