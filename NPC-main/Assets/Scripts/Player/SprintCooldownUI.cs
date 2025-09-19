using UnityEngine;
using UnityEngine.UI;

public class SprintCooldownUI : MonoBehaviour
{
    [SerializeField] private PlayerMovement player;
    [SerializeField] private Slider slider;
    [SerializeField] private bool hideWhenReady = false; 

    private void Reset()
    {
        slider = GetComponentInChildren<Slider>();
        player = FindObjectOfType<PlayerMovement>();
    }

    private void Awake()
    {
        if (slider != null)
        {
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.wholeNumbers = false;
        }
    }

    private void Update()
    {
        if (player == null || slider == null) return;

        float value;

        if (player.IsSprintOnCooldown)
        {
            float remaining = player.CooldownRemaining;
            float duration = Mathf.Max(0.0001f, player.SprintCooldownDuration);
            value = 1f - (remaining / duration);
        }
        else if (player.IsSprinting)
        {
            value = player.SprintRemaining01;
        }
        else
        {
            value = 1f;
        }

        slider.value = Mathf.Clamp01(value);

        if (hideWhenReady)
            slider.gameObject.SetActive(!(value >= 1f && !player.IsSprinting && !player.IsSprintOnCooldown));
    }
}
