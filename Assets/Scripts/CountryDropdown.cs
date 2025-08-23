using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;

public class CountryDropdown : MonoBehaviour
{
    [Header("Dropdown Configuration")]
    public TMP_Dropdown dropdown;
    public bool sortAlphabetically = true;
    public bool includeEmptyOption = true;
    
    [Header("Debug Info")]
    [SerializeField] private List<string> availableCountries = new List<string>();
    [SerializeField] private string selectedCountry = "";
    
    private CountryGameManager countryGameManager;

    private void Start()
    {
        // Find the CountryGameManager in the scene
        countryGameManager = FindObjectOfType<CountryGameManager>();
        
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
        }
        
        InitializeDropdown();
    }

    public void InitializeDropdown()
    {
        if (dropdown == null)
        {
            Debug.LogError("TMP_Dropdown component not found!");
            return;
        }

        // Get all unique countries from CountryData
        PopulateCountryList();
        
        // Setup dropdown options
        SetupDropdownOptions();
        
        // Add listener for selection changes
        dropdown.onValueChanged.RemoveAllListeners();
        dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        
        Debug.Log($"CountryDropdown initialized with {availableCountries.Count} countries");
    }

    private void PopulateCountryList()
    {
        availableCountries.Clear();
        
        if (countryGameManager == null || countryGameManager.countryData == null)
        {
            Debug.LogWarning("CountryGameManager or CountryData not found! Using sample countries.");
            // Fallback list if no data is available
            availableCountries.AddRange(new string[] 
            { 
                "Germany", "France", "Italy", "Spain", "Poland", "Romania", 
                "Netherlands", "Belgium", "Greece", "Portugal", "Czech Republic",
                "Hungary", "Sweden", "Austria", "Belarus", "Switzerland",
                "Bulgaria", "Serbia", "Denmark", "Finland", "Slovakia",
                "Norway", "Ireland", "Croatia", "Bosnia and Herzegovina"
            });
        }
        else
        {
            // Extract all unique countries from CountryData
            HashSet<string> uniqueCountries = new HashSet<string>();
            
            foreach (var countryInfo in countryGameManager.countryData.countryInfo)
            {
                if (countryInfo.countryName != null)
                {
                    foreach (string country in countryInfo.countryName)
                    {
                        if (!string.IsNullOrEmpty(country))
                        {
                            uniqueCountries.Add(country.Trim());
                        }
                    }
                }
            }
            
            availableCountries.AddRange(uniqueCountries);
        }

        // Sort alphabetically if requested
        if (sortAlphabetically)
        {
            availableCountries.Sort();
        }
        
        Debug.Log($"Found {availableCountries.Count} unique countries in the data");
    }

    private void SetupDropdownOptions()
    {
        dropdown.ClearOptions();
        
        List<string> dropdownOptions = new List<string>();
        
        // Add empty option if requested
        if (includeEmptyOption)
        {
            dropdownOptions.Add("Select a country...");
        }
        
        // Add all countries
        dropdownOptions.AddRange(availableCountries);
        
        dropdown.AddOptions(dropdownOptions);
        
        // Set to first option (empty or first country)
        dropdown.value = 0;
        dropdown.RefreshShownValue();
    }

    private void OnDropdownValueChanged(int selectedIndex)
    {
        if (includeEmptyOption && selectedIndex == 0)
        {
            selectedCountry = "";
            Debug.Log("No country selected");
        }
        else
        {
            int countryIndex = includeEmptyOption ? selectedIndex - 1 : selectedIndex;
            if (countryIndex >= 0 && countryIndex < availableCountries.Count)
            {
                selectedCountry = availableCountries[countryIndex];
                Debug.Log($"Selected country: {selectedCountry}");
            }
        }
    }

    // Public method to get the selected country (for validation)
    public string GetSelectedCountry()
    {
        return selectedCountry;
    }

    // Public method to set a specific country (useful for testing)
    public void SetSelectedCountry(string country)
    {
        if (string.IsNullOrEmpty(country))
        {
            dropdown.value = 0;
            selectedCountry = "";
            return;
        }

        int index = availableCountries.FindIndex(c => 
            string.Equals(c, country, System.StringComparison.OrdinalIgnoreCase));
        
        if (index >= 0)
        {
            dropdown.value = includeEmptyOption ? index + 1 : index;
            selectedCountry = availableCountries[index];
            dropdown.RefreshShownValue();
            Debug.Log($"Set dropdown to: {selectedCountry}");
        }
        else
        {
            Debug.LogWarning($"Country '{country}' not found in dropdown options");
        }
    }

    // Public method to reset the dropdown
    public void ResetDropdown()
    {
        dropdown.value = 0;
        selectedCountry = "";
        dropdown.RefreshShownValue();
    }

    // Method to refresh the dropdown with new data (useful when CountryData changes)
    public void RefreshDropdown()
    {
        InitializeDropdown();
    }

    // Validation method that works like the input field validation
    public bool ValidateSelection(string correctAnswer)
    {
        return string.Equals(selectedCountry.Trim(), correctAnswer.Trim(), 
                           System.StringComparison.OrdinalIgnoreCase);
    }

    // Debug method to show all available countries
    [ContextMenu("Debug - Show All Countries")]
    public void DebugShowAllCountries()
    {
        Debug.Log($"Available countries ({availableCountries.Count}):");
        for (int i = 0; i < availableCountries.Count; i++)
        {
            Debug.Log($"  {i + 1}. {availableCountries[i]}");
        }
    }
}
