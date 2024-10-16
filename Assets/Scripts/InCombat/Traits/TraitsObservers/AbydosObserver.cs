// AbydosObserver.cs
using UnityEngine;
using GameEnum;
public class AbydosObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    private bool isOnDesertTile;

    public AbydosObserver(int level, CharacterCTRL character)
    {
        this.traitLevel = level;
        this.character = character;
        character.OnGridChanged += OnTileChanged;
        CheckTileEffect(character.CurrentHex);
        UpdateDesertifiedTiles();
    }

    private void OnTileChanged(HexNode oldTile, HexNode newTile)
    {
        CheckTileEffect(newTile);
    }

    private void CheckTileEffect(HexNode grid)
    {
        bool wasOnDesertTile = isOnDesertTile;
        isOnDesertTile = grid != null && grid.isDesertified;

        if (isOnDesertTile && !wasOnDesertTile)
        {
            ApplyTileEffect();
        }
        else if (!isOnDesertTile && wasOnDesertTile)
        {
            RemoveTileEffect();
        }
    }

    private void ApplyTileEffect()
    {
        if (character.traitController.HasTrait(Traits.Abydos))
        {
            CustomLogger.Log(this, $"{character.name} 站在沙漠化格子上，战斗力增加。");
        }
        else
        {
            CustomLogger.Log(this, $"{character.name} （非阿拜多斯）站在沙漠化格子上，战斗力减少。");
        }
    }
    private void RemoveTileEffect()
    {
        if (character.traitController.HasTrait(Traits.Abydos))
        {
            CustomLogger.Log(this, $"[AbydosObserver] {character.name} 离开沙漠化格子，战斗力恢复。");
        }
        else
        {
            CustomLogger.Log(this, $"[AbydosObserver] {character.name} （非阿拜多斯）离开沙漠化格子，战斗力恢复。");
        }
    }
    private void UpdateDesertifiedTiles()
    {
        int gameStage = GameStageManager.Instance.CurrentStage;
        SpawnGrid.Instance.UpdateDesertifiedTiles(traitLevel, gameStage);
        CheckTileEffect(character.CurrentHex);
    }
    public override void OnDying(CharacterCTRL characterCTRL)
    {
        character.OnGridChanged -= OnTileChanged;
    }
}
