using UnityEngine;
using UnityEditor;
using TMPro;

#if UNITY_EDITOR
[System.Serializable]
public class DropdownPrefabCreator : EditorWindow
{
    [Header("Prefab Creation Settings")]
    public GameObject inputFieldPrefab;
    public bool replaceInputWithDropdown = true;
    public bool keepOriginalStyling = true;
    
    [MenuItem("Tools/Country Game/Create Dropdown Prefab")]
    public static void ShowWindow()
    {
        GetWindow<DropdownPrefabCreator>("Create Country Dropdown Prefab");
    }

    private void OnGUI()
    {
        try
        {
            GUILayout.Label("Country Dropdown Prefab Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox("This tool helps you create dropdown prefabs for the country selection game.", MessageType.Info);
            GUILayout.Space(10);
            
            inputFieldPrefab = (GameObject)EditorGUILayout.ObjectField("Input Field Prefab", inputFieldPrefab, typeof(GameObject), false);
            
            GUILayout.Space(10);
            replaceInputWithDropdown = EditorGUILayout.Toggle("Replace Input with Dropdown", replaceInputWithDropdown);
            keepOriginalStyling = EditorGUILayout.Toggle("Keep Original Styling", keepOriginalStyling);
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Create Dropdown Prefab", GUILayout.Height(30)))
            {
                CreateDropdownPrefab();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create Simple Dropdown from Scratch", GUILayout.Height(30)))
            {
                CreateSimpleDropdownPrefab();
            }
        }
        catch (System.Exception e)
        {
            EditorGUILayout.HelpBox($"Error in UI: {e.Message}", MessageType.Error);
            Debug.LogError($"DropdownPrefabCreator UI Error: {e}");
        }
    }

    private void CreateDropdownPrefab()
    {
        try
        {
            if (inputFieldPrefab == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign an Input Field Prefab first!", "OK");
                return;
            }

            // Instantiate the original prefab
            GameObject newPrefab = Instantiate(inputFieldPrefab);
            newPrefab.name = inputFieldPrefab.name.Replace("Input", "Dropdown");

            // Find the TMP_InputField component
            TMP_InputField inputField = newPrefab.GetComponentInChildren<TMP_InputField>();
            
            if (inputField == null)
            {
                EditorUtility.DisplayDialog("Error", "No TMP_InputField found in the prefab!", "OK");
                DestroyImmediate(newPrefab);
                return;
            }

            GameObject inputParent = inputField.gameObject;
            
            if (replaceInputWithDropdown)
            {
                // Get the RectTransform for positioning
                RectTransform inputRect = inputField.GetComponent<RectTransform>();
                Vector3 position = inputRect.anchoredPosition3D;
                Vector2 sizeDelta = inputRect.sizeDelta;
                Vector2 anchorMin = inputRect.anchorMin;
                Vector2 anchorMax = inputRect.anchorMax;

                // Remove the input field
                DestroyImmediate(inputField);

                // Create dropdown
                GameObject dropdownObj = new GameObject("CountryDropdown");
                dropdownObj.transform.SetParent(inputParent.transform, false);

                // Setup RectTransform
                RectTransform dropdownRect = dropdownObj.AddComponent<RectTransform>();
                dropdownRect.anchoredPosition3D = position;
                dropdownRect.sizeDelta = sizeDelta;
                dropdownRect.anchorMin = anchorMin;
                dropdownRect.anchorMax = anchorMax;

                // Add TMP_Dropdown component
                TMP_Dropdown dropdown = dropdownObj.AddComponent<TMP_Dropdown>();
                
                // Create the dropdown structure
                CreateDropdownStructure(dropdown, dropdownObj);
                
                // Add our CountryDropdown script
                dropdownObj.AddComponent<CountryDropdown>();
            }

            // Save as prefab
            string path = $"Assets/Prefabs/{newPrefab.name}.prefab";
            
            // Ensure Prefabs directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            
            PrefabUtility.SaveAsPrefabAsset(newPrefab, path);
            
            // Clean up
            DestroyImmediate(newPrefab);
            
            EditorUtility.DisplayDialog("Success", $"Dropdown prefab created at: {path}", "OK");
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating dropdown prefab: {e}");
            EditorUtility.DisplayDialog("Error", $"Failed to create dropdown prefab: {e.Message}", "OK");
        }
    }

    private void CreateSimpleDropdownPrefab()
    {
        // Create root object
        GameObject dropdownPrefab = new GameObject("CountryDropdownPrefab");
        
        // Add RectTransform
        RectTransform rectTransform = dropdownPrefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(200, 30);

        // Add TMP_Dropdown
        TMP_Dropdown dropdown = dropdownPrefab.AddComponent<TMP_Dropdown>();
        
        // Create dropdown structure
        CreateDropdownStructure(dropdown, dropdownPrefab);
        
        // Add our CountryDropdown script
        dropdownPrefab.AddComponent<CountryDropdown>();

        // Save as prefab
        string path = "Assets/Prefabs/CountryDropdownPrefab.prefab";
        PrefabUtility.SaveAsPrefabAsset(dropdownPrefab, path);
        
        // Clean up
        DestroyImmediate(dropdownPrefab);
        
        EditorUtility.DisplayDialog("Success", $"Simple dropdown prefab created at: {path}", "OK");
        AssetDatabase.Refresh();
    }

    private void CreateDropdownStructure(TMP_Dropdown dropdown, GameObject parent)
    {
        // Create Label
        GameObject label = new GameObject("Label");
        label.transform.SetParent(parent.transform, false);
        
        TextMeshProUGUI labelText = label.AddComponent<TextMeshProUGUI>();
        labelText.text = "Select a country...";
        labelText.fontSize = 14;
        labelText.color = Color.black;
        
        RectTransform labelRect = label.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(10, 6);
        labelRect.offsetMax = new Vector2(-25, -7);

        // Create Arrow
        GameObject arrow = new GameObject("Arrow");
        arrow.transform.SetParent(parent.transform, false);
        
        UnityEngine.UI.Image arrowImage = arrow.AddComponent<UnityEngine.UI.Image>();
        arrowImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/DropdownArrow.psd");
        
        RectTransform arrowRect = arrow.GetComponent<RectTransform>();
        arrowRect.anchorMin = new Vector2(1, 0.5f);
        arrowRect.anchorMax = new Vector2(1, 0.5f);
        arrowRect.sizeDelta = new Vector2(20, 20);
        arrowRect.anchoredPosition = new Vector2(-15, 0);

        // Create Template
        GameObject template = new GameObject("Template");
        template.transform.SetParent(parent.transform, false);
        template.SetActive(false);
        
        UnityEngine.UI.Image templateImage = template.AddComponent<UnityEngine.UI.Image>();
        templateImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        templateImage.type = UnityEngine.UI.Image.Type.Sliced;
        
        RectTransform templateRect = template.GetComponent<RectTransform>();
        templateRect.anchorMin = new Vector2(0, 0);
        templateRect.anchorMax = new Vector2(1, 0);
        templateRect.pivot = new Vector2(0.5f, 1);
        templateRect.anchoredPosition = new Vector2(0, 2);
        templateRect.sizeDelta = new Vector2(0, 150);

        // Create Viewport
        GameObject viewport = new GameObject("Viewport");
        viewport.transform.SetParent(template.transform, false);
        
        UnityEngine.UI.Mask mask = viewport.AddComponent<UnityEngine.UI.Mask>();
        viewport.AddComponent<UnityEngine.UI.Image>();
        
        RectTransform viewportRect = viewport.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;

        // Create Content
        GameObject content = new GameObject("Content");
        content.transform.SetParent(viewport.transform, false);
        
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = new Vector2(0, 0);
        contentRect.sizeDelta = new Vector2(0, 28);

        // Create Item
        GameObject item = new GameObject("Item");
        item.transform.SetParent(content.transform, false);
        
        UnityEngine.UI.Toggle itemToggle = item.AddComponent<UnityEngine.UI.Toggle>();
        item.AddComponent<UnityEngine.UI.Image>();
        
        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.anchorMin = Vector2.zero;
        itemRect.anchorMax = new Vector2(1, 1);
        itemRect.sizeDelta = Vector2.zero;
        itemRect.anchoredPosition = Vector2.zero;

        // Create Item Background
        GameObject itemBackground = new GameObject("Item Background");
        itemBackground.transform.SetParent(item.transform, false);
        
        UnityEngine.UI.Image itemBgImage = itemBackground.AddComponent<UnityEngine.UI.Image>();
        itemBgImage.color = new Color(0.961f, 0.961f, 0.961f);
        
        RectTransform itemBgRect = itemBackground.GetComponent<RectTransform>();
        itemBgRect.anchorMin = Vector2.zero;
        itemBgRect.anchorMax = Vector2.one;
        itemBgRect.sizeDelta = Vector2.zero;
        itemBgRect.anchoredPosition = Vector2.zero;

        // Create Item Checkmark
        GameObject itemCheckmark = new GameObject("Item Checkmark");
        itemCheckmark.transform.SetParent(item.transform, false);
        
        UnityEngine.UI.Image checkmarkImage = itemCheckmark.AddComponent<UnityEngine.UI.Image>();
        checkmarkImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Checkmark.psd");
        
        RectTransform checkmarkRect = itemCheckmark.GetComponent<RectTransform>();
        checkmarkRect.anchorMin = new Vector2(0, 0.5f);
        checkmarkRect.anchorMax = new Vector2(0, 0.5f);
        checkmarkRect.sizeDelta = new Vector2(20, 20);
        checkmarkRect.anchoredPosition = new Vector2(10, 0);

        // Create Item Label
        GameObject itemLabel = new GameObject("Item Label");
        itemLabel.transform.SetParent(item.transform, false);
        
        TextMeshProUGUI itemLabelText = itemLabel.AddComponent<TextMeshProUGUI>();
        itemLabelText.text = "Option A";
        itemLabelText.fontSize = 14;
        itemLabelText.color = Color.black;
        
        RectTransform itemLabelRect = itemLabel.GetComponent<RectTransform>();
        itemLabelRect.anchorMin = Vector2.zero;
        itemLabelRect.anchorMax = Vector2.one;
        itemLabelRect.offsetMin = new Vector2(20, 1);
        itemLabelRect.offsetMax = new Vector2(-10, -2);

        // Setup dropdown references
        UnityEngine.UI.Image backgroundImage = parent.GetComponent<UnityEngine.UI.Image>();
        if (backgroundImage == null)
        {
            backgroundImage = parent.AddComponent<UnityEngine.UI.Image>();
            backgroundImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            backgroundImage.type = UnityEngine.UI.Image.Type.Sliced;
        }
        
        dropdown.targetGraphic = backgroundImage;
        
        dropdown.captionText = labelText;
        dropdown.itemText = itemLabelText;
        dropdown.template = templateRect;
        
        // Setup toggle
        itemToggle.targetGraphic = itemBgImage;
        itemToggle.graphic = checkmarkImage;
        itemToggle.group = template.AddComponent<UnityEngine.UI.ToggleGroup>();

        Debug.Log("Dropdown structure created successfully!");
    }
}
#endif
