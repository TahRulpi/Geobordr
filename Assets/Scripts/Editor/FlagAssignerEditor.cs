using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class FlagAssignerEditor : EditorWindow
{
    private CountryData targetCountryData;
    private string flagsFolderPath = "Assets/Flags";

    [MenuItem("Tools/Country Game/Flag Assigner and Importer")] // I renamed the menu item to be more descriptive
    public static void ShowWindow()
    {
        GetWindow<FlagAssignerEditor>("Flag Assigner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Flag Assigner & Importer", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "This tool does two things:\n\n" +
            "1. Assigns flag sprites to existing countries in your data.\n" +
            "2. Finds flags in the folder below that do NOT exist in your data and adds them as new entries.",
            MessageType.Info);

        GUILayout.Space(10);

        targetCountryData = (CountryData)EditorGUILayout.ObjectField("Target Country Data", targetCountryData, typeof(CountryData), false);
        flagsFolderPath = EditorGUILayout.TextField("Flags Folder Path", flagsFolderPath);

        GUILayout.Space(20);

        GUI.enabled = targetCountryData != null;

        if (GUILayout.Button("Assign Flags & Add Missing Countries", GUILayout.Height(40)))
        {
            ProcessFlags();
        }

        GUI.enabled = true;
    }

    private void ProcessFlags()
    {
        if (targetCountryData == null || !AssetDatabase.IsValidFolder(flagsFolderPath))
        {
            Debug.LogError("Flag Assigner: Please assign a CountryData asset and a valid folder path.");
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
        Debug.Log($"Flag Assigner: Found {flagSprites.Count} flag sprites in '{flagsFolderPath}'.");

        Undo.RecordObject(targetCountryData, "Assign Flags and Add Missing Countries");

        // --- Step 2: Assign flags to existing countries ---
        int assignedCount = 0;
        foreach (var info in targetCountryData.countryInfo)
        {
            for (int i = 0; i < info.countries.Count; i++)
            {
                var detail = info.countries[i];
                // Only assign if the flag is missing
                if (detail.countryFlag == null && flagSprites.TryGetValue(detail.countryName, out Sprite flagSprite))
                {
                    detail.countryFlag = flagSprite;
                    info.countries[i] = detail; // Re-assign because it's a struct
                    assignedCount++;
                }
            }
        }
        Debug.Log($"<color=green>Assigned {assignedCount} flags to existing countries.</color>");

        // --- Step 3: Find and add missing countries ---
        HashSet<string> existingCountries = new HashSet<string>(targetCountryData.GetAllUniqueCountries(), System.StringComparer.OrdinalIgnoreCase);
        List<CountryInfo> newCountryInfos = new List<CountryInfo>();
        int addedCount = 0;

        foreach (var flagPair in flagSprites)
        {
            string countryName = flagPair.Key;
            Sprite flagSprite = flagPair.Value;

            // If a country with this flag name does NOT exist in our data, add it.
            if (!existingCountries.Contains(countryName))
            {
                // Create a new CountryDetail for this missing country
                CountryDetail newDetail = new CountryDetail
                {
                    countryName = countryName,
                    countryFlag = flagSprite
                };

                // Create a new CountryInfo to hold it. This entry will have no map image.
                CountryInfo newInfo = new CountryInfo
                {
                    gridImage = null, // No map image
                    countries = new List<CountryDetail> { newDetail },
                    optionsPrefabs = new GameObject[0] // No prefabs needed
                };

                newCountryInfos.Add(newInfo);
                addedCount++;
            }
        }

        // --- Step 4: Add the newly created CountryInfo objects to the main data asset ---
        if (newCountryInfos.Count > 0)
        {
            List<CountryInfo> combinedList = targetCountryData.countryInfo.ToList();
            combinedList.AddRange(newCountryInfos);
            targetCountryData.countryInfo = combinedList.ToArray();
            Debug.Log($"<color=cyan>Added {addedCount} new countries from flags (like '{newCountryInfos.First().countries.First().countryName}').</color>");
        }

        // --- Step 5: Save everything ---
        EditorUtility.SetDirty(targetCountryData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Process Complete", $"Assigned {assignedCount} flags and added {addedCount} new countries.", "OK");
    }
}