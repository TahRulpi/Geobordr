using UnityEngine;
using DG.Tweening; // We'll use DOTween for a smooth scroll animation

public class KeyboardScrollManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The main RectTransform of the panel that holds your input fields.")]
    public RectTransform scrollablePanel;

    [Header("Settings")]
    [Tooltip("How fast the panel moves up and down.")]
    public float scrollSpeed = 0.3f;

    private Vector2 originalPanelPosition;
    private bool isKeyboardVisible = false;

    void Start()
    {
        if (scrollablePanel != null)
        {
            // Store the original position of your UI panel
            originalPanelPosition = scrollablePanel.anchoredPosition;
        }
        else
        {
            Debug.LogError("KeyboardScrollManager: The 'Scrollable Panel' is not assigned!");
        }
    }

    void Update()
    {
        // Check if the keyboard is currently visible on screen
        if (TouchScreenKeyboard.visible)
        {
            if (!isKeyboardVisible)
            {
                // Keyboard has just appeared
                isKeyboardVisible = true;
                MovePanelUp();
            }
        }
        else
        {
            if (isKeyboardVisible)
            {
                // Keyboard has just disappeared
                isKeyboardVisible = false;
                MovePanelDown();
            }
        }
    }

    private void MovePanelUp()
    {
        if (scrollablePanel == null) return;

        // Get the keyboard's height in pixels
        float keyboardHeight = TouchScreenKeyboard.area.height;
        
        // Convert the pixel height to a value that works with the Canvas's scaling
        float canvasHeight = GetComponentInParent<Canvas>().GetComponent<RectTransform>().rect.height;
        float screenHeight = Screen.height;
        float moveAmount = (keyboardHeight / screenHeight) * canvasHeight;

        // Use DOTween to smoothly move the panel up
        scrollablePanel.DOAnchorPosY(originalPanelPosition.y + moveAmount, scrollSpeed).SetEase(Ease.OutQuad);
        Debug.Log($"Keyboard appeared. Moving panel up by {moveAmount} units.");
    }

    private void MovePanelDown()
    {
        if (scrollablePanel == null) return;

        // Use DOTween to smoothly move the panel back to its original position
        scrollablePanel.DOAnchorPos(originalPanelPosition, scrollSpeed).SetEase(Ease.OutQuad);
        Debug.Log("Keyboard hidden. Moving panel back to original position.");
    }
}