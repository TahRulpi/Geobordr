using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI; 

public class CountryObject : MonoBehaviour
{
   
    public CountryData countryData;

    // Reference to a Text element if you want to display the name.
    public Text nameText;

    private void Start()
    {
        // This is a simple example. In a real game, you might do this differently.
        if (countryData != null)
        {
            // Set the sprite of the GameObject to match the one in the Scriptable Object.
            // (This is redundant if you dragged the sprite directly, but good practice).
            //GetComponent<SpriteRenderer>().sprite = countryData.countrySprite;

            // If you have a Text component, update it with the country name.
            if (nameText != null)
            {
                //nameText.text = countryData.countryName;
            }
        }
    }

    // Example of a function that could be called when the player clicks on the country.
    void OnMouseDown()
    {
        if (countryData != null)
        {
            //Debug.Log("Clicked on " + countryData.countryName);
           
        }
    }
}