using GameEnum;

public class RapidfireObserver : CharacterObserverBase
{
    private CharacterCTRL parent;
    private Effect RapidfireObserverEffect;
    public RapidfireObserver(int level, CharacterCTRL character)
    {
        this.parent = character;
        character.effectCTRL.characterCTRL = character;
        Effect effect = EffectFactory.StatckableIncreaseStatsEffct(0,"RapidfireObserver", 0, StatsType.AttackSpeed,parent,true);
        effect.SetActions(
            (character) => character.ModifyStats(StatsType.AttackSpeed, effect.Value, effect.Source),
            (character) => character.ModifyStats(StatsType.AttackSpeed, -effect.Value, effect.Source)
            );

        parent.effectCTRL.AddEffect(effect);
        RapidfireObserverEffect = effect;
    }
    public override void OnAttacking(CharacterCTRL character)
    {
        base.OnAttacking(character);
        CustomLogger.Log(this,$"character {character } override onAttacking , attakspeed = {character.GetStat(StatsType.AttackSpeed)}");
        RapidfireObserverEffect.AddValue(0.05f);
    }
}
