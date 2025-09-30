using UnityEngine;

public class CameraVerticalMover : MonoBehaviour
{
    public enum MotionMode { Sine, PingPong }

    [Header("Movimiento Vertical")] 
    [Tooltip("Modo de movimiento: Seno (continuo) o PingPong (ida y vuelta lineal).")]
    [SerializeField] private MotionMode mode = MotionMode.Sine;

    [Tooltip("Altura máxima desde la posición inicial (amplitud). La cámara oscila entre -amplitud y +amplitud en modo Seno.")]
    [SerializeField] private float amplitude = 0.5f;

    [Tooltip("Duración (en segundos) de un ciclo completo (subir + bajar).")]
    [SerializeField] private float period = 4f;

    [Tooltip("Desfase inicial en segundos dentro del ciclo.")]
    [SerializeField] private float timeOffset = 0f;

    [Header("Suavizado")]
    [Tooltip("Curva opcional para moldear la interpolación (solo en modo PingPong). 0= inicio del recorrido, 1= fin.")]
    [SerializeField] private AnimationCurve pingPongCurve = AnimationCurve.EaseInOut(0,0,1,1);

    [Tooltip("Aplicar suavizado adicional con Lerp hacia la posición objetivo.")]
    [Range(0f,1f)] [SerializeField] private float followSmoothing = 0.2f;

    [Header("Debug")] 
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0.2f,0.8f,1f,0.35f);

    private Vector3 basePosition;
    private float elapsed;

    private void Start()
    {
        basePosition = transform.position;
        elapsed = timeOffset;
        if (period < 0.1f) period = 0.1f; // Evitar división por cero
        if (amplitude < 0f) amplitude = 0f;
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float tCycle = (elapsed / period) % 1f; // Normalizado 0..1 dentro del ciclo

        float yOffset = 0f;
        switch (mode)
        {
            case MotionMode.Sine:
                // Seno: valor entre -1 y 1 -> multiplicamos por amplitud
                yOffset = Mathf.Sin(tCycle * Mathf.PI * 2f) * amplitude;
                break;
            case MotionMode.PingPong:
                // PingPong lineal 0..1..0 -> moldeado por curva -> escalar a amplitud (0..amplitude) y centrar (-ampl/2..+ampl/2)
                float pp = Mathf.PingPong(tCycle * 2f, 1f); // 0..1..0 en un ciclo
                pp = pingPongCurve != null ? pingPongCurve.Evaluate(pp) : pp;
                yOffset = (pp - 0.5f) * 2f * amplitude; // centrar
                break;
        }

        Vector3 targetPos = basePosition + new Vector3(0f, yOffset, 0f);

        if (followSmoothing > 0f && followSmoothing < 1f)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, 1f - Mathf.Pow(1f - followSmoothing, Time.deltaTime * 60f));
        }
        else
        {
            transform.position = targetPos;
        }
    }

    public void ResetBasePositionToCurrent()
    {
        basePosition = transform.position;
    }

    private void OnValidate()
    {
        if (period < 0.1f) period = 0.1f;
        if (amplitude < 0f) amplitude = 0f;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;
        if (basePosition == Vector3.zero && Application.isPlaying == false)
        {
            basePosition = transform.position; // Para vista en editor antes de Play
        }
        Gizmos.color = gizmoColor;
        float top = basePosition.y + amplitude;
        float bottom = basePosition.y - amplitude;
        Vector3 c1 = basePosition; c1.y = top;
        Vector3 c2 = basePosition; c2.y = bottom;
        Gizmos.DrawLine(c1, c2);
        Gizmos.DrawWireSphere(c1, 0.05f);
        Gizmos.DrawWireSphere(c2, 0.05f);
    }
}
