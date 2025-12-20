using UnityEngine;

public class SellGrid : MonoBehaviour
{
    public Shop shop; // ¤Þ¥Î°Ó©±

    public void OnCharacterEnter(CharacterCTRL character)
    {
        SelectedAugments.Instance.CheckIfConditionMatch(1035, true);
        int refundAmount = CalculateRefund(character);
        GameController.Instance.AddGold(refundAmount);
        RemoveCharacter(character);
        shop.UpdateShopUI();
    }

    private int CalculateRefund(CharacterCTRL character)
    {
        return character.star * character.characterStats.Level;
    }

    private void RemoveCharacter(CharacterCTRL character)
    {
        character.equipmentManager.RemoveAllItem();
        character.transform.parent.GetComponent<CharacterParent>().childCharacters.Remove(character.gameObject);
        Destroy(character.gameObject);
    }
}
