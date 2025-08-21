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
}

[CreateAssetMenu(fileName = "New Country Data", menuName = "Custom Data/Country Data")]
public class CountryData : ScriptableObject
{
    public CountryInfo[] countryInfo;
}