using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROVER
{
    /// <summary>
    /// Manages the display and content of breadcrumbs in the UI.
    /// </summary>
    public class BreadcrumbManager : MonoBehaviour
    {
        [Header("Breadcrumb Labels")]
        public TextMeshProUGUI breadcrumbItemLabel;
        public TextMeshProUGUI breadcrumbQuestSectionLabel;
        public TextMeshProUGUI breadcrumbQuestIndexLabel;
        public TextMeshProUGUI breadcrumbSectionLabel;

        [Header("Breadcrumb Backgrounds")]
        public Image breadcrumbItemBackground;
        public Image breadcrumbQuestSectionBackground;
        public Image breadcrumbQuestIndexBackground;
        public Image breadcrumbSectionBackground;

        /// <summary>
        /// Sets the content of the breadcrumbs.
        /// </summary>
        /// <param name="item">The item name to display.</param>
        /// <param name="itemSet">The item set name to display.</param>
        /// <param name="section">The section name to display.</param>
        public void SetContent(string item, string itemSet, string section)
        {
            // Set the item name in the breadcrumb
            breadcrumbItemLabel.text = item;

            // Set the item set name in the breadcrumb
            breadcrumbQuestSectionLabel.text = itemSet;

            // Set the section name in the breadcrumb
            breadcrumbQuestIndexLabel.text = section;

            // Clear the section label (if necessary, could be modified as needed)
            breadcrumbSectionLabel.text = "";
        }
    }
}
