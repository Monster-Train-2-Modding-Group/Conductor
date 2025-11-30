using static TargetHelper;

namespace Conductor.TargetModes
{
    /// <summary>
    /// Target mode that sets the chosen target to override_target_character.
    /// 
    /// Only useful for card effects within a Custom CharacterTrigger which can set the field.
    /// </summary>
    public class OverrideTargetCharacter : CharacterTargetSelector
    {
        public override CardTargetMode CardTargetMode { get; } = CardTargetMode.Targetless;
        public override bool ResolvesToSingleTarget => true;

        public override bool CollectTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, List<CharacterState> targetsOut)
        {
            var overrideTarget = data.overrideTargetCharacter;
            if (overrideTarget != null)
            {
                targetsOut.Add(overrideTarget);
            }
            return true;
        }
    }
}
