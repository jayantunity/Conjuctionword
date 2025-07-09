using UnityEngine;

public class FeedBackPanel : MonoBehaviour
{
    public RectTransform panel;            // Drag your panel here
    public float slideDuration = 0.5f;     // Duration in seconds

    Vector2 onScreenPos;
    Vector2 offScreenTop;

    void Start()
    {
        onScreenPos = panel.anchoredPosition;
        offScreenTop = new Vector2(onScreenPos.x, Screen.height);
        panel.anchoredPosition = offScreenTop;  // Start hidden
    }

    public void ShowPanel()
    {
        this.gameObject.SetActive(true);
        LeanTween.move(panel, onScreenPos, slideDuration).setEaseOutCubic();
    }

    public void HidePanel()
    {
        Invoke("hidePanel", 1f);
        
        LeanTween.move(panel, offScreenTop, slideDuration).setEaseInCubic();
    }
    void hidePanel()
    {
this.gameObject.SetActive(false);
    }
}
