using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Required for IPointerDownHandler

public class AutocompleteInputField : MonoBehaviour, IPointerDownHandler
{
    [Header("Settings")]
    public int maxSuggestions = 5;

    [Header("Debug Info")]
    [SerializeField] private List<string> availableCountries = new List<string>();
    [SerializeField] private List<GameObject> suggestionButtons = new List<GameObject>();
    [SerializeField] private string lastValidatedAnswer = ""; // Track last validated answer to prevent duplicate point deductions

    private TMP_InputField inputField;
    private CountryGameManager countryGameManager;
    private GameObject scrollView; // Reference to the new ScrollView parent
    private GameObject suggestionPanel; // Reference to the content panel
    private bool isShowingSuggestions = false;


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

        // No longer using onDeselect to hide suggestions
        // inputField.onDeselect.AddListener(OnInputDeselected);

        // Initialize country list
        PopulateCountryList();

        // Create scrollable suggestion panel automatically
        CreateSuggestionPanel();

        Debug.Log($"✅ AutocompleteInputField initialized with {availableCountries.Count} countries");
    }

    private void CreateSuggestionPanel()
    {
        // Create the main ScrollView container
        scrollView = new GameObject("AutoSuggestionScrollView");
        scrollView.transform.SetParent(transform, false);

        // Add RectTransform
        RectTransform scrollViewRect = scrollView.AddComponent<RectTransform>();
        RectTransform inputRect = inputField.GetComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0, 0);
        scrollViewRect.anchorMax = new Vector2(1, 0);
        scrollViewRect.pivot = new Vector2(0.5f, 1f);
        scrollViewRect.anchoredPosition = new Vector2(0, -5f);
        scrollViewRect.sizeDelta = new Vector2(0, 150); // Set a fixed height for the scroll area

        // Add ScrollRect component
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollRect.horizontal = false; // Disable horizontal scrolling
        scrollRect.vertical = true; // Enable vertical scrolling

        // Create the Viewport GameObject
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.pivot = new Vector2(0.5f, 1f);
        viewportRect.sizeDelta = new Vector2(0, 0);

        // Add Mask component to the viewport to hide content outside its bounds
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false; // Hide the mask graphic itself

        // Add background image to the viewport
        Image viewportBg = viewport.AddComponent<Image>();
        viewportBg.color = new Color(112f / 255f, 196f / 255f, 196f / 255f, 0.95f);

        // Add border (optional)
        Outline outline = viewport.AddComponent<Outline>();
        outline.effectColor = Color.gray;
        outline.effectDistance = new Vector2(1, -1);

        // Create the Content panel (where suggestion buttons will be placed)
        suggestionPanel = new GameObject("AutoSuggestionContent");
        suggestionPanel.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = suggestionPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0, 0);

        // Add Vertical Layout Group for automatic spacing
        VerticalLayoutGroup layoutGroup = suggestionPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.spacing = 2f; // Small gap between suggestions

        // Add Content Size Fitter to automatically adjust the content panel's height
        ContentSizeFitter sizeFitter = suggestionPanel.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Connect the ScrollRect to the Viewport and Content
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        // Hide initially
        scrollView.SetActive(false);

        Debug.Log("✅ Auto-created scrollable suggestion panel underneath input field");
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
                "Norway", "Ireland", "Croatia", "Bosnia and Herzegovina",
                "Pakistan", "Russia", "Turkey", "Iran", "Iraq"
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

    // Implementing this interface method to detect clicks anywhere on the UI
    public void OnPointerDown(PointerEventData eventData)
    {
        // This is the correct way to call the method
        if (!EventSystem.current.IsPointerOverGameObject())
        {
            HideSuggestions();
        }
        // Alternatively, to check specifically if the click was on the input or suggestion panel
        else if (!IsChildOf(eventData.pointerCurrentRaycast.gameObject, scrollView) && eventData.pointerCurrentRaycast.gameObject != inputField.gameObject)
        {
            HideSuggestions();
        }
    }

    // Helper method to check if a GameObject is a child of another
    private bool IsChildOf(GameObject child, GameObject parent)
    {
        if (child == null || parent == null)
            return false;

        Transform current = child.transform;
        while (current != null)
        {
            if (current == parent.transform)
                return true;
            current = current.parent;
        }
        return false;
    }


    private void ShowSuggestions(List<string> suggestions)
    {
        if (scrollView == null)
        {
            Debug.LogWarning("Suggestion scroll view not created!");
            return;
        }

        // Clear existing suggestions
        ClearSuggestions();

        // Create suggestion buttons directly
        for (int i = 0; i < suggestions.Count; i++)
        {
            CreateSimpleSuggestionButton(suggestions[i], i);
        }

        // Show the panel
        scrollView.SetActive(true);
        isShowingSuggestions = true;

        Debug.Log($"✅ Showing {suggestions.Count} suggestions");
    }

    private void CreateSimpleSuggestionButton(string countryName, int index)
    {
        // Create button GameObject
        GameObject button = new GameObject($"Suggestion_{countryName}");
        button.transform.SetParent(suggestionPanel.transform, false);

        // Add RectTransform (Layout Group will handle positioning)
        RectTransform buttonRect = button.AddComponent<RectTransform>();

        // Add Image background
        Image buttonBg = button.AddComponent<Image>();
        buttonBg.color = new Color(112f / 255f, 196f / 255f, 196f / 255f, 0.8f);

        // Add Button component
        Button buttonComponent = button.AddComponent<Button>();
        buttonComponent.targetGraphic = buttonBg;

        // Add Layout Element for better size control
        LayoutElement layoutElement = button.AddComponent<LayoutElement>();
        layoutElement.minHeight = 35f;
        layoutElement.preferredHeight = 35f;

        // Set button colors for hover effect
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = new Color(112f / 255f, 196f / 255f, 196f / 255f, 0.8f);
        colors.highlightedColor = new Color(90f / 255f, 220f / 255f, 220f / 255f, 1f);
        colors.pressedColor = new Color(70f / 255f, 170f / 255f, 170f / 255f, 1f);
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

        // Enable text wrapping and overflow handling
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

        Debug.Log($"✅ Selected country: {countryName}");
    }

    private void HideSuggestions()
    {
        if (!isShowingSuggestions) return; // Early exit if already hidden

        if (scrollView != null)
        {
            scrollView.SetActive(false);
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
                DestroyImmediate(button);
            }
        }
        suggestionButtons.Clear();
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
        HideSuggestions();
    }

    public void SetText(string text)
    {
        if (inputField != null)
        {
            inputField.text = text;
        }
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

        // Only validate if the text matches exactly one of the available countries
        string trimmedCountry = selectedCountry.Trim();
        bool isCompleteCountryName = availableCountries.Any(country =>
            string.Equals(country, trimmedCountry, System.StringComparison.OrdinalIgnoreCase));

        if (!isCompleteCountryName)
        {
            Debug.Log($"Skipping validation - '{trimmedCountry}' is not a complete country name from dropdown");
            return;
        }

        // Don't validate the same answer twice
        if (string.Equals(lastValidatedAnswer, trimmedCountry, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"Skipping validation - already validated: '{trimmedCountry}'");
            return;
        }

        // Get current round's correct answers
        var currentRoundCountries = countryGameManager.GetCurrentRoundCountries();
        if (currentRoundCountries == null || currentRoundCountries.Count == 0)
        {
            Debug.LogWarning("No current round countries available for validation");
            return;
        }

        // Check if the selected country is correct (case insensitive)
        bool isCorrect = currentRoundCountries.Any(country =>
            string.Equals(country.Trim(), trimmedCountry, System.StringComparison.OrdinalIgnoreCase));

        // Apply visual feedback
        if (isCorrect)
        {
            SetInputFieldColor(new Color(0.7f, 1f, 0.7f, 1f));
            Debug.Log($"✅ CORRECT: '{trimmedCountry}' is valid for this round");

            // NEW: Increment the total correct guesses
            countryGameManager.IncrementCorrectGuesses();
            // NEW: Increment the correct guesses for this specific round
            countryGameManager.IncrementCorrectGuessesThisRound();
            // NEW: Check for round completion
            countryGameManager.CheckRoundCompletion();
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
                colorApplied = true;
            }
        }
    }
}