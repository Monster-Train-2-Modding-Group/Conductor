using Conductor.Interfaces;

namespace Conductor.StatusEffects
{
    class StatusEffectHexState : StatusEffectState, IOnOtherStatusEffectAdded
    {
        public int OnOtherStatusEffectBeingAdded(int myStacks, string statusId, int numStacks)
        {
            if (statusId == GetStatusId())
            {
                return numStacks;
            }

            var statusEffectData = StatusEffectManager.Instance.GetStatusEffectDataById(statusId);
            if (statusEffectData == null)
                return numStacks;

            if (statusEffectData.IsPropagatable() && statusEffectData.GetDisplayCategory() == StatusEffectData.DisplayCategory.Negative)
            {
                return numStacks + myStacks;
            }

            return numStacks;
        }
    }
}
