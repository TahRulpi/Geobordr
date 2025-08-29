using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AutocompleteInputField : MonoBehaviour, IPointerDownHandler
{
    [Header("Settings")]
    public int maxSuggestions = 5;

    [Header("UI References")] 
    public Image flagImage; // Assign the flag Image UI element from your prefab
    public Sprite defaultFlag; // Optional: A default image to show when no country is typed

    [Header("Debug Info")]
    [SerializeField] private List<string> availableCountries = new List<string>();
    [SerializeField] private List<GameObject> suggestionButtons = new List<GameObject>();
    
    // Dictionary for quick flag lookups, ignoring case
    private Dictionary<string, Sprite> countryFlags = new Dictionary<string, Sprite>(System.StringComparer.OrdinalIgnoreCase);

    private TMP_InputField inputField;
    private CountryGameManager countryGameManager;
    private GameObject scrollView;
    private GameObject suggestionPanel;
    private bool isShowingSuggestions = false;
    private string lastValidatedAnswer = ""; // Track last validated answer to prevent duplicate point deductions

    //take a color
    public Color oddCardColor = new Color32(0, 255, 0, 255);
    public Color evenCardColor = new Color32(0, 255, 0, 255);


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
        
        // Initialize country list
        PopulateCountryList();
        
        // Create scrollable suggestion panel
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
        Transform inputParent = transform.parent;
        if (inputParent == null)
        {
            Debug.LogError("Input field must have a parent to create the suggestion panel.");
            return;
        }

        // Create scrollable container
        scrollView = new GameObject("AutoSuggestionScrollView");
        scrollView.transform.SetParent(inputParent, false);

        RectTransform scrollViewRect = scrollView.AddComponent<RectTransform>();
        RectTransform inputRect = inputField.GetComponent<RectTransform>();

        // Calculate position relative to input field
        Vector3[] inputCorners = new Vector3[4];
        inputRect.GetWorldCorners(inputCorners);

        Vector3[] parentCorners = new Vector3[4];
        inputParent.GetComponent<RectTransform>().GetWorldCorners(parentCorners);

        Vector2 localBottomLeft;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(inputParent.GetComponent<RectTransform>(), inputCorners[0], null, out localBottomLeft);

        Vector2 localBottomRight;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(inputParent.GetComponent<RectTransform>(), inputCorners[3], null, out localBottomRight);

        Vector2 localTopRight;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(inputParent.GetComponent<RectTransform>(), inputCorners[2], null, out localTopRight);

        scrollViewRect.pivot = new Vector2(0.5f, 1f);
        scrollViewRect.anchorMin = new Vector2(0.5f, 0.5f);
        scrollViewRect.anchorMax = new Vector2(0.5f, 0.5f);

        float inputWidth = Vector2.Distance(localBottomLeft, localBottomRight);
        float inputHeight = Vector2.Distance(localBottomRight, localTopRight);
        Vector2 inputCenter = new Vector2((localBottomLeft.x + localBottomRight.x) / 2, (localBottomLeft.y + localTopRight.y) / 2);

        scrollViewRect.sizeDelta = new Vector2(inputWidth, 350f); // Height for scrollable area
        scrollViewRect.anchoredPosition = new Vector2(inputCenter.x, inputCenter.y - inputHeight / 2 - 5f);

        // Add ScrollRect component
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

        // Create viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform, false);
        RectTransform viewportRect = viewport.AddComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.pivot = new Vector2(0.5f, 1f);
        viewportRect.sizeDelta = Vector2.zero;

        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;

        Image viewportBg = viewport.AddComponent<Image>();
        viewportBg.color = new Color(112f / 255f, 196f / 255f, 196f / 255f, 0.95f);

        Outline outline = viewport.AddComponent<Outline>();
        outline.effectColor = Color.gray;
        outline.effectDistance = new Vector2(1, -1);

        // Create content panel
        suggestionPanel = new GameObject("AutoSuggestionContent");
        suggestionPanel.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = suggestionPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = Vector2.zero;

        // Add layout components
        VerticalLayoutGroup layoutGroup = suggestionPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.spacing = 0f; // Increased spacing between suggestions
        layoutGroup.padding = new RectOffset(0, 0, 16, 16); // Add top and bottom padding to each item

        ContentSizeFitter sizeFitter = suggestionPanel.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // Connect ScrollRect components
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        // Hide initially
        scrollView.SetActive(false);
        
        Debug.Log("✅ Auto-created scrollable suggestion panel underneath input field");
    }

    private void PopulateCountryList()
    {
        availableCountries.Clear();
        countryFlags.Clear(); // Clear the flag dictionary
        
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
            // Get all unique country details (name and flag)
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
            UpdateFlag(inputText); // Update flag on input change
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
        
        UpdateFlag(inputText); // Update flag on input change
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

    public void OnPointerDown(PointerEventData eventData)
    {
        // Hide suggestions when clicking outside of input field or suggestion panel
        if (!EventSystem.current.IsPointerOverGameObject() ||
            (!IsChildOf(eventData.pointerCurrentRaycast.gameObject, scrollView) && eventData.pointerCurrentRaycast.gameObject != inputField.gameObject))
        {
            HideSuggestions();
        }
    }

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
        
        // Ensure the scroll view appears above other UI elements
        scrollView.transform.SetAsLastSibling();
        
        // Clear existing suggestions
        ClearSuggestions();

        // Create suggestion buttons
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
        
        // Add Image background with alternating colors
        Image buttonBg = button.AddComponent<Image>();
        // Alternate between two contrasting shades for better visibility
        Color bgColor = (index % 2 == 0)
            ? oddCardColor// Light blue (even rows)
            : evenCardColor; // Slightly darker blue (odd rows)
        buttonBg.color = bgColor;
        
        // Add Button component
        Button buttonComponent = button.AddComponent<Button>();
        buttonComponent.targetGraphic = buttonBg;
        
        // Add Layout Element for better size control
        LayoutElement layoutElement = button.AddComponent<LayoutElement>();
        layoutElement.minHeight = 120f; // Increased height for better visibility
        layoutElement.preferredHeight = 120f;
        
        // Set button colors for hover effect with alternating base colors
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = bgColor; // Use the alternating background color
        colors.highlightedColor = (index % 2 == 0) 
            ? new Color(130f/255f, 210f/255f, 210f/255f, 1f)   // Brighter hover for even rows
            : new Color(105f/255f, 170f/255f, 170f/255f, 1f);  // Darker hover for odd rows
        colors.pressedColor = new Color(70f/255f, 170f/255f, 170f/255f, 1f); // Same pressed color for all
        buttonComponent.colors = colors;
        
        // Create flag image (if available)
        GameObject flagObj = new GameObject("Flag");
        flagObj.transform.SetParent(button.transform, false);
        
        RectTransform flagRect = flagObj.AddComponent<RectTransform>();
        flagRect.anchorMin = new Vector2(0, 0.5f);
        flagRect.anchorMax = new Vector2(0, 0.5f);
        flagRect.pivot = new Vector2(0, 0.5f);
        flagRect.sizeDelta = new Vector2(48, 36); // Flag size (4:3 aspect ratio)
        flagRect.localScale = new Vector3(1.83000004f, 1.42771554f, 1.42771554f);
        flagRect.anchoredPosition = new Vector2(50, 0); // Position from left edge
        
        Image flagImage = flagObj.AddComponent<Image>();
        
        // Try to get the flag for this country
        if (countryFlags.TryGetValue(countryName, out Sprite flagSprite))
        {
            flagImage.sprite = flagSprite;
        }
        else
        {
            // If no flag available, use a placeholder or hide the image
            flagObj.SetActive(false);
        }
        
        // Create text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        // Adjust left padding to account for flag (flag width + some spacing)
        float leftPadding = countryFlags.ContainsKey(countryName) ? 160f : 15f;
        textRect.offsetMin = new Vector2(leftPadding, 0);
        textRect.offsetMax = new Vector2(-15, 0);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = countryName;
        text.fontSize = 65; // Increased font size for better readability
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
        
        // Update flag when suggestion is clicked
        UpdateFlag(countryName);
        
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
                Destroy(button); // Use Destroy instead of DestroyImmediate for better performance
            }
        }
        suggestionButtons.Clear();
    }
    
    // Method to update the flag image based on the current input text
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
            countryGameManager.IncrementCorrectGuesses();
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