using GameEnum;
using UnityEngine;

public class Shiroko_Terror_DroneCTRL : MonoBehaviour
{
    public Animator Animator;
    private string attacking = "Attacking";
    private CharacterCTRL Parent;
    public int stack;
    private int enhanced_stack = 0;
    public void Update()
    {
        if (Parent != null && Parent.Target != null)
        {
            transform.LookAt(Parent.Target.transform);
        }
    }
    public void AssistAttack(CharacterCTRL target,CharacterCTRL parent)
    {
        Parent = parent;
        Animator.SetBool(attacking, true);

        Animator.speed = parent.GetStat(StatsType.AttackSpeed);
        Vector3 v = target.transform.position;
        transform.LookAt(v);
        int dmg = (int)parent.GetStat(StatsType.Attack)*stack;
        (bool, int) tuple = parent.CalculateCrit(dmg);
        if (GameController.Instance.CheckCharacterEnhance(22, parent.IsAlly))
        {
            enhanced_stack++;
            if (enhanced_stack >= 3)
            {
                enhanced_stack -= 3;
                Utility.DealDamageInRange(target.CurrentHex, 2, parent, tuple.Item2, "Enhanced_Shiroko_Drone", tuple.Item1);
            }
        }
        target.GetHit(tuple.Item2,parent,"Shiroko_Terror_Drone",tuple.Item1);
        CustomLogger.Log(this,$"Dealing {dmg}");
    }
    public void EndAttack()
    {
        Animator.SetBool(attacking, false);
        CustomLogger.Log(this, $" end attack");
    }
}
