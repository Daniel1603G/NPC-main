using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class WeaponHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponManager weaponManager;
    
    [Header("UI Elements")]
    [SerializeField] private Image weaponIcon;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI ammoReserveText;
    [SerializeField] private Image ammoBar; 
    
    [Header("Style")]
    [SerializeField] private Color infiniteAmmoColor = Color.cyan;
    [SerializeField] private Color normalAmmoColor = Color.white;
    [SerializeField] private Color lowAmmoColor = Color.yellow;
    [SerializeField] private Color criticalAmmoColor = Color.red;
    [SerializeField] private float lowAmmoThreshold = 0.3f; // 30%
    [SerializeField] private float criticalAmmoThreshold = 0.1f; // 10%
    
    [Header("Animation")]
    [SerializeField] private bool useShakeOnFire = true;
    [SerializeField] private float shakeDuration = 0.1f;
    [SerializeField] private float shakeAmount = 5f;
    
    private Weapon currentWeapon;
    private Vector3 originalPosition;
    private float shakeTimer;
    private CanvasGroup canvasGroup;
    
    private void Awake()
    {
        if (weaponManager == null)
            weaponManager = FindObjectOfType<WeaponManager>();
            
        originalPosition = transform.localPosition;
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }
    
    private void OnEnable()
    {
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged += OnWeaponChanged;
        }
    }
    
    private void OnDisable()
    {
        if (weaponManager != null)
        {
            weaponManager.OnWeaponChanged -= OnWeaponChanged;
        }
        
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= OnAmmoChanged;
            currentWeapon.OnWeaponFired -= OnWeaponFired;
        }
    }
    
    private void Update()
    {
       
        if (currentWeapon != null)
        {
            UpdateAmmoDisplay();
        }
        
    
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;
            float shakeIntensity = shakeTimer / shakeDuration;
            
            Vector3 shakeOffset = Random.insideUnitCircle * shakeAmount * shakeIntensity;
            transform.localPosition = originalPosition + shakeOffset;
        }
        else
        {
            transform.localPosition = originalPosition;
        }
    }
    
 
    private void OnWeaponChanged(Weapon newWeapon)
    {
       
        if (currentWeapon != null)
        {
            currentWeapon.OnAmmoChanged -= OnAmmoChanged;
            currentWeapon.OnWeaponFired -= OnWeaponFired;
        }
        
        currentWeapon = newWeapon;
        
        if (currentWeapon == null)
        {
            HideHUD();
            return;
        }
        
      
        currentWeapon.OnAmmoChanged += OnAmmoChanged;
        currentWeapon.OnWeaponFired += OnWeaponFired;
        
     
        UpdateWeaponInfo();
        ShowHUD();
        
      
        StartCoroutine(FadeIn());
    }
    
    
    private void UpdateWeaponInfo()
    {
        if (currentWeapon == null) return;
        
        // Nombre del arma
        if (weaponNameText != null)
        {
            weaponNameText.text = currentWeapon.Data.weaponName.ToUpper();
            weaponNameText.color = currentWeapon.Data.weaponColor;
        }
        
        // Icono del arma
        if (weaponIcon != null && currentWeapon.Data.weaponIcon != null)
        {
            weaponIcon.sprite = currentWeapon.Data.weaponIcon;
            weaponIcon.color = Color.white;
            weaponIcon.gameObject.SetActive(true);
        }
        else if (weaponIcon != null)
        {
            weaponIcon.gameObject.SetActive(false);
        }
        
        // Actualizar munición
        UpdateAmmoDisplay();
    }
    
    /// <summary>
    /// Actualiza el display de munición.
    /// </summary>
    private void UpdateAmmoDisplay()
    {
        if (currentWeapon == null) return;
        
        int current = currentWeapon.CurrentAmmo;
        int max = currentWeapon.Data.ammoCapacity;
        bool isInfinite = max < 0;
        
        // Texto principal de munición
        if (ammoText != null)
        {
            if (isInfinite)
            {
                ammoText.text = "∞";
                ammoText.color = infiniteAmmoColor;
            }
            else
            {
                ammoText.text = current.ToString();
                ammoText.color = GetAmmoColor(current, max);
            }
        }
        
        // Texto de reserva (actual / máximo)
        if (ammoReserveText != null)
        {
            if (isInfinite)
            {
                ammoReserveText.text = "/ ∞";
                ammoReserveText.color = infiniteAmmoColor * 0.7f;
            }
            else
            {
                ammoReserveText.text = $"/ {max}";
                ammoReserveText.color = normalAmmoColor * 0.7f;
            }
        }
        
        // Barra de munición
        if (ammoBar != null)
        {
            if (isInfinite)
            {
                ammoBar.fillAmount = 1f;
                ammoBar.color = infiniteAmmoColor;
            }
            else
            {
                float fillAmount = max > 0 ? (float)current / max : 0f;
                ammoBar.fillAmount = fillAmount;
                ammoBar.color = GetAmmoColor(current, max);
            }
        }
    }
    
    /// <summary>
    /// Obtiene el color según la cantidad de munición.
    /// </summary>
    private Color GetAmmoColor(int current, int max)
    {
        if (max <= 0) return normalAmmoColor;
        
        float percentage = (float)current / max;
        
        if (percentage <= criticalAmmoThreshold)
            return criticalAmmoColor;
        else if (percentage <= lowAmmoThreshold)
            return lowAmmoColor;
        else
            return normalAmmoColor;
    }
    
    /// <summary>
    /// Se llama cuando cambia la munición.
    /// </summary>
    private void OnAmmoChanged(int newAmmo)
    {
        UpdateAmmoDisplay();
    }
    
    /// <summary>
    /// Se llama cuando el arma dispara.
    /// </summary>
    private void OnWeaponFired()
    {
        if (useShakeOnFire)
        {
            shakeTimer = shakeDuration;
        }
    }
    
    private void ShowHUD()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }
    
    private void HideHUD()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
    
    private System.Collections.IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        float duration = 0.2f;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
    }
}