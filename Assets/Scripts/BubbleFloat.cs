using UnityEngine;

public class BubbleFloat : MonoBehaviour
{
    public float minSpeed = 80f;
    public float maxSpeed = 160f;
    public float swayAmplitude = 20f;
    public float swayFrequency = 1f;

    private float floatSpeed;
    private RectTransform rt;
    private Vector2 basePosition;
    private float randomOffset;

    void OnEnable()
    {
        if (rt == null)
            rt = GetComponent<RectTransform>();

        basePosition = rt.anchoredPosition;
        floatSpeed = Random.Range(minSpeed, maxSpeed);
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
    }

    void Update()
    {
        basePosition.y += floatSpeed * Time.deltaTime;

        float sway = Mathf.Sin(Time.time * swayFrequency + randomOffset) * swayAmplitude;
        rt.anchoredPosition = new Vector2(basePosition.x + sway, basePosition.y);

        if (rt.anchoredPosition.y > 300f)
        {
            UnityEngine.Debug.LogError("deactivating"+this.GetComponent<BubbleLogic>().wordText.text);
            gameObject.SetActive(false);
             FindObjectOfType<BubbleSpawner>().RespawnSpecificWordFromBubble(gameObject);
        }
    }
}