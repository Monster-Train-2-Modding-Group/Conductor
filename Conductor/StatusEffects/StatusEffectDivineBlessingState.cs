using Conductor.Interfaces;

namespace Conductor.StatusEffects
{
    class StatusEffectDivineBlessingState : StatusEffectState, IOnOtherStatusEffectAdded
    {
        public const string StatusId = "conductor_divine_blessing";

        public int OnOtherStatusEffectBeingAdded(int myStacks, string statusId, int numStacks)
        {
            if (statusId == StatusId)
            {
                return numStacks;
            }

            var statusEffectData = StatusEffectManager.Instance.GetStatusEffectDataById(statusId);
            if (statusEffectData == null)
                return numStacks;

            if (statusEffectData.IsPropagatable() && statusEffectData.GetDisplayCategory() == StatusEffectData.DisplayCategory.Positive)
            {
                return numStacks + myStacks;
            }

            return numStacks;
        }
    }
}
