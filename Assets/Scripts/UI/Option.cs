using UnityEngine;
using UnityEngine.UI;

public class Option : MonoBehaviour
{
    public int optionIndex;
    public Image Image;
    public Button DescriptionButton;
    public string Description;
    public void Start()
    {
        DescriptionButton.onClick.AddListener(ToggleDescription);
    }
    public void ToggleDescription()
    {
        int indx = References.Instance.DescriptionPanelParent.GetComponent<DescriptionPanel>().DescriptionIndex;
        if (indx != optionIndex)
        {
            References.Instance.DescriptionPanelParent.GetComponent<DescriptionPanel>().DescriptionIndex = optionIndex;
            References.Instance.DescriptionText.text = Description;
            References.Instance.DescriptionPanel.SetActive(true);
        }
        else
        {
             
            References.Instance.DescriptionPanel.SetActive(!References.Instance.DescriptionPanel.activeInHierarchy);
        }
    }
}
