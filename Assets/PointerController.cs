using UnityEngine;
using UnityEngine.InputSystem;

public class PointerController : MonoBehaviour
{
    public float speed = 5f; // Ajusta la velocidad
    private RectTransform rectTransform;
    private Vector2 moveInput;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Conecta este método al Input Action "PointerMove" (performed)
    public void OnPointerMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        // Mueve el puntero según el input
        Vector3 delta = new Vector3(moveInput.x, moveInput.y, 0) * speed * Time.deltaTime;
        rectTransform.localPosition += delta;
    }
}