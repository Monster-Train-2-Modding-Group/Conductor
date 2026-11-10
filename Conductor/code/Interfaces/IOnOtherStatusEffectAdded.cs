using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Interfaces
{
    /// <summary>
    /// Interface for StatusEffectState to allow changing the number of stacks added.
    /// </summary>
    [Obsolete("Use IStatusEffectModifyOtherStatusEffect instead. This interface will be removed on 11/24/2026.")]
    public interface IOnOtherStatusEffectAdded
    {
        /// <summary>
        /// Called after the amount of status given is modified by dualism, card traits, and room modifiers.
        /// </summary>
        /// <param name="myStacks">Number of stacks of the status effect class</param>
        /// <param name="statusId">Status effect id being added</param>
        /// <param name="numStacks">Number of stacks of status id being added</param>
        /// <returns>The modified amount of status effect to add</returns>
        int OnOtherStatusEffectBeingAdded(int myStacks, string statusId, int numStacks);
    }

    /// <summary>
    /// Interface for StatusEffectState to allow changing the number of stacks added.
    /// </summary>
    public interface IStatusEffectModifyOtherStatusEffect
    {
        /// <summary>
        /// Called after the amount of status given is modified by dualism, card traits, and room modifiers.
        /// </summary>
        /// <param name="myStacks">Number of stacks of the status effect class</param>
        /// <param name="statusId">Status effect id being added</param>
        /// <param name="numStacks">Number of stacks of status id being added</param>
        /// <returns>The modified amount of status effect to add</returns>
        int OnOtherStatusEffectBeingAdded(int myStacks, string statusId, int numStacks);
    }
}
