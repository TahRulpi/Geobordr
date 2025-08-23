using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public struct CountryInfo
{
    // The main image for the country grid
    public Sprite gridImage;

    // An array of GameObjects, where each GameObject is a prefab.
    // The InputField component would be on this prefab.
    public GameObject[] optionsPrefabs;

    // You can keep the list of country names as a string list.
    public List<string> countryName;
    
    // Optional: Store the original filename for reference
    [SerializeField] private string sourceFileName;
    
    // Helper property to get the number of countries in this cluster
    public int CountryCount => countryName?.Count ?? 0;
    
    // Helper method to check if a country is in this cluster
    public bool ContainsCountry(string country)
    {
        if (countryName == null) return false;
        return countryName.Exists(c => string.Equals(c, country, StringComparison.OrdinalIgnoreCase));
    }
    
    // Constructor for easier creation
    public CountryInfo(Sprite sprite, List<string> countries, GameObject[] prefabs, string fileName = "")
    {
        gridImage = sprite;
        countryName = countries;
        optionsPrefabs = prefabs;
        sourceFileName = fileName;
    }
}

[CreateAssetMenu(fileName = "New Country Data", menuName = "Custom Data/Country Data")]
public class CountryData : ScriptableObject
{
    [Header("Auto-imported Country Clusters")]
    public CountryInfo[] countryInfo;
    
    [Header("Import Settings")]
    [Tooltip("Minimum number of countries to show per round")]
    public int minCountriesPerRound = 2;
    
    [Tooltip("Maximum number of countries to show per round")]
    public int maxCountriesPerRound = 5;
    
    // Helper methods
    public int GetTotalClusters() => countryInfo?.Length ?? 0;
    
    public CountryInfo GetRandomCluster()
    {
        if (countryInfo == null || countryInfo.Length == 0)
            return new CountryInfo();
        
        int randomIndex = UnityEngine.Random.Range(0, countryInfo.Length);
        return countryInfo[randomIndex];
    }
    
    public List<CountryInfo> GetClustersContaining(string countryName)
    {
        List<CountryInfo> results = new List<CountryInfo>();
        
        if (countryInfo != null)
        {
            foreach (var info in countryInfo)
            {
                if (info.ContainsCountry(countryName))
                {
                    results.Add(info);
                }
            }
        }
        
        return results;
    }
    
    // Get all unique country names across all clusters
    public List<string> GetAllUniqueCountries()
    {
        HashSet<string> uniqueCountries = new HashSet<string>();
        
        if (countryInfo != null)
        {
            foreach (var info in countryInfo)
            {
                if (info.countryName != null)
                {
                    foreach (string country in info.countryName)
                    {
                        uniqueCountries.Add(country);
                    }
                }
            }
        }
        
        return new List<string>(uniqueCountries);
    }
    
    // Get statistics about the data
    public string GetDataStats()
    {
        if (countryInfo == null || countryInfo.Length == 0)
            return "No data available";
        
        int totalClusters = countryInfo.Length;
        int totalCountries = GetAllUniqueCountries().Count;
        int minCountriesInCluster = int.MaxValue;
        int maxCountriesInCluster = 0;
        
        foreach (var info in countryInfo)
        {
            int count = info.CountryCount;
            if (count > 0)
            {
                minCountriesInCluster = Mathf.Min(minCountriesInCluster, count);
                maxCountriesInCluster = Mathf.Max(maxCountriesInCluster, count);
            }
        }
        
        if (minCountriesInCluster == int.MaxValue) minCountriesInCluster = 0;
        
        return $"Clusters: {totalClusters} | Unique Countries: {totalCountries} | " +
               $"Cluster Size Range: {minCountriesInCluster}-{maxCountriesInCluster}";
    }
}