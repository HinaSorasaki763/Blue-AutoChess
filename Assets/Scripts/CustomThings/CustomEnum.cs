using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

namespace GameEnum
{
    public enum GamePhase
    {
        Preparing,
        Battling
    }
    public enum CharacterState
    {
        Idling,
        Moving,
        Attacking,
        PickedUp,
        CastSkill,
        HaveTarget,
        Stunned,
        Dying
    }
    public enum Traits
    {
        Abydos,
        Gehenna,
        Hyakkiyako,
        Millennium,
        Trinity,
        Supremacy,
        Precision,
        Barrage,
        Aegis,
        Healer,
        Disruptor,
        RapidFire,
        logistic,
        Mystic,
        Arius,
        SRT,
        None
    }
    [SerializeField]
    public enum StatsType
    {
        Health,
        currHealth,
        MaxMana,
        Mana,
        Attack,
        AttackSpeed,
        Resistence,
        Range,
        HealthGrowth,
        ManaGrowth,
        AttackSpeedGrowth,
        ResistenceGrowth,
        CritChance,
        CritRatio,
        DodgeChance,
        Accuracy,
        Shield,
        Lifesteal,
        PercentageResistence,
        healAbility,
        DamageIncrease,
        Null
    }
    public enum ColorState
    {
        Default,
        Burning,
        Reserved,
        TemporaryYellow,
        oasis
    }
    public enum CollectionRewardType
    {
        None,
        Gold,
        RandComponent,
        RandCompleteItem,
        Character,
        Others
    }
    public enum EffectType
    {
        Positive,
        Negative
    }
    public enum DamageSourceType
    {
        Skill,
        NormalAttack,
        Burn,
        Default
    }

    public enum ClearEffectCondition
    {
        OnSkillCastFinished,
        OnDying,
        Never
    }
    public enum SpecialEffectType
    {
        None,
        Stun,
        Marked,
        CCImmune,
        UnTargetable,
        Invincible,
        Fear,
        Taunt
    }
    public enum EquipmentType
    {
        Mana,
        Attack,
        AttackSpeed,
        Defense,
        CriticalRate,
        Health,
        LifeSteal
    }
    public interface IEquipment
    {
        public int Id { get; }
        public string EquipmentName { get; }
        public string EquipmentDetail { get; }
        public string EquipmentDescriptionEnglish { get; }
        public Sprite Icon { get; }
        public bool IsConsumable { get; }
        public CharacterObserverBase Observer { get; set; }
        Dictionary<EquipmentType, int> GetStats();
        void OnRemove(CharacterCTRL character)
        {

        }
        IEquipment Clone();
    }

    [System.Serializable]
    public class BasicEquipment : IEquipment
    {
        public EquipmentType equipmentType;
        public Sprite icon;
        public string equipmentName;
        public string equipmentDetail;
        public string equipmentDescriptionEnglish;
        public int value;
        public bool isConsumable;
        public List<EquipmentType> combinableWith;
        public int id;
        public int Id => id;
        public string EquipmentName => equipmentName;
        public string EquipmentDetail => equipmentDetail;
        public string EquipmentDescriptionEnglish => equipmentDescriptionEnglish;
        public Sprite Icon => icon;
        public bool IsConsumable => isConsumable;
        public List<EquipmentType> Attributes;
        public List<int> Value;
        public CharacterObserverBase observer;
        public CharacterObserverBase Observer
        {
            get => observer;
            set => observer = value;
        }

        public BasicEquipment(EquipmentSO equipmentSO)
        {
            id = equipmentSO.Index;
            icon = equipmentSO.icon;
            equipmentName = equipmentSO.equipmentName;
            equipmentDetail = equipmentSO.equipmentDescription;
            equipmentDescriptionEnglish = equipmentSO.equipmentDescriptionEnglish;
            isConsumable = equipmentSO.IsConsumable;
            Attributes = equipmentSO.Attributes;
            Value = equipmentSO.Value;
        }
        public Dictionary<EquipmentType, int> GetStats()
        {
            Dictionary<EquipmentType, int> combinedStats = new Dictionary<EquipmentType, int>();
            for (int i = 0; i < Attributes.Count; i++)
            {
                combinedStats[Attributes[i]] = Value[i];
            }
            return combinedStats;
        }
        public void OnRemove(CharacterCTRL character)
        {
            CustomLogger.Log(this, $"{EquipmentName} on remove");
        }
        public BasicEquipment() { }
        public IEquipment Clone()
        {
            BasicEquipment copy = new BasicEquipment();

            copy.id = this.id;
            copy.equipmentName = this.equipmentName;
            copy.equipmentDetail = this.equipmentDetail;
            copy.equipmentDescriptionEnglish = this.equipmentDescriptionEnglish;
            copy.icon = this.icon;
            copy.isConsumable = this.isConsumable;
            copy.value = this.value;
            copy.equipmentType = this.equipmentType;

            // 如果 combinableWith 需要複製一份獨立 List，可這樣:
            copy.combinableWith = this.combinableWith != null
                ? new List<EquipmentType>(this.combinableWith)
                : null;
            copy.Observer = null;
            if (this.Attributes != null)
            {
                copy.Attributes = new List<EquipmentType>(this.Attributes);
            }
            else
            {
                copy.Attributes = null;
            }

            if (this.Value != null)
            {
                copy.Value = new List<int>(this.Value);
            }
            else
            {
                copy.Value = null;
            }
            return copy;
        }
    }

    [System.Serializable]
    public class CombinedEquipment : IEquipment
    {
        public BasicEquipment equipment1;
        public BasicEquipment equipment2;
        public string equipmentName;
        public string equipmentDetail;
        public string equipmentDescriptionEnglish;
        public Sprite icon;
        public bool isConsumable;
        public Dictionary<EquipmentType, int> combinedStats;
        public List<EquipmentType> Attributes;
        public List<int> Value;

        public CharacterObserverBase observer;
        public int id;

        public int Id => id;
        public string EquipmentName => equipmentName;
        public string EquipmentDetail => equipmentDetail;
        public string EquipmentDescriptionEnglish => equipmentDescriptionEnglish;
        public Sprite Icon => icon;
        public bool IsConsumable => isConsumable;
        public CharacterObserverBase Observer
        {
            get => observer;
            set => observer = value;
        }

        // 這個建構子是你現有的，用來根據 EquipmentSO 初始化
        public CombinedEquipment(EquipmentSO equipmentSO)
        {
            id = equipmentSO.Index;
            icon = equipmentSO.icon;
            equipmentName = equipmentSO.equipmentName;
            equipmentDetail = equipmentSO.equipmentDescription;
            equipmentDescriptionEnglish = equipmentSO.equipmentDescriptionEnglish;
            isConsumable = equipmentSO.IsConsumable;
            combinedStats = equipmentSO.combinedStats;
            Attributes = equipmentSO.Attributes;
            Value = equipmentSO.Value;
        }

        // 需要一個無參數建構子，給 Clone() 用
        public CombinedEquipment() { }

        public Dictionary<EquipmentType, int> GetStats()
        {
            Dictionary<EquipmentType, int> combinedStats = new Dictionary<EquipmentType, int>();
            for (int i = 0; i < Attributes.Count; i++)
            {
                combinedStats[Attributes[i]] = Value[i];
            }
            return combinedStats;
        }

        public void OnRemove(CharacterCTRL character)
        {
            CustomLogger.Log(this, $"{EquipmentName} on remove");
        }
        public IEquipment Clone()
        {
            CombinedEquipment copy = new CombinedEquipment();
            copy.id = this.id;
            copy.equipmentName = this.equipmentName;
            copy.equipmentDetail = this.equipmentDetail;
            copy.equipmentDescriptionEnglish = this.equipmentDescriptionEnglish;
            copy.icon = this.icon;
            copy.isConsumable = this.isConsumable;
            if (this.combinedStats != null)
            {
                copy.combinedStats = new Dictionary<EquipmentType, int>(this.combinedStats);
            }
            else
            {
                copy.combinedStats = null;
            }
            if (this.Attributes != null)
            {
                copy.Attributes = new List<EquipmentType>(this.Attributes);
            }
            else
            {
                copy.Attributes = null;
            }

            if (this.Value != null)
            {
                copy.Value = new List<int>(this.Value);
            }
            else
            {
                copy.Value = null;
            }
            if (this.equipment1 != null)
            {
                copy.equipment1 = (BasicEquipment)this.equipment1.Clone();
            }
            if (this.equipment2 != null)
            {
                copy.equipment2 = (BasicEquipment)this.equipment2.Clone();
            }
            copy.observer = null;
            return copy;
        }
    }

    public class SpecialEquipment : IEquipment
    {
        public EquipmentType equipmentType;
        public Sprite icon;
        public string equipmentName;
        public string equipmentDetail;
        public string equipmentDescriptionEnglish;
        public bool isSpecial;
        public bool isConsumable;
        public List<Traits> Traits;
        public Traits OriginalstudentTrait;
        public List<EquipmentType> combinableWith;
        public CharacterObserverBase observer;
        public List<EquipmentType> Attributes = new List<EquipmentType>();
        public List<int> Value = new List<int>();
        public int id;

        // ----------------------------------------
        // 實作 IEquipment 介面
        // ----------------------------------------
        public int Id => id;
        public CharacterObserverBase Observer
        {
            get => observer;
            set => observer = value;
        }

        public string EquipmentName => equipmentName;
        public string EquipmentDetail => equipmentDetail;
        public string EquipmentDescriptionEnglish => equipmentDescriptionEnglish;
        public Sprite Icon => icon;
        public bool IsConsumable => isConsumable;
        public SpecialEquipment(EquipmentSO equipmentSO)
        {
            Attributes = equipmentSO.Attributes;
            equipmentType = equipmentSO.Attributes[0]; // 假設只有一個屬性
            icon = equipmentSO.icon;
            equipmentName = equipmentSO.equipmentName;
            equipmentDetail = equipmentSO.equipmentDescription;
            equipmentDescriptionEnglish = equipmentSO.equipmentDescriptionEnglish;
            Value = equipmentSO.Value;
            Traits = equipmentSO.Traits;
            isSpecial = equipmentSO.isSpecial;
            id = equipmentSO.Id;
            combinableWith = new List<EquipmentType>(equipmentSO.Attributes);
        }

        // 無參數建構子 (給 Clone 使用)
        public SpecialEquipment() { }

        // ----------------------------------------
        // 其他方法
        // ----------------------------------------
        public Dictionary<EquipmentType, int> GetStats()
        {
            Dictionary<EquipmentType, int> combinedStats = new Dictionary<EquipmentType, int>();
            for (int i = 0; i < Attributes.Count; i++)
            {
                combinedStats.Add(Attributes[i], Value[i]);
            }
            return combinedStats;
        }

        public void OnRemove(CharacterCTRL character)
        {
            CustomLogger.Log(this, $"{EquipmentName} on remove");
            List<Traits> t = new List<Traits>(Traits);
            t.RemoveAll(x => x == OriginalstudentTrait);
            character.traitController.RemoveTrait(t[0]);
            character.traitController.AddTrait(OriginalstudentTrait);
            OriginalstudentTrait = GameEnum.Traits.None;
        }

        // ----------------------------------------
        // Clone 方法
        // ----------------------------------------
        public IEquipment Clone()
        {
            // 建立一個新的 SpecialEquipment
            SpecialEquipment copy = new SpecialEquipment();

            // 複製基本欄位
            copy.id = this.id;
            copy.equipmentType = this.equipmentType;
            copy.icon = this.icon;
            copy.equipmentName = this.equipmentName;
            copy.equipmentDetail = this.equipmentDetail;
            copy.equipmentDescriptionEnglish = this.equipmentDescriptionEnglish;
            copy.isSpecial = this.isSpecial;
            copy.isConsumable = this.isConsumable;
            copy.Traits = this.Traits;
            copy.OriginalstudentTrait = this.OriginalstudentTrait;
            if (this.Value != null)
            {
                copy.Value = new List<int>(this.Value);
            }
            else
            {
                copy.Value = null;
            }
            if (this.combinableWith != null)
            {
                copy.combinableWith = new List<EquipmentType>(this.combinableWith);
            }
            else
            {
                copy.combinableWith = null;
            }
            if (this.Attributes != null)
            {
                copy.Attributes = new List<EquipmentType>(this.Attributes);
            }
            else
            {
                copy.Attributes = null;
            }
            if (this.Value != null)
            {
                copy.Value = new List<int>(this.Value);
            }
            else
            {
                copy.Value = null;
            }
            copy.observer = null;
            return copy;
        }
    }

    public class ConsumableItem : IEquipment
    {
        public EquipmentType equipmentType;
        public Sprite icon;
        public string equipmentName;
        public string equipmentDetail;
        public string equipmentDescriptionEnglish;
        public int value;
        public bool isSpecial;
        public bool isConsumable;
        public Traits trait;
        public Traits OriginalstudentTrait;
        public List<EquipmentType> combinableWith;
        public IConsumableEffect consumableEffect;
        public CharacterObserverBase observer;
        public int id;

        // --- IEquipment 實作 ---
        public int Id => id;
        public CharacterObserverBase Observer
        {
            get => observer;
            set => observer = value;
        }
        public string EquipmentName => equipmentName;
        public string EquipmentDetail => equipmentDetail;
        public string EquipmentDescriptionEnglish => equipmentDescriptionEnglish;
        public Sprite Icon => icon;
        public bool IsSpecial => isSpecial;
        public bool IsConsumable => isConsumable;

        // 原本的建構子：供從 EquipmentSO 初始化時使用
        public ConsumableItem(EquipmentSO equipmentSO, IConsumableEffect effect, int index)
        {
            icon = equipmentSO.icon;
            equipmentName = equipmentSO.equipmentName;
            equipmentDetail = equipmentSO.equipmentDescription;
            equipmentDescriptionEnglish = equipmentSO.equipmentDescriptionEnglish;
            isSpecial = equipmentSO.isSpecial;
            isConsumable = equipmentSO.IsConsumable;
            consumableEffect = effect;
            id = index;
            // 如果有要初始化 combinableWith、id、trait 等，也可視需求加上
        }

        // 額外新增一個無參數建構子 (給 Clone 用)
        public ConsumableItem() { }

        // 若有從 EquipmentSO 讀取 id 或其他欄位，也可在此補完
        // ...

        // 其餘方法
        public Dictionary<EquipmentType, int> GetStats()
        {
            return new Dictionary<EquipmentType, int> { { equipmentType, value } };
        }

        public void OnActivated()
        {
            CustomLogger.Log(this, $"{equipmentName} activated");
        }

        public void OnRemove(CharacterCTRL character)
        {
            CustomLogger.Log(this, $"{EquipmentName} on remove");
        }

        // --------------------------------
        // Clone 方法
        // --------------------------------
        public IEquipment Clone()
        {
            // 1. 建立一個新的 ConsumableItem 實例
            ConsumableItem copy = new ConsumableItem();

            // 2. 逐一複製欄位
            copy.equipmentType = this.equipmentType;
            copy.icon = this.icon;
            copy.equipmentName = this.equipmentName;
            copy.equipmentDetail = this.equipmentDetail;
            copy.equipmentDescriptionEnglish = this.equipmentDescriptionEnglish;
            copy.value = this.value;
            copy.isSpecial = this.isSpecial;
            copy.isConsumable = this.isConsumable;
            copy.trait = this.trait;
            copy.OriginalstudentTrait = this.OriginalstudentTrait;
            copy.id = this.id;

            // 若你在別處有設定 combinableWith，就做深拷貝
            if (this.combinableWith != null)
                copy.combinableWith = new List<EquipmentType>(this.combinableWith);
            else
                copy.combinableWith = null;

            // consumableEffect 若需要深度複製，則視 IConsumableEffect 是否有 Clone()
            // 這裡僅作淺拷貝(直接複製參考)
            copy.consumableEffect = this.consumableEffect;

            // Observer 設為 null，避免與原本裝備共用
            copy.observer = null;

            // 3. 回傳新實例 (注意回傳型別是 IEquipment)
            return copy;
        }
    }

    public enum ConsumableEffectType
    {
        None,
        Remover,
        AriusSelector,
        Duplicator,
        Oasis
    }
    public interface IConsumableEffect
    {
        bool Permanent { get; }
        void ApplyEffect(CharacterCTRL target);
        void RemoveEffect(CharacterCTRL target);

    }
    public class None : IConsumableEffect
    {
        public bool Permanent => false;
        public void ApplyEffect(CharacterCTRL target)
        {
            CustomLogger.LogWhenThingShouldntHappened(this);
        }

        public void RemoveEffect(CharacterCTRL target)
        {
            CustomLogger.LogWhenThingShouldntHappened(this);
        }
    }
    public class Remover : IConsumableEffect
    {
        public bool Permanent => true;
        public void ApplyEffect(CharacterCTRL target)
        {
            target.equipmentManager.RemoveAllItem();
            CustomLogger.Log(this, $"Applied Remover to {target.name}");
        }

        public void RemoveEffect(CharacterCTRL target)
        {
            CustomLogger.Log(this, $"Remove Remover from {target.name}");
        }
    }
    public class AriusSelector : IConsumableEffect
    {
        public bool Permanent => true;

        public void ApplyEffect(CharacterCTRL target)
        {
            if (!target.traitController.HasTrait(Traits.Arius))
            {
                PopupManager.Instance.CreatePopup("Not Arius squad's character!", 2);
            }
            else if (!target.CurrentHex.IsBattlefield)
            {
                PopupManager.Instance.CreatePopup("Not On Board!", 2);
            }
            else
            {
                // 獲取 AriusObserver
                var observer = target.traitController.GetObserverForTrait(Traits.Arius) as AriusObserver;

                if (observer != null)
                {
                    bool b = !observer.IsSonOfGod;
                    AriusManager.Instance.ResetAllGodOfSonFlags();
                    observer.SetGodOfSon(b);
                }
                else
                {
                    PopupManager.Instance.CreatePopup($"Arius Trait not found for the target character.{target.name}", 2);
                }
            }
        }
        public void RemoveEffect(CharacterCTRL target)
        {
            CustomLogger.Log(this, $"Remove AriusSelector from {target.name}");
        }
    }
    public class Oasis : IConsumableEffect
    {
        public bool Permanent => false;
        public void UpdateSlot(HexNode h)
        {
            foreach (var item in h.Neighbors)
            {
                h.isDesertified = false;
                h.oasis = true;
            }
        }
        public void ApplyEffect(CharacterCTRL _)
        {

        }
        public void RemoveEffect(CharacterCTRL target)
        {
            
        }
    }
    public class Duplicator : IConsumableEffect
    {
        public bool Permanent => false;
        public void ApplyEffect(CharacterCTRL target)
        {
            if (!ResourcePool.Instance.BenchManager.IsBenchFull())
            {
                ResourcePool.Instance.BenchManager.AddToBench(target.characterStats.Model);

            }
            else
            {
                CustomLogger.Log(this, "备战席已满，无法添加新角色。");
            }
            CustomLogger.Log(this, $"Applied Duplicator to {target.name}");
        }

        public void RemoveEffect(CharacterCTRL target)
        {
            CustomLogger.Log(this, $"Remove Duplicator from {target.name}");
        }
    }
    [Serializable]
    public class StatsContainer
    {
        [SerializeField]
        private List<Stat> stats = new List<Stat>();

        // 初始化時確保每個StatsType都有對應的Stat
        public StatsContainer()
        {
            foreach (StatsType type in Enum.GetValues(typeof(StatsType)))
            {
                stats.Add(new Stat { statType = type, value = 0 });
            }
        }
        public StatsContainer MultiplyBy(float ratio)
        {
            StatsContainer newS = this.Clone();
            foreach (var item in newS.stats)
            {
                item.value *= ratio;
            }
            return newS;
        }
        public StatsContainer Clone()
        {
            StatsContainer newContainer = new StatsContainer();
            newContainer.stats = new List<Stat>();

            foreach (Stat stat in this.stats)
            {
                newContainer.stats.Add(stat.Clone());
            }

            return newContainer;
        }

        public void SetStat(StatsType type, float value)
        {
            Stat stat = stats.Find(s => s.statType == type);
            if (stat != null)
            {
                stat.value = value;
            }
            else
            {

                Debug.LogWarning($"Stat of type {type} not found.");
            }
        }

        // 取得 Stat 的值
        public float GetStat(StatsType type)
        {
            Stat stat = stats.Find(s => s.statType == type);
            if (stat != null)
            {
                return stat.value;
            }
            stats.Add(new Stat { statType = type, value = 0 });
            return 0;
        }
        public void AddFrom(StatsContainer other)
        {
            foreach (var otherStat in other.stats)
            {
                float currentValue = GetStat(otherStat.statType);
                float newValue = currentValue + otherStat.value;
                SetStat(otherStat.statType, newValue);
            }
        }
        public void RemoveFrom(StatsContainer other)
        {
            foreach (var otherStat in other.stats)
            {
                float currentValue = GetStat(otherStat.statType);
                float newValue = currentValue - otherStat.value;
                SetStat(otherStat.statType, newValue);
            }
        }
        public void AddValue(StatsType type, float value)
        {
            Stat stat = stats.Find(s => s.statType == type);
            if (stat != null)
            {
                stat.value += value;
            }
            else
            {
                stats.Add(new Stat { statType = type, value = 0 });
                stat.value += value;
            }
        }

        public float SumAllStats()
        {
            float total = 0;
            foreach (var s in stats)
            {
                total += s.value;
            }
            return total;
        }
        public void Clear()
        {
            foreach (var stat in stats)
            {
                stat.value = 0;
            }
        }
        public List<Stat> GetAllStats()
        {
            List<Stat> copy = new List<Stat>();
            foreach (var stat in stats)
            {
                copy.Add(stat.Clone());
            }
            return copy;
        }
        public static StatsContainer FromDict(Dictionary<string, object> dict)
        {
            StatsContainer container = new StatsContainer();

            foreach (var kvp in dict)
            {
                if (Enum.TryParse(kvp.Key, out StatsType type))
                {
                    // Firestore float/double 都可能是 System.Double
                    float val = 0f;
                    if (kvp.Value is long l) val = l;
                    else if (kvp.Value is double d) val = (float)d;
                    else if (kvp.Value is float f) val = f;

                    container.SetStat(type, val);
                }
                else
                {
                    Debug.LogWarning($"Unknown stat key: {kvp.Key}");
                }
            }

            return container;
        }
    }
    [Serializable]
    public class Stat
    {
        public StatsType statType;
        public float value;
        public Stat Clone()
        {
            return new Stat
            {
                statType = this.statType,
                value = this.value
            };
        }
    }
    public enum BattleDisplayEffect
    {
        Weak,
        Resist,
        Immune,
        Block,
        Miss,
        None
    }
    public enum SkillPrefab
    {
        PenetrateTrailedBullet,
        HealPack,
        NormalTrailedBullet,
        MissleFragmentsPrefab,
        SmallPenetrateTrailedBullet,
        Missle,
        Beam,
    }
    public enum ModifierType
    {
        DamageTaken,  // 受到的伤害
        DamageDealt,
        None
    }

    public class StatModifier
    {
        public ModifierType ModifierType { get; private set; }
        public float Value { get; set; }
        public string Source { get; private set; }
        public bool IsPermanent { get; set; }
        public float Duration { get; set; } // 仅对暂时修正器有效

        public StatModifier(ModifierType modifierType, float value, string source, bool isPermanent, float duration = 0f)
        {
            ModifierType = modifierType;
            Value = Value;
            Source = source;
            IsPermanent = isPermanent;
            Duration = duration;
        }
    }


    [System.Serializable]
    public class RoundProbability
    {
        public int OneCostProbability;
        public int TwoCostProbability;
        public int ThreeCostProbability;
        public int FourCostProbability;
        public int FiveCostProbability;
    }
    public static class Utility
    {
        public static IEquipment GetSpecificEquipment(int index)
        {
            foreach (var item in EquipmentManager.Instance.availableEquipments)
            {
                if (item.Id == index)
                {
                    return item;
                }
            }
            return null;
        }

        public static Traits IsAcademy(List<Traits> traits)
        {
            foreach (var item in traits)
            {
                if (item == Traits.Abydos ||
                    item == Traits.Gehenna ||
                    item == Traits.Hyakkiyako ||
                    item == Traits.Millennium ||
                    item == Traits.SRT ||
                    item == Traits.Arius ||
                    item == Traits.Trinity) return item;
            }
            return Traits.None;
        }
        public static GameObject GetSpecificCharacterToSpawn(int index)
        {
            List<Character> allCharacters = ResourcePool.Instance.GetAllCharacters();
            foreach (var item in allCharacters)
            {
                if (item.CharacterId == index)
                {
                    return item.Model;
                }
            }
            return null;
        }
        public static void ChangeImageAlpha(Image image, float alpha)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
        public static IEquipment GetExchangeCirtificate(Traits trait1,Traits trait2)
        {
            IEquipment equipment = GetSpecificEquipment(101);
            IEquipment eq = equipment.Clone();
            if (eq is SpecialEquipment special)
            {
                special.Traits.Clear();
                special.Traits.Add(trait1);
                special.Traits.Add(trait2);
                special.equipmentDetail = $"{special.Traits[0]} and {special.Traits[1]} exchange certificate";
                special.equipmentDescriptionEnglish = $"{special.Traits[0]} and {special.Traits[1]} exchange certificate";
                return special;
            }
            else
            {
                return null;
            }
        }
        public static List<CharacterCTRL> GetSpecificCharacters(List<CharacterCTRL> characters, StatsType statsType, bool descending, int count, bool filterTargetable)
        {
            if (filterTargetable)
            {
                characters = characters.Where(item => item.isTargetable).ToList();
            }
            if (descending)
            {
                return characters
                    .OrderByDescending(item => item.GetStat(statsType))
                    .Take(count)
                    .ToList();
            }
            else
            {
                return characters
                    .OrderBy(item => item.GetStat(statsType))
                    .Take(count)
                    .ToList();
            }
        }


        public static HexNode GetHexOnPos(Vector3 pos)
        {
            Vector3Int cubeCoords = PositionToCubeCoordinates(pos);
            string key = CubeCoordinatesToKey(cubeCoords.x, cubeCoords.y, cubeCoords.z);

            Debug.Log($"GetHexOnPos - Position: {pos}, CubeCoords: {cubeCoords}, Key: {key}");

            if (SpawnGrid.Instance.hexNodes.TryGetValue(key, out HexNode node))
            {
                return node;
            }
            else
            {
                Debug.LogError($"No HexNode found at position {pos} with cube coordinates {cubeCoords}");
                return null;
            }
        }
        public static HexNode FindFarthestNode(CharacterCTRL c, int minDistance)
        {
            bool isAlly = c.IsAlly;
            List<HexNode> enemyNodes = new List<HexNode>();
            foreach (var node in SpawnGrid.Instance.hexNodes.Values)
            {
                if (node.OccupyingCharacter != null && node.OccupyingCharacter.IsAlly != isAlly)
                {
                    enemyNodes.Add(node);
                }
            }
            if (enemyNodes.Count == 0)
            {
                return null;
            }

            Dictionary<HexNode, int> distanceMap = new Dictionary<HexNode, int>();
            Queue<HexNode> queue = new Queue<HexNode>();
            foreach (var eNode in enemyNodes)
            {
                distanceMap[eNode] = 0;
                queue.Enqueue(eNode);
            }

            while (queue.Count > 0)
            {
                HexNode current = queue.Dequeue();
                int currentDist = distanceMap[current];

                foreach (var neighbor in current.Neighbors)
                {
                    if (!distanceMap.ContainsKey(neighbor))
                    {
                        distanceMap[neighbor] = currentDist + 1;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            HexNode farthestNode = null;
            int maxDist = -1;
            foreach (var kvp in distanceMap)
            {
                var node = kvp.Key;
                int dist = kvp.Value;

                if (node.OccupyingCharacter == null && dist <= minDistance)
                {
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        farthestNode = node;
                    }
                    else if (dist == maxDist && farthestNode != null)
                    {
                        if (isAlly)
                        {
                            if (node.Index < farthestNode.Index)
                            {
                                farthestNode = node;
                            }
                        }
                        else
                        {
                            if (node.Index > farthestNode.Index)
                            {
                                farthestNode = node;
                            }
                        }
                    }
                }
            }
            return farthestNode;
        }

        public static HexNode FindSpotToSpawnEnemy(bool ascending = false)
        {
            IEnumerable<HexNode> nodes = SpawnGrid.Instance.hexNodes.Values
                .Where(node => node.Index >= 32 && node.Index <= 64 && node.OccupyingCharacter == null);

            return ascending
                ? nodes.OrderBy(node => node.Index).FirstOrDefault()       // 正序
                : nodes.OrderByDescending(node => node.Index).FirstOrDefault(); // 倒序
        }



        private static Vector3Int PositionToCubeCoordinates(Vector3 position)
        {
            // 根据您的网格设置调整以下参数
            float hexSize = 1f; // 六边形的大小
            float hexWidth = Mathf.Sqrt(3f) * hexSize; // 六边形的宽度
            float hexHeight = 2f * hexSize * 0.75f; // 六边形的高度（考虑到垂直间距为 3/4）
            float q = (position.x * Mathf.Sqrt(3f) / 3f - position.z / 3f) / hexSize;
            float r = position.z * 2f / 3f / hexSize;
            float x = q;
            float z = r;
            float y = -x - z;
            int rx = Mathf.RoundToInt(x);
            int ry = Mathf.RoundToInt(y);
            int rz = Mathf.RoundToInt(z);

            // 处理舍入误差
            float xDiff = Mathf.Abs(rx - x);
            float yDiff = Mathf.Abs(ry - y);
            float zDiff = Mathf.Abs(rz - z);

            if (xDiff > yDiff && xDiff > zDiff)
            {
                rx = -ry - rz;
            }
            else if (yDiff > zDiff)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            return new Vector3Int(rx, ry, rz);
        }

        // 生成字符串键的方法
        private static string CubeCoordinatesToKey(int x, int y, int z)
        {
            return $"{x},{y},{z}";
        }
        public static List<HexNode> GetHexInRange(HexNode startNode, int range)
        {

            List<HexNode> visited = new List<HexNode> { startNode };
            if (range == 0) return visited;
            Queue<HexNode> frontier = new Queue<HexNode>();
            frontier.Enqueue(startNode);
            int currentRange = 0;

            while (frontier.Count > 0 && currentRange < range)
            {
                int frontierCount = frontier.Count;
                for (int i = 0; i < frontierCount; i++)
                {
                    HexNode currentNode = frontier.Dequeue();
                    foreach (var neighbor in currentNode.Neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            frontier.Enqueue(neighbor);
                        }
                    }
                }
                currentRange++;
            }
            return visited;
        }
        public static CharacterCTRL GetSpecificCharacterByIndex(List<CharacterCTRL> list, int index)
        {
            CharacterCTRL bestMatch = null;
            float highestAttack = float.MinValue;
            foreach (var item in list)
            {
                if (item.characterStats.CharacterId == index)
                {
                    float currentAttack = item.GetAttack();
                    if (currentAttack > highestAttack)
                    {
                        highestAttack = currentAttack;
                        bestMatch = item;
                    }
                }
            }
            if (bestMatch != null)
            {
                CustomLogger.Log(bestMatch, $"Selected with highest attack: {highestAttack}");
            }
            return bestMatch;
        }
        public static bool CompareTwoGroups(List<CharacterCTRL> list1, List<CharacterCTRL> list2)
        {
            int[] sums1 = SumStats(list1);
            int[] sums2 = SumStats(list2);

            for (int i = 0; i < sums1.Length; i++)
                if (sums1[i] > sums2[i]) return true;
                else if (sums1[i] < sums2[i]) return false;

            return false;

            static int[] SumStats(List<CharacterCTRL> list)
            {
                int star = 0, attack = 0, health = 0, def = 0;
                foreach (var c in list)
                {
                    star += c.star;
                    attack += c.GetAttack();
                    health += (int)c.GetStat(StatsType.Health);
                    def += (int)c.GetStat(StatsType.Resistence);
                }
                return new[] { star, attack, health, def };
            }
        }

        public static bool CheckExecuted(CharacterCTRL target,CharacterCTRL source,float percentage,string detailedSource)
        {
            if (target.GetHealthPercentage() <= percentage)
            {
                target.Executed(source, detailedSource);
                return true;
            }
            return false;
        }
        public static List<CharacterCTRL> GetCharacterInSet(List<HexNode> nodes, CharacterCTRL finder, bool findingAlly)
        {
            var characters = new List<CharacterCTRL>();
            foreach (var item in nodes)
            {
                if (item.OccupyingCharacter != null && (item.OccupyingCharacter.IsAlly == finder.IsAlly) == findingAlly && !item.OccupyingCharacter.characterStats.logistics)
                {
                    characters.Add(item.OccupyingCharacter);
                    CustomLogger.Log(item,$"[GetCharacterInSet]Add {item.OccupyingCharacter} to set");
                }
            }
            return characters;
        }
        public static List<CharacterCTRL> GetCharacterInrange(HexNode startNode, int range, CharacterCTRL finder, bool findingAlly, bool isInjured = false)
        {
            var characters = new HashSet<CharacterCTRL>();
            if (finder == null) return characters.ToList();
            CustomLogger.Log(finder, $"GetCharacterInrange - StartNode: {startNode.name}, Range: {range}, Finder: {finder.name}, FindingAlly: {findingAlly}");

            var nodes = GetHexInRange(startNode, range);
            CustomLogger.Log(finder, $"GetCharacterInrange - Nodes: {string.Join(", ", nodes.Select(n => n.name))}");
            foreach (var item in nodes)
            {
                if (item.OccupyingCharacter != null && item.OccupyingCharacter.IsAlly == (finder.IsAlly == findingAlly) && !item.OccupyingCharacter.characterStats.logistics)
                {
                    characters.Add(item.OccupyingCharacter);
                }
            }
            return characters.ToList();
        }
        public static List<(CharacterCTRL character, float weight)> GetCharacterInrangeWithWeight(
    HexNode startNode,
    int range,
    CharacterCTRL finder,
    bool findingAlly,
    bool isInjured = false)
        {
            var result = new List<(CharacterCTRL, float)>();
            CustomLogger.Log(finder, $"GetCharacterInrangeWithWeight - StartNode: {startNode.name}, " +
                                     $"Range: {range}, Finder: {finder.name}, FindingAlly: {findingAlly}, " +
                                     $"isInjured: {isInjured}");

            var nodes = GetHexInRange(startNode, range);
            CustomLogger.Log(finder, $"GetCharacterInrangeWithWeight - Nodes: {string.Join(", ", nodes.Select(n => n.name))}");

            foreach (var node in nodes)
            {
                var occupant = node.OccupyingCharacter;
                if (occupant != null
                    && occupant.IsAlly == (finder.IsAlly == findingAlly)
                    && !occupant.characterStats.logistics
                    && occupant.isAlive)
                {
                    float weight = 1;
                    if (isInjured)
                    {
                        weight += 1 - occupant.GetHealthPercentage();
                    }

                    // 收集 (角色, 權重)
                    result.Add((occupant, weight));
                }
            }

            return result;
        }

        public static void DealDamageInRange(HexNode startNode, int range, CharacterCTRL sourceCharacter, int dmg, string source, bool iscrit)
        {
            foreach (var item in GetCharacterInrange(startNode, range, sourceCharacter, false))
            {
                item.GetHit(dmg, sourceCharacter, source, iscrit);
            }
        }
        public static bool TryAssign(CharacterCTRL character,HashSet<HexNode> visited,Dictionary<CharacterCTRL, List<HexNode>> adjacency,ref Dictionary<HexNode, CharacterCTRL> matchNode)
        {
            if (!adjacency.ContainsKey(character)) return false;
            var nodes = adjacency[character];

            foreach (var node in nodes)
            {
                if (visited.Contains(node)) continue;
                visited.Add(node);
                if (matchNode[node] == null || TryAssign(matchNode[node], visited, adjacency, ref matchNode))
                {
                    matchNode[node] = character;
                    return true;
                }
            }
            return false;
        }
        public static void SetRandKey(CharacterCTRL c = null)
        {
            int battleCounterInt = Mathf.FloorToInt(GameStageManager.Instance.enteringBattleCounter * 1000);
            int seed = ResourcePool.Instance.RandomKeyThisGame + battleCounterInt;
            if (c != null)
            {
                int i = c.IsAlly ? 1 : 0;
                seed += c.characterStats.CharacterId + i + seed;
            }
            UnityEngine.Random.InitState(seed);
        }
        public static int GetRand(CharacterCTRL c = null)
        {
            SetRandKey(c);
            return UnityEngine.Random.Range(0, 101);
        }

        public static float GetRandfloat(CharacterCTRL c = null)
        {
            SetRandKey(c);
            return UnityEngine.Random.Range(0f, 1f);
        }

        public static bool Iscrit(float critChance, CharacterCTRL c)
        {
            int rand = GetRand(c);
            return rand <= critChance;
        }

        public static List<CharacterCTRL> GetAllBattlingCharacter(CharacterParent parent)
        {
            List<CharacterCTRL> list = new List<CharacterCTRL>();
            foreach (var item in parent.childCharacters)
            {
                CharacterCTRL c = item.GetComponent<CharacterCTRL>();
                if (c.enterBattle && c.isAlive)
                {
                    list.Add(c);
                }
            }
            return list;
        }
        public static (List<HexNode> bestNodes, HexNode oppositeNode, int maxCount, int direction) FindMaxOccupantArcNode(CharacterCTRL character, bool findAlly)
        {
            int globalMaxCount = -1;
            float globalMinDistance = int.MaxValue;
            HexNode globalBestNode = null;
            HexNode globalOppositeNode = null;
            List<HexNode> globalBestNodeList = new List<HexNode>();
            int dir = 0;

            foreach (var node in SpawnGrid.Instance.hexNodes.Values)
            {
                List<(int occupantCount, int direction, HexNode opposite, List<HexNode> nodeset)> directionInfo
                    = new List<(int occupantCount, int direction, HexNode opposite, List<HexNode> nodeset)>();
                for (int i = 0; i < 6; i++)
                {
                    List<HexNode> nodelist = new List<HexNode>();
                    int occupantCount = 0;
                    nodelist.Add(node);
                    occupantCount += CountIfHasOccupant(GetNeighbor(node, i), character, findAlly);
                    nodelist.Add(GetNeighbor(node, i));
                    occupantCount += CountIfHasOccupant(GetNeighbor(node, (i + 1) % 6), character, findAlly);
                    nodelist.Add(GetNeighbor(node, (i + 1) % 6));
                    occupantCount += CountIfHasOccupant(GetNeighbor(node, (i + 5) % 6), character, findAlly);
                    nodelist.Add(GetNeighbor(node, (i + 5) % 6));
                    HexNode oppositeNode = GetNeighbor(node, (i + 3) % 6);
                    directionInfo.Add((occupantCount, i, oppositeNode, nodelist));
                }

                directionInfo.Sort((a, b) => b.occupantCount.CompareTo(a.occupantCount));
                foreach (var info in directionInfo)
                {
                    if (info.opposite != null && info.opposite.OccupyingCharacter == null)
                    {
                        HexNode nextNeighbor = GetNeighbor(info.opposite, (info.direction + 3) % 6);
                        if (nextNeighbor != null && nextNeighbor.OccupyingCharacter == null)
                        {
                            if (info.occupantCount > globalMaxCount)
                            {
                                globalMaxCount = info.occupantCount;
                                globalMinDistance = Vector3.Distance(character.CurrentHex.transform.position,node.transform.position);
                                globalBestNode = node;
                                dir = info.direction;
                                globalOppositeNode = info.opposite;
                                globalBestNodeList = info.nodeset;
                            }
                            else if (info.occupantCount == globalMaxCount)
                            {
                                float dist = Vector3.Distance(character.CurrentHex.transform.position, node.transform.position);
                                if (dist < globalMinDistance)
                                {
                                    globalMinDistance = dist;
                                    globalBestNode = node;
                                    dir = info.direction;
                                    globalOppositeNode = info.opposite;
                                    globalBestNodeList = info.nodeset;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            CustomLogger.Log(globalBestNode, $"Max occupant arc = {globalMaxCount}, Node = {globalBestNode?.name}, Opposite = {globalOppositeNode?.name}");
            return (globalBestNodeList, globalOppositeNode, globalMaxCount, dir);
        }


        /// <summary>
        /// 安全取得某一個節點的第 dir 個鄰居(如果超過鄰居列表範圍或者 null 就直接返回 null)。
        /// </summary>
        public static HexNode GetNeighbor(HexNode node, int dir)
        {
            if (node.Neighbors == null) return null;
            if (dir < 0 || dir >= node.Neighbors.Count) return null;
            return node.Neighbors[dir];
        }

        /// <summary>
        /// 若該鄰居有角色，回傳1，否則回傳0。
        /// </summary>
        private static int CountIfHasOccupant(HexNode neighbor, CharacterCTRL character, bool findAlly)
        {
            if (neighbor != null && neighbor.OccupyingCharacter != null && (character.IsAlly == neighbor.OccupyingCharacter.IsAlly) == findAlly)
                return 1;
            return 0;
        }
        public static List<Character> GetCharactersWithTrait(Traits trait)
        {
            var allCharacters = ResourcePool.Instance.Lists.SelectMany(list => list);
            var matchedCharacters = allCharacters
                .Where(character => character.Traits != null && character.Traits.Contains(trait))
                .ToList();
            foreach (var c in matchedCharacters)
            {
                CustomLogger.Log(c, $"Character {c.CharacterName} has trait {trait}");
            }
            return matchedCharacters;
        }
        public static List<CharacterCTRL> GetInBattleCharactersWithTrait(Traits trait, bool isAlly)
        {
            CharacterParent characterParent = isAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
            List<CharacterCTRL> allCharacters = new();
            foreach (var item in characterParent.childCharacters)
            {
                CharacterCTRL c = item.GetComponent<CharacterCTRL>();
                if (!c.isObj && c.CurrentHex.IsBattlefield)
                {
                    allCharacters.Add(c);
                }

            }
            var matchedCharacters = allCharacters
                .Where(character => character.traitController.GetCurrentTraits().Contains(trait))
                .ToList();
            foreach (var c in matchedCharacters)
            {
                CustomLogger.Log(c, $"Character {c.name} has trait {trait}");
            }
            return matchedCharacters;
        }
        /// <summary>
        /// 
        /// 取得最近的友軍(沒有其他友軍會傳回自己)
        /// </summary>
        public static CharacterCTRL GetNearestAlly(CharacterCTRL c)
        {
            CharacterParent characterParent = c.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
            CharacterCTRL t = null;
            float dist = float.MaxValue;

            foreach (var item in characterParent.childCharacters)
            {
                CharacterCTRL cTRL = item.GetComponent<CharacterCTRL>();

                if (cTRL == null || cTRL == c) continue; // 排除自己

                if (cTRL.isAlive && !cTRL.characterStats.logistics && cTRL.enterBattle && cTRL.gameObject.activeInHierarchy)
                {
                    float d = Vector3.Distance(c.transform.position, cTRL.transform.position);
                    if (d < dist)
                    {
                        dist = d;
                        t = cTRL;
                    }
                }
            }
            return t != null ? t : c;
        }

        public static CharacterCTRL GetNearestEnemy(CharacterCTRL c)
        {
            CharacterParent characterParent = c.IsAlly ? ResourcePool.Instance.enemy : ResourcePool.Instance.ally;
            CharacterCTRL t = null;
            float dist = int.MaxValue;
            foreach (var item in characterParent.childCharacters)
            {
                float d = Vector3.Distance(c.transform.position, item.transform.position);
                if (d < dist)
                {
                    CharacterCTRL cTRL = item.GetComponent<CharacterCTRL>();
                    if (cTRL.isAlive && !cTRL.characterStats.logistics && cTRL.enterBattle && cTRL.gameObject.activeInHierarchy)
                    {
                        dist = d;
                        t = cTRL;
                    }
                }

            }
            return t;
        }
        /// <summary>
        /// 建立一個 RewardContext，其內容包含：
        /// 1) 隨機組件(指定數量) -> componentCount
        /// 2) 多件裝備(一次打包成一個選項) -> multiEquipCount (新增) 
        /// 3) 單一裝備(指定數量，每件單獨一個選項) -> equipCount
        /// 4) 金幣(指定金額) -> gold
        /// </summary>
        public static RewardContext BuildMixedRewards(
            int componentCount,
            int multiEquipCount,
            int equipCount,
            int gold
        )
        {
            RewardContext context = new RewardContext();
            if (componentCount > 0)
            {
                (List<IReward> r, List<IEquipment> eqList)
                    = GetMultipleRandomEquipRewards(componentCount, 0, 6);
                IReward randomComponents = new CompositeReward(r);
                References.DescriptionIndex++;
                RewardEntry compEntry = new RewardEntry(
                    new List<IReward>() { randomComponents },
                    ResourcePool.Instance.RandomRewardSprite,
                    $"隨機組件*{componentCount} ",
                    $"{componentCount} random components",
                    $"隨機組件*{componentCount} ",
                    $"{componentCount} random component",
                    References.DescriptionIndex
                );
                context.AddRewardEntry(compEntry);
            }

            if (multiEquipCount > 0)
            {
                (List<IReward> multiRewards, List<IEquipment> eqList)
                    = GetMultipleRandomEquipRewards(multiEquipCount, 6, 26);
                IReward multiEquipReward = new CompositeReward(multiRewards);

                References.DescriptionIndex++;
                RewardEntry multiEquipEntry = new RewardEntry(
                    new List<IReward>() { multiEquipReward },
                    ResourcePool.Instance.RandomRewardSprite,
                    $"隨機裝備 *{multiEquipCount}",
                    $"Random Equipment *{multiEquipCount}",
                    $"隨機裝備 *{multiEquipCount}",
                    $"Random Equipment *{multiEquipCount}",
                    References.DescriptionIndex
                );
                context.AddRewardEntry(multiEquipEntry);
            }
            if (equipCount > 0)
            {
                List<IEquipment> pool = EquipmentManager.Instance.availableEquipments
                    .Where(x => x.Id >= 6 && x.Id < 26)
                    .ToList();

                for (int i = 0; i < equipCount; i++)
                {
                    if (pool.Count == 0) break;
                    int index = UnityEngine.Random.Range(0, pool.Count);
                    IEquipment eq = pool[index];
                    pool.RemoveAt(index);

                    IReward singleRandom = new EquipmentReward(eq);
                    References.DescriptionIndex++;
                    RewardEntry entry = new RewardEntry(
                        new List<IReward>() { singleRandom },
                        eq.Icon,
                        eq.EquipmentName,
                        eq.EquipmentName,
                        eq.EquipmentDetail,
                        eq.EquipmentDescriptionEnglish,
                        References.DescriptionIndex
                    );
                    context.AddRewardEntry(entry);
                }
            }
            if (gold > 0)
            {
                IReward goldReward = new CurrencyReward(gold);
                References.DescriptionIndex++;
                RewardEntry goldEntry = new RewardEntry(
                    new List<IReward>() { goldReward },
                    ResourcePool.Instance.GoldSprite,
                    $"招募點數 *{gold}",
                    $"recurite point *{gold}",
                    $"招募點數 *{gold}",
                    $"recurite point *{gold}",
                    References.DescriptionIndex
                );
                context.AddRewardEntry(goldEntry);
            }

            return context;
        }

        public static (List<IReward>, List<IEquipment>) GetMultipleRandomEquipRewards(int count, int minId, int maxId)
        {
            List<IReward> result = new List<IReward>();
            List<IEquipment> eqs = new List<IEquipment>();
            List<IEquipment> pool = EquipmentManager.Instance.availableEquipments
                .Where(x => x.Id >= minId && x.Id < maxId)
                .ToList();

            for (int i = 0; i < count && pool.Count > 0; i++)
            {
                int index = UnityEngine.Random.Range(0, pool.Count);
                IEquipment eq = pool[index];
                pool.RemoveAt(index);
                result.Add(new EquipmentReward(eq));
                eqs.Add(eq);
                CustomLogger.Log(eq, $"抽到裝備: {eq.EquipmentName}");
            }
            return (result, eqs);
        }
    }

    public class Effect
    {
        public EffectType EffectType { get; private set; }
        public string Source { get; private set; }
        public float Duration { get; set; }
        public ModifierType ModifierType { get; private set; }
        public float Value { get; private set; }
        public bool IsPermanent { get; private set; }
        public SpecialEffectType SpecialType { get; private set; }
        public CharacterCTRL Parent { get; private set; }
        public Action<CharacterCTRL> OnApply { get; private set; } // 當效果被應用時
        public Action<CharacterCTRL> OnRemove { get; private set; } // 當效果被移除時
        public ClearEffectCondition ClearEffectCondition { get; private set; }
        public bool Stackable { get; private set; }
        public bool IsLogisticBuff { get; private set; }
        public bool StatsEffect { get; private set; } = false;
        public Effect(
            EffectType effectType,
            ModifierType modifierType,
            float value,
            string source,
            bool isPermanent,
            Action<CharacterCTRL> onApply,
            Action<CharacterCTRL> onRemove,
            float duration = 0f,
            SpecialEffectType specialType = SpecialEffectType.None,
            CharacterCTRL parent = null,
            bool stackable = false,
            ClearEffectCondition clearEffectCondition = ClearEffectCondition.Never,
            bool isLogisticBuff = false,
            bool statsEffect = false
        )
        {
            EffectType = effectType;
            ModifierType = modifierType;
            Value = value;
            Source = source;
            IsPermanent = isPermanent;
            Duration = duration;
            SpecialType = specialType;
            OnApply = onApply;
            OnRemove = onRemove;
            Parent = parent;
            Stackable = stackable;
            ClearEffectCondition = clearEffectCondition;
            IsLogisticBuff = isLogisticBuff;
            StatsEffect = statsEffect;
        }
        public void UpdateValue(float newValue,CharacterCTRL c)
        {
            Parent = c;
            Value = newValue;
            c.RecalculateStats();
        }
        public void AddValue(float valAdded)
        {
            Value += valAdded;
            OnApply.Invoke(Parent);
            CustomLogger.Log(this, $"source {Parent} added {valAdded} , value = {Value}");
        }
        public void SetActions(Action<CharacterCTRL> onApply, Action<CharacterCTRL> onRemove)
        {
            OnApply = onApply;
            OnRemove = onRemove;
        }

    }
}
public class Shield
{
    public int amount; // 護盾數值
    public float remainingTime; // 剩餘持續時間

    public Shield(int amount, float remainingTime)
    {
        this.amount = amount;
        this.remainingTime = remainingTime;
    }
}
public class CustomLogger
{
    public static void Log(object caller, string message)
    {
        string callerType = caller.GetType().Name;
        Debug.Log($"[{callerType}] {message}");
    }
    public static void LogError(object caller, string message)
    {
        string callerType = caller.GetType().Name;
        Debug.LogError($"[{callerType}] {message}");
    }
    public static void LogWarning(object caller, string message)
    {
        string callerType = caller.GetType().Name;
        Debug.LogWarning($"[{callerType}] {message}");
    }
    public static void LogWhenThingShouldntHappened(object caller)
    {
        string callerType = caller.GetType().Name;
        Debug.LogError($"[{callerType}] THIS TEXT SHOULDNT BE PRINTED!");
    }
    public static void LogTODO(object caller, string message)
    {
        string callerType = caller.GetType().Name;
        Debug.LogWarning($"[{callerType}] TODO: 尚未完成的功能，未來需實作{message}");
    }
}
[System.Serializable]
public class EnemyWaveData
{
    [System.Serializable]
    public class GridSlotData
    {
        public int GridIndex;
        public int CharacterID;
        public int Level;
        public int[] EquipmentIDs;
        public int DummyGridIndex;
    }
    public string EnemyName;
    public List<GridSlotData> gridSlots = new List<GridSlotData>();
    public GridSlotData logisticSlot1 = new();
    public GridSlotData logisticSlot2 = new();
    public int Pressurestack;
}
public static class EnemyWaveConverter
{
    // 把 ScriptableObject(EnemyWave) 轉成普通資料結構(EnemyWaveData)
    public static EnemyWaveData ToData(EnemyWave wave)
    {
        var data = new EnemyWaveData();
        data.EnemyName = wave.EnemyName;
        data.gridSlots = new List<EnemyWaveData.GridSlotData>();

        // 轉換各欄位
        foreach (var slot in wave.gridSlots)
        {
            data.gridSlots.Add(new EnemyWaveData.GridSlotData
            {
                GridIndex = slot.GridIndex,
                CharacterID = slot.CharacterID,
                Level = slot.Level,
                EquipmentIDs = slot.EquipmentIDs,
                DummyGridIndex = slot.DummyGridIndex
            });
        }

        // logisticSlot1
        data.logisticSlot1 = new EnemyWaveData.GridSlotData
        {
            GridIndex = wave.logisticSlot1.GridIndex,
            CharacterID = wave.logisticSlot1.CharacterID,
            Level = wave.logisticSlot1.Level,
            EquipmentIDs = wave.logisticSlot1.EquipmentIDs,
            DummyGridIndex = wave.logisticSlot1.DummyGridIndex
        };

        // logisticSlot2
        data.logisticSlot2 = new EnemyWaveData.GridSlotData
        {
            GridIndex = wave.logisticSlot2.GridIndex,
            CharacterID = wave.logisticSlot2.CharacterID,
            Level = wave.logisticSlot2.Level,
            EquipmentIDs = wave.logisticSlot2.EquipmentIDs,
            DummyGridIndex = wave.logisticSlot2.DummyGridIndex
        };

        data.Pressurestack = wave.Pressurestack;

        return data;
    }
    public static EnemyWave FromData(EnemyWaveData data)
    {
        var wave = new EnemyWave();
        wave.EnemyName = data.EnemyName;
        wave.gridSlots.Clear();

        foreach (var slotData in data.gridSlots)
        {
            var slot = new EnemyWave.GridSlotData
            {
                GridIndex = slotData.GridIndex,
                CharacterID = slotData.CharacterID,
                Level = slotData.Level,
                EquipmentIDs = slotData.EquipmentIDs,
                DummyGridIndex = slotData.DummyGridIndex
            };
            wave.gridSlots.Add(slot);
        }

        wave.logisticSlot1.GridIndex = data.logisticSlot1.GridIndex;
        wave.logisticSlot1.CharacterID = data.logisticSlot1.CharacterID;
        wave.logisticSlot1.Level = data.logisticSlot1.Level;
        wave.logisticSlot1.EquipmentIDs = data.logisticSlot1.EquipmentIDs;
        wave.logisticSlot1.DummyGridIndex = data.logisticSlot1.DummyGridIndex;

        wave.logisticSlot2.GridIndex = data.logisticSlot2.GridIndex;
        wave.logisticSlot2.CharacterID = data.logisticSlot2.CharacterID;
        wave.logisticSlot2.Level = data.logisticSlot2.Level;
        wave.logisticSlot2.EquipmentIDs = data.logisticSlot2.EquipmentIDs;
        wave.logisticSlot2.DummyGridIndex = data.logisticSlot2.DummyGridIndex;

        wave.Pressurestack = data.Pressurestack;
        return wave;
    }
}
public class WaveGridSlotData
{
    public int GridIndex;
    public int CharacterID;
    public int Star;
    public List<int> EquipmentID = new List<int>();
    public int DummyGridIndex;
}