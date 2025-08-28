using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(CountryData))]
public class CountryDataEditor : Editor
{
    private Vector2 scrollPosition;
    private bool showStats = true;
    private bool showPreview = true;
    private string searchFilter = "";

    public override void OnInspectorGUI()
    {
        CountryData countryData = (CountryData)target;
        
        // Header
        EditorGUILayout.LabelField("Country Data Manager", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Stats section
        showStats = EditorGUILayout.Foldout(showStats, "Statistics", true);
        if (showStats)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Data Overview:", countryData.GetDataStats(), EditorStyles.helpBox);
            
            if (countryData.countryInfo != null && countryData.countryInfo.Length > 0)
            {
                var allCountries = countryData.GetAllUniqueCountries();
                EditorGUILayout.LabelField($"All Countries ({allCountries.Count}):", 
                    string.Join(", ", allCountries.Take(10)) + (allCountries.Count > 10 ? "..." : ""));
            }
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Import settings
        EditorGUILayout.LabelField("Game Settings", EditorStyles.boldLabel);
        countryData.minCountriesPerRound = EditorGUILayout.IntSlider("Min Countries Per Round", 
            countryData.minCountriesPerRound, 1, 10);
        countryData.maxCountriesPerRound = EditorGUILayout.IntSlider("Max Countries Per Round", 
            countryData.maxCountriesPerRound, countryData.minCountriesPerRound, 10);
        
        EditorGUILayout.Space();
        
        // Quick actions
        EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Open PNG Importer"))
        {
            PNGMapImporter.ShowWindow();
        }
        
        if (GUILayout.Button("Test Random Cluster"))
        {
            if (countryData.countryInfo != null && countryData.countryInfo.Length > 0)
            {
                var randomCluster = countryData.GetRandomCluster();
                // UPDATED: Reads from the new 'countries' list
                Debug.Log($"Random cluster: {string.Join(", ", randomCluster.countries.Select(c => c.countryName))}");
            }
            else
            {
                Debug.Log("No clusters available to test.");
            }
        }
        
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space();
        
        // Country info array (with preview)
        showPreview = EditorGUILayout.Foldout(showPreview, $"Country Clusters ({countryData.countryInfo?.Length ?? 0})", true);
        
        if (showPreview && countryData.countryInfo != null)
        {
            EditorGUI.indentLevel++;
            
            // Search filter
            searchFilter = EditorGUILayout.TextField("Search Countries:", searchFilter);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300));
            
            for (int i = 0; i < countryData.countryInfo.Length; i++)
            {
                var info = countryData.countryInfo[i];
                
                // Apply search filter
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    bool matchFound = false;
                    // UPDATED: Reads from the new 'countries' list
                    if (info.countries != null)
                    {
                        matchFound = info.countries.Any(countryDetail => 
                            countryDetail.countryName.ToLower().Contains(searchFilter.ToLower()));
                    }
                    
                    if (!matchFound) continue;
                }
                
                // Cluster preview
                EditorGUILayout.BeginVertical("box");
                
                EditorGUILayout.BeginHorizontal();
                
                // Sprite preview (small)
                if (info.gridImage != null)
                {
                    GUILayout.Label(AssetPreview.GetAssetPreview(info.gridImage), 
                        GUILayout.Width(50), GUILayout.Height(50));
                }
                else
                {
                    GUILayout.Box("No Image", GUILayout.Width(50), GUILayout.Height(50));
                }
                
                EditorGUILayout.BeginVertical();
                
                // Countries list
                // UPDATED: Reads from the new 'countries' list
                if (info.countries != null && info.countries.Count > 0)
                {
                    EditorGUILayout.LabelField($"Cluster {i + 1}:", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(string.Join(", ", info.countries.Select(c => c.countryName)), EditorStyles.wordWrappedLabel);
                }
                else
                {
                    EditorGUILayout.LabelField($"Cluster {i + 1}: No countries", EditorStyles.miniLabel);
                }
                
                // Additional info
                EditorGUILayout.LabelField($"Countries: {info.CountryCount} | Prefabs: {info.optionsPrefabs?.Length ?? 0}", 
                    EditorStyles.miniLabel);
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                
                EditorGUILayout.Space(2);
            }
            
            EditorGUILayout.EndScrollView();
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Default inspector for the array (collapsed by default)
        SerializedProperty countryInfoProp = serializedObject.FindProperty("countryInfo");
        EditorGUILayout.PropertyField(countryInfoProp, true);

        // Mark as dirty if GUI changed
        if (GUI.changed)
        {
            EditorUtility.SetDirty(countryData);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}