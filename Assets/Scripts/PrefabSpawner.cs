using UnityEngine;

public class PrefabSpawner : MonoBehaviour
{
    [Header("Data Source")]
    public CountryData countryData;   // Assign your ScriptableObject here
    public int countryIndex = 0;      // Which country info to use

    [Header("Spacing Settings")]
    public float verticalSpacing = 200f;   // distance between each field
    public Vector3 startPosition = Vector3.zero;

    void Start()
    {
        SpawnInputFields();
    }

    void SpawnInputFields()
    {
        // safety check
        if (countryData == null || countryData.countryInfo.Length <= countryIndex)
        {
            Debug.LogError("CountryData not assigned or index out of range!");
            return;
        }

        CountryInfo info = countryData.countryInfo[countryIndex];

        if (info.optionsPrefabs == null || info.optionsPrefabs.Length == 0)
        {
            Debug.LogWarning("No input field prefabs assigned in ScriptableObject!");
            return;
        }

        for (int i = 0; i < info.optionsPrefabs.Length; i++)
        {
            Vector3 spawnPos = startPosition + new Vector3(0, -i * verticalSpacing, 0);

            GameObject field = Instantiate(info.optionsPrefabs[i], transform);
            field.transform.localPosition = spawnPos;
        }
    }
}
