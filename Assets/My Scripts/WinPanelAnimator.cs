using UnityEngine;

// Fades the win screen in and "pops" the card. Uses UNSCALED time, because the game
// is frozen (Time.timeScale = 0) when the win panel is shown.
[RequireComponent(typeof(CanvasGroup))]
public class WinPanelAnimator : MonoBehaviour
{
    [SerializeField] RectTransform card;      // the box in the middle (drag "Card" here)
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

    // slight overshoot, then settles
    static float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float p = x - 1f;
        return 1f + c3 * p * p * p + c1 * p * p;
    }
}