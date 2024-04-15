using UnityEngine;

public class StaminaUI : MonoBehaviour
{
    [SerializeField]
    RectTransform staminaBar;
    private PlayerController controller;

    public void SetController(PlayerController _controller)
    {
        controller = _controller;
    }


    void SetStaminaAmount(float _staminaAmount)
    {
        staminaBar.localScale = new Vector3(1f, _staminaAmount, 1f);
    }

    void Update()
    {
        SetStaminaAmount(controller.GetStaminaAmount());
    }
}
