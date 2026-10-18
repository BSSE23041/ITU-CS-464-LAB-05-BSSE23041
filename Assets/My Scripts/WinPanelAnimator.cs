
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class WinPanelAnimator : MonoBehaviour
{
    [SerializeField] RectTransform card;
    [SerializeField] float duration = 0.6f;
    [SerializeField] float startScale = 0.6f;

    CanvasGroup group;
    float t = 1f;

    void Awake() => group = GetComponent<CanvasGroup>();

    void OnEnable()
    {
        if (group == null) group = GetComponent<CanvasGroup>();
        t = 0f;
        group.alpha = 0f;
        if (card != null) card.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        if (t >= 1f) return;
        t = Mathf.Min(1f, t + Time.unscaledDeltaTime / Mathf.Max(0.01f, duration));
        group.alpha = t;
        if (card != null)
            card.localScale = Vector3.one * Mathf.LerpUnclamped(startScale, 1f, EaseOutBack(t));
    }

    // small pop effect
    static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float p = x - 1f;
        return 1f + c3 * p * p * p + c1 * p * p;
    }
}
