using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    public GameObject bubblePrefab;
    public RectTransform spawnArea;
    public GameManager gameManager;
    private Dictionary<string, GameObject> wordToBubble = new Dictionary<string, GameObject>();
    private QuestionData currentQuestion;
    private Coroutine spawnCoroutine;
    private List<GameObject> activeBubbles = new List<GameObject>();


    public void Spawn(QuestionData question)
    {
        ClearBubbles();
        currentQuestion = question;

        RectTransform area = spawnArea;
        float y = -area.rect.height - Random.Range(100f, 250f);

        wordToBubble.Clear();

        foreach (string word in question.options)
        {
            GameObject bubble = Instantiate(bubblePrefab, spawnArea);
            bubble.SetActive(true);

            SetupBubble(bubble, word, question);
            Vector2 pos = TryGetNonOverlappingPosition(y);
            bubble.GetComponent<RectTransform>().anchoredPosition = pos;

            wordToBubble[word] = bubble;
            activeBubbles.Add(bubble);
        }
    }
    void SetupBubble(GameObject bubble, string word, QuestionData question)
    {
        BubbleLogic logic = bubble.GetComponent<BubbleLogic>();
        bool isCorrect = word == question.correctAnswer;

        logic.SetWord(word, isCorrect);

        logic.OnBubbleClicked = (selectedWord, bubbleLogic) =>
        {
            gameManager.OnWordSelected(selectedWord);
            StartCoroutine(RespawnSpecificWordAfterDelay(selectedWord));
        };
    }

IEnumerator RespawnSpecificWordAfterDelay(string word)
{
    yield return new WaitForSeconds(5f);
    if (!wordToBubble.ContainsKey(word)) yield break;

    GameObject bubble = wordToBubble[word];
    bubble.SetActive(false);
    yield return new WaitForSeconds(0.5f);

    Vector2 pos = TryGetNonOverlappingPosition(-spawnArea.rect.height - Random.Range(100f, 250f));
    bubble.GetComponent<RectTransform>().anchoredPosition = pos;
    bubble.SetActive(true);
}
    public void RespawnBubble(GameObject bubble)
    {
        if (!gameManager.allowBubbleRespawn) return;

        QuestionData question = gameManager.GetCurrentQuestion(); // <-- make sure you expose this
        if (question == null) return;

        string randomWord = question.options[Random.Range(0, question.options.Count)];
        SetupBubble(bubble, randomWord, question);

        float y = -spawnArea.rect.height - Random.Range(100f, 250f);
        Vector2 pos = TryGetNonOverlappingPosition(y);
        bubble.GetComponent<RectTransform>().anchoredPosition = pos;

        bubble.SetActive(true);
    }
    public void RespawnSpecificWordFromBubble(GameObject bubble)
{
    foreach (var kvp in wordToBubble)
    {
        if (kvp.Value == bubble)
        {
            StartCoroutine(RespawnSpecificWordAfterDelay(kvp.Key));
            break;
        }
    }
}
    private Vector2 TryGetNonOverlappingPosition(float y, float minDistance = 150f, int maxAttempts = 20)
    {
        RectTransform area = spawnArea;
        float areaWidth = area.rect.width;
        float xMin = -areaWidth / 2 + 100f;
        float xMax = areaWidth / 2 - 100f;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float x = Random.Range(xMin, xMax);
            Vector2 testPos = new Vector2(x, y);

            bool overlaps = false;
            foreach (GameObject bubble in activeBubbles)
            {
                if (!bubble.activeInHierarchy) continue;

                RectTransform rt = bubble.GetComponent<RectTransform>();
                if (Vector2.Distance(rt.anchoredPosition, testPos) < minDistance)
                {
                    overlaps = true;
                    break;
                }
            }

            if (!overlaps)
                return testPos;
        }

        // fallback: allow overlap
        return new Vector2(Random.Range(xMin, xMax), y);
    }

    IEnumerator RespawnBubbleAfterDelay(GameObject bubble, QuestionData question)
    {
        bubble.SetActive(false);
        yield return new WaitForSeconds(1f); // delay before respawn
        RespawnBubble(bubble);
    }
    IEnumerator SpawnLoop(QuestionData question)
    {
        List<string> words = new List<string>(question.options);
        RectTransform area = spawnArea;
        Vector2 areaSize = area.rect.size;
        float padding = 100f;
        float totalWidth = areaSize.x - (2 * padding);
        float spacing = (words.Count > 1) ? totalWidth / (words.Count - 1) : 0;

        while (gameManager.allowBubbleRespawn)
        {
            // Clear previous batch
            activeBubbles.Clear();

            for (int i = 0; i < words.Count; i++)
            {
                GameObject bubble = Instantiate(bubblePrefab, spawnArea);
                activeBubbles.Add(bubble);

                BubbleLogic logic = bubble.GetComponent<BubbleLogic>();
                string word = words[i];
                bool isCorrect = word == question.correctAnswer;

                logic.SetWord(word, isCorrect);

                logic.OnBubbleClicked = (selectedWord, bubbleLogic) =>
                {
                    gameManager.OnWordSelected(selectedWord);
                    activeBubbles.Remove(bubbleLogic.gameObject);
                    StartCoroutine(DestroyAfterFeedback(bubbleLogic.gameObject));
                };

                // Spawn below canvas
                float x = -areaSize.x / 2 + padding + spacing * i;
                float y = -areaSize.y - Random.Range(100f, 250f);
                bubble.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y);
            }

            // Wait until all bubbles in this batch are gone or off-screen
            yield return new WaitUntil(() => AllBubblesGoneOrOffscreen());

            //yield return new WaitForSeconds(0.5f); // short pause between batches
        }
    }


    bool AllBubblesGoneOrOffscreen()
    {
        float offscreenY = spawnArea.rect.height; // buffer above top edge

        for (int i = activeBubbles.Count - 1; i >= 0; i--)
        {
            GameObject bubble = activeBubbles[i];

            if (bubble == null || !bubble.activeInHierarchy)
            {
                activeBubbles.RemoveAt(i);
                continue;
            }

            RectTransform rt = bubble.GetComponent<RectTransform>();

            // If ANY bubble is still visible (not offscreen), return false
            if (rt.anchoredPosition.y < offscreenY)
                return false;
        }

        return true; // All bubbles are offscreen or deactivated
    }

   void ClearBubbles()
{
    foreach (GameObject bubble in wordToBubble.Values)
    {
        if (bubble != null)
            bubble.SetActive(false);
    }

    activeBubbles.Clear();
    wordToBubble.Clear();
}

    IEnumerator DestroyAfterFeedback(GameObject bubble)
    {
        yield return new WaitForSeconds(2f);
        if (bubble != null)
        {
            Destroy(bubble);
        }
    }
}