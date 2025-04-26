using GameEnum;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ShizukoActiveSkill : MonoBehaviour
{
    public GameObject TruckPrefab;
    public GameObject Reference;
    readonly float fallSpeed = 10.0f;
    private StaticObject ctrl;
    public CharacterCTRL Parent;
    public float counter = 0;
    const float updateCounter = 1;
    public void Start()
    {

    }
    public void OnEnable()
    {

    }
    public void Update()
    {
        if (GameController.Instance.CheckCharacterEnhance(10,Parent.IsAlly) && ctrl.isTargetable)
        {
            counter++;
            if (counter>=updateCounter)
            {
                counter = 0;
                foreach (var item in Utility.GetCharacterInrange(ctrl.CurrentHex,2,ctrl,true))
                {
                    int amount = (int)(ctrl.GetStat(StatsType.Health)*0.1f);
                    item.Heal(amount, ctrl);
                }
            }
        }

        if (Reference != null)
        {
            if (Reference.transform.position.y > 0.25f)
            {
                Reference.transform.position += fallSpeed * Time.deltaTime * Vector3.down;
            }
            else
            {
                Vector3 v = Reference.transform.position;
                Reference.transform.position = new Vector3(v.x, 0.25f, v.z);
            }
        }
    }
    public void ReinforceTruck()
    {
        ctrl = Reference.GetComponent<StaticObject>();
        int amount = Parent.ActiveSkill.GetAttackCoefficient(Parent.GetSkillContext());
        int healthDiff = (int)(ctrl.GetStat(StatsType.Health) - ctrl.GetStat(StatsType.currHealth));
        if (healthDiff > ctrl.BeforeHealing(amount, Parent))
        {
            ctrl.Heal(amount, Parent);
        }
        else
        {
            int diff = ctrl.BeforeHealing(amount, Parent) - healthDiff;
            Effect effect = EffectFactory.StatckableStatsEffct(0, "Shizuko EnhancedSkill", diff, StatsType.Health, ctrl, true);
            effect.SetActions(
                (character) => character.ModifyStats(StatsType.Health, effect.Value, effect.Source),
                (character) => character.ModifyStats(StatsType.Health, -effect.Value, effect.Source)
            );

            ctrl.effectCTRL.AddEffect(effect, ctrl);
            ctrl.HealToFull(Parent);
            ctrl.RecalculateStats();
        }

    }
    public void SpawnTruck(HexNode h, CharacterCTRL parent)
    {
        if (Reference != null)
        {
            Reference.GetComponent<StaticObject>().CurrentHex.HardRelease();
            Destroy(Reference);
            Reference = null;
        }
        Parent = parent;
        Reference = Instantiate(TruckPrefab, h.Position + new Vector3(0, 5, 0), Quaternion.Euler(-90, 0, 0));
        ctrl = Reference.GetComponent<StaticObject>();
        CharacterParent characterParent = parent.IsAlly ? ResourcePool.Instance.ally : ResourcePool.Instance.enemy;
        GameObject obj = ctrl.gameObject;
        obj.name = $"{parent.name}'s summon";
        CharacterBars bar = ResourcePool.Instance.GetBar(h.Position).GetComponent<CharacterBars>();
        ctrl.SetBarChild(bar);
        ctrl.characterBars = bar;
        bar.SetBarsParent(obj.transform);
        StaticObject staticObj = obj.GetComponent<StaticObject>();
        staticObj.InitStaticObjStats(parent,parent.ActiveSkill.GetAttackCoefficient(parent.GetSkillContext()));
        characterParent.childCharacters.Add(ctrl.gameObject);
        ctrl.IsAlly = Parent.IsAlly;
        ctrl.CurrentHex = h;
        h.Reserve(ctrl);
        h.OccupyingCharacter = ctrl;
    }
}
