using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class RewardEntryView : MonoBehaviour
{
    public TextMeshProUGUI descriptionText;
    public Toggle selectToggle;

    private RewardEntry rewardEntry;
    private Action<RewardEntry, bool> onSelectionChanged;

    public void Setup(RewardEntry entry, Action<RewardEntry, bool> onSelectionChanged)
    {
        rewardEntry = entry;
        this.onSelectionChanged = onSelectionChanged;
        if (descriptionText != null)
            descriptionText.text = entry.GetName();

        if (selectToggle != null)
        {
            selectToggle.isOn = false;
            selectToggle.onValueChanged.RemoveAllListeners();
            selectToggle.onValueChanged.AddListener((bool isOn) =>
            {
                onSelectionChanged?.Invoke(rewardEntry, isOn);
            });
        }
    }
}
