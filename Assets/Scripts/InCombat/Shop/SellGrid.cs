using UnityEngine;

public class SellGrid : MonoBehaviour
{
    public Shop shop; // �ޥΰө�

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
        // �q�����󤤲�������
        character.transform.parent.GetComponent<CharacterParent>().childCharacters.Remove(character.gameObject);
        Destroy(character.gameObject);
    }
}
