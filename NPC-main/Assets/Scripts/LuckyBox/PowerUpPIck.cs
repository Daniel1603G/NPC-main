using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Caja de power-up que el jugador puede recoger.
/// Usa Roulette Wheel Selection para elegir power-up aleatorio.
/// Ahora hereda de BasePickup para reutilizar código común.
/// </summary>
public class PowerUpPickup : BasePickup
{
    [Header("Power-Up Pool")]
    [Tooltip("Lista de power-ups disponibles con sus pesos")]
    [SerializeField] private List<PowerUpEffect> availableEffects = new List<PowerUpEffect>();
    
    protected override void Awake()
    {
        base.Awake(); // Llamar a la base
        
        if (availableEffects.Count == 0)
        {
            SetupDefaultEffects();
        }
    }
    
    /// <summary>
    /// Configura efectos por defecto si no hay ninguno asignado.
    /// </summary>
    private void SetupDefaultEffects()
    {
        availableEffects.Add(new PowerUpEffect("Speed", 25f, 8f, Color.yellow, "Aumenta la velocidad de movimiento"));
        availableEffects.Add(new PowerUpEffect("Invisibility", 15f, 5f, Color.cyan, "Te vuelve invisible a los enemigos"));
        availableEffects.Add(new PowerUpEffect("Infinite Sprint", 20f, 10f, Color.green, "Sprint ilimitado sin cooldown"));
        availableEffects.Add(new PowerUpEffect("Healing", 25f, 0f, Color.red, "Restaura salud completamente"));
        availableEffects.Add(new PowerUpEffect("Super Jump", 15f, 12f, Color.magenta, "Aumenta la altura de salto"));
    }
    
    /// <summary>
    /// Implementación específica del pickup de power-ups.
    /// </summary>
    protected override bool OnPickup(Collider player)
    {
        var powerUpManager = player.GetComponent<PowerUpManager>();
        if (powerUpManager == null)
        {
            Debug.LogWarning("El jugador no tiene PowerUpManager");
            return false;
        }
        
        // Seleccionar power-up usando Roulette Wheel
        PowerUpEffect selectedEffect = SelectRandomEffect();
        if (selectedEffect != null)
        {
            powerUpManager.ApplyPowerUp(selectedEffect);
            
            // Cambiar color del renderer si existe
            if (objectRenderer != null)
                objectRenderer.material.color = selectedEffect.EffectColor;
            
            return true; // Pickup exitoso
        }
        
        return false; // No había efectos disponibles
    }
    
    /// <summary>
    /// Selecciona un power-up usando Roulette Wheel Selection.
    /// </summary>
    private PowerUpEffect SelectRandomEffect()
    {
        if (availableEffects.Count == 0) return null;
        
        float totalWeight = 0f;
        foreach (var effect in availableEffects)
        {
            totalWeight += effect.Weight;
        }
        
        if (totalWeight <= 0f) return null;
        
        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;
        
        foreach (var effect in availableEffects)
        {
            currentWeight += effect.Weight;
            if (randomValue <= currentWeight)
            {
                Debug.Log($"Power-up seleccionado: {effect.EffectName}");
                return effect;
            }
        }
        
        return availableEffects[availableEffects.Count - 1];
    }
}