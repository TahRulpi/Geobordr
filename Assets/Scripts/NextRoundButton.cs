using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class NextRoundButton : MonoBehaviour
{
    public RoundManager roundManager;
    public CountryGameManager countryGameManager;

    public void ResetInputFieldColors()
    {
        if (roundManager == null || roundManager.SpawnedObjects == null)
        {
            Debug.LogWarning("Cannot reset colors: RoundManager or SpawnedObjects is null");
            return;
        }

        int resetCount = 0;
        foreach (GameObject spawnedPrefab in roundManager.SpawnedObjects)
        {
            AutocompleteInputField autocompleteInput = spawnedPrefab.GetComponentInChildren<AutocompleteInputField>();
            if (autocompleteInput != null)
            {
                autocompleteInput.ResetValidationState();
                resetCount++;
            }
            else
            {
                TMP_InputField inputField = spawnedPrefab.GetComponentInChildren<TMP_InputField>();
                if (inputField != null)
                {
                    SetInputFieldColor(inputField, Color.white);
                    resetCount++;
                }
            }
        }
        
    }

    public void OnNextRoundClicked()
    {
        Debug.Log("--- Moving to next round. ---");

        // The button is only active when all answers are correct, so no
        // additional validation is needed here.
        if (countryGameManager != null)
        {
            // Call the OnCorrectAnswer method to log the event and update the score.
            countryGameManager.OnCorrectAnswer();
            // Start the next round.
            countryGameManager.NextRound();
        }
        else
        {
            Debug.LogError("CountryGameManager reference is missing on NextRoundButton.");
        }
    }

    // The ValidateInputs() method is now obsolete as validation is handled by AutocompleteInputField
    // and CountryGameManager in real-time. It can be safely removed.
    // The following method is for setting the color of a generic TMP_InputField,
    // which is used as a fallback in the ResetInputFieldColors() method.
    private void SetInputFieldColor(TMP_InputField inputField, Color color)
    {
        Image backgroundImage = inputField.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
        else
        {
            Image parentImage = inputField.GetComponentInParent<Image>();
            if (parentImage != null)
            {
                parentImage.color = color;
            }
        }
    }
}