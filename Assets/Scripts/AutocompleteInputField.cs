using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;

public class AutocompleteInputField : MonoBehaviour
{
    [Header("Settings")]
    public int maxSuggestions = 5;

    [Header("UI References")] // NEW
    public Image flagImage; // Assign the flag Image UI element from your prefab
    public Sprite defaultFlag; // Optional: A default image to show when no country is typed

    [Header("Debug Info")]
    [SerializeField] private List<string> availableCountries = new List<string>();
    [SerializeField] private List<GameObject> suggestionButtons = new List<GameObject>();
    
    // NEW: Dictionary for quick flag lookups, ignoring case
    private Dictionary<string, Sprite> countryFlags = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);

    private TMP_InputField inputField;
    private CountryGameManager countryGameManager;
    private GameObject suggestionPanel;
    private bool isShowingSuggestions = false;
    private string lastValidatedAnswer = ""; // Track last validated answer to prevent duplicate point deductions

    private void Start()
    {
        // Find components automatically
        inputField = GetComponent<TMP_InputField>();
        countryGameManager = FindObjectOfType<CountryGameManager>();
        
        if (inputField == null)
        {
            inputField = GetComponentInChildren<TMP_InputField>();
        }
        
        if (inputField == null)
        {
            Debug.LogError("No TMP_InputField found! Make sure this script is on a GameObject with TMP_InputField.");
            return;
        }
        
        // Set up listeners
        inputField.onValueChanged.AddListener(OnInputChanged);
        inputField.onSelect.AddListener(OnInputSelected);
        inputField.onDeselect.AddListener(OnInputDeselected);
        
        // Initialize country list
        PopulateCountryList();
        
        // Create suggestion panel automatically
        CreateSuggestionPanel();
        
        // Set initial flag state
        if (flagImage != null)
        {
            flagImage.sprite = defaultFlag;
            flagImage.gameObject.SetActive(defaultFlag != null); // Show only if a default is assigned
        }
        
        Debug.Log($"✅ AutocompleteInputField initialized with {availableCountries.Count} countries");
    }

    private void CreateSuggestionPanel()
    {
        // Create the suggestion panel as a child of this GameObject
        suggestionPanel = new GameObject("AutoSuggestionPanel");
        suggestionPanel.transform.SetParent(transform, false);
        
        // Add RectTransform
        RectTransform panelRect = suggestionPanel.AddComponent<RectTransform>();
        
        // Position it below the input field
        RectTransform inputRect = inputField.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot = new Vector2(0.5f, 1f);
        panelRect.anchoredPosition = new Vector2(0, -5f);
        panelRect.sizeDelta = new Vector2(0, 150);
        
        // Add Canvas component with high sort order to ensure it appears above other UI elements
        Canvas panelCanvas = suggestionPanel.AddComponent<Canvas>();
        panelCanvas.overrideSorting = true;
        panelCanvas.sortingOrder = 1000; // High value to appear above other UI elements
        
        // Add GraphicRaycaster for UI interaction
        suggestionPanel.AddComponent<GraphicRaycaster>();
        
        // Add Content Size Fitter for better layout handling
        ContentSizeFitter sizeFitter = suggestionPanel.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // Add Vertical Layout Group for automatic spacing
        VerticalLayoutGroup layoutGroup = suggestionPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.spacing = 2f; // Small gap between suggestions
        
        // Add background image
        Image panelBg = suggestionPanel.AddComponent<Image>();
        panelBg.color = new Color(112f/255f, 196f/255f, 196f/255f, 0.95f);
        
        // Add border (optional)
        Outline outline = suggestionPanel.AddComponent<Outline>();
        outline.effectColor = Color.gray;
        outline.effectDistance = new Vector2(1, -1);
        
        // Hide initially
        suggestionPanel.SetActive(false);
        
        Debug.Log("✅ Auto-created suggestion panel underneath input field");
    }

    private void PopulateCountryList()
    {
        availableCountries.Clear();
        countryFlags.Clear(); // UPDATED: Clear the dictionary
        
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
                "Norway", "Ireland", "Croatia", "Bosnia and Herzegovina",
                "Pakistan", "Russia", "Turkey", "Iran", "Iraq"
            });
        }
        else
        {
            // UPDATED: Get all unique country details (name and flag)
            var allCountryDetails = countryGameManager.countryData.GetAllUniqueCountryDetails();
            
            foreach (var detail in allCountryDetails)
            {
                if (!string.IsNullOrEmpty(detail.countryName))
                {
                    string trimmedName = detail.countryName.Trim();
                    availableCountries.Add(trimmedName);
                    
                    // Populate the flag dictionary
                    if (detail.countryFlag != null && !countryFlags.ContainsKey(trimmedName))
                    {
                        countryFlags.Add(trimmedName, detail.countryFlag);
                    }
                }
            }
        }

        // Sort alphabetically
        availableCountries.Sort();
        
        Debug.Log($"Found {availableCountries.Count} unique countries for autocomplete");
    }

    private void OnInputChanged(string inputText)
    {
        if (string.IsNullOrEmpty(inputText) || inputText.Length < 1)
        {
            // If user cleared the input, hide suggestions and reset any validation/visual feedback
            HideSuggestions();
            ResetValidationState();
            UpdateFlag(inputText); // NEW: Update flag on input change
            return;
        }

        // Filter countries that match the input
        List<string> matchingCountries = availableCountries
            .Where(country => country.ToLower().StartsWith(inputText.ToLower()))
            .Take(maxSuggestions)
            .ToList();

        if (matchingCountries.Count > 0)
        {
            ShowSuggestions(matchingCountries);
        }
        else
        {
            HideSuggestions();
        }
        
        UpdateFlag(inputText); // NEW: Update flag on input change
        Debug.Log($"Input: '{inputText}' - Found {matchingCountries.Count} matches");
    }

    private void OnInputSelected(string text)
    {
        // When input field is selected, show suggestions if there's text
        if (!string.IsNullOrEmpty(text))
        {
            OnInputChanged(text);
        }
    }

    private void OnInputDeselected(string text)
    {
        // Delay hiding to allow suggestion selection
        Invoke(nameof(HideSuggestions), 0.2f);
    }

    private void ShowSuggestions(List<string> suggestions)
    {
        if (suggestionPanel == null)
        {
            Debug.LogWarning("Suggestion panel not created!");
            return;
        }

        // Clear existing suggestions
        ClearSuggestions();

        // Create suggestion buttons directly
        foreach (string suggestion in suggestions)
        {
            CreateSimpleSuggestionButton(suggestion);
        }

        // Show the panel
        suggestionPanel.SetActive(true);
        isShowingSuggestions = true;
        
        Debug.Log($"✅ Showing {suggestions.Count} suggestions");
    }

    private void CreateSimpleSuggestionButton(string countryName)
    {
        // Create button GameObject
        GameObject button = new GameObject($"Suggestion_{countryName}");
        button.transform.SetParent(suggestionPanel.transform, false);
        
        // Add RectTransform (Layout Group will handle positioning)
        button.AddComponent<RectTransform>();
        
        // Add Image background
        Image buttonBg = button.AddComponent<Image>();
        buttonBg.color = new Color(112f/255f, 196f/255f, 196f/255f, 0.8f);
        
        // Add Button component
        Button buttonComponent = button.AddComponent<Button>();
        buttonComponent.targetGraphic = buttonBg;
        
        // Add Layout Element for better size control
        LayoutElement layoutElement = button.AddComponent<LayoutElement>();
        layoutElement.minHeight = 35f;
        layoutElement.preferredHeight = 35f;
        
        // Set button colors for hover effect
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = new Color(112f/255f, 196f/255f, 196f/255f, 0.8f);
        colors.highlightedColor = new Color(90f/255f, 220f/255f, 220f/255f, 1f);
        colors.pressedColor = new Color(70f/255f, 170f/255f, 170f/255f, 1f);
        buttonComponent.colors = colors;
        
        // Create text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = countryName;
        text.fontSize = 14;
        text.color = Color.black;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        text.horizontalAlignment = HorizontalAlignmentOptions.Left;
        
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Ellipsis;
        
        // Add click listener
        buttonComponent.onClick.AddListener(() => OnSuggestionClicked(countryName));
        
        suggestionButtons.Add(button);
    }

    private void OnSuggestionClicked(string countryName)
    {
        // Set the input field text to the selected country
        if (inputField != null)
        {
            inputField.text = countryName;
        }
        
        // Hide suggestions
        HideSuggestions();
        
        // Perform real-time validation
        ValidateSelectionRealTime(countryName);
        
        // NEW: Update flag when suggestion is clicked
        UpdateFlag(countryName);
        
        Debug.Log($"✅ Selected country: {countryName}");
    }

    private void HideSuggestions()
    {
        if (!isShowingSuggestions) return; // Early exit if already hidden
        
        if (suggestionPanel != null)
        {
            suggestionPanel.SetActive(false);
        }
        
        ClearSuggestions();
        isShowingSuggestions = false;
    }

    private void ClearSuggestions()
    {
        foreach (GameObject button in suggestionButtons)
        {
            if (button != null)
            {
                Destroy(button); // Use Destroy instead of DestroyImmediate
            }
        }
        suggestionButtons.Clear();
    }
    
    // NEW: Method to update the flag image based on the current input text
    private void UpdateFlag(string countryName)
    {
        if (flagImage == null) return;

        // Trim the input to handle potential whitespace
        string trimmedName = countryName.Trim();
        
        if (!string.IsNullOrEmpty(trimmedName) && countryFlags.TryGetValue(trimmedName, out Sprite flagSprite))
        {
            // A matching flag was found
            flagImage.sprite = flagSprite;
            flagImage.gameObject.SetActive(true);
        }
        else
        {
            // No match, show the default flag or hide the image
            flagImage.sprite = defaultFlag;
            flagImage.gameObject.SetActive(defaultFlag != null);
        }
    }

    public string GetSelectedCountry()
    {
        if (inputField != null)
        {
            return inputField.text.Trim();
        }
        return "";
    }

    public void ResetValidationState()
    {
        lastValidatedAnswer = "";
        // Reset color to white
        if (inputField != null)
        {
            SetInputFieldColor(Color.white);
        }
    }

    public void ResetInput()
    {
        if (inputField != null)
        {
            inputField.text = "";
        }
        UpdateFlag(""); // Reset flag on input reset
        HideSuggestions();
    }

    public void SetText(string text)
    {
        if (inputField != null)
        {
            inputField.text = text;
        }
        UpdateFlag(text); // Update flag when text is set programmatically
        HideSuggestions();
    }

    public void RefreshCountryList()
    {
        PopulateCountryList();
    }

    private void OnDestroy()
    {
        // Clean up listeners
        if (inputField != null)
        {
            inputField.onValueChanged.RemoveAllListeners();
            inputField.onSelect.RemoveAllListeners();
            inputField.onDeselect.RemoveAllListeners();
        }
    }

    private void ValidateSelectionRealTime(string selectedCountry)
    {
        if (countryGameManager == null)
        {
            Debug.LogWarning("Cannot validate selection: CountryGameManager not found");
            return;
        }

        string trimmedCountry = selectedCountry.Trim();
        bool isCompleteCountryName = availableCountries.Any(country => 
            string.Equals(country, trimmedCountry, System.StringComparison.OrdinalIgnoreCase));

        if (!isCompleteCountryName)
        {
            Debug.Log($"Skipping validation - '{trimmedCountry}' is not a complete country name from dropdown");
            return;
        }

        if (string.Equals(lastValidatedAnswer, trimmedCountry, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"Skipping validation - already validated: '{trimmedCountry}'");
            return;
        }

        var currentRoundCountries = countryGameManager.GetCurrentRoundCountries();
        if (currentRoundCountries == null || currentRoundCountries.Count == 0)
        {
            Debug.LogWarning("No current round countries available for validation");
            return;
        }

        bool isCorrect = currentRoundCountries.Any(country => 
            string.Equals(country.Trim(), trimmedCountry, System.StringComparison.OrdinalIgnoreCase));

        if (isCorrect)
        {
            SetInputFieldColor(new Color(0.7f, 1f, 0.7f, 1f));
            Debug.Log($"✅ CORRECT: '{trimmedCountry}' is valid for this round");
            FindObjectOfType<CountryGameManager>().IncrementCorrectGuesses();
        }
        else
        {
            SetInputFieldColor(new Color32(253, 104, 104, 255));
            Debug.Log($"❌ INCORRECT: '{trimmedCountry}' is not valid for this round");
            countryGameManager.OnWrongAnswer();
        }

        lastValidatedAnswer = trimmedCountry;
    }

    private void SetInputFieldColor(Color color)
    {
        bool colorApplied = false;
        
        Image backgroundImage = inputField.GetComponent<Image>();
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
            colorApplied = true;
        }
        
        Image[] childImages = inputField.GetComponentsInChildren<Image>();
        foreach (Image img in childImages)
        {
            if (img.gameObject.name.ToLower().Contains("background") || 
                img.gameObject.name.ToLower().Contains("field") ||
                img.gameObject == inputField.gameObject)
            {
                img.color = color;
                colorApplied = true;
            }
        }
        
        if (!colorApplied)
        {
            Image parentImage = inputField.GetComponentInParent<Image>();
            if (parentImage != null)
            {
                parentImage.color = color;
            }
        }
    }
}