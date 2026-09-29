using Conductor.Interfaces;

namespace Conductor.StatusEffects
{
    public class StatusEffectPierceState : StatusEffectState, IChangeAttackingTargetModeStatusEffect
    {
        public int Precedence => 1;
        public int Priority => 0;
        public TargetMode TargetMode => Enums.TargetModes.NUnits;
    }
}