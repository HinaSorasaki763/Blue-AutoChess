// AbydosObserver.cs
using UnityEngine;
using GameEnum;
using System.Collections.Generic;
public class AbydosObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    private bool isOnDesertTile;
    public AbydosObserver(int level, CharacterCTRL character)
    {
        this.traitLevel = level;
        this.character = character;
        UpdateDesertifiedTiles();
    }

    private void UpdateDesertifiedTiles()
    {
        CustomLogger.Log(this, $"traitLevel = {traitLevel}");
        AbydosManager.Instance.level = traitLevel;
        SpawnGrid.Instance.UpdateDesertifiedTiles(ResourcePool.Instance.RandomKeyThisGame, AbydosManager.Instance.GetTraitObserverLevel()[traitLevel].Data1);
    }
}
