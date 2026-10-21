using Conductor.Interfaces;
using HarmonyLib;
using JetBrains.Annotations;
using ShinyShoe.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CharacterState;

namespace Conductor.StatusEffects
{
    class StatusEffectHyperState : StatusEffectState, IStatusEffectModifyTriggerFireCount
    {
        public int GetTriggerFireCount(CharacterTriggerData.Trigger trigger, CharacterTriggerState? triggerStateContext)
        {
            return 1;
        }
    }
}
