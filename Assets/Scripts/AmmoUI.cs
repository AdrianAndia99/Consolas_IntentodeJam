using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Slider ammoSlider;
    [SerializeField] private GameObject reloadingIndicator; // Opcional: un texto o imagen que dice "Recargando"

    // Se ejecuta cuando el objeto se activa
    private void OnEnable()
    {
        // Nos suscribimos a los eventos de PlayerShoot
        PlayerShoot.OnAmmoChanged += UpdateAmmoBar;
        PlayerShoot.OnReloadStart += ShowReloadingIndicator;
        PlayerShoot.OnReloadFinish += HideReloadingIndicator;
    }

    // Se ejecuta cuando el objeto se desactiva
    private void OnDisable()
    {
        // Nos desuscribimos para evitar errores
        PlayerShoot.OnAmmoChanged -= UpdateAmmoBar;
        PlayerShoot.OnReloadStart -= ShowReloadingIndicator;
        PlayerShoot.OnReloadFinish -= HideReloadingIndicator;
    }

    private void Start()
    {
        // Asegurarse de que el indicador de recarga esté oculto al empezar
        if (reloadingIndicator != null)
        {
            reloadingIndicator.SetActive(false);
        }
    }

    private void UpdateAmmoBar(int currentAmmo, int maxAmmo)
    {
        if (ammoSlider == null) return;

        // Calculamos el porcentaje de munición (un valor entre 0.0 y 1.0)
        float fillValue = (maxAmmo > 0) ? (float)currentAmmo / maxAmmo : 0f;

        ammoSlider.value = fillValue;
    }

    private void ShowReloadingIndicator()
    {
        if (reloadingIndicator != null)
        {
            reloadingIndicator.SetActive(true);
        }
    }

    private void HideReloadingIndicator()
    {
        if (reloadingIndicator != null)
        {
            reloadingIndicator.SetActive(false);
        }
    }

    // Función de conveniencia para autoconfigurar la referencia en el editor
    private void Reset()
    {
        ammoSlider = GetComponent<Slider>();
    }
}