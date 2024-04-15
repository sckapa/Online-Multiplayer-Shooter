using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : NetworkBehaviour
{
    private Vector3 velocity;
    private Vector3 rotate;
    private float cameraRotateX = 0f;
    private float staminaAmount = 1f;
    private float currentCameraRotation = 0f;
    [SerializeField]
    private float walkSpeed = 0.3f;
    [SerializeField]
    private Animator animator;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private float cameraRotationLimit = 85f;
    [SerializeField]
    private PlayerController playerController;

    void Start()
    {
        rb.GetComponent<Rigidbody>();
        playerController.GetComponent<PlayerController>();
    }

    void FixedUpdate()
    {
        MovementPerformed();
        RotationPerformed();
        staminaAmount = playerController.GetStaminaAmount();
    }

    public void Move(Vector3 _velocity)
    {
        velocity = _velocity;
    }

    public void Rotation(Vector3 _rotation)
    {
        rotate = _rotation;
    }

    public void cameraRotation(float _cameraRotationX)
    {
        cameraRotateX = _cameraRotationX;
    }
    
    void MovementPerformed()
    {
        if(IsOwner)
        {
            if(velocity!=Vector3.zero && staminaAmount>=0.01f)
            {
                animator.SetBool("isRunning", true);
                rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            }
            else
            {
                animator.SetBool("isRunning", false);
            }

            if(velocity!=Vector3.zero && staminaAmount<=0.01f)
            {
                animator.SetBool("isWalking", true);
                velocity = velocity * walkSpeed;
                rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
            }
            else
            {
                animator.SetBool("isWalking", false);
            }
        }
    }
    void RotationPerformed()
    {
        rb.MoveRotation(rb.rotation * Quaternion.Euler(rotate));
        if(cam != null)
        {
            currentCameraRotation -= cameraRotateX;
            currentCameraRotation = Mathf.Clamp(currentCameraRotation, -cameraRotationLimit, cameraRotationLimit);

            cam.transform.localEulerAngles = new Vector3(currentCameraRotation, 0f, 0f);
        }
    }
}
