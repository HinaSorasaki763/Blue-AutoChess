using GameEnum;
using System.Collections.Generic;


public class AegisObserver : CharacterObserverBase
{
    private CharacterCTRL character;
    public AegisObserver(int level, CharacterCTRL character)
    {
        if (character == null) return;
        this.character = character;
    }
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {//秒數,阻擋次數,分攤到原主人的傷害
            {0, new TraitLevelStats(0,0,100)},
            {1, new TraitLevelStats(3,1,30)},
            {2, new TraitLevelStats(5,1,40)},
            {3, new TraitLevelStats(10,3,50)},
            {4, new TraitLevelStats(999,5,55)}
        };
        return statsByStarLevel;
    }
    public override void OnCastedSkill(CharacterCTRL character)
    {
        CustomLogger.Log(this,$"character at {character.CurrentHex.Position} casted skill");
        List<HexNode> inner = Utility.GetHexInRange(character.CurrentHex, 1);
        List<HexNode> outer = Utility.GetHexInRange(character.CurrentHex, 2);
        foreach (var item in inner)
        {
            foreach (var neighbor in item.Neighbors)
            {
                if (!inner.Contains(neighbor))
                {
                    SpawnGrid.Instance.CreateWallIfNotExist(item, neighbor, character.IsAlly,false);
                }
            }

        }

    }
}
