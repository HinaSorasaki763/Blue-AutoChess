using GameEnum;
using System.Collections.Generic;
using UnityEngine;
public class HexNode : MonoBehaviour
{
    public Vector3 Position;
    public CharacterCTRL OccupyingCharacter;
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
    public bool isAllyHex;

    // Burning effect fields
    public bool isBurning = false;
    public bool AllyBlockingZonecenter { get; set; }
    public bool EnemyBlockingZonecenter { get; set; }
    public bool TargetedAllyZone { get; set; }
    public bool TargetedEnemyzone { get; set; }
    public ColorState currentColorState = ColorState.Default;
    public float temporaryColorDuration = 0f;
    private Color temporaryColor = Color.yellow;

    private bool isTemporarilyReserved = false;
    private List<BurningEffect> burningEffects = new List<BurningEffect>();
    public bool IsHexReserved()
    {
        return reservedBy != null;
    }
    public bool IsReservedBy(CharacterCTRL character)
    {
        return reservedBy == character;
    }
    public void HardReserve(CharacterCTRL character)
    {
        CustomLogger.Log(this, $"character {character}HardReserve");
        reservedBy = character;
        OccupyingCharacter = character;
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
    public void HardResetAll()
    {
        HardRelease();
        isTemporarilyReserved = false;
        isBurning = false;
        TargetedEnemyzone = false;
        TargetedAllyZone = false;
        AllyBlockingZonecenter = false;
        EnemyBlockingZonecenter = false;
    }
    public void TemporarilyReserve()
    {
        isTemporarilyReserved = true;
    }

    public void ClearTemporaryReservation()
    {
        isTemporarilyReserved = false;
    }

    public bool IsTemporarilyReserved()
    {
        return isTemporarilyReserved;
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

    public void UpdateTileColor()
    {
        if (!IsBattlefield) return;

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
            if (isDesertified)
            {
                tileMaterial.color = Color.yellow;
            }
            renderer.material = tileMaterial; // Ensure the material is applied
        }
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
        if (character != null)
        {
            OccupyingCharacter = character;
        }
        else
        {
            OccupyingCharacter = null;
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
