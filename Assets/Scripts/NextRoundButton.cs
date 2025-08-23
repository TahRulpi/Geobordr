using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
// You need to add this line to use TextMeshPro components!
using TMPro;

public class NextRoundButton : MonoBehaviour
{
    public RoundManager roundManager;
    public CountryGameManager countryGameManager; // This is all you need now!

    public void OnNextRoundClicked()
    {
        Debug.Log("--- Next Round button clicked. Starting validation. ---");
        
        // Check if game is over
        if (countryGameManager != null && countryGameManager.IsGameOver())
        {
            Debug.Log("Game is over! Cannot proceed.");
            return;
        }
        
        if (ValidateInputs())
        {
            Debug.Log("✅ All answers are correct! Moving to the next round.");
            
            // Notify CountryGameManager of correct answer
            if (countryGameManager != null)
            {
                countryGameManager.OnCorrectAnswer();
            }
            
            countryGameManager.NextRound();
        }
        else
        {
            Debug.Log("❌ Some answers are wrong! Please try again.");
            
            // Notify CountryGameManager of wrong answer
            if (countryGameManager != null)
            {
                countryGameManager.OnWrongAnswer();
            }
        }
    }

    private bool ValidateInputs()
    {
        if (countryGameManager == null || roundManager == null || roundManager.CountryData == null)
        {
            Debug.LogError("? Missing required references in NextRoundButton script. Please assign them in the Inspector.");
            return false;
        }

        // Since we're using filtered CountryData, we always use index 0
        int currentRoundIndex = 0;

        if (currentRoundIndex < 0 || currentRoundIndex >= roundManager.CountryData.countryInfo.Length)
        {
            Debug.LogError("? Round index is out of bounds. Cannot validate.");
            return false;
        }

        CountryInfo currentRoundInfo = roundManager.CountryData.countryInfo[currentRoundIndex];

        if (roundManager.SpawnedObjects.Count != currentRoundInfo.countryName.Count)
        {
            Debug.LogError($"? Data Mismatch: Spawned objects ({roundManager.SpawnedObjects.Count}) do not match the number of country names ({currentRoundInfo.countryName.Count}). Check your CountryData and prefabs.");
            return false;
        }

        Debug.Log($"Validating {roundManager.SpawnedObjects.Count} country names for round {currentRoundIndex + 1}...");

        // Get all player answers
        List<string> playerAnswers = new List<string>();
        List<string> correctAnswers = new List<string>();
        
        for (int i = 0; i < roundManager.SpawnedObjects.Count; i++)
        {
            GameObject spawnedPrefab = roundManager.SpawnedObjects[i];
            string playerAnswer = "";
            bool foundInputComponent = false;

            // Try to find AutocompleteInputField component first
            AutocompleteInputField autocompleteInput = spawnedPrefab.GetComponentInChildren<AutocompleteInputField>();
            if (autocompleteInput != null)
            {
                playerAnswer = autocompleteInput.GetSelectedCountry();
                foundInputComponent = true;
                Debug.Log($"Found autocomplete component - text: '{playerAnswer}'");
            }
            else
            {
                // Fallback to TMP_InputField
                TMP_InputField input = spawnedPrefab.GetComponentInChildren<TMP_InputField>();
                if (input != null)
                {
                    playerAnswer = input.text.Trim();
                    foundInputComponent = true;
                    Debug.Log($"Found input field component - text: '{playerAnswer}'");
                }
            }

            if (!foundInputComponent)
            {
                Debug.LogError($"? FATAL ERROR: The spawned object at index {i} ('{spawnedPrefab.name}') is missing AutocompleteInputField or TMP_InputField components. You must add one of them to the prefab.");
                return false;
            }

            playerAnswers.Add(playerAnswer.ToLower());
            correctAnswers.Add(currentRoundInfo.countryName[i].Trim().ToLower());
        }

        // Check if all correct answers are present (regardless of order)
        List<string> missingAnswers = new List<string>();
        List<string> incorrectAnswers = new List<string>();
        
        Debug.Log($"Player answers: [{string.Join(", ", playerAnswers)}]");
        Debug.Log($"Correct answers: [{string.Join(", ", correctAnswers)}]");

        foreach (string correctAnswer in correctAnswers)
        {
            if (!playerAnswers.Contains(correctAnswer))
            {
                missingAnswers.Add(correctAnswer);
            }
        }

        foreach (string playerAnswer in playerAnswers)
        {
            if (!string.IsNullOrEmpty(playerAnswer) && !correctAnswers.Contains(playerAnswer))
            {
                incorrectAnswers.Add(playerAnswer);
            }
        }

        bool allCorrect = missingAnswers.Count == 0 && incorrectAnswers.Count == 0;

        if (!allCorrect)
        {
            Debug.LogError("========== WRONG ANSWERS ==========");
            if (missingAnswers.Count > 0)
            {
                Debug.LogError($"   🔴 MISSING: You need to enter these countries: {string.Join(", ", missingAnswers)}");
            }
            if (incorrectAnswers.Count > 0)
            {
                Debug.LogError($"   🔴 INCORRECT: These are not valid for this map: {string.Join(", ", incorrectAnswers)}");
            }
            Debug.LogError("===================================");
        }
        else
        {
            Debug.Log("🎉 ALL ANSWERS CORRECT! (Order doesn't matter) 🎉");
        }

        return allCorrect;
    }
}
