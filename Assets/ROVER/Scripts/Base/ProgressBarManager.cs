using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the progress bar UI, updating its visual state based on progress percentage.
/// </summary>
public class ProgressBarManager : MonoBehaviour
{
    [Header("UI Elements")]
    public Image background;
    public Image bar;
    public TextMeshProUGUI label;

    /// <summary>
    /// Sets the progress of the progress bar.
    /// </summary>
    /// <param name="percent">The progress percentage to display.</param>
    public void SetProgress(int percent)
    {
        // Calculate the new width of the progress bar based on the percentage
        float newWidth = percent * 0.01f * 1360f;

        // Update the size of the progress bar
        bar.rectTransform.sizeDelta = new Vector2(newWidth, bar.rectTransform.rect.height);

        // Update the position of the progress bar to reflect the new width
        bar.rectTransform.anchoredPosition = new Vector2(newWidth * 0.5f, bar.rectTransform.anchoredPosition.y);

        // Update the label text to display the progress percentage
        label.text = percent > 9 ? $"{percent} %" : $"0{percent} %";
    }
}
