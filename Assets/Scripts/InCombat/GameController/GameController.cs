using GameEnum;
using TMPro;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance { get; private set; }
    private int gold;
    public CharacterParent CharacterParent;
    public TextMeshProUGUI GoldText;
    public int AllyEnhancedCharacterIndex = -1;
    public int EnemyEnhancedCharacterIndex = -1;
    public StatsContainer TeamExtraStats = new StatsContainer();
    private int SerinaEnhancedSkill_CritCount;
    private bool triggerSerinaEnhancedSkill = false;
    readonly float yOffset = 0.23f;
    private void Update()
    {
        GoldText.text = $":{gold}";
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public int GetEnhanchedCharacterIndex(bool isAlly)
    {
        return isAlly ? AllyEnhancedCharacterIndex : EnemyEnhancedCharacterIndex;
    }
    public void SetExtraStat(StatsType statsType, float amount)
    {
        TeamExtraStats.SetStat(statsType, amount);
    }
    public void AddExtraStat(StatsType statsType, float amount)
    {
        SetExtraStat(statsType, GetExtraStat(statsType) + amount);
    }
    public void AddSerinaEnhancedSkill_CritCountStack(bool isAlly)
    {
        SerinaEnhancedSkill_CritCount++;
        CharacterParent characterParent = isAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
        CharacterCTRL c = Utility.GetSpecificCharacters(characterParent.GetBattleFieldCharacter(), StatsType.currHealth, true, 1, false)[0];
        CharacterCTRL serina = Utility.GetSpecificCharacterByIndex(characterParent.GetBattleFieldCharacter(), 9);
        c.Heal((int)(0.01f * c.GetStat(StatsType.Health)), c);
        if (!triggerSerinaEnhancedSkill && SerinaEnhancedSkill_CritCount >= 30)
        {
            triggerSerinaEnhancedSkill = true;

            foreach (var item in characterParent.GetBattleFieldCharacter())
            {
                Effect effect = EffectFactory.StatckableStatsEffct(0, "SerinaEnhancedSkill", 50, StatsType.CritRatio, item, true);
                effect.SetActions(
                    (character) => character.ModifyStats(StatsType.CritRatio, effect.Value, effect.Source),
                    (character) => character.ModifyStats(StatsType.CritRatio, -effect.Value, effect.Source)
                );
                item.effectCTRL.AddEffect(effect);
            }
        }
    }
    public float GetExtraStat(StatsType statstype) => TeamExtraStats.GetStat(statstype);
    public (bool, bool) TryMoveCharacter(CharacterCTRL character, HexNode targetSlot)
    {
        if (!targetSlot)
        {
            Debug.Log($"!targetSlot");
            return (false, false);
        }
        bool isBattlefield = targetSlot.IsBattlefield;
        if (targetSlot.Index >= 32 || !targetSlot.isAllyHex)
        {
            PopupManager.Instance.CreatePopup("enemy territory", 2);
            ReturnToOriginalSlot(character);
            return (false, isBattlefield);
        }
        if (!isBattlefield)
        {
            MoveOrSwapCharacter(character, targetSlot);
            BugReportLogger.Instance.MoveCharacterToTile(character.name, targetSlot.name);
            return (true, isBattlefield);
        }
        if (character.characterStats.logistics != targetSlot.IsLogistics)
        {
            Debug.Log(character.characterStats.logistics
                ? $"後勤角色只能放置在後勤格子"
                : $"前線角色不能放置在後勤格子");
            ReturnToOriginalSlot(character);
            return (false, isBattlefield);
        }
        int characterLimit = GameStageManager.Instance.GetCharacterLimit();
        CustomLogger.Log(this, $"GameStageManager.Instance.GetCharacterLimit() = {characterLimit}");
        if (ResourcePool.Instance.ally.GetBattleFieldCharacter().Count >= characterLimit &&
            !character.CurrentHex.IsBattlefield &&
            targetSlot.OccupyingCharacter == null)
        {
            PopupManager.Instance.CreatePopup("On Limit", 2);
            ReturnToOriginalSlot(character);
            return (false, isBattlefield);
        }
        MoveOrSwapCharacter(character, targetSlot);
        BugReportLogger.Instance.MoveCharacterToTile(character.name, targetSlot.name);
        return (true, isBattlefield);
    }


    private void ReturnToOriginalSlot(CharacterCTRL character)
    {
        if (character.CurrentHex != null && !character.characterStats.logistics)
        {
            Vector3 originalPosition = new Vector3(character.CurrentHex.transform.position.x, yOffset, character.CurrentHex.transform.position.z);
            character.transform.position = originalPosition;
        }
        else
        {
            Debug.LogWarning("角色當前沒有有效的格子");
        }
    }

    private void MoveOrSwapCharacter(CharacterCTRL character, HexNode targetSlot)
    {
        if (character.CurrentHex == targetSlot)
        {
            Vector3 v = character.CurrentHex.transform.position;
            character.transform.position = new Vector3(v.x, yOffset, v.z);
            targetSlot.GetComponent<HexNode>().Reserve(character);
            return;
        }

        if (targetSlot.OccupyingCharacter == null)
        {
            MoveCharacterToSlot(character, targetSlot);
        }
        else if (!targetSlot.OccupyingCharacter.isObj || (character.CurrentHex.IsBattlefield && targetSlot.IsBattlefield))
        {
            SwapCharacters(character, targetSlot.OccupyingCharacter);
        }
        else
        {
            ReturnToOriginalSlot(character);
        }
        CharacterParent.UpdateTraitEffects();
    }
    public int GetAliveCount()
    {
        int count = 0;
        foreach (var item in CharacterParent.childCharacters)
        {
            CharacterCTRL ctrl = item.GetComponent<CharacterCTRL>();
            if (ctrl.enterBattle && ctrl.isAlive)
            {
                count++;
            }
        }
        return count;
    }
    private void MoveCharacterToSlot(CharacterCTRL character, HexNode targetSlot)
    {

        if (character.CurrentHex != null)
        {
            character.CurrentHex.SetOccupyingCharacter(null);
            if (character.CurrentHex.TryGetComponent<HexNode>(out HexNode He))
            {
                He.Release();
            }

        }
        targetSlot.SetOccupyingCharacter(character);
        character.CurrentHex = targetSlot;
        CheckSlot(character, character.CurrentHex);
        if (targetSlot.TryGetComponent<HexNode>(out HexNode H))
        {
            H.Reserve(character);
        }
        character.transform.position = new Vector3(targetSlot.transform.position.x, yOffset, targetSlot.transform.position.z);
    }

    private void SwapCharacters(CharacterCTRL character1, CharacterCTRL character2)
    {
        HexNode slot1 = character1.CurrentHex;
        HexNode slot2 = character2.CurrentHex;

        slot1.OccupyingCharacter = character2;
        slot2.OccupyingCharacter = character1;

        character1.CurrentHex = slot2;
        slot2.GetComponent<HexNode>().Reserve(character1);
        character2.CurrentHex = slot1;
        slot1.GetComponent<HexNode>().Reserve(character2);
        CheckSlot(character1, slot2);
        CheckSlot(character2, slot1);
        character1.transform.position = new Vector3(slot2.transform.position.x, yOffset, slot2.transform.position.z);
        character2.transform.position = new Vector3(slot1.transform.position.x, yOffset, slot1.transform.position.z);

        Debug.Log($"Swapped {character1.name} and {character2.name} between slots {slot1.name} and {slot2.name}");
    }
    public void CheckSlot(CharacterCTRL c, HexNode hexNode)
    {
        if (!hexNode.IsBattlefield)
        {
            c.traitController.TriggerOnLeaveBattleField();
        }
        else
        {
            c.traitController.TriggerOnEnterBattleField();
        }
    }
    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log($"gold = {gold}");
    }
    public void SetGold(int amount)
    {
        gold = amount;
    }
    public int GetGoldAmount()
    {
        return gold;
    }
}
