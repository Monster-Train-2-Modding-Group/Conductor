using Conductor.Enums;
using Conductor.Interfaces;
using ShinyShoe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.TextCore.Text;

namespace Conductor.StatusEffects
{
    public class StatusEffectPierceState : StatusEffectState, IChangeAttackingTargetModeStatusEffect
    {
        public int Precedence => 1;
        public int Priority => 0;
        public TargetMode TargetMode => Enums.TargetModes.NUnits;
    }
}
    }
}
