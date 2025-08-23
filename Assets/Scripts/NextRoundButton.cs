using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
// You need to add this line to use TextMeshPro components!
using TMPro;

public class NextRoundButton : MonoBehaviour
{
    public GameManager gameManager;
    public RoundManager roundManager;

    public void OnNextRoundClicked()
    {
        Debug.Log("--- Next Round button clicked. Starting validation. ---");
        if (ValidateInputs())
        {
            Debug.Log("? All answers are correct! Moving to the next round.");
            gameManager.NextRound();
        }
        else
        {
            Debug.Log("? Some answers are wrong! Please try again.");
        }
    }

    private bool ValidateInputs()
    {
        if (gameManager == null || roundManager == null || roundManager.CountryData == null)
        {
            Debug.LogError("? Missing required references in NextRoundButton script. Please assign them in the Inspector.");
            return false;
        }

        int currentRoundIndex = gameManager.GetCurrentRoundIndex();

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

        for (int i = 0; i < roundManager.SpawnedObjects.Count; i++)
        {
            GameObject spawnedPrefab = roundManager.SpawnedObjects[i];

            // --- THE FIX IS ON THIS LINE ---
            // We now look for the correct component type: TMP_InputField
            TMP_InputField input = spawnedPrefab.GetComponentInChildren<TMP_InputField>();

            if (input == null)
            {
                Debug.LogError($"? FATAL ERROR: The spawned object at index {i} ('{spawnedPrefab.name}') is missing an InputField or TMP_InputField component. You must add it to the prefab.");
                return false;
            }

            string playerAnswer = input.text.Trim().ToLower();
            string correctAnswer = currentRoundInfo.countryName[i].Trim().ToLower();

            if (playerAnswer == correctAnswer)
            {
                Debug.Log($"   ? Match! Player entered '{playerAnswer}' which matches '{correctAnswer}'.");
            }
            else
            {
                Debug.Log($"   ? Mismatch! Player entered '{playerAnswer}', but the correct answer is '{correctAnswer}'.");
                return false;
            }
        }

        return true;
    }
}
