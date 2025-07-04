using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class BubbleSpawner : MonoBehaviour
{
    public GameObject bubblePrefab;
    public RectTransform spawnArea;
    public GameManager gameManager;

    private Coroutine spawnCoroutine;
    private List<GameObject> activeBubbles = new List<GameObject>();

    public void Spawn(QuestionData question)
    {
        ClearBubbles();

        if (spawnCoroutine != null)
            StopCoroutine(spawnCoroutine);

        spawnCoroutine = StartCoroutine(SpawnLoop(question));
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

    bool AllBubblesGoneOrOffscreen1()
    {
        for (int i = activeBubbles.Count - 1; i >= 0; i--)
        {
            GameObject bubble = activeBubbles[i];
            if (bubble == null)
            {
                activeBubbles.RemoveAt(i);
                continue;
            }

            RectTransform rt = bubble.GetComponent<RectTransform>();
            if (rt.anchoredPosition.y < spawnArea.rect.height) // still onscreen
                return false;
        }

        return true; // all popped or floated off
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
        foreach (Transform child in spawnArea)
        {
            Destroy(child.gameObject);
        }

        activeBubbles.Clear();
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