using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

#if UNITY_EDITOR
[System.Serializable]
public class AutocompletePrefabCreator : EditorWindow
{
    [Header("Prefab Creation Settings")]
    public GameObject inputFieldPrefab;
    
    [MenuItem("Tools/Country Game/Create Autocomplete Prefab")]
    public static void ShowWindow()
    {
        // Since the new AutocompleteInputField is super simple, just show instructions
        bool result = EditorUtility.DisplayDialog(
            "Create Autocomplete Prefab", 
            "The new AutocompleteInputField is super easy to use!\n\n" +
            "SIMPLE SETUP:\n" +
            "1. Select your input field prefab\n" +
            "2. Add Component → AutocompleteInputField\n" +
            "3. Done! It creates suggestions automatically.\n\n" +
            "No manual setup needed!\n\n" +
            "Would you like to see the old complex creator instead?",
            "Got it!", 
            "Show Complex Creator"
        );
        
        if (!result)
        {
            // User wants the complex creator
            GetWindow<AutocompletePrefabCreator>("Create Autocomplete Prefab");
        }
    }

    private void OnGUI()
    {
        try
        {
            GUILayout.Label("Country Autocomplete Prefab Creator", EditorStyles.boldLabel);
            GUILayout.Space(10);
            
            EditorGUILayout.HelpBox("The new AutocompleteInputField automatically creates suggestion dropdowns. Just add the component to any GameObject with TMP_InputField!", MessageType.Info);
            GUILayout.Space(10);
            
            inputFieldPrefab = (GameObject)EditorGUILayout.ObjectField("Input Field Prefab", inputFieldPrefab, typeof(GameObject), false);
            
            GUILayout.Space(20);
            
            if (GUILayout.Button("Add Autocomplete to Existing Prefab", GUILayout.Height(30)))
            {
                AddAutocompleteToExistingPrefab();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create New Autocomplete Prefab", GUILayout.Height(30)))
            {
                CreateNewAutocompletePrefab();
            }

            GUILayout.Space(20);
            
            EditorGUILayout.HelpBox("Simple Setup:\n1. Select a prefab with TMP_InputField\n2. Click 'Add Autocomplete'\n3. Done! No manual setup needed.", MessageType.Info);
        }
        catch (System.Exception e)
        {
            EditorGUILayout.HelpBox($"Error in UI: {e.Message}", MessageType.Error);
            Debug.LogError($"AutocompletePrefabCreator UI Error: {e}");
        }
    }

    private void AddAutocompleteToExistingPrefab()
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
            newPrefab.name = inputFieldPrefab.name.Replace("Input", "Autocomplete");

            // Find the TMP_InputField component
            TMP_InputField inputField = newPrefab.GetComponentInChildren<TMP_InputField>();
            
            if (inputField == null)
            {
                EditorUtility.DisplayDialog("Error", "No TMP_InputField found in the prefab!", "OK");
                DestroyImmediate(newPrefab);
                return;
            }

            // Add AutocompleteInputField component to the same GameObject as the input field
            AutocompleteInputField autocomplete = inputField.gameObject.AddComponent<AutocompleteInputField>();
            
            // The new AutocompleteInputField automatically creates its own suggestion panel!
            // No manual setup needed anymore.

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
            
            EditorUtility.DisplayDialog("Success", $"Autocomplete prefab created at: {path}\n\nThe autocomplete will automatically show suggestions when you type!", "OK");
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error adding autocomplete to prefab: {e}");
            EditorUtility.DisplayDialog("Error", $"Failed to add autocomplete: {e.Message}", "OK");
        }
    }

    private void CreateNewAutocompletePrefab()
    {
        try
        {
            // Create root object
            GameObject autocompletePrefab = new GameObject("CountryAutocompletePrefab");
            
            // Add RectTransform
            RectTransform rectTransform = autocompletePrefab.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(200, 30);

            // Create input field
            GameObject inputFieldObj = new GameObject("InputField");
            inputFieldObj.transform.SetParent(autocompletePrefab.transform, false);
            
            // Add components to input field
            RectTransform inputRect = inputFieldObj.AddComponent<RectTransform>();
            inputRect.anchorMin = Vector2.zero;
            inputRect.anchorMax = Vector2.one;
            inputRect.sizeDelta = Vector2.zero;
            inputRect.anchoredPosition = Vector2.zero;
            
            Image inputBg = inputFieldObj.AddComponent<Image>();
            inputBg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/InputFieldBackground.psd");
            inputBg.type = Image.Type.Sliced;
            
            TMP_InputField inputField = inputFieldObj.AddComponent<TMP_InputField>();
            
            // Create text area
            GameObject textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputFieldObj.transform, false);
            
            RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.sizeDelta = Vector2.zero;
            textAreaRect.offsetMin = new Vector2(10, 6);
            textAreaRect.offsetMax = new Vector2(-10, -7);
            
            RectMask2D mask = textArea.AddComponent<RectMask2D>();
            
            // Create placeholder text
            GameObject placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            
            RectTransform placeholderRect = placeholder.AddComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.sizeDelta = Vector2.zero;
            placeholderRect.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI placeholderText = placeholder.AddComponent<TextMeshProUGUI>();
            placeholderText.text = "Type country name...";
            placeholderText.fontSize = 14;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.fontStyle = FontStyles.Italic;
            
            // Create input text
            GameObject text = new GameObject("Text");
            text.transform.SetParent(textArea.transform, false);
            
            RectTransform textRect = text.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            TextMeshProUGUI inputText = text.AddComponent<TextMeshProUGUI>();
            inputText.text = "";
            inputText.fontSize = 14;
            inputText.color = Color.black;
            
            // Setup input field references
            inputField.textViewport = textAreaRect;
            inputField.textComponent = inputText;
            inputField.placeholder = placeholderText;
            inputField.targetGraphic = inputBg;
            
            // Add AutocompleteInputField component
            AutocompleteInputField autocomplete = inputFieldObj.AddComponent<AutocompleteInputField>();
            
            // The new AutocompleteInputField automatically creates its own suggestion panel!
            // No manual setup needed anymore.

            // Save as prefab
            string path = "Assets/Prefabs/CountryAutocompletePrefab.prefab";
            
            // Ensure Prefabs directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            
            PrefabUtility.SaveAsPrefabAsset(autocompletePrefab, path);
            
            // Clean up
            DestroyImmediate(autocompletePrefab);
            
            EditorUtility.DisplayDialog("Success", $"New autocomplete prefab created at: {path}\n\nThe autocomplete will automatically show suggestions when you type!", "OK");
            AssetDatabase.Refresh();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating new autocomplete prefab: {e}");
            EditorUtility.DisplayDialog("Error", $"Failed to create autocomplete prefab: {e.Message}", "OK");
        }
    }
}
#endif
