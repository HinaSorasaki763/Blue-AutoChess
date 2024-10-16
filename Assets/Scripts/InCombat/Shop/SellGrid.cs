using UnityEngine;

public class SellGrid : MonoBehaviour
{
    public Shop shop; // 引用商店

    public void OnCharacterEnter(CharacterCTRL character)
    {
        int refundAmount = CalculateRefund(character);
        GameController.Instance.AddGold(refundAmount);
        RemoveCharacter(character);
        shop.UpdateShopUI();
    }

    private int CalculateRefund(CharacterCTRL character)
    {
        int baseCost = character.characterStats.Level;
        return baseCost;
    }

    private void RemoveCharacter(CharacterCTRL character)
    {
        // 從父物件中移除角色
        character.transform.parent.GetComponent<CharacterParent>().childCharacters.Remove(character.gameObject);
        Destroy(character.gameObject);
    }
}
