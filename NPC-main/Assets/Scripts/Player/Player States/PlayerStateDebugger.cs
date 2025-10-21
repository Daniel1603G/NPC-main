using UnityEngine;

/// <summary>
/// Script de debug para visualizar el estado actual del player.
/// Opcional - solo para desarrollo.
/// </summary>
public class PlayerStateDebugger : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private bool showDebugUI = true;
    [SerializeField] private bool showInConsole = false;
    
    private string lastStateName = "";
    
    private void Awake()
    {
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
    }
    
    private void Update()
    {
        if (showInConsole)
        {
            LogStateChanges();
        }
    }
    
    private void OnGUI()
    {
        if (!showDebugUI || playerController == null) return;
        
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 16;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = Color.white;
        
        // Fondo semi-transparente
        GUI.Box(new Rect(10, 10, 250, 120), "");
        
        // Estado actual
        string stateName = GetCurrentStateName();
        GUI.Label(new Rect(20, 20, 230, 25), $"Estado: {stateName}", style);
        
        // Info adicional
        style.fontSize = 12;
        style.fontStyle = FontStyle.Normal;
        
        if (playerController.Movement != null)
        {
            string sprintInfo = playerController.Movement.IsSprinting ? "SI" : "NO";
            string cooldownInfo = playerController.Movement.IsSprintOnCooldown ? 
                $"CD: {playerController.Movement.CooldownRemaining:F1}s" : "Listo";
            
            GUI.Label(new Rect(20, 50, 230, 20), $"Sprint: {sprintInfo}", style);
            GUI.Label(new Rect(20, 70, 230, 20), cooldownInfo, style);
            
            if (playerController.Controller != null)
            {
                string groundedInfo = playerController.Controller.isGrounded ? "Suelo" : "Aire";
                GUI.Label(new Rect(20, 90, 230, 20), $"Posición: {groundedInfo}", style);
            }
        }
    }
    
    private string GetCurrentStateName()
    {
        if (playerController.CurrentState == null)
            return "None";
            
        string fullName = playerController.CurrentState.GetType().Name;
        
        // Remover "PlayerState" del nombre para hacerlo más corto
        return fullName.Replace("PlayerState", "").Replace("State", "");
    }
    
    private void LogStateChanges()
    {
        string currentName = GetCurrentStateName();
        
        if (currentName != lastStateName && !string.IsNullOrEmpty(lastStateName))
        {
            Debug.Log($"[Player FSM] {lastStateName} → {currentName}");
        }
        
        lastStateName = currentName;
    }
}