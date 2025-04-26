using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HiyoriEnhancedSkill_EffectCounter : MonoBehaviour
{
    private CharacterCTRL lastTarget;
    public float ratio = 1.0f;
    public void Start()
    {
        lastTarget = null;
    }
    public void SetLastTarget( CharacterCTRL characterCTRL)
    {

        if (characterCTRL == lastTarget)
        {
            ratio *= 1.2f;
            CustomLogger.Log(this, $"SetLastTarget() {characterCTRL.name},ratio = {ratio}");
            return;
        }
        lastTarget = characterCTRL;
        ratio = 1.0f;
        CustomLogger.Log(this, $"SetLastTarget() {characterCTRL.name},ratio = {ratio}");
    }
}
