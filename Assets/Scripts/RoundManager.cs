using UnityEngine;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    public CountryData CountryData;
    public Transform spawnParent;
    public float verticalSpacing = 150f;

    // --- Added based on your request ---
    [Tooltip("Additional vertical offset to move the whole group down.")]
    public float yAxisOffset = 170f;
    // ------------------------------------

    public Sprite gridSpriteDisplay;

    private List<GameObject> _spawnedObjects = new List<GameObject>();
    public List<GameObject> SpawnedObjects { get { return _spawnedObjects; } }

    public void LoadRound(int roundIndex)
    {
        ClearCurrentRound();

        if (CountryData == null || CountryData.countryInfo.Length <= roundIndex)
        {
            Debug.LogError("? CountryData not assigned or index out of range!");
            return;
        }

        CountryInfo roundInfo = CountryData.countryInfo[roundIndex];

        if (roundInfo.gridImage != null)
        {
            gridSpriteDisplay = roundInfo.gridImage;
          
        }

        // --- Modified based on your requests ---
        // 1. Get the total number of prefabs to be spawned.
        int numPrefabs = roundInfo.optionsPrefabs.Length;

        // 2. Calculate the total height and the starting Y position to center the group.
        float totalHeight = (numPrefabs - 1) * verticalSpacing;
        float startY = totalHeight / 2f;
        // ---------------------------------------

        for (int i = 0; i < numPrefabs; i++)
        {
            // ✅ *** THIS IS THE FIX ***
            // We add a check to make sure the prefab is not null before trying to create it.
            if (roundInfo.optionsPrefabs[i] != null)
            {
                // 3. Calculate position using the centering logic and the final offset.
                Vector3 spawnPos = new Vector3(0, startY - (i * verticalSpacing) - yAxisOffset, 0);

                GameObject prefab = Instantiate(roundInfo.optionsPrefabs[i], spawnParent);
                prefab.transform.localPosition = spawnPos;
                _spawnedObjects.Add(prefab);
            }
        }
    }

    public void ClearCurrentRound()
    {
        foreach (GameObject obj in _spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        _spawnedObjects.Clear();
    }

    // ? Completely reset round manager when restarting
    public void ResetManager()
    {
        ClearCurrentRound();
        gridSpriteDisplay = null;
        CountryData = null; // so new filtered data is reassigned later
       
    }

    public int GetSpawnedObjectsCount()
    {
        return _spawnedObjects.Count;
    }
}