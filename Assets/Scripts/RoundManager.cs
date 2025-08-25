using UnityEngine;
using System.Collections.Generic;

public class RoundManager : MonoBehaviour
{
    public CountryData CountryData;
    public Transform spawnParent;
    public float verticalSpacing = 100f;

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
            Debug.Log($"? Loaded new sprite for round {roundIndex}.");
        }

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
        Debug.Log("?? RoundManager fully reset.");
    }

    public int GetSpawnedObjectsCount()
    {
        return _spawnedObjects.Count;
    }
}
