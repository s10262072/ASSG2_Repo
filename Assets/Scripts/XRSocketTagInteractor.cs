using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRSocketTagInteractor : XRSocketInteractor
{
    private bool timerActive = false; // Tracks timer state
    private float timeTaken = 0f; // Tracks elapsed time
    public TextMeshProUGUI timeDisplay; // Display for the timer
    private Coroutine timerCoroutine; // Reference to the timer coroutine
    public string targetTag;

    // Coroutine to handle the timer logic
    private IEnumerator TimerCoroutine()
    {
        while (timerActive)
        {
            timeTaken += Time.deltaTime;
            UpdateTimeDisplay();
            yield return null; // Wait for the next frame
        }
    }

    // Start the timer
    public void TimerStart()
    {
        if (!timerActive)
        {
            timerActive = true; // Set timer as active
            timerCoroutine = StartCoroutine(TimerCoroutine()); // Start the coroutine
            Debug.Log("Timer started.");
        }
        else
        {
            Debug.LogWarning("Timer is already active.");
        }
    }

    // Stop the timer
    public void TimerStop()
    {
        if (timerActive)
        {
            timerActive = false; // Set timer as inactive
            if (timerCoroutine != null)
            {
                StopCoroutine(timerCoroutine); // Stop the specific coroutine
                timerCoroutine = null; // Clear the coroutine reference
            }
            timeTaken = 0f; // Reset time
            UpdateTimeDisplay(); // Reset the display
            Debug.Log("Timer stopped.");
        }
        else
        {
            Debug.LogWarning("Timer is already inactive.");
        }
    }

    // Update the timer display
    private void UpdateTimeDisplay()
    {
        int minutes = Mathf.FloorToInt(timeTaken / 60F);
        int seconds = Mathf.FloorToInt(timeTaken % 60F);
        timeDisplay.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Override for hover validation
    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return base.CanHover(interactable) && interactable.transform.tag == targetTag;
    }

    // Override for select validation
    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return base.CanSelect(interactable) && interactable.transform.tag == targetTag;
    }

    private void Update()
    {
        Debug.Log("Timer Status:" + timerActive);
    }
}
