using UnityEngine;
using UnityEngine.UI;

public class PrefabSpawner : MonoBehaviour
{
    public CountryData myCountryData; // Assign your CountryData asset here in the Inspector
    public Transform parentPanel;    // A UI parent transform for your prefabs

    void Start()
    {
        // Example of how to access and instantiate a prefab
        if (myCountryData != null && myCountryData.countryInfo.Length > 0)
        {
            // Get the first country's info
            CountryInfo firstCountryInfo = myCountryData.countryInfo[0];

            // Loop through the array of prefabs
            foreach (GameObject prefab in firstCountryInfo.optionsPrefabs)
            {
                if (prefab != null)
                {
                    // Instantiate the prefab. We cast to GameObject just to be clear.
                    GameObject newPrefabInstance = Instantiate(prefab, parentPanel);

                    // You can now access the InputField component on the instantiated object
                    InputField inputField = newPrefabInstance.GetComponent<InputField>();
                    if (inputField != null)
                    {
                        Debug.Log("Instantiated a prefab with an InputField component!");
                    }
                }
            }
        }
    }
}