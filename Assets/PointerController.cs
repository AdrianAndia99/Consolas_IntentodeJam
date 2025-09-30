using UnityEngine;
using UnityEngine.InputSystem;

public class PointerController : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad en píxeles por segundo (UI).")]
    public float speed = 600f;
    private RectTransform rectTransform;
    private Vector2 moveInput;

    [Header("Límites Básicos")]
    [Tooltip("Limitar dentro del RectTransform padre (Canvas si es hijo directo).")]
    [SerializeField] private bool clampInsideParent = true;

    [Tooltip("Si se asigna, se usa este RectTransform como área de movimiento en lugar del padre.")]
    [SerializeField] private RectTransform boundsOverride;

    [Tooltip("Márgenes internos (píxeles) respecto al área de movimiento.")]
    [SerializeField] private Vector2 margin = new Vector2(8f, 8f);

    [Header("Área Normalizada Opcional (0..1)")]
    [Tooltip("Esquina inferior-izquierda normalizada dentro del área (poner -1 para desactivar).")]
    [SerializeField] private Vector2 normalizedMin = new Vector2(-1f, -1f);
    [Tooltip("Esquina superior-derecha normalizada dentro del área (poner -1 para desactivar).")]
    [SerializeField] private Vector2 normalizedMax = new Vector2(-1f, -1f);

    [Header("Debug")]
    [SerializeField] private bool drawBoundsGizmo = true;
    [SerializeField] private Color boundsColor = new Color(0f, 1f, 0.4f, 0.35f);

    private RectTransform parentRect;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        parentRect = rectTransform.parent as RectTransform;
    }

    public void OnPointerMove(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    void Update()
    {
        if (!rectTransform) return;

        Vector2 pos = rectTransform.anchoredPosition;
        pos += moveInput * speed * Time.deltaTime;

        Rect area = GetMovementArea();

        // Ajustar por márgenes y tamaño del puntero
        float halfW = rectTransform.rect.width * 0.5f;
        float halfH = rectTransform.rect.height * 0.5f;

        float minX = area.xMin + halfW + margin.x;
        float maxX = area.xMax - halfW - margin.x;
        float minY = area.yMin + halfH + margin.y;
        float maxY = area.yMax - halfH - margin.y;

        // Área normalizada (si activada)
        if (normalizedMin.x >= 0f && normalizedMax.x >= 0f && normalizedMax.x >= normalizedMin.x && normalizedMax.y >= normalizedMin.y)
        {
            float nMinX = Mathf.Lerp(area.xMin, area.xMax, normalizedMin.x) + halfW;
            float nMaxX = Mathf.Lerp(area.xMin, area.xMax, normalizedMax.x) - halfW;
            float nMinY = Mathf.Lerp(area.yMin, area.yMax, normalizedMin.y) + halfH;
            float nMaxY = Mathf.Lerp(area.yMin, area.yMax, normalizedMax.y) - halfH;

            minX = Mathf.Max(minX, nMinX);
            maxX = Mathf.Min(maxX, nMaxX);
            minY = Mathf.Max(minY, nMinY);
            maxY = Mathf.Min(maxY, nMaxY);
        }

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        rectTransform.anchoredPosition = pos;
    }

    private Rect GetMovementArea()
    {
        if (boundsOverride)
            return boundsOverride.rect;
        if (clampInsideParent && parentRect)
            return parentRect.rect;
        // Fallback: un rect grande centrado
        return new Rect(-500f, -500f, 1000f, 1000f);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!drawBoundsGizmo) return;
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (parentRect == null) parentRect = rectTransform.parent as RectTransform;

        Rect area = GetMovementArea();
        RectTransform space = boundsOverride ? boundsOverride : parentRect;
        if (space == null) return;

        // Convertimos corners del rect a mundo
        Vector3[] corners = new Vector3[4];
        // area está en espacio local del 'space'. Tomamos los 4 puntos manualmente.
        Vector3 bl = new Vector3(area.xMin, area.yMin, 0f); // bottom-left
        Vector3 br = new Vector3(area.xMax, area.yMin, 0f);
        Vector3 tr = new Vector3(area.xMax, area.yMax, 0f);
        Vector3 tl = new Vector3(area.xMin, area.yMax, 0f);
        corners[0] = space.TransformPoint(bl);
        corners[1] = space.TransformPoint(br);
        corners[2] = space.TransformPoint(tr);
        corners[3] = space.TransformPoint(tl);

        UnityEditor.Handles.color = boundsColor;
        for (int i = 0; i < 4; i++)
        {
            UnityEditor.Handles.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }
#endif
}