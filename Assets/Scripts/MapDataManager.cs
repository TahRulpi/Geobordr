using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// This will make it a singleton or a static class for easy access from anywhere.
public class MapDataManager : MonoBehaviour
{
    // We can hold all our country data in a list.
    public List<CountryData> allCountries;

    // This makes the class a singleton for easy access.
    public static MapDataManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // Optionally, you can use DontDestroyOnLoad if you need this object to persist
            // through different scenes.
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}