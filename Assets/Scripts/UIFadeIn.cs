using UnityEngine;

public class UIFadeIn : MonoBehaviour
{
    public float fadeDuration = 2f;
    private CanvasGroup cg;
    private float t = 0f;

    void Awake()
    {
        cg = GetComponent<CanvasGroup>();
        cg.alpha = 0f;
    }

    void Update()
    {
        if (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Clamp01(t / fadeDuration);
        }
    }
}