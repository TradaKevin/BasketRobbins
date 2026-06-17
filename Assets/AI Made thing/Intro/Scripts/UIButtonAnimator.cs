using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Adds simple arcade button feedback: hover scale, pressed scale, and optional sounds.
/// Works with Unity UI Buttons on PC and touch devices.
/// </summary>
public class UIButtonAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    [Header("Scale")]
    public float hoverScale = 1.05f;
    public float pressedScale = 0.95f;
    public float scaleSpeed = 14f;

    [Header("Sound Hooks")]
    public AudioSource audioSource;
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private RectTransform rectTransform;
    private Vector3 baseScale;
    private Vector3 targetScale;
    private bool pointerInside;
    private Button button;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        button = GetComponent<Button>();
        baseScale = rectTransform.localScale;
        targetScale = baseScale;
    }

    private void OnEnable()
    {
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }

        baseScale = rectTransform.localScale;
        targetScale = baseScale;
    }

    private void Update()
    {
        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * scaleSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!IsInteractable())
        {
            return;
        }

        pointerInside = true;
        targetScale = baseScale * hoverScale;
        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pointerInside = false;
        targetScale = baseScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!IsInteractable())
        {
            return;
        }

        targetScale = baseScale * pressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsInteractable())
        {
            targetScale = baseScale;
            return;
        }

        targetScale = pointerInside ? baseScale * hoverScale : baseScale;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsInteractable())
        {
            PlaySound(clickSound);
        }
    }

    private bool IsInteractable()
    {
        return button == null || button.interactable;
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}
