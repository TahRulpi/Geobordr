using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FlagAssignerEditor : EditorWindow
{
    private CountryData targetCountryData;
    private string flagsFolderPath = "Assets/Flags";

    [MenuItem("Tools/Auto Flag Assigner")]
    public static void ShowWindow()
    {
        GetWindow<FlagAssignerEditor>("Auto Flag Assigner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Auto Flag Assigner", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool will automatically assign flag sprites to countries in your CountryData asset. Ensure your flag image filenames exactly match the country names (e.g., 'Germany.png', 'United States of America.png').", MessageType.Info);
        
        GUILayout.Space(10);

        targetCountryData = (CountryData)EditorGUILayout.ObjectField("Target Country Data", targetCountryData, typeof(CountryData), false);
        flagsFolderPath = EditorGUILayout.TextField("Flags Folder Path", flagsFolderPath);

        GUILayout.Space(20);

        GUI.enabled = targetCountryData != null;

        if (GUILayout.Button("Assign Flags Automatically", GUILayout.Height(40)))
        {
            AssignFlags();
        }

        GUI.enabled = true;
    }

    private void AssignFlags()
    {
        if (targetCountryData == null)
        {
            Debug.LogError("Flag Assigner: No CountryData asset selected.");
            return;
        }
        
        if (!AssetDatabase.IsValidFolder(flagsFolderPath))
        {
            Debug.LogError($"Flag Assigner: The folder '{flagsFolderPath}' was not found.");
            return;
        }

        // --- Step 1: Load all flag sprites from the specified folder ---
        Dictionary<string, Sprite> flagSprites = new Dictionary<string, Sprite>();
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { flagsFolderPath });

        foreach (string guid in spriteGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (sprite != null)
            {
                flagSprites[sprite.name] = sprite;
            }
        }

        if (flagSprites.Count == 0)
        {
            Debug.LogWarning("Flag Assigner: No flag sprites found. Make sure your images are Texture Type 'Sprite (2D and UI)'.");
            return;
        }

        Debug.Log($"Flag Assigner: Found {flagSprites.Count} flag sprites to process.");
        // NEW DEBUG LOG: Show a sample of the flag names it found
        Debug.Log($"<color=cyan>Flag name sample: '{string.Join("', '", flagSprites.Keys.Take(5))}'</color>");


        // --- Step 2: Iterate through all countries in the CountryData asset ---
        int assignedCount = 0;
        int notFoundCount = 0;
        HashSet<string> notFoundFlags = new HashSet<string>();
        
        // NEW DEBUG LOG: Get a sample of country names from the data asset
        var countryNameSample = targetCountryData.GetAllUniqueCountries().Take(5);
        Debug.Log($"<color=yellow>CountryData name sample: '{string.Join("', '", countryNameSample)}'</color>");


        Undo.RecordObject(targetCountryData, "Assign Country Flags");

        for (int i = 0; i < targetCountryData.countryInfo.Length; i++)
        {
            for (int j = 0; j < targetCountryData.countryInfo[i].countries.Count; j++)
            {
                CountryDetail countryDetail = targetCountryData.countryInfo[i].countries[j];

                if (countryDetail.countryFlag != null) continue;

                if (flagSprites.TryGetValue(countryDetail.countryName, out Sprite flagSprite))
                {
                    countryDetail.countryFlag = flagSprite;
                    targetCountryData.countryInfo[i].countries[j] = countryDetail;
                    assignedCount++;
                }
                else
                {
                    if (notFoundFlags.Add(countryDetail.countryName))
                    {
                        notFoundCount++;
                    }
                }
            }
        }
        
        EditorUtility.SetDirty(targetCountryData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // --- Step 3: Log the results ---
        Debug.Log($"<color=green>Flag Assigner: Successfully assigned {assignedCount} flags!</color>");

        if (notFoundCount > 0)
        {
            // UPDATED DEBUG LOG: Show ALL names that didn't have a match
            Debug.LogWarning($"Flag Assigner: Could not find matching sprites for {notFoundCount} countries. Please check for naming differences. Examples of names not found: '{string.Join("', '", notFoundFlags.Take(20))}'...");
        }

        EditorUtility.DisplayDialog("Process Complete", $"Assigned {assignedCount} flags automatically.", "OK");
    }
}