using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public QuestionManager questionManager;
    public BubbleSpawner bubbleSpawner;
    public TMP_Text sentenceStart;
    public TMP_Text sentenceEnd;
    public TMP_Text wordSlot;
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
    public UnityEngine.UI.Button doneButtonUI;
    private Animator doneButtonAnimator;
    public float questionTimeLimit = 20f;
    private float currentTimeLeft;
    private bool timerRunning = false;
    public TMP_Text timerText;
    public GameObject wordSlotUI;

    void Start()
    {
        questionManager.LoadQuestions();
        LoadNextQuestion();
        doneButtonAnimator = doneButton.GetComponent<Animator>();
        SetDoneButtonState(false); // starts disabled
    }

    public void LoadNextQuestion()
    {
         StopBlinking(); 
        currentQuestion = questionManager.GetRandomQuestion();
        if (currentQuestion == null) return;

        originalSentenceTemplate = currentQuestion.sentence;

        // Split sentence parts before and after blank
        string[] parts = originalSentenceTemplate.Split(new string[] { "___" }, System.StringSplitOptions.None);
        sentenceStart.text = parts[0];
        sentenceEnd.text = parts.Length > 1 ? parts[1] : "";
        //wordSlot.text = "_____";
        wordSlot.text = "";
        bubbleSpawner.Spawn(currentQuestion);

        //   feedbackPanel.SetActive(false);
        wrongAttempts = 0;
        wordSelected = false;
        currentTimeLeft = questionTimeLimit;
        timerRunning = true;
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

        wordSlot.text = $"<b>{word}</b>";
        SetDoneButtonState(true);
    }

    void SetDoneButtonState(bool hasWord)
    {
        ColorBlock cb = doneButtonUI.colors;

        if (!hasWord)
        {
            cb.normalColor = new Color(1f, 1f, 1f, 0.6f); // light transparent white
            doneButtonUI.colors = cb;
            doneButtonAnimator.enabled = false;
        }
        else
        {
            cb.normalColor = Color.white;
            doneButtonUI.colors = cb;
            doneButtonAnimator.enabled = true;
            doneButtonAnimator.Play("Blink", -1, 0); // restart blinking
        }
    }


    // Called when Done button is clicked
    public void OnDoneButtonClicked()
    {
        if (!wordSelected)
        {
            // feedbackText.text = "❗ Please select a word first!";
            // feedbackPanel.SetActive(true);
            //okButton.SetActive(false);
            return;
        }

        if (feedbackShown) return;

        allowBubbleRespawn = false; // 💥 stop new bubbles
        EvaluateSelectedAnswer();
        doneButtonAnimator.enabled = false;
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
        timerRunning = false; // pause

        // feedbackPanel.SetActive(true);
        okButton.SetActive(true);

        string explanationText;

        if (currentQuestion.explanations != null &&
            currentQuestion.explanations.ContainsKey(playerAnswer))
        {
            explanationText = currentQuestion.explanations[playerAnswer];
        }
        else
        {
            explanationText = "No explanation found.";
        }

        if (correct)
        {
            feedbackText.text = $"Correct!\n\n{explanationText}";
        }
        else if (wrongAttempts >= 2)
        {
            feedbackText.text = $" Incorrect.\n\n{explanationText}\n\nMoving to next question.";
        }
        else
        {
            feedbackText.text = $" Incorrect.\n\n{explanationText}\n\nTry again!";
        }
        feedbackPanel.GetComponent<FeedBackPanel>().ShowPanel();
    }

    public void OnOkButtonClicked()
    {
        //  feedbackPanel.SetActive(false);


        if (isAnswerCorrect || wrongAttempts >= 2 || currentTimeLeft <= 0f)
        {
            timerRunning = false;
            LoadNextQuestion();
        }
        else if (currentTimeLeft > 0f)
        {
            timerRunning = true; // resume
            feedbackShown = false;
        }
        wordSlot.text = "";
        feedbackPanel.GetComponent<FeedBackPanel>().HidePanel();
        // Else: retry allowed (player can click another bubble)
    }
    bool isBlinking;
    Coroutine blinkCoroutine;
    void Update()
    {
        if (timerRunning && !feedbackShown)
        {
            currentTimeLeft -= Time.deltaTime;

            // Optional: Update timer UI
            if (currentTimeLeft <= 5f && !isBlinking)
            {
                isBlinking = true;
                blinkCoroutine = StartCoroutine(BlinkWordSlot());
            }
            timerText.text = "Time: " + Mathf.CeilToInt(currentTimeLeft).ToString() + "s";

            if (currentTimeLeft <= 0f)
            {
                timerRunning = false;
                feedbackShown = true;
                allowBubbleRespawn = false;

                feedbackText.text = "Time's up!\nMoving to next question.";
                feedbackPanel.GetComponent<FeedBackPanel>().ShowPanel();
                okButton.SetActive(true);
            }
        }
    }
    CanvasGroup cg;
    IEnumerator BlinkWordSlot()
    {
      cg = wordSlotUI.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = wordSlotUI.AddComponent<CanvasGroup>();
        }

        while (true)
        {
            cg.alpha = 0.2f;
            yield return new WaitForSeconds(0.3f);
            cg.alpha = 1f;
            yield return new WaitForSeconds(0.3f);
        }
    }
void StopBlinking()
{
    if (blinkCoroutine != null)
        StopCoroutine(blinkCoroutine);

    isBlinking = false;

    //CanvasGroup cg = wordSlotUI.GetComponent<CanvasGroup>();
    if (cg != null) cg.alpha = 1f;
}
}