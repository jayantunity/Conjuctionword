using UnityEngine;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public QuestionManager questionManager;
    public BubbleSpawner bubbleSpawner;
    public TMP_Text sentenceText;
    public TMP_Text feedbackText;
    public TMP_Text scoreText;
    public GameObject feedbackPanel;
    public GameObject okButton;
    public GameObject doneButton; // ← Assign this in Inspector

    private string originalSentenceTemplate;
    private string playerAnswer;
    private QuestionData currentQuestion;
    private int score;
    private int wrongAttempts;
    private bool wordSelected = false;
    private bool feedbackShown = false;
    private bool isAnswerCorrect = false;
public bool allowBubbleRespawn = true;
    void Start()
    {
        questionManager.LoadQuestions();
        LoadNextQuestion();
    }

    public void LoadNextQuestion()
    {
        currentQuestion = questionManager.GetRandomQuestion();
        if (currentQuestion == null) return;

        originalSentenceTemplate = currentQuestion.sentence;
        sentenceText.text = originalSentenceTemplate.Replace("___", "_____");
        bubbleSpawner.Spawn(currentQuestion);

        feedbackPanel.SetActive(false);
        wrongAttempts = 0;
        wordSelected = false;
        feedbackShown = false;
        isAnswerCorrect = false;
        playerAnswer = "";
        allowBubbleRespawn = true;
    }

    // Called when bubble is clicked (select word only, no feedback)
    public void OnWordSelected(string word)
    {
        playerAnswer = word;
        wordSelected = true;
        feedbackShown = false;

        string filled = originalSentenceTemplate.Replace("___", $"<b>{word}</b>");
        sentenceText.text = filled;
    }

    // Called when Done button is clicked

    public void OnDoneButtonClicked()
    {
        if (!wordSelected)
        {
            feedbackText.text = "❗ Please select a word first!";
            feedbackPanel.SetActive(true);
            okButton.SetActive(false);
            return;
        }

        if (feedbackShown) return;

        allowBubbleRespawn = false; // 💥 stop new bubbles
        EvaluateSelectedAnswer();
    }

public QuestionData GetCurrentQuestion()
{
    return currentQuestion;
}

    private void EvaluateSelectedAnswer()
    {
        isAnswerCorrect = playerAnswer == currentQuestion.correctAnswer;

        if (isAnswerCorrect)
        {
            score++;
            scoreText.text = "Score: " + score;
        }
        else
        {
            wrongAttempts++;
        }

        feedbackShown = true;
        ShowFeedback(isAnswerCorrect);
    }

    void ShowFeedback(bool correct)
    {
        feedbackPanel.SetActive(true);
        okButton.SetActive(true);

        if (correct)
        {
            feedbackText.text = $"✅ Correct!\n\n{currentQuestion.explanation}";
        }
        else if (wrongAttempts >= 2)
        {
            feedbackText.text = $"❌ Incorrect.\n\nMoving to next question.";
        }
        else
        {
            feedbackText.text = $"❌ Incorrect.\n\nTry again!";
        }
    }

    public void OnOkButtonClicked()
    {
        feedbackPanel.SetActive(false);

        if (isAnswerCorrect || wrongAttempts >= 2)
        {
            LoadNextQuestion();
        }
        // Else: retry allowed (player can click another bubble)
    }
}