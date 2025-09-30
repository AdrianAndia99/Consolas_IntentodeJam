using UnityEngine;
using UnityEngine.InputSystem;

public class PointerController : MonoBehaviour
{
    public float speed = 5f;
    private RectTransform rectTransform;
    private Vector2 moveInput;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

  
    public void OnPointerMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0) * speed * Time.deltaTime;
        rectTransform.localPosition += delta;
    }
}