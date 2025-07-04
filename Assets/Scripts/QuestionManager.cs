using System.Collections.Generic;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
[System.Serializable]
public class QuestionData
{
    public string sentence;
    public string correctAnswer;
    public List<string> options;
    public string explanation;
    public string topic;
}

// ---------- 2. Question Loader ----------
 // Install via NuGet or use Unity's JsonUtility

public class QuestionManager : MonoBehaviour
{
    public List<QuestionData> allQuestions;
    public List<QuestionData> filteredQuestions;
    public string currentTopic = "conjunction";

    public void LoadQuestions()
    {
        TextAsset json = Resources.Load<TextAsset>("questions");
        allQuestions = JsonConvert.DeserializeObject<List<QuestionData>>(json.text);
        filteredQuestions = allQuestions.FindAll(q => q.topic == currentTopic);
    }

    public QuestionData GetRandomQuestion()
    {
        if (filteredQuestions.Count == 0) return null;
        int index = Random.Range(0, filteredQuestions.Count);
        return filteredQuestions[index];
    }
}
