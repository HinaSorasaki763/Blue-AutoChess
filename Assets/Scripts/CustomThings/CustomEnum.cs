using System;
using System.Collections.Generic;
using UnityEngine;

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
        Mystic
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
        Shield
    }
    public enum ColorState
    {
        Default,
        Burning,
        Reserved,
        TemporaryYellow // 新增一個暫時的顏色狀態
    }
    public enum EffectType
    {
        Positive,
        Negative
    }
    public enum SpecialEffectType
    {
        None,
        Stun,
        Marked,
        CCImmune,
        UnTargetable,
        Invincible
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

        public void SetStat(StatsType type, int value)
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
        public int GetStat(StatsType type)
        {
            Stat stat = stats.Find(s => s.statType == type);
            if (stat != null)
            {
                return stat.value;
            }
            stats.Add(new Stat { statType = type, value = 0 });
            return 0;
        }
    }
    [Serializable]
    public class Stat
    {
        public StatsType statType;
        public int value;
        public Stat Clone()
        {
            return new Stat
            {
                statType = this.statType,
                value = this.value
            };
        }
    }
    public enum SkillPrefab
    {
        PenetrateTrailedBullet,
        HealPack,
        NormalTrailedBullet
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
            Value = value;
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
    }
    public static class Utility
    {
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
        public static bool Iscrit(int critChance)
        {
            int rand = UnityEngine.Random.Range(0, 101);
            if (rand >critChance)
            {
                return true;
            }
            return false;
        }
    }
    public class Effect
    {
        public EffectType EffectType { get; private set; }
        public string Source { get; private set; }
        public float Duration { get; set; }
        public ModifierType ModifierType { get; private set; }
        public int Value { get; private set; }
        public bool IsPermanent { get; private set; }
        public SpecialEffectType SpecialType { get; private set; }
        public Action<CharacterCTRL> OnApply { get; private set; } // 當效果被應用時
        public Action<CharacterCTRL> OnRemove { get; private set; } // 當效果被移除時

        public Effect(
            EffectType effectType,
            ModifierType modifierType,
            int value,
            string source,
            bool isPermanent,
            Action<CharacterCTRL> onApply,
            Action<CharacterCTRL> onRemove,
            float duration = 0f,
            SpecialEffectType specialType = SpecialEffectType.None

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
}