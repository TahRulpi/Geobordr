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

    [Header("Debug Info")]
    [SerializeField] private List<string> availableCountries = new List<string>();
    [SerializeField] private List<GameObject> suggestionButtons = new List<GameObject>();
    [SerializeField] private string lastValidatedAnswer = "";

    private TMP_InputField inputField;
    private CountryGameManager countryGameManager;
    private GameObject scrollView;
    private GameObject suggestionPanel;
    private bool isShowingSuggestions = false;


    private void Start()
    {
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

        inputField.onValueChanged.AddListener(OnInputChanged);
        inputField.onSelect.AddListener(OnInputSelected);

        PopulateCountryList();
        CreateSuggestionPanel();

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

        scrollView = new GameObject("AutoSuggestionScrollView");
        scrollView.transform.SetParent(inputParent, false);

        RectTransform scrollViewRect = scrollView.AddComponent<RectTransform>();
        RectTransform inputRect = inputField.GetComponent<RectTransform>();

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

        scrollViewRect.sizeDelta = new Vector2(inputWidth, 350f); // Adjusted height for more visible suggestions
        scrollViewRect.anchoredPosition = new Vector2(inputCenter.x, inputCenter.y - inputHeight / 2 - 5f);

        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;

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

        suggestionPanel = new GameObject("AutoSuggestionContent");
        suggestionPanel.transform.SetParent(viewport.transform, false);
        RectTransform contentRect = suggestionPanel.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layoutGroup = suggestionPanel.AddComponent<VerticalLayoutGroup>();
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        layoutGroup.childForceExpandHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.spacing = 2f;

        ContentSizeFitter sizeFitter = suggestionPanel.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;

        scrollView.SetActive(false);
        Debug.Log("✅ Auto-created scrollable suggestion panel underneath input field");
    }

    private void PopulateCountryList()
    {
        availableCountries.Clear();
        if (countryGameManager == null || countryGameManager.countryData == null)
        {
            Debug.LogWarning("CountryGameManager or CountryData not found! Using sample countries.");
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
        availableCountries.Sort();
        Debug.Log($"Found {availableCountries.Count} unique countries for autocomplete");
    }

    private void OnInputChanged(string inputText)
    {
        if (string.IsNullOrEmpty(inputText) || inputText.Length < 1)
        {
            HideSuggestions();
            ResetValidationState();
            return;
        }
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
        if (!string.IsNullOrEmpty(text))
        {
            OnInputChanged(text);
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
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
        scrollView.transform.SetAsLastSibling();
        ClearSuggestions();
        for (int i = 0; i < suggestions.Count; i++)
        {
            CreateSimpleSuggestionButton(suggestions[i], i);
        }
        scrollView.SetActive(true);
        isShowingSuggestions = true;
        Debug.Log($"✅ Showing {suggestions.Count} suggestions");
    }

    private void CreateSimpleSuggestionButton(string countryName, int index)
    {
        GameObject button = new GameObject($"Suggestion_{countryName}");
        button.transform.SetParent(suggestionPanel.transform, false);
        RectTransform buttonRect = button.AddComponent<RectTransform>();
        Image buttonBg = button.AddComponent<Image>();
        buttonBg.color = new Color(112f / 255f, 196f / 255f, 196f / 255f, 0.8f);
        Button buttonComponent = button.AddComponent<Button>();
        buttonComponent.targetGraphic = buttonBg;
        LayoutElement layoutElement = button.AddComponent<LayoutElement>();
        // Increased height
        layoutElement.minHeight = 100f;
        layoutElement.preferredHeight = 50f;
        ColorBlock colors = buttonComponent.colors;
        colors.normalColor = new Color(112f / 255f, 196f / 255f, 196f / 255f, 0.8f);
        colors.highlightedColor = new Color(90f / 255f, 220f / 255f, 220f / 255f, 1f);
        colors.pressedColor = new Color(70f / 255f, 170f / 255f, 170f / 255f, 1f);
        buttonComponent.colors = colors;
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        // Increased left and right offsets for better padding
        textRect.offsetMin = new Vector2(20, 0);
        textRect.offsetMax = new Vector2(-20, 0);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = countryName;
        // Increased font size
        text.fontSize = 50;
        text.color = Color.black;
        text.verticalAlignment = VerticalAlignmentOptions.Middle;
        text.horizontalAlignment = HorizontalAlignmentOptions.Left;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Ellipsis;
        buttonComponent.onClick.AddListener(() => OnSuggestionClicked(countryName));
        suggestionButtons.Add(button);
    }

    private void OnSuggestionClicked(string countryName)
    {
        if (inputField != null)
        {
            inputField.text = countryName;
        }
        HideSuggestions();
        ValidateSelectionRealTime(countryName);
        Debug.Log($"✅ Selected country: {countryName}");
    }

    private void HideSuggestions()
    {
        if (!isShowingSuggestions) return;
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
            countryGameManager.IncrementCorrectGuesses();
            countryGameManager.IncrementCorrectGuessesThisRound();
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