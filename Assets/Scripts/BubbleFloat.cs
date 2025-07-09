using UnityEngine;

public class BubbleFloat : MonoBehaviour
{
    public float minSpeed = 160f;  // faster
    public float maxSpeed = 240f;  // faster
    public float swayAmplitude = 20f;
    public float swayFrequency = 1f;

    private float floatSpeed;
    private RectTransform rt;
    private Vector2 basePosition;
    private float randomOffset;

    private bool initialized = false;

    void OnEnable()
    {
        if (rt == null)
            rt = GetComponent<RectTransform>();
    }

    public void ResetFloat(Vector2 startPosition)
{
    if (rt == null)
        rt = GetComponent<RectTransform>();

    rt.anchoredPosition = startPosition;
    basePosition = startPosition;

    floatSpeed = Random.Range(minSpeed, maxSpeed);
    randomOffset = Random.Range(0f, 2f * Mathf.PI);
    initialized = true;
}

    void Update()
    {
        if (!initialized || rt == null) return;

        basePosition.y += floatSpeed * Time.deltaTime;

        float sway = Mathf.Sin(Time.time * swayFrequency + randomOffset) * swayAmplitude;
        rt.anchoredPosition = new Vector2(basePosition.x + sway, basePosition.y);

        float topLimit = BubbleSpawner.Instance.spawnArea.rect.height / 2f + 100f;

        if (rt.anchoredPosition.y > topLimit)
        {
            gameObject.SetActive(false);
            BubbleSpawner.Instance.RespawnSpecificWordFromBubble(gameObject);
        }
    }
}