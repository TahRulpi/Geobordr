using UnityEngine;
using UnityEngine.UI;

public class Loadmain : MonoBehaviour
{
    public Image LoadingBar;            // Assign in Inspector
    public float fillSpeed = 0.5f;      // Speed of loading
    public GameObject loadingPanel;     // The panel with loading UI
    public GameObject startPanel;       // The panel to show after loading

    private void Start()
    {
        if (LoadingBar != null)
        {
            LoadingBar.fillAmount = 0f; // Start empty
        }

        if (startPanel != null)
        {
            startPanel.SetActive(false); // Start panel hidden at first
        }
    }

    private void Update()
    {
        if (LoadingBar != null && LoadingBar.fillAmount < 1.0f)
        {
            LoadingBar.fillAmount += fillSpeed * Time.deltaTime;
        }
        else if (LoadingBar != null && LoadingBar.fillAmount >= 1.0f)
        {
            // Hide loading panel and show start panel
            if (loadingPanel != null) loadingPanel.SetActive(false);
            if (startPanel != null) startPanel.SetActive(true);
        }
    }

    public void ResetLoadBar()
    {
        if (LoadingBar != null)
        {
            LoadingBar.fillAmount = 0f;
        }
        if (loadingPanel != null) loadingPanel.SetActive(true);
        if (startPanel != null) startPanel.SetActive(false);
    }
}
