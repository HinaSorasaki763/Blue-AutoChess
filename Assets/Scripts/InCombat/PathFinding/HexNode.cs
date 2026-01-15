using GameEnum;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public class HexNode : MonoBehaviour
{
    public Vector3 Position;
    public bool EditMode = false;
    public CharacterCTRL OccupyingCharacter
    {
        get { return occupyingCharacter; }
        set
        {
            CustomLogger.Log(this, $"Setting OccupyingCharacter to {value}");
            occupyingCharacter = value;
        }
    }
    [SerializeField]
    private CharacterCTRL occupyingCharacter;
    public List<HexNode> Neighbors = new List<HexNode>();
    public float gCost = Mathf.Infinity;
    public float hCost = Mathf.Infinity;
    public float fCost { get { return gCost + hCost; } }
    public HexNode CameFrom;
    [SerializeField]
    private CharacterCTRL reservedBy;

    public int Index;
    // Cube coordinates
    public int X;
    public int Y;
    public int Z;
    public GameObject head;
    public GameObject nail;
    public bool IsBattlefield; // 格子是否位於戰場
    public bool IsLogistics;
    public bool isDesertified = false;
    public bool TempDesert = false;
    public bool TempDesert1;
    public bool KasumiEnhancedSkill_mark = false;
    public bool KasumiEnhancedSkill_steamed = false;
    public bool oasis = false;
    public bool isAllyHex;
    public bool Augment1006HexSelected = false;
    // Burning effect fields
    public bool isBurning = false;
    public ColorState currentColorState = ColorState.Default;
    public float temporaryColorDuration = 0f;
    private Color temporaryColor = Color.yellow;
    private List<BurningEffect> burningEffects = new List<BurningEffect>();
    private List<FloorEffect> floorEffects = new List<FloorEffect>();
    private float effectCountDown = 1f;
    public bool IsHexReserved()
    {
        if (OccupyingCharacter != null) return true;
        return reservedBy != null;
    }
    public bool Passable()
    {
        return OccupyingCharacter == null;
    }
    public bool IsReservedBy(CharacterCTRL character)
    {
        return reservedBy == character;
    }
    public bool IsDesertified()
    {
        return isDesertified || TempDesert || TempDesert1;
    }
    public void HardReserve(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"character {character}HardReserve");
        reservedBy = character;
        OccupyingCharacter = character;
        character.CurrentHex = this;
        SetColorState(ColorState.Reserved);
    }
    public void Reserve(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"character {character}reserving node {name}");
        reservedBy = character;
        SetColorState(ColorState.Reserved);
    }

    public void Release()
    {
        CustomLogger.Log(this, $"node {name} Release()");
        if (OccupyingCharacter != null)
        {
            return;
        }
        reservedBy = null;
        CameFrom = null;
        gCost = Mathf.Infinity;
        hCost = Mathf.Infinity;
        SetColorState(isBurning ? ColorState.Burning : ColorState.Default);
    }


    public void HardRelease()
    {
        CustomLogger.Log(this, $"node {name} HardRelease()");
        OccupyingCharacter = null;
        reservedBy = null;
        CameFrom = null;
        SetColorState(isBurning ? ColorState.Burning : ColorState.Default);
    }
    public bool IsBurnedBy(CharacterCTRL c)
    {
        foreach (BurningEffect effect in burningEffects)
        {
            if (effect.Source == c)
            {
                return true;
            }
        }
        return false;
    }
    public void HardResetAll()
    {
        HardRelease();
        isBurning = false;
    }
    public void ApplyBurningEffect(float duration, int damagePerTick, float tickInterval, CharacterCTRL source)
    {
        // 檢查是否已有相同來源的燃燒效果
        BurningEffect existingEffect = burningEffects.Find(effect => effect.Source == source);
        if (existingEffect != null)
        {
            // 疊加持續時間與傷害
            existingEffect.Duration += duration;
            existingEffect.Timer += duration;
            existingEffect.DamagePerTick += damagePerTick;
        }
        else
        {
            // 新增新的燃燒效果
            burningEffects.Add(new BurningEffect(source, duration, damagePerTick, tickInterval));
        }

        isBurning = true;
        UpdateTileColor();
    }

    private void Update()
    {
        if (!EditMode)
        {
            UpdateFloorEffect();
        }

        if (temporaryColorDuration > 0)
        {
            temporaryColorDuration -= Time.deltaTime;
        }
        else
        {
            temporaryColorDuration = 0;
            SetColorState(IsHexReserved() ? ColorState.Reserved : ColorState.Default);
        }
        if (burningEffects.Count > 0)
        {
            for (int i = burningEffects.Count - 1; i >= 0; i--)
            {
                BurningEffect effect = burningEffects[i];
                effect.Timer -= Time.deltaTime;
                effect.TickTimer -= Time.deltaTime;

                if (effect.TickTimer <= 0f)
                {
                    effect.TickTimer = effect.TickInterval;

                    if (OccupyingCharacter != null)
                    {
                        if (OccupyingCharacter.IsAlly != effect.Source.IsAlly)
                        {
                            (bool, int) tuple = effect.Source.CalculateCrit(effect.DamagePerTick);
                            OccupyingCharacter.GetHit(tuple.Item2, effect.Source, $"Burn from {effect.Source.name}", tuple.Item1);
                        }
                    }
                }

                // 如果此燃燒效果已結束，移除它
                if (effect.Timer <= 0f)
                {
                    burningEffects.RemoveAt(i);
                }
            }

            // 如果所有燃燒效果都結束，重設 isBurning 狀態
            if (burningEffects.Count == 0)
            {
                isBurning = false;
                SetColorState(IsHexReserved() ? ColorState.Reserved : ColorState.Default);
            }
        }
    }
    public void AddFloorEffect(FloorEffect floorEffect)
    {
        floorEffects.Add(floorEffect);
    }
    public void UpdateFloorEffect()
    {
        effectCountDown -= Time.deltaTime;
        List<FloorEffect> removeList = new List<FloorEffect>();
        foreach (var item in floorEffects)
        {
            if (OccupyingCharacter != null)
            {
                if (!item.Update(OccupyingCharacter, item.source))
                {
                    removeList.Add(item);
                }
            }
        }
        foreach (var item in removeList)
        {
            floorEffects.Remove(item);
        }
        if (effectCountDown <= 0)
        {
            foreach (FloorEffect effect in floorEffects)
            {
                effect.ApplyEffect(OccupyingCharacter, effect.source);
            }
            effectCountDown = 1f;
        }
    }

    public void UpdateTileColor()
    {
        if (!IsBattlefield || EditMode) return;
        Renderer renderer = head.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material tileMaterial = renderer.material;

            // 根據狀態優先級設置顏色
            switch (currentColorState)
            {
                case ColorState.Burning:
                    tileMaterial.color = Color.red; // Burning state
                    break;
                case ColorState.Reserved:
                    tileMaterial.color = Color.black; // Reserved state
                    break;
                case ColorState.TemporaryYellow:
                    tileMaterial.color = temporaryColor; // Temporary yellow
                    break;
                default:
                    tileMaterial.color = Color.white; // Default state
                    break;
            }
            if (currentColorState != ColorState.TemporaryYellow)
            {
                if (IsDesertified())
                {
                    tileMaterial.color = Color.red;
                }
                if (oasis)
                {
                    tileMaterial.color = Color.green;
                }
            }
            renderer.material = tileMaterial; // Ensure the material is applied
        }
    }
    public void CreateFloatingPiece(Color color, float duration = 0f)
    {
        GameObject go = ResourcePool.Instance.GetColoredHex(transform.position+ new Vector3(0, 0.3f, 0));
        go.transform.rotation = Quaternion.Euler(-90, 0, 0);
        Renderer renderer = go.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = color;

        if (duration > 0f)
            StartCoroutine(ReturnAfterDelay(go, duration));
    }

    private IEnumerator ReturnAfterDelay(GameObject go, float duration)
    {
        yield return new WaitForSeconds(duration);
        ResourcePool.Instance.DisableColoredHex(go);
    }


    public void SetColorState(ColorState state, float duration = 0f)
    {
        if (IsLogistics) return;
        currentColorState = state;

        // 如果是暫時顏色，記錄其持續時間
        if (state == ColorState.TemporaryYellow)
        {
            temporaryColorDuration = duration;
        }

        UpdateTileColor();
    }

    public void AddNeighbor(HexNode neighbor)
    {
        Neighbors.Add(neighbor);
    }
    public List<CharacterCTRL> GetCharacterOnNeighborHex(int radius, bool withItSelf)
    {
        HashSet<HexNode> visited = new HashSet<HexNode> { this };
        List<HexNode> currentLayer = new List<HexNode> { this };
        List<CharacterCTRL> characters = new List<CharacterCTRL>();
        for (int i = 0; i < radius; i++)
        {
            List<HexNode> nextLayer = new List<HexNode>();
            foreach (HexNode node in currentLayer)
            {
                foreach (HexNode neighbor in node.Neighbors)
                {
                    if (visited.Add(neighbor))
                    {
                        nextLayer.Add(neighbor);
                    }
                }
            }
            currentLayer = nextLayer;
        }
        if (withItSelf)
        {
            currentLayer.Add(this);
        }
        foreach (HexNode node in currentLayer)
        {
            if (node.OccupyingCharacter != null && !node.OccupyingCharacter.characterStats.logistics)
            {
                characters.Add(node.OccupyingCharacter);
            }
        }
        return characters;
    }

    public void SetOccupyingCharacter(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"set {name} occupying to {character}");
        if (character != null)
        {
            OccupyingCharacter = character;
            reservedBy = character;
        }
        else
        {
            OccupyingCharacter = null;
            reservedBy = null;
        }
    }
    public void Start()
    {
    }
    private class BurningEffect
    {
        public CharacterCTRL Source;
        public float Duration;
        public int DamagePerTick;
        public float TickInterval;
        public float Timer;
        public float TickTimer;

        public BurningEffect(CharacterCTRL source, float duration, int damagePerTick, float tickInterval)
        {
            Source = source;
            Duration = duration;
            DamagePerTick = damagePerTick;
            TickInterval = tickInterval;
            Timer = duration;
            TickTimer = tickInterval;
        }
    }
}
public abstract class FloorEffect
{
    private float duration;
    public CharacterCTRL source { get; private set; }
    public FloorEffect(float duration, CharacterCTRL character)
    {

        this.duration = duration;
        this.source = character;
    }
    public bool Update(CharacterCTRL target, CharacterCTRL source)
    {
        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            duration = 0;
            ApplyEffect(target, source);
            return false;
        }
        return true;
    }
    public abstract void ApplyEffect(CharacterCTRL target, CharacterCTRL source);
}
public class MichiruFloorBuff : FloorEffect
{
    public MichiruFloorBuff(float duration, CharacterCTRL character) : base(duration, character)
    {

    }
    public override void ApplyEffect(CharacterCTRL target, CharacterCTRL source)
    {
        if (target == null || source == null) return;
        if (target.IsAlly == source.IsAlly)
        {
            Effect effect = EffectFactory.StatckableStatsEffct(3, "MichiruAtkSpeedBuff", 0.3f, StatsType.AttackSpeed, source, false);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.AttackSpeed, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.AttackSpeed, -effect.Value, effect.Source)
            );
            target.effectCTRL.AddEffect(effect, target);
            StarLevelStats stats = source.ActiveSkill.GetCharacterLevel()[source.star];
            int amount = (int)(stats.Data1 + stats.Data4 * 0.01f * source.GetAttack());
            target.Heal(amount, source);
        }
    }
}