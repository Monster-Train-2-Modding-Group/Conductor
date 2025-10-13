﻿using Conductor.Interfaces;
using static TargetHelper;

namespace Conductor.TargetModes
{
    /// <summary>
    /// Target mode that targets the unit behind self. Only meaningful when played from an effect in a CharacterTrigger.
    /// </summary>
    class BehindSelf : CharacterTargetSelector
    {
        public override CardTargetMode CardTargetMode { get; } = CardTargetMode.Targetless;

        public override void FilterTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, IReadOnlyList<CharacterState> allValidTargets, List<CharacterState> chosenTargets)
        {
            CharacterState? self = data.selfTarget;

            if (self == null)
            {
                Plugin.Logger.LogWarning($"{GetType().Name} is not used for an effect within a CharacterTrigger. There is no selfTarget for this TargetMode to work.");
                return;
            }

            int index = allValidTargets.IndexOf(self);

            if (self.IsInBackOfRoom() || index == -1 || index == allValidTargets.Count - 1)
                return;

            chosenTargets.Add(allValidTargets[index + 1]);
        }
    }
}
