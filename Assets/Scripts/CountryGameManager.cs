using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro; // Add this for TextMeshPro support
using DG.Tweening;

public class CountryGameManager : MonoBehaviour
{
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
        // Add these two lines to reset the round and hide the button
        correctGuessesInThisRound = 0;
        if (nextRoundButtonObject != null) nextRoundButtonObject.SetActive(false);

        // Check if game is over
        if (isGameOver)
        {
            Debug.Log("Game is over! Cannot start new round.");
            return;
        }

        // Check if we've completed all rounds
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

        // Update the round display text
        UpdateRoundDisplay($"Round:{currentGameRound}");
        
        // Update the chance left display
        UpdateChanceLeftDisplay();

        // Select a random country cluster
        currentRoundIndex = random.Next(0, countryData.countryInfo.Length);
        currentRoundInfo = countryData.countryInfo[currentRoundIndex];
        
        // Determine how many countries to show (between min and max)
        int totalCountries = currentRoundInfo.CountryCount;
        int countriesToShow = Mathf.Clamp(
            random.Next(minCountriesPerRound, maxCountriesPerRound + 1), 
            minCountriesPerRound, 
            totalCountries
        );
        
        // Randomly select which countries to show
        // UPDATED: Now selects from the `countries` list of `CountryDetail`
        currentRoundCountries = currentRoundInfo.countries
            .OrderBy(x => random.Next())
            .Take(countriesToShow)
            .Select(c => c.countryName) // We only need the names for the current round logic
            .ToList();
        
        Debug.Log($"Round {currentGameRound}/{maxRounds} started with {countriesToShow} countries: {string.Join(", ", currentRoundCountries)}");
        
        // Create a temporary CountryData with only the selected countries
        CreateFilteredCountryData(countriesToShow);
        
        // Use your existing RoundManager to load the round (always index 0 for filtered data)
        roundManager.LoadRound(0);
        
        // Reset input field colors after objects are spawned
        StartCoroutine(ResetColorsAfterDelay());
        
        // Display the map image
        DisplayMapImage();
    }

    private void CreateFilteredCountryData(int countriesToShow)
    {
        // UPDATED: Filter the `CountryDetail` list based on the selected country names
        var selectedCountryDetails = currentRoundInfo.countries
            .Where(detail => currentRoundCountries.Contains(detail.countryName))
            .ToList();

        // Create a temporary CountryInfo with only the selected countries and prefabs
        CountryInfo filteredInfo = new CountryInfo
        {
            gridImage = currentRoundInfo.gridImage,
            countries = selectedCountryDetails, // Use the filtered list of CountryDetail
            optionsPrefabs = new GameObject[countriesToShow]
        };

        // Take only the number of prefabs we need
        for (int i = 0; i < countriesToShow && i < currentRoundInfo.optionsPrefabs.Length; i++)
        {
            filteredInfo.optionsPrefabs[i] = currentRoundInfo.optionsPrefabs[i];
        }

        // Create temporary CountryData
        CountryData tempCountryData = ScriptableObject.CreateInstance<CountryData>();
        tempCountryData.countryInfo = new CountryInfo[] { filteredInfo };

        // Temporarily assign this filtered data to RoundManager
        roundManager.CountryData = tempCountryData;

        Debug.Log($"Created filtered CountryData with {filteredInfo.countries.Count} countries and {filteredInfo.optionsPrefabs.Length} prefabs");
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

        // Update chance left display
        UpdateChanceLeftDisplay();

        if (totalAttempts >= maxTotalAttempts)
        {
            // Game Over - Used all total attempts
            Debug.LogError($"💀 GAME OVER! Used all {maxTotalAttempts} attempts across all rounds");
            ShowGameOverText($"💀 GAME OVER! 💀\nUsed all {maxTotalAttempts} attempts\n\nCorrect answers were:\n{string.Join(", ", currentRoundCountries)}");
            isGameOver = true;

            if (gamePanel != null) gamePanel.SetActive(false);

            ShowResultPopup();

            UpdateFinalScoreDisplay();
        }
    }


    public void OnCorrectAnswer()
    {
        Debug.Log($"✅ Correct! Moving to next round. Total attempts used so far: {totalAttempts}/{maxTotalAttempts}");

        // Just update chance display, don’t increment score here
        UpdateChanceLeftDisplay();
    }


    public void IncrementCorrectGuesses()
    {
        correctGuessesInThisRound++;
        totalCorrectGuesses++;
        Debug.Log($"Correct guess! Progress for this round: {correctGuessesInThisRound}/{currentRoundCountries.Count}");
        
        // Check if the round is complete
        if (correctGuessesInThisRound >= currentRoundCountries.Count)
        {
            Debug.Log("🎉 Round Complete! Showing Next Round button.");
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
            mapDisplayImage.sprite = currentRoundInfo.gridImage;
            Debug.Log($"Displaying map: {currentRoundInfo.gridImage.name}");
        }
        else
        {
            Debug.LogWarning("Map Display Image not assigned or no grid image available!");
        }
    }

    // Public methods for UI buttons
    public void NextRound()
    {
        if (!isGameOver)
        {
            currentGameRound++; // Increment round counter
            StartNewRound();
        }
    }

    public void SkipRound()
    {
        if (!isGameOver)
        {
            currentGameRound++; // Increment round counter
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

        // Reset values
        totalAttempts = 0;
        currentGameRound = 1;
        isGameOver = false;
        currentRoundCountries = new List<string>();
        correctGuessesInThisRound = 0;
        totalCorrectGuesses = 0;

        // Reset RoundManager state
        if (roundManager != null)
        {
            roundManager.ResetManager();
        }

        // Reset UI & Panels
        if (gamePanel != null) gamePanel.SetActive(true);
        if (resultPanel != null) resultPanel.SetActive(false);

        // Clear the final score text to prevent it from showing on the restart
        if (finalScoreText != null)
        {
            finalScoreText.text = "";
        }

        HideGameOverText();
        UpdateRoundDisplay($"Round:{currentGameRound}");
        UpdateChanceLeftDisplay(); // will show "Chance Left: 6"

        // Start new round fresh
        StartNewRound();

        Debug.Log($"Game restarted: Round {currentGameRound}, Attempts {totalAttempts}/{maxTotalAttempts}");
    }

    // Getter methods for external scripts
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
        // Wait one frame to ensure objects are spawned
        yield return null;
        
        // Reset input field colors for the new round
        if (nextRoundButton != null)
        {
            nextRoundButton.ResetInputFieldColors();
        }
    }

    private void UpdateFinalScoreDisplay()
    {
        if (finalScoreText != null)
        {
            // Display the total correct guesses on the result panel
            //finalScoreText.text = $"YOUR CORRECT GUESSES\n<size=150%>{totalCorrectGuesses}</size>";
            
            finalScoreText.text = $"{totalCorrectGuesses}";
            finalScoreText_2.text = $"{totalCorrectGuesses}";
            Debug.Log($"Final Score Displayed: {totalCorrectGuesses} correct guesses.");
        }
    }

    private void ShowResultPopup()
    {
        if (resultPanel == null) return;

        resultPanel.SetActive(true);

        // Reset state
        resultPanel.transform.localScale = Vector3.zero;
        CanvasGroup canvasGroup = resultPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = resultPanel.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        // Animate: fade + scale
        Sequence popupSequence = DOTween.Sequence();
        popupSequence.Append(resultPanel.transform.DOScale(1f, 0.4f).SetEase(Ease.OutBack));
        popupSequence.Join(canvasGroup.DOFade(1f, 0.4f));
    }
}