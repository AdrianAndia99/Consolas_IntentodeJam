using UnityEngine;
using UnityEngine.UI;

public class HealthColorIndicator : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Gradient colorByHealth; // 0 = sin vida (rojo), 1 = vida llena (verde)
    [SerializeField] private bool showAlphaPulseOnLow = true;
    [SerializeField] private float lowThreshold = 0.25f;
    [SerializeField] private float pulseSpeed = 4f;
    [SerializeField] private float minAlpha = 0.5f;
    [SerializeField] private float maxAlpha = 1f;

    private float currentPct = 1f;
    private bool low;

    private void Reset()
    {
        targetImage = GetComponent<Image>();
    }

    private void OnEnable()
    {
        PlayerHealth.OnHealthChanged += OnHealthChanged;
    }

    private void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= OnHealthChanged;
    }

    private void Update()
    {
        if (targetImage == null) return;
        if (showAlphaPulseOnLow && low)
        {
            // pulso de alpha para llamar la atención
            float s = (Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.5f + 0.5f); // 0..1
            Color c = colorByHealth.Evaluate(currentPct);
            c.a = Mathf.Lerp(minAlpha, maxAlpha, s);
            targetImage.color = c;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        currentPct = (max > 0) ? Mathf.Clamp01(current / (float)max) : 0f;
        low = currentPct <= lowThreshold;

        if (targetImage != null)
        {
            Color c = colorByHealth.Evaluate(currentPct);
            c.a = 1f;
            targetImage.color = c;
        }
    }

    private void OnValidate()
    {
        if (colorByHealth.colorKeys == null || colorByHealth.colorKeys.Length < 2)
        {
            // Config de ejemplo: 0 -> rojo, 0.5 -> amarillo, 1 -> verde
            Gradient g = new Gradient();
            g.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.red, 0f),
                    new GradientColorKey(Color.yellow, 0.5f),
                    new GradientColorKey(Color.green, 1f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
            colorByHealth = g;
        }
    }
}
