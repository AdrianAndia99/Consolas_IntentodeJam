using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class HitFeedbackVignette : MonoBehaviour
{
    [Header("URP Volume")]
    [SerializeField] private Volume volume; // Asigna tu Volume (global) de la escena

    [Header("Golpe (pico y fade)")]
    [SerializeField] private float vignettePeak = 0.55f;      // intensidad al recibir golpe
    [SerializeField] private float vignetteFadeTo = 0.25f;     // valor al que cae tras el golpe
    [SerializeField] private float vignetteUpTime = 0.06f;     // subida rápida
    [SerializeField] private float vignetteDownTime = 0.25f;   // bajada

    [Header("Desaturación breve")]
    [SerializeField] private bool useDesaturation = true;
    [SerializeField] private float desatValue = -20f;          // Saturation (negativo = desaturar)
    [SerializeField] private float desatTime = 0.12f;          // duración breve

    [Header("Bajo HP: pulso opcional")]
    [SerializeField] private bool lowHpPulse = true;
    [SerializeField] private float lowHpThreshold = 0.25f;     // 25% de la vida
    [SerializeField] private float pulseMin = 0.2f;
    [SerializeField] private float pulseMax = 0.35f;
    [SerializeField] private float pulseSpeed = 2.0f;

    private Vignette vignette;
    private ColorAdjustments colorAdj;
    private Coroutine hitCo;
    private Coroutine desatCo;
    private Coroutine pulseCo;

    private void Awake()
    {
        if (volume == null) volume = FindObjectOfType<Volume>();

        if (volume != null && volume.profile != null)
        {
            volume.profile.TryGet(out vignette);
            volume.profile.TryGet(out colorAdj);
        }

        if (vignette != null && !vignette.intensity.overrideState) vignette.intensity.overrideState = true;
        if (colorAdj != null && !colorAdj.saturation.overrideState) colorAdj.saturation.overrideState = true;

        // Estado inicial suave
        if (vignette != null) vignette.intensity.value = Mathf.Clamp01(vignetteFadeTo);
        if (colorAdj != null) colorAdj.saturation.value = 0f;
    }

    private void OnEnable()
    {
        PlayerHealth.OnDamaged += HandleDamaged;
        PlayerHealth.OnHealthChanged += HandleHealthChanged;
    }

    private void OnDisable()
    {
        PlayerHealth.OnDamaged -= HandleDamaged;
        PlayerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleDamaged()
    {
        if (vignette == null) return;

        if (hitCo != null) StopCoroutine(hitCo);
        hitCo = StartCoroutine(HitVignetteRoutine());

        if (useDesaturation && colorAdj != null)
        {
            if (desatCo != null) StopCoroutine(desatCo);
            desatCo = StartCoroutine(DesatRoutine());
        }
    }

    private IEnumerator HitVignetteRoutine()
    {
        float start = vignette.intensity.value;
        // Subida rápida
        float t = 0f;
        while (t < vignetteUpTime)
        {
            t += Time.unscaledDeltaTime;
            vignette.intensity.value = Mathf.Lerp(start, vignettePeak, t / vignetteUpTime);
            yield return null;
        }
        // Baja a valor base
        t = 0f;
        while (t < vignetteDownTime)
        {
            t += Time.unscaledDeltaTime;
            vignette.intensity.value = Mathf.Lerp(vignettePeak, vignetteFadeTo, t / vignetteDownTime);
            yield return null;
        }
        vignette.intensity.value = vignetteFadeTo;
    }

    private IEnumerator DesatRoutine()
    {
        float orig = colorAdj.saturation.value;
        colorAdj.saturation.value = desatValue;
        float t = 0f;
        while (t < desatTime)
        {
            t += Time.unscaledDeltaTime;
            colorAdj.saturation.value = Mathf.Lerp(desatValue, orig, t / desatTime);
            yield return null;
        }
        colorAdj.saturation.value = orig;
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (!lowHpPulse || vignette == null) return;

        float pct = (max > 0) ? (current / (float)max) : 0f;

        if (pct <= lowHpThreshold)
        {
            if (pulseCo == null) pulseCo = StartCoroutine(PulseRoutine());
        }
        else
        {
            if (pulseCo != null)
            {
                StopCoroutine(pulseCo);
                pulseCo = null;
            }
            // Regresa a valor base cuando no está en bajo HP
            vignette.intensity.value = vignetteFadeTo;
        }
    }

    private IEnumerator PulseRoutine()
    {
        float t = 0f;
        while (true)
        {
            t += Time.unscaledDeltaTime * pulseSpeed;
            float s = (Mathf.Sin(t) * 0.5f + 0.5f); // 0..1
            vignette.intensity.value = Mathf.Lerp(pulseMin, pulseMax, s);
            yield return null;
        }
    }
}
