using UnityEngine;
using Unity.Netcode;


[RequireComponent(typeof(PlayerMotor))]
public class PlayerController : NetworkBehaviour
{
    [SerializeField]
    private PlayerMotor motor;
    [SerializeField]
    private float speed = 5f;
    [SerializeField]
    private float sens = 3f;
    [SerializeField]
    private GameObject[] RespawnPoints;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private float staminaRegenSpeed = 0.3f;
    [SerializeField]
    private float staminaBurnSpeed = 1f;
    [SerializeField]
    private float staminaAmount=1f;
    
    public void Start()
    {
        motor.GetComponent<PlayerMotor>();
        animator.GetComponent<Animator>();
    }
    
    private void Update()
    {
        if(IsOwner)
        {
            float xMov = Input.GetAxis("Horizontal");
            float zMov = Input.GetAxis("Vertical");

            Vector3 xMovement = transform.right * xMov;
            Vector3 zMovement = transform.forward * zMov;

            Vector3 _velocity = (xMovement+zMovement) * speed;

            float yRot = Input.GetAxisRaw("Mouse X");
            float xRot = Input.GetAxisRaw("Mouse Y");

            Vector3 _rotation = new Vector3 (0f, yRot, 0f) * sens;
            float _cameraRotationX = xRot * sens;

            motor.Move(_velocity);
            motor.Rotation(_rotation);
            motor.cameraRotation(_cameraRotationX);

            staminaAmount = Mathf.Clamp(staminaAmount, 0f, 1f);
            if(_velocity != Vector3.zero)
            {
                staminaAmount -= staminaBurnSpeed * Time.deltaTime;
            }
            else
            {
                staminaAmount += staminaRegenSpeed * Time.deltaTime;
            }

            if(xMov != 0 || zMov != 0)
            {
                animator.SetFloat("ForwardVelocity", zMov);
                animator.SetFloat("SideVelocity", xMov);
            }
        }
    }

    public float GetStaminaAmount()
    {
        return staminaAmount;
    }

    public override void OnNetworkSpawn()
    {
        int i = Random.Range(0, RespawnPoints.Length);
        transform.position = RespawnPoints[i].transform.position;
        transform.rotation = RespawnPoints[i].transform.rotation;
    }
}
