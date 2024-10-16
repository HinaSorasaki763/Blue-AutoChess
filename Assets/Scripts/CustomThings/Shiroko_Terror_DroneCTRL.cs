using UnityEngine;

public class Shiroko_Terror_DroneCTRL : MonoBehaviour
{
    public int Dmg = 10;
    public Animator Animator;
    private string attacking = "Attacking";
    private CharacterCTRL Parent;
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
        Vector3 v = target.transform.position;
        transform.LookAt(v);
        target.GetHit(Dmg,parent);
        CustomLogger.Log(this,$"Dealing {Dmg}");
    }
    public void EndAttack()
    {
        Animator.SetBool(attacking, false);
        CustomLogger.Log(this, $" end attack");
    }
}
