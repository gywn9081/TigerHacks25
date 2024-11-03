using UnityEngine;
using UnityEngine.UI;

public class UprisingMeterDisplay : MonoBehaviour
{
    public RectTransform meterBar; // Assign this in the Inspector
    public int maxMeterLevel = 100; // Set this to the maximum level
    public float currentMeterLevel = 0; // You may want to keep this updated elsewhere

    private void Start()
    {
        UpdateMeterBar();
    }

    private void Update()
    {
        UpdateMeterBar();
    }

    private void UpdateMeterBar()
    {
        currentMeterLevel = currentMeterLevel + Time.deltaTime;//UprisingMeter.Instance.GetMeterLevel(); // Get the current level
        float fillAmount = Mathf.Clamp01((float)currentMeterLevel / maxMeterLevel);
        
        // Adjust the width of the RectTransform based on fill amount
        meterBar.sizeDelta = new Vector2(fillAmount * 250, meterBar.sizeDelta.y); // Adjust 100 to your desired full width
    }
}