using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BubbleLogic : MonoBehaviour
{
    [Header("Bubble Data")]
    public string word;
    public bool isCorrect;

    [Header("UI References")]
    public TextMeshProUGUI wordText;
    public Button bubbleButton;
    public Animator bubbleAnimator;
    public GameObject blastEffectPrefab;
    public System.Action<string, BubbleLogic> OnBubbleClicked;

    void Start()
    {
        wordText.text = word;
        bubbleButton.onClick.AddListener(Click);
    }

public void Click()
{
    if (bubbleAnimator != null)
        bubbleAnimator.SetTrigger("Pop");

    if (blastEffectPrefab != null)
    {
        GameObject effect = Instantiate(blastEffectPrefab, transform.position, Quaternion.identity, transform.parent);
        Destroy(effect, 2f);
    }

    OnBubbleClicked?.Invoke(word, this);
    bubbleButton.interactable = false;
}

    public void SetWord(string newWord, bool correct)
    {
        word = newWord;
        isCorrect = correct;
        wordText.text = newWord;
    }

    public void DisableBubble()
    {
        bubbleButton.interactable = false;
    }
}