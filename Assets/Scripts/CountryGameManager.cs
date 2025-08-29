using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro; // Add this for TextMeshPro support
using DG.Tweening;

public class CountryGameManager : MonoBehaviour
{
    public GameObject gameView; // Reference to the main game view (optional)


    [Header("Game Configuration")]
    public CountryData countryData;
    public RoundManager roundManager; // Use your existing RoundManager
    public int minCountriesPerRound = 2;
    public int maxCountriesPerRound = 5;
    public int maxRounds = 10; // Total number of rounds
    public int maxTotalAttempts = 6; // Total attempts across all rounds
    
    [Header("UI References")]
    public UnityEngine.UI.Image mapDisplayImage;
    public TextMeshProUGUI roundDisplayText; // Reference to your "Round -" text
    public TextMeshProUGUI gameOverText; // Reference to your game over text
    public TextMeshProUGUI chanceLeftText; // Reference to your "Chance Left: " text
    public NextRoundButton nextRoundButton; // Reference to reset input field colors
    public GameObject nextRoundButtonObject;
    
    [Header("Result Panel UI")] // Add this new header
    public TMP_Text finalScoreText;
    public TMP_Text finalScoreText_2;


    [Header("Current Round Info")]
    [SerializeField] private int currentRoundIndex;
    [SerializeField] private int currentGameRound = 1; // Track which round we're on (1-10)
    [SerializeField] private int totalAttempts = 0; // Track total attempts across all rounds
    [SerializeField] private CountryInfo currentRoundInfo;
    [SerializeField] private List<string> currentRoundCountries; // This still holds strings for validation
    [SerializeField] private bool isGameOver = false;


    [SerializeField] private int correctGuessesInThisRound = 0;

    [Header("Score Tracking")]
    [SerializeField] private int totalCorrectGuesses = 0; // Add this line

    private System.Random random;



    [Header("Panels")]
    public GameObject gamePanel;   // The main gameplay panel
    public GameObject resultPanel; // The result screen panel

    private void Start()
    {
        random = new System.Random();
        StartNewRound();
    }

    public void StartNewRound()
    {
        correctGuessesInThisRound = 0;
        if (nextRoundButtonObject != null) nextRoundButtonObject.SetActive(false);

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

        HideGameOverText();
        UpdateRoundDisplay($"Round:{currentGameRound}");
        UpdateChanceLeftDisplay();

        // ✅ *** FIX #1: Only select from rounds that have a map image. ***
        var roundsWithMaps = countryData.countryInfo.Where(info => info.gridImage != null).ToList();
        if (roundsWithMaps.Count == 0)
        {
            Debug.LogError("No rounds with map images found in CountryData!");
            return;
        }
        
        // Now, select a random country cluster from our filtered list
        currentRoundInfo = roundsWithMaps[random.Next(0, roundsWithMaps.Count)];
        
        // Determine how many countries to show (between min and max)
        int totalCountries = currentRoundInfo.CountryCount;
        int countriesToShow = Mathf.Clamp(
            random.Next(minCountriesPerRound, maxCountriesPerRound + 1), 
            minCountriesPerRound, 
            totalCountries
        );
        
        currentRoundCountries = currentRoundInfo.countries
            .OrderBy(x => random.Next())
            .Take(countriesToShow)
            .Select(c => c.countryName)
            .ToList();
        
        Debug.Log($"Round {currentGameRound}/{maxRounds} started with {countriesToShow} countries: {string.Join(", ", currentRoundCountries)}");
        
        CreateFilteredCountryData(countriesToShow);
        
        roundManager.LoadRound(0);
        
        StartCoroutine(ResetColorsAfterDelay());
        
        DisplayMapImage();
    }

    private void CreateFilteredCountryData(int countriesToShow)
    {
        var selectedCountryDetails = currentRoundInfo.countries
            .Where(detail => currentRoundCountries.Contains(detail.countryName))
            .ToList();

        CountryInfo filteredInfo = new CountryInfo
        {
            gridImage = currentRoundInfo.gridImage,
            countries = selectedCountryDetails,
            optionsPrefabs = new GameObject[countriesToShow]
        };

        for (int i = 0; i < countriesToShow && i < currentRoundInfo.optionsPrefabs.Length; i++)
        {
            filteredInfo.optionsPrefabs[i] = currentRoundInfo.optionsPrefabs[i];
        }

        CountryData tempCountryData = ScriptableObject.CreateInstance<CountryData>();
        tempCountryData.countryInfo = new CountryInfo[] { filteredInfo };

        roundManager.CountryData = tempCountryData;
    }

    private void UpdateRoundDisplay(string text)
    {
        if (roundDisplayText != null)
        {
            roundDisplayText.text = text;
        }
    }

    private void UpdateChanceLeftDisplay()
    {
        if (chanceLeftText != null)
        {
            int chancesRemaining = maxTotalAttempts - totalAttempts;
            chanceLeftText.text = $"Chance Left: {chancesRemaining}";
        }
    }


    private void ShowGameOverText(string message)
    {
        if (gameOverText != null)
        {
            gameOverText.text = message;
            gameOverText.gameObject.SetActive(true);
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
        UpdateChanceLeftDisplay();

        if (totalAttempts >= maxTotalAttempts)
        {
            Debug.LogError($"💀 GAME OVER! Used all {maxTotalAttempts} attempts.");
            ShowGameOverText($"💀 GAME OVER! 💀\nUsed all {maxTotalAttempts} attempts\n\nCorrect answers were:\n{string.Join(", ", currentRoundCountries)}");
            isGameOver = true;

            if (gamePanel != null) gamePanel.SetActive(false);
            ShowResultPopup();
            UpdateFinalScoreDisplay();
        }
    }


    public void OnCorrectAnswer()
    {
        Debug.Log($"✅ Correct! Moving to next round.");
        UpdateChanceLeftDisplay();
    }


    public void IncrementCorrectGuesses()
    {
        correctGuessesInThisRound++;
        totalCorrectGuesses++;
        Debug.Log($"Correct guess! Progress: {correctGuessesInThisRound}/{currentRoundCountries.Count}");
        
        if (correctGuessesInThisRound >= currentRoundCountries.Count)
        {
            Debug.Log("🎉 Round Complete!");
            if (nextRoundButtonObject != null)
            {
                nextRoundButtonObject.SetActive(true);
            }
        }
    }

    private void DisplayMapImage()
    {
        if (mapDisplayImage != null && currentRoundInfo.gridImage != null)
        {
            // ✅ *** FIX #2 (Part A): If there is a map, make sure the image component is active. ***
            mapDisplayImage.gameObject.SetActive(true);
            mapDisplayImage.sprite = currentRoundInfo.gridImage;
            Debug.Log($"Displaying map: {currentRoundInfo.gridImage.name}");
        }
        else
        {
            // ✅ *** FIX #2 (Part B): If there is NO map, hide the image component entirely. ***
            if (mapDisplayImage != null)
            {
                mapDisplayImage.gameObject.SetActive(false);
            }
            Debug.LogWarning("No grid image available for this round. Hiding map display.");
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

    public void RestartGame()
    {
        Debug.Log("🔄 Restarting Game...");

        totalAttempts = 0;
        currentGameRound = 1;
        isGameOver = false;
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

        StartNewRound();
    }

    public List<string> GetCurrentRoundCountries()
    {
        return currentRoundCountries;
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
            finalScoreText.text = $"{totalCorrectGuesses}";
            finalScoreText_2.text = $"{totalCorrectGuesses}";
        }
    }

    private void ShowResultPopup()
    {
        if (resultPanel == null) return;
        resultPanel.SetActive(true);

        resultPanel.transform.localScale = Vector3.zero;
        var canvasGroup = resultPanel.GetComponent<CanvasGroup>() ?? resultPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        Sequence popupSequence = DOTween.Sequence();
        popupSequence.Append(resultPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
        popupSequence.Join(canvasGroup.DOFade(1f, 0.4f));
    }
    
    public void MoveGameViewUp()
    {
        if (gameView != null)
        {
            gameView.transform.DOLocalMoveY(180f, 0.5f).SetEase(Ease.OutQuad);
        }
    }
    
    public void MoveGameViewDown()
    {
        if (gameView != null)
        {
            gameView.transform.DOLocalMoveY(0f, 0.5f).SetEase(Ease.OutQuad);
        }
    }
}