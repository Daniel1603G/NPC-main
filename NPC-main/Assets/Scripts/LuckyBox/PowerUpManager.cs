using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private FirstPersonLook firstPersonLook;
    
    [Header("Effect Durations (seconds)")]
    [SerializeField, Range(1f, 60f)] private float speedBoostDuration = 12f;
    [SerializeField, Range(1f, 30f)] private float invisibilityDuration = 8f;
    [SerializeField, Range(5f, 60f)] private float infiniteSprintDuration = 15f;
    [SerializeField, Range(5f, 60f)] private float superJumpDuration = 10f;
    
    [Header("Effect Multipliers")]
    [SerializeField] private float speedMultiplier = 2f;
    [SerializeField] private float jumpMultiplier = 2f;
    
    [Header("Global Settings")]
    [SerializeField, Range(0.1f, 3f)] private float globalDurationMultiplier = 1f;
    [SerializeField, Range(0.1f, 3f)] private float globalEffectMultiplier = 1f;
    private bool isSpeedBoostActive = false;
    private bool isInvisible = false;
    private bool hasInfiniteSprint = false;
    private bool hasSuperJump = false;
    
    private ISpin spinComponent;
    
    public bool IsInvisible => isInvisible;
    public bool HasInfiniteSprint => hasInfiniteSprint;
    public bool IsSpeedBoostActive => isSpeedBoostActive;
    public bool HasSuperJump => hasSuperJump;
    
    public float CurrentSpeedMultiplier => speedMultiplier * globalEffectMultiplier;
    public float CurrentJumpMultiplier => jumpMultiplier * globalEffectMultiplier;
    
    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
            
        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
            
        if (firstPersonLook == null)
            firstPersonLook = GetComponent<FirstPersonLook>();
            
        spinComponent = GetComponent<ISpin>();
    }
    
    public void ApplyPowerUp(PowerUpEffect effect)
    {
        switch (effect.EffectName.ToLower())
        {
            case "speed":
            case "velocity":
            case "speed boost":
                StartCoroutine(SpeedBoostEffect(effect.Duration));
                break;
                
            case "invisibility":
            case "invisible":
                StartCoroutine(InvisibilityEffect(effect.Duration));
                break;
                
            case "infinite sprint":
            case "unlimited sprint":
            case "sprint":
                StartCoroutine(InfiniteSprintEffect(effect.Duration));
                break;
                
            case "healing":
            case "heal":
            case "health":
                HealingEffect();
                break;
                
            case "super jump":
            case "superjump":
            case "jump boost":
         
                StartCoroutine(SuperJumpEffect(effect.Duration));
                break;
                
            default:
                Debug.LogWarning($"Efecto no reconocido: {effect.EffectName}");
                break;
        }
        
        Debug.Log($"¡Power-up activado! {effect.EffectName}: {effect.Description}");
    }
    
    private IEnumerator SpeedBoostEffect(float duration)
    {
        if (isSpeedBoostActive) yield break;
        
        isSpeedBoostActive = true;
        
        Debug.Log("¡Velocidad aumentada!");
        
        yield return new WaitForSeconds(duration);
        
        isSpeedBoostActive = false;
        Debug.Log("Efecto de velocidad terminado");
    }
    
    private IEnumerator InvisibilityEffect(float duration)
    {
        if (isInvisible) yield break;
        
        isInvisible = true;
        
        var playerModel = GetComponent<PlayerModel>();
        
        if (playerModel != null)
        {
            bool wasAlreadyInvisible = !playerModel.IsDetectable;
            
            if (!wasAlreadyInvisible)
            {
                playerModel.SetDetectable(false, false);
                Debug.Log("¡Power-up de invisibilidad activado!");
            }
            else
            {
                Debug.Log("¡Ya eras invisible, extendiendo duración!");
            }
            
            yield return new WaitForSeconds(duration);
            
            if (!wasAlreadyInvisible)
            {
                playerModel.SetDetectable(true, false);
                Debug.Log("Power-up de invisibilidad terminado");
            }
        }
        else
        {
            Debug.Log("¡Invisible a los enemigos! (sin PlayerModel)");
            yield return new WaitForSeconds(duration);
            Debug.Log("Invisibilidad terminada");
        }
        
        isInvisible = false;
    }
    
    private IEnumerator InfiniteSprintEffect(float duration)
    {
        if (hasInfiniteSprint) yield break;
        
        hasInfiniteSprint = true;
        Debug.Log("¡Sprint infinito activado!");
        
        yield return new WaitForSeconds(duration);
        
        hasInfiniteSprint = false;
        Debug.Log("Sprint infinito terminado");
    }
    
    private void HealingEffect()
    {
        if (playerHealth != null)
        {
            playerHealth.Heal(playerHealth.MaxHealth);
            Debug.Log("¡Salud completamente restaurada!");
        }
    }
    
    private IEnumerator SuperJumpEffect(float duration)
    {
        if (hasSuperJump) yield break;
        
        hasSuperJump = true;
        Debug.Log("¡Súper salto activado!");
        
        yield return new WaitForSeconds(duration);
        
        hasSuperJump = false;
        Debug.Log("Súper salto terminado");
    }
    
    private float GetConfigurableDuration(string effectName)
    {
        float baseDuration = 0f;
        
        string normalizedName = effectName.ToLower().Trim();
        
        switch (normalizedName)
        {
            case "speed":
            case "velocity":
            case "speed boost":
                baseDuration = speedBoostDuration;
                break;
                
            case "invisibility":
            case "invisible":
                baseDuration = invisibilityDuration;
                break;
                
            case "infinite sprint":
            case "unlimited sprint":
            case "sprint":
                baseDuration = infiniteSprintDuration;
                break;
                
            case "super jump":
            case "superjump":
            case "jump boost":
                baseDuration = superJumpDuration;
                break;
                
            case "healing":
            case "heal":
            case "health":
                return 0f; 
        }
        
        return baseDuration * globalDurationMultiplier;
    }
}