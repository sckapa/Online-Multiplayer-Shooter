using Unity.Netcode;
using UnityEngine;


public class Weapon : NetworkBehaviour
{
    public GameObject graphics;
    public string NameOfGun = "Glock";
    public int Damage = 10;
    public float Range = 100f;
    public float fireRate = 0f;
}
