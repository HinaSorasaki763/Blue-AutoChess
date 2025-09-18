using GameEnum;
using System.Collections.Generic;
using UnityEngine;

public class KarinCostumSkill : MonoBehaviour
{
    private CharacterCTRL character;
    void Start()
    {
        character = GetComponent<CharacterCTRL>();
    }

    // Update is called once per frame
    void Update()
    {

    }
    public Dictionary<int, StarLevelStats> GetCharacterLevel()
    {
        Dictionary<int, StarLevelStats> statsByStarLevel = new Dictionary<int, StarLevelStats>()
        {
            {1, new StarLevelStats(5,8,1,0,0.25f)},
            {2, new StarLevelStats(60,8,1,0,0.25f)},
            {3, new StarLevelStats(999,219,10,0,0.25f)}
        };
        return statsByStarLevel;
    }
    public int GetAttackCoefficient(SkillContext skillContext)
    {
        StarLevelStats stats = GetCharacterLevel()[skillContext.CharacterLevel];
        int BaseDmg = stats.Data1;
        int DmgRatio = stats.Data2;
        return BaseDmg + (int)(DmgRatio * 0.01f * skillContext.Parent.GetAttack());
    }
    public void SkillShot1()
    {
        SkillContext skillContext = character.GetSkillContext();
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        int dmg = (int)(GetAttackCoefficient(skillContext) * 0.9f);
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        CharacterCTRL target = Utility.GetNearestEnemy(character);
        character.transform.LookAt(target.transform.position);
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, target.gameObject, true, iscrit);
    }
    public void SkillShot2()
    {
        SkillContext skillContext = character.GetSkillContext();
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        int dmg = (int)(GetAttackCoefficient(skillContext) * 1.1f);
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        CharacterCTRL target = Utility.GetNearestEnemy(character);
        character.transform.LookAt(target.transform.position);
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, target.gameObject, true, iscrit);
    }
    public void SkillShot3()
    {
        SkillContext skillContext = character.GetSkillContext();
        GameObject bullet = ResourcePool.Instance.SpawnObject(SkillPrefab.NormalTrailedBullet, skillContext.Parent.FirePoint.position, Quaternion.identity);
        int dmg = (int)(GetAttackCoefficient(skillContext) * 1.5f);
        (bool iscrit, int dmg1) = skillContext.Parent.CalculateCrit(dmg);
        CharacterCTRL target = Utility.GetNearestEnemy(character);
        character.transform.LookAt(target.transform.position);
        bullet.GetComponent<NormalBullet>().Initialize(dmg1, skillContext.Parent.GetTargetLayer(), skillContext.Parent, 15f, target.gameObject, true, iscrit);
    }
}
