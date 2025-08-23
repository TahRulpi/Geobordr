using UnityEngine;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    // ScriptableObject reference
    public CountryData CountryData;
    public Transform spawnParent;        // Where prefabs will appear
    public float verticalSpacing = 100f; // Spacing between input fields

    // --- NOW USING SPRITE INSTEAD OF IMAGE ---
    public Sprite gridSpriteDisplay;

    private List<GameObject> _spawnedObjects = new List<GameObject>();
    public List<GameObject> SpawnedObjects { get { return _spawnedObjects; } }



    public void LoadRound(int roundIndex)
    {
        if (CountryData == null || CountryData.countryInfo.Length <= roundIndex)
        {
            Debug.LogError("? CountryData not assigned or index out of range!");
            return;
        }

        CountryInfo roundInfo = CountryData.countryInfo[roundIndex];

        // ? Now you just store sprite, no Image component needed
        if (roundInfo.gridImage != null)
        {
            gridSpriteDisplay = roundInfo.gridImage;
            Debug.Log($"? Loaded new sprite for round {roundIndex}.");
        }
        else
        {
            Debug.LogWarning("?? Missing Sprite for this round in ScriptableObject data.");
        }

        // Spawn objects as before
        for (int i = 0; i < roundInfo.optionsPrefabs.Length; i++)
        {
            Vector3 spawnPos = new Vector3(0, -i * verticalSpacing, 0);
            GameObject prefab = Instantiate(roundInfo.optionsPrefabs[i], spawnParent);
            prefab.transform.localPosition = spawnPos;
            _spawnedObjects.Add(prefab);
        }
    }

    public void ClearCurrentRound()
    {
        foreach (GameObject obj in _spawnedObjects)
        {
            Destroy(obj);
        }
        _spawnedObjects.Clear();
    }
}
