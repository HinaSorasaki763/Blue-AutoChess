using System.Collections.Generic;

public class DisruptorObserver : CharacterObserverBase
{
    private int traitLevel;
    private CharacterCTRL character;
    public override Dictionary<int, TraitLevelStats> GetTraitObserverLevel()
    {
        Dictionary<int, TraitLevelStats> statsByStarLevel = new Dictionary<int, TraitLevelStats>()
        {//增加秒數(*0.1),效果
            {1, new TraitLevelStats(0,0)},
            {1, new TraitLevelStats(5,10)},
            {2, new TraitLevelStats(10,20)},
            {3, new TraitLevelStats(15,30)}
        };
        return statsByStarLevel;
    }
    public override (float, float) AdjustNegetiveEffect(float length, float effective)
    {

        length += GetTraitObserverLevel()[traitLevel].Data1 * 0.1f;
        effective *= (1 + GetTraitObserverLevel()[traitLevel].Data2 * 0.01f);
        CustomLogger.Log(this, $"NegetiveEffect adjusted by {GetTraitObserverLevel()[traitLevel].Data1} ,{GetTraitObserverLevel()[traitLevel].Data2}  to ({length},{effective})");
        return (length, effective);
    }
    public DisruptorObserver(int level, CharacterCTRL character)
    {
        this.traitLevel = level;
        this.character = character;
    }

}
