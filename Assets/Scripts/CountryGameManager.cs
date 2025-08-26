using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class CountryGameManager : MonoBehaviour
{
    [Header("Game Configuration")]
    public CountryData countryData;
    public RoundManager roundManager;
    public int minCountriesPerRound = 2;
    public int maxCountriesPerRound = 5;
    public int maxRounds = 10;
    public int maxTotalAttempts = 6;

    [Header("UI References")]
    public UnityEngine.UI.Image mapDisplayImage;
    public TextMeshProUGUI roundDisplayText;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI chanceLeftText;
    public NextRoundButton nextRoundButton;

    [Header("Result Panel UI")]
    public TextMeshProUGUI finalScoreText;

    [Header("Current Round Info")]
    [SerializeField] private int currentRoundIndex;
    [SerializeField] private int currentGameRound = 1;
    [SerializeField] private int totalAttempts = 0;
    [SerializeField] private CountryInfo currentRoundInfo;
    [SerializeField] private List<string> currentRoundCountries;
    [SerializeField] private bool isGameOver = false;

    [SerializeField] private int totalCorrectGuesses = 0;
    [SerializeField] private int correctGuessesThisRound = 0;
    private System.Random random;

    [Header("Panels")]
    public GameObject gamePanel;
    public GameObject resultPanel;

    private void Start()
    {
        random = new System.Random();
        StartNewRound();
    }

    public void StartNewRound()
    {
        if (isGameOver)
        {
            Debug.Log("Game is over! Cannot start new round.");
            return;
        }

        if (currentGameRound > maxRounds)
        {
            Debug.Log($"🎉 GAME COMPLETED! You finished all {maxRounds} rounds! 🎉");
            UpdateRoundDisplay("GAME COMPLETE!");
            ShowGameOverText("🎉 CONGRATULATIONS! 🎉\nYou completed all rounds!");
            isGameOver = true;
            return;
        }

        if (countryData == null || countryData.countryInfo.Length == 0)
        {
            Debug.LogError("No country data available!");
            return;
        }

        if (roundManager == null)
        {
            Debug.LogError("RoundManager not assigned!");
            return;
        }

        if (nextRoundButton != null)
        {
            nextRoundButton.gameObject.SetActive(false);
        }

        HideGameOverText();
        UpdateRoundDisplay($"Round:{currentGameRound}");
        UpdateChanceLeftDisplay();

        currentRoundIndex = random.Next(0, countryData.countryInfo.Length);
        currentRoundInfo = countryData.countryInfo[currentRoundIndex];

        int totalCountries = currentRoundInfo.countryName.Count;
        int countriesToShow = Mathf.Clamp(
            random.Next(minCountriesPerRound, maxCountriesPerRound + 1),
            minCountriesPerRound,
            totalCountries
        );

        currentRoundCountries = currentRoundInfo.countryName
            .OrderBy(x => random.Next())
            .Take(countriesToShow)
            .ToList();

        Debug.Log($"Round {currentGameRound}/{maxRounds} started with {countriesToShow} countries: {string.Join(", ", currentRoundCountries)}");

        CreateFilteredCountryData(countriesToShow);
        roundManager.LoadRound(0);
        StartCoroutine(ResetColorsAfterDelay());
        DisplayMapImage();

        correctGuessesThisRound = 0;
    }

    private void CreateFilteredCountryData(int countriesToShow)
    {
        CountryInfo filteredInfo = new CountryInfo
        {
            gridImage = currentRoundInfo.gridImage,
            countryName = new List<string>(currentRoundCountries),
            optionsPrefabs = new GameObject[countriesToShow]
        };

        for (int i = 0; i < countriesToShow && i < currentRoundInfo.optionsPrefabs.Length; i++)
        {
            filteredInfo.optionsPrefabs[i] = currentRoundInfo.optionsPrefabs[i];
        }

        CountryData tempCountryData = ScriptableObject.CreateInstance<CountryData>();
        tempCountryData.countryInfo = new CountryInfo[] { filteredInfo };
        roundManager.CountryData = tempCountryData;
        Debug.Log($"Created filtered CountryData with {filteredInfo.countryName.Count} countries and {filteredInfo.optionsPrefabs.Length} prefabs");
    }

    private void UpdateRoundDisplay(string text)
    {
        if (roundDisplayText != null)
        {
            roundDisplayText.text = text;
            Debug.Log($"Updated round display: {text}");
        }
        else
        {
            Debug.LogWarning("Round Display Text not assigned! Please assign your TextMeshPro component in the inspector.");
        }
    }

    private void UpdateChanceLeftDisplay()
    {
        if (chanceLeftText != null)
        {
            int chancesRemaining = maxTotalAttempts - totalAttempts;
            chanceLeftText.text = $"Chance Left: {chancesRemaining}";
            Debug.Log($"Updated chance left: {chancesRemaining} (Total attempts: {totalAttempts}/{maxTotalAttempts})");
        }
    }

    private void ShowGameOverText(string message)
    {
        if (gameOverText != null)
        {
            gameOverText.text = message;
            gameOverText.gameObject.SetActive(true);
            Debug.Log($"Game Over: {message}");
        }
        else
        {
            Debug.LogWarning("Game Over Text not assigned! Please assign your TextMeshPro component in the inspector.");
        }
    }

    private void HideGameOverText()
    {
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
    }

    public void OnWrongAnswer()
    {
        totalAttempts++;
        int attemptsLeft = maxTotalAttempts - totalAttempts;
        Debug.Log($"Wrong answer! Total attempts: {totalAttempts}/{maxTotalAttempts}. {attemptsLeft} attempts remaining across all rounds.");
        UpdateChanceLeftDisplay();

        if (totalAttempts >= maxTotalAttempts)
        {
            Debug.LogError($"💀 GAME OVER! Used all {maxTotalAttempts} attempts across all rounds");
            ShowGameOverText($"💀 GAME OVER! 💀\nUsed all {maxTotalAttempts} attempts\n\nCorrect answers were:\n{string.Join(", ", currentRoundCountries)}");
            isGameOver = true;

            if (gamePanel != null) gamePanel.SetActive(false);
            ShowResultPopup();
            UpdateFinalScoreDisplay();
        }
    }

    // RE-ADDED: This method is called from NextRoundButton to signal a correct round completion
    public void OnCorrectAnswer()
    {
        Debug.Log($"✅ All answers were correct for this round! Moving to the next round.");
    }

    public void IncrementCorrectGuesses()
    {
        totalCorrectGuesses++;
        Debug.Log($"Correct guess! Total correct guesses: {totalCorrectGuesses}");
    }

    public void IncrementCorrectGuessesThisRound()
    {
        correctGuessesThisRound++;
        Debug.Log($"Correct guess for this round! Total: {correctGuessesThisRound}");
    }

    public void CheckRoundCompletion()
    {
        if (nextRoundButton == null) return;

        int countriesRequired = GetCurrentRoundCountries().Count;

        if (correctGuessesThisRound >= countriesRequired)
        {
            Debug.Log("✅ All answers for this round are correct! Displaying Next button.");
            nextRoundButton.gameObject.SetActive(true);
        }
    }

    private void DisplayMapImage()
    {
        if (mapDisplayImage != null && currentRoundInfo.gridImage != null)
        {
            mapDisplayImage.sprite = currentRoundInfo.gridImage;
            Debug.Log($"Displaying map: {currentRoundInfo.gridImage.name}");
        }
        else
        {
            Debug.LogWarning("Map Display Image not assigned or no grid image available!");
        }
    }

    public void NextRound()
    {
        if (!isGameOver)
        {
            currentGameRound++;
            StartNewRound();
        }
    }

    public void SkipRound()
    {
        if (!isGameOver)
        {
            currentGameRound++;
            StartNewRound();
        }
    }

    public void ShowAnswers()
    {
        Debug.Log($"Round {currentGameRound}/{maxRounds} - Answers: {string.Join(", ", currentRoundCountries)}");
    }

    public void RestartGame()
    {
        Debug.Log("🔄 Restarting Game...");
        totalAttempts = 0;
        currentGameRound = 1;
        isGameOver = false;
        currentRoundCountries = new List<string>();
        totalCorrectGuesses = 0;

        if (roundManager != null)
        {
            roundManager.ResetManager();
        }

        if (gamePanel != null) gamePanel.SetActive(true);
        if (resultPanel != null) resultPanel.SetActive(false);

        if (finalScoreText != null)
        {
            finalScoreText.text = "";
        }

        HideGameOverText();
        UpdateRoundDisplay($"Round:{currentGameRound}");
        UpdateChanceLeftDisplay();
        StartNewRound();
        Debug.Log($"Game restarted: Round {currentGameRound}, Attempts {totalAttempts}/{maxTotalAttempts}");
    }

    public bool IsGameOver()
    {
        return isGameOver;
    }

    public int GetCurrentAttempts()
    {
        return totalAttempts;
    }

    public int GetMaxAttempts()
    {
        return maxTotalAttempts;
    }

    public List<string> GetCurrentRoundCountries()
    {
        return currentRoundCountries;
    }

    public int GetTotalAvailableRounds()
    {
        return countryData != null ? countryData.countryInfo.Length : 0;
    }

    public string GetCurrentRoundInfo()
    {
        if (currentRoundCountries != null && currentRoundCountries.Count > 0)
        {
            return $"Round: {string.Join(", ", currentRoundCountries)} (Total clusters: {GetTotalAvailableRounds()})";
        }
        return "No active round";
    }

    private void OnValidate()
    {
        minCountriesPerRound = Mathf.Max(1, minCountriesPerRound);
        maxCountriesPerRound = Mathf.Max(minCountriesPerRound, maxCountriesPerRound);
    }

    private IEnumerator ResetColorsAfterDelay()
    {
        yield return null;
        if (nextRoundButton != null)
        {
            nextRoundButton.ResetInputFieldColors();
        }
    }

    private void UpdateFinalScoreDisplay()
    {
        if (finalScoreText != null)
        {
            finalScoreText.text = $"YOUR CORRECT GUESSES\n<size=150%>{totalCorrectGuesses}</size>";
            Debug.Log($"Final Score Displayed: {totalCorrectGuesses} correct guesses.");
        }
    }

    private void ShowResultPopup()
    {
        if (resultPanel == null) return;
        resultPanel.SetActive(true);
        resultPanel.transform.localScale = Vector3.zero;
        CanvasGroup canvasGroup = resultPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = resultPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        Sequence popupSequence = DOTween.Sequence();
        popupSequence.Append(resultPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
        popupSequence.Join(canvasGroup.DOFade(1f, 0.4f));
    }
}