using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class PNGMapImporter : EditorWindow
{
    [MenuItem("Tools/PNG Map Importer")]
    public static void ShowWindow()
    {
        GetWindow<PNGMapImporter>("PNG Map Importer");
    }

    private CountryData targetCountryData;
    private GameObject inputFieldPrefab;
    private string pngFolderPath = "Assets/PNG_Maps";
    private Vector2 scrollPosition;

    private void OnGUI()
    {
        GUILayout.Label("PNG Map Importer", EditorStyles.boldLabel);
        GUILayout.Space(10);

        targetCountryData = (CountryData)EditorGUILayout.ObjectField(
            "Target Country Data", 
            targetCountryData, 
            typeof(CountryData), 
            false
        );

        inputFieldPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Input Field Prefab", 
            inputFieldPrefab, 
            typeof(GameObject), 
            false
        );

        pngFolderPath = EditorGUILayout.TextField("PNG Folder Path", pngFolderPath);

        GUILayout.Space(10);

        if (GUILayout.Button("Import PNG Maps"))
        {
            ImportPNGMaps();
        }

        if (GUILayout.Button("Clear All Country Data"))
        {
            ClearCountryData();
        }

        GUILayout.Space(20);

        // Preview
        if (targetCountryData != null)
        {
            GUILayout.Label($"Current entries: {targetCountryData.countryInfo.Length}", EditorStyles.helpBox);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
            
            for (int i = 0; i < targetCountryData.countryInfo.Length; i++)
            {
                var info = targetCountryData.countryInfo[i];
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.ObjectField(info.gridImage, typeof(Sprite), false, GUILayout.Width(60));
                
                if (info.countryName != null && info.countryName.Count > 0)
                {
                    EditorGUILayout.LabelField($"Countries: {string.Join(", ", info.countryName)}");
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
            }
            
            EditorGUILayout.EndScrollView();
        }
    }

    private void ImportPNGMaps()
    {
        if (targetCountryData == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a CountryData ScriptableObject.", "OK");
            return;
        }

        if (inputFieldPrefab == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign an Input Field Prefab.", "OK");
            return;
        }

        if (!AssetDatabase.IsValidFolder(pngFolderPath))
        {
            EditorUtility.DisplayDialog("Error", $"Folder not found: {pngFolderPath}", "OK");
            return;
        }

        // Get PNG files
        string[] assetGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { pngFolderPath });
        List<string> pngAssetPaths = new List<string>();
        
        foreach (string guid in assetGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
            {
                pngAssetPaths.Add(assetPath);
            }
        }
        
        if (pngAssetPaths.Count == 0)
        {
            EditorUtility.DisplayDialog("Info", "No PNG files found. Make sure PNG files are imported as sprites.", "OK");
            return;
        }

        List<CountryInfo> countryInfoList = new List<CountryInfo>();
        int successCount = 0;

        foreach (string assetPath in pngAssetPaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            
            // Extract country names from filename
            List<string> countryNames = ExtractCountryNames(fileName);
            
            if (countryNames.Count == 0)
            {
                continue;
            }

            // Load sprite
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            
            if (sprite == null)
            {
                continue;
            }

            // Create country info
            CountryInfo countryInfo = new CountryInfo
            {
                gridImage = sprite,
                countryName = countryNames,
                optionsPrefabs = CreateInputFieldPrefabs(countryNames.Count)
            };

            countryInfoList.Add(countryInfo);
            successCount++;
        }

        // Assign to CountryData
        targetCountryData.countryInfo = countryInfoList.ToArray();
        
        EditorUtility.SetDirty(targetCountryData);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Import Complete", 
            $"Imported {successCount} PNG maps with country data.", "OK");
    }

    private List<string> ExtractCountryNames(string fileName)
    {
        List<string> countryNames = new List<string>();
        
        // Pattern: cluster_XXX_Country1_Country2_Country3
        string pattern = @"^cluster_\d+_(.+)$";
        Match match = Regex.Match(fileName, pattern);
        
        if (match.Success)
        {
            string countriesString = match.Groups[1].Value;
            string[] countries = countriesString.Split('_');
            
            foreach (string country in countries)
            {
                if (!string.IsNullOrEmpty(country))
                {
                    string readableCountry = ConvertToReadableCountryName(country);
                    countryNames.Add(readableCountry);
                }
            }
        }
        
        return countryNames;
    }

    private string ConvertToReadableCountryName(string countryName)
    {
        // Handle special cases
        if (countryName == "UnitedStatesofAmerica") return "United States of America";
        if (countryName == "UnitedKingdom") return "United Kingdom";
        if (countryName == "UnitedArabEmirates") return "United Arab Emirates";
        if (countryName == "UnitedRepublicofTanzania") return "United Republic of Tanzania";
        if (countryName == "SaudiArabia") return "Saudi Arabia";
        if (countryName == "SouthKorea") return "South Korea";
        if (countryName == "NorthKorea") return "North Korea";
        if (countryName == "SouthAfrica") return "South Africa";
        if (countryName == "SouthSudan") return "South Sudan";
        if (countryName == "NorthMacedonia") return "North Macedonia";
        if (countryName == "NorthernCyprus") return "Northern Cyprus";
        if (countryName == "WesternSahara") return "Western Sahara";
        if (countryName == "ElSalvador") return "El Salvador";
        if (countryName == "CostaRica") return "Costa Rica";
        if (countryName == "PapuaNewGuinea") return "Papua New Guinea";
        if (countryName == "EastTimor") return "East Timor";
        if (countryName == "SierraLeone") return "Sierra Leone";
        if (countryName == "IvoryCoast") return "Ivory Coast";
        if (countryName == "BurkinaFaso") return "Burkina Faso";
        if (countryName == "CentralAfricanRepublic") return "Central African Republic";
        if (countryName == "DemocraticRepublicoftheCongo") return "Democratic Republic of the Congo";
        if (countryName == "RepublicoftheCongo") return "Republic of the Congo";
        if (countryName == "RepublicofSerbia") return "Republic of Serbia";
        if (countryName == "BosniaandHerzegovina") return "Bosnia and Herzegovina";
        if (countryName == "DominicanRepublic") return "Dominican Republic";
        if (countryName == "EquatorialGuinea") return "Equatorial Guinea";
        if (countryName == "Guinea-Bissau") return "Guinea-Bissau";

        // Add spaces before capital letters
        string result = Regex.Replace(countryName, "([a-z])([A-Z])", "$1 $2");
        return result;
    }

    private GameObject[] CreateInputFieldPrefabs(int count)
    {
        if (inputFieldPrefab == null || count == 0)
            return new GameObject[0];

        GameObject[] prefabs = new GameObject[count];
        for (int i = 0; i < count; i++)
        {
            prefabs[i] = inputFieldPrefab;
        }
        return prefabs;
    }

    private void ClearCountryData()
    {
        if (targetCountryData == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a CountryData ScriptableObject.", "OK");
            return;
        }

        if (EditorUtility.DisplayDialog("Confirm", 
            "Are you sure you want to clear all country data?", 
            "Yes", "No"))
        {
            targetCountryData.countryInfo = new CountryInfo[0];
            EditorUtility.SetDirty(targetCountryData);
            AssetDatabase.SaveAssets();
            
            EditorUtility.DisplayDialog("Success", "Country data cleared.", "OK");
        }
    }
}
