using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Country Data", menuName = "Custom Data/Country Data")]
public class CountryData : ScriptableObject
{
    // The name of the country.
    public string countryName;

    // A reference to the sprite associated with this country.
    // Assign the specific country sprite from your grid here.
    public Sprite countrySprite;

    // You can add more data fields here as needed, e.g.,
    // public string capital;
    // public int population;
    // public Color mapColor;
}