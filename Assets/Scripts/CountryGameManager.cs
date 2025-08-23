using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro; // Add this for TextMeshPro support

public class CountryGameManager : MonoBehaviour
{
    [Header("Game Configuration")]
    public CountryData countryData;
    public RoundManager roundManager; // Use your existing RoundManager
    public int minCountriesPerRound = 2;
    public int maxCountriesPerRound = 5;
    public int maxRounds = 10; // Total number of rounds
    public int maxAttempts = 3; // Maximum attempts per round
    
    [Header("UI References")]
    public UnityEngine.UI.Image mapDisplayImage;
    public TextMeshProUGUI roundDisplayText; // Reference to your "Round -" text
    public TextMeshProUGUI gameOverText; // Reference to your game over text
    public TextMeshProUGUI chanceLeftText; // Reference to your "Chance Left: " text
    
    [Header("Current Round Info")]
    [SerializeField] private int currentRoundIndex;
    [SerializeField] private int currentGameRound = 1; // Track which round we're on (1-10)
    [SerializeField] private int currentAttempts = 0; // Track attempts in current round
    [SerializeField] private CountryInfo currentRoundInfo;
    [SerializeField] private List<string> currentRoundCountries;
    [SerializeField] private bool isGameOver = false;
    
    private System.Random random;

    private void Start()
    {
        random = new System.Random();
        StartNewRound();
    }

    public void StartNewRound()
    {
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

        // Reset attempts for new round
        currentAttempts = 0;
        HideGameOverText();

        // Update the round display text
        UpdateRoundDisplay($"Round - {currentGameRound}");
        
        // Update the chance left display
        UpdateChanceLeftDisplay();

        // Select a random country cluster
        currentRoundIndex = random.Next(0, countryData.countryInfo.Length);
        currentRoundInfo = countryData.countryInfo[currentRoundIndex];
        
        // Determine how many countries to show (between min and max)
        int totalCountries = currentRoundInfo.countryName.Count;
        int countriesToShow = Mathf.Clamp(
            random.Next(minCountriesPerRound, maxCountriesPerRound + 1), 
            minCountriesPerRound, 
            totalCountries
        );
        
        // Randomly select which countries to show
        currentRoundCountries = currentRoundInfo.countryName
            .OrderBy(x => random.Next())
            .Take(countriesToShow)
            .ToList();
        
        Debug.Log($"Round {currentGameRound}/{maxRounds} started with {countriesToShow} countries: {string.Join(", ", currentRoundCountries)}");
        
        // Create a temporary CountryData with only the selected countries
        CreateFilteredCountryData(countriesToShow);
        
        // Use your existing RoundManager to load the round (always index 0 for filtered data)
        roundManager.LoadRound(0);
        
        // Display the map image
        DisplayMapImage();
    }

    private void CreateFilteredCountryData(int countriesToShow)
    {
        // Create a temporary CountryInfo with only the selected countries and prefabs
        CountryInfo filteredInfo = new CountryInfo
        {
            gridImage = currentRoundInfo.gridImage,
            countryName = new List<string>(currentRoundCountries),
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
            int chancesRemaining = maxAttempts - currentAttempts;
            chanceLeftText.text = $"Chance Left: {chancesRemaining}";
            Debug.Log($"Updated chance left: {chancesRemaining}");
        }
        else
        {
            Debug.LogWarning("Chance Left Text not assigned! Please assign your TextMeshPro component in the inspector.");
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
        currentAttempts++;
        int attemptsLeft = maxAttempts - currentAttempts;
        
        Debug.Log($"Wrong answer! Attempt {currentAttempts}/{maxAttempts}. {attemptsLeft} attempts remaining.");
        
        // Update chance left display
        UpdateChanceLeftDisplay();
        
        if (currentAttempts >= maxAttempts)
        {
            // Game Over - Failed 3 times
            Debug.LogError($"💀 GAME OVER! Failed {maxAttempts} times in Round {currentGameRound}");
            ShowGameOverText($"💀 GAME OVER! 💀\nFailed {maxAttempts} times in Round {currentGameRound}\n\nCorrect answers were:\n{string.Join(", ", currentRoundCountries)}");
            isGameOver = true;
        }
        else
        {
            // Still have attempts left
            UpdateRoundDisplay($"Round - {currentGameRound} (Attempt {currentAttempts + 1}/{maxAttempts})");
        }
    }

    public void OnCorrectAnswer()
    {
        Debug.Log($"✅ Correct! Moving to next round. Attempts used: {currentAttempts + 1}/{maxAttempts}");
        // Reset attempts counter since round was completed successfully
        currentAttempts = 0;
        UpdateChanceLeftDisplay();
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
        currentGameRound = 1;
        currentAttempts = 0;
        isGameOver = false;
        HideGameOverText();
        UpdateRoundDisplay($"Round - {currentGameRound}");
        UpdateChanceLeftDisplay();
        StartNewRound();
    }

    // Getter methods for external scripts
    public bool IsGameOver()
    {
        return isGameOver;
    }

    public int GetCurrentAttempts()
    {
        return currentAttempts;
    }

    public int GetMaxAttempts()
    {
        return maxAttempts;
    }

    // Helper method to get available countries count
    public int GetTotalAvailableRounds()
    {
        return countryData != null ? countryData.countryInfo.Length : 0;
    }

    // Helper method to get current round info for debugging
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
}
