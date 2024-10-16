using GameEnum;
using System.Collections.Generic;
using System;
using UnityEngine;

public class TraitEffectApplier
{
    private Dictionary<Traits, Action<int, CharacterCTRL>> traitEffectMap;

    public TraitEffectApplier()
    {
        // ��l�ƬM�g�A�C�� Traits �����@�ӳB�z��k
        traitEffectMap = new Dictionary<Traits, Action<int, CharacterCTRL>>()
        {
            { Traits.Abydos, ApplyAbydosEffect },
            { Traits.Gehenna, ApplyGehennaEffect },
            { Traits.Hyakkiyako, ApplyHyakkiyakoEffect },
            { Traits.Millennium, ApplyMillenniumEffect },
            { Traits.Trinity, ApplyTrinityEffect },
        };
    }

    public void ApplyTraitEffects(Traits trait, int traitLevel, CharacterCTRL character)
    {
        // �p�G�r�夤�s�b�����̪��B�z��k�A�h�եΥ�
        if (traitEffectMap.TryGetValue(trait, out var applyEffect))
        {
            applyEffect(traitLevel, character);
        }
        else
        {
            Debug.Log($"���w�q {trait} �����̮ĪG");
        }
    }

    private void ApplyAbydosEffect(int traitLevel, CharacterCTRL character)
    {
        Debug.Log($"Abydos ���̵��� {traitLevel}�A���ίS�w�ĪG");
    }

    private void ApplyGehennaEffect(int traitLevel, CharacterCTRL character)
    {
        Debug.Log($"Gehenna ���̵��� {traitLevel}�A���ίS�w�ĪG");
    }
    private void ApplyHyakkiyakoEffect(int traitLevel, CharacterCTRL character)
    {
        Debug.Log($"Hyakkiyako ���̵��� {traitLevel}�A���ίS�w�ĪG");
    }
    private void ApplyMillenniumEffect(int traitLevel, CharacterCTRL character)
    {
        Debug.Log($"Millennium ���̵��� {traitLevel}�A���ίS�w�ĪG");
    }
    private void ApplyTrinityEffect(int traitLevel, CharacterCTRL character)
    {
        Debug.Log($"Trinity ���̵��� {traitLevel}�A���ίS�w�ĪG");
    }
}
