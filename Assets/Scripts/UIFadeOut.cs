using UnityEngine;

public class UIFadeOut : MonoBehaviour
{
    public float fadeDuration = 2f;
    public CanvasGroup cg;
    private float t;
    private bool enable = false;

    public void setEnable()
    {
        t = fadeDuration;
        enable = true;
    }

    void Update()
    {
        if (enable && (t > 0))
        {
            t -= Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / fadeDuration);
        }
    }
}