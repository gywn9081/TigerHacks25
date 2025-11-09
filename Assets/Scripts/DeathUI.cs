using UnityEngine;
using TMPro; // If using TextMeshPro
// using UnityEngine.UI; // If using legacy Text

public class DeathCountUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI deathCountText; // For TextMeshPro
    // [SerializeField] private Text deathCountText; // For legacy UI Text
    
    private void OnEnable()
    {
        // Subscribe to the death event to update UI
        DeathSystem.OnAnyPlayerDied += UpdateDeathCount;
        
        // Update immediately in case there are already deaths
        UpdateDeathCount();
    }
    
    private void OnDisable()
    {
        DeathSystem.OnAnyPlayerDied -= UpdateDeathCount;
    }
    
    private void Start()
    {
        // Initialize the display
        UpdateDeathCount();
    }
    
    private void UpdateDeathCount()
    {
        // Read the static deathCount from DeathSystem
        deathCountText.text = $"Deaths: {DeathSystem.deathCount}";
        // Or: deathCountText.text = "Deaths: " + DeathSystem.deathCount;
    }
}