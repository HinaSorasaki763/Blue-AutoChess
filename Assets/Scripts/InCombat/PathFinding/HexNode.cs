using GameEnum;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.TextCore.Text;
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
    private float burningDuration = 0f;
    private int burningDamagePerTick = 0;
    private float burningTickInterval = 1f;
    private float burningTimer = 0f;
    private float burningTickTimer = 0f;
    private bool isBurningAppliedByAlly = false;
    public ColorState currentColorState = ColorState.Default;
    public float temporaryColorDuration = 0f;
    private Color temporaryColor = Color.yellow;

    private bool isTemporarilyReserved = false;
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
        reservedBy = character;
        OccupyingCharacter = character;
        SetColorState(ColorState.Reserved);
    }
    public void Reserve(CharacterCTRL character)
    {
        reservedBy = character;
        SetColorState(ColorState.Reserved);
    }

    public void Release()
    {
        if (OccupyingCharacter != null)
        {
            return;
        }
        reservedBy = null;
        SetColorState(isBurning ? ColorState.Burning : ColorState.Default);
    }


    public void HardRelease()
    {
        OccupyingCharacter = null;
        reservedBy = null;
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
    public void ApplyBurningEffect(float duration, int damagePerTick, float tickInterval, bool appliedByAlly)
    {
        isBurning = true;
        burningDuration = duration;
        burningDamagePerTick = damagePerTick;
        burningTickInterval = tickInterval;
        burningTimer = duration;
        burningTickTimer = tickInterval;
        isBurningAppliedByAlly = appliedByAlly;
        UpdateTileColor();
    }

    private void Update()
    {
        if (isBurning)
        {
            burningTimer -= Time.deltaTime;
            burningTickTimer -= Time.deltaTime;

            if (burningTickTimer <= 0f)
            {
                burningTickTimer = burningTickInterval;

                if (OccupyingCharacter != null)
                {
                    bool isEnemy = OccupyingCharacter.IsAlly != isBurningAppliedByAlly;

                    if (isEnemy)
                    {
                        OccupyingCharacter.GetHit(burningDamagePerTick, null);//TODO: to set not null
                    }
                }
            }

            if (burningTimer <= 0f)
            {
                isBurning = false;
                SetColorState(IsHexReserved() ? ColorState.Reserved : ColorState.Default);
            }
        }

        // 處理暫時顏色效果
        if (temporaryColorDuration > 0f)
        {
            temporaryColorDuration -= Time.deltaTime;

            // 當時間結束後，將狀態設置回正常狀態
            if (temporaryColorDuration <= 0f)
            {
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

            renderer.material = tileMaterial; // Ensure the material is applied
        }
    }

    public void SetColorState(ColorState state, float duration = 0f)
    {
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
}
