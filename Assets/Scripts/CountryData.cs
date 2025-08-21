using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

[Serializable]
public struct CountryInfo
{
    public Image gridImage;
    public List<InputField> options;
    public List<string> countryName;
}

[CreateAssetMenu(fileName = "New Country Data", menuName = "Custom Data/Country Data")]

public class CountryData : ScriptableObject
{
    public CountryInfo[] countryInfo;
}