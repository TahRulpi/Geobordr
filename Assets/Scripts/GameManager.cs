using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public RoundManager roundManager;

    // A list to hold the indices of all available rounds
    private List<int> availableRounds = new List<int>();

    // The index of the current round being played (for internal tracking)
    private int currentRoundIndex;

    private void Start()
    {
        Debug.Log("--- Game started. Initializing rounds. ---");
        InitializeRounds();
        StartRound();
    }

    // This new method populates the list of available rounds
    private void InitializeRounds()
    {
        if (roundManager == null || roundManager.CountryData == null)
        {
            Debug.LogError("? GameManager is missing a reference to RoundManager or CountryData!");
            return;
        }

        // Fill the list with indices from 0 to the total number of rounds
        availableRounds.Clear();
        for (int i = 0; i < roundManager.CountryData.countryInfo.Length; i++)
        {
            availableRounds.Add(i);
        }
    }

    public void StartRound()
    {
        // Check if there are any rounds left to play
        if (availableRounds.Count == 0)
        {
            Debug.Log("?? All rounds have been completed! Game Over.");
            return;
        }

        // --- THE FIX IS IN THIS SECTION ---
        // 1. Pick a random index from the list of available rounds
        int randomIndex = Random.Range(0, availableRounds.Count);

        // 2. Get the actual round index from the list
        currentRoundIndex = availableRounds[randomIndex];

        // 3. Remove the selected round from the list so it can't be picked again
        availableRounds.RemoveAt(randomIndex);

        Debug.Log($"--- Starting a new round. Loading random round with original index: {currentRoundIndex} ---");
        roundManager.LoadRound(currentRoundIndex);
    }

    public void NextRound()
    {
        // This method simply starts a new random round
        roundManager.ClearCurrentRound();
        StartRound();
    }

    public int GetCurrentRoundIndex()
    {
        return currentRoundIndex;
    }
}