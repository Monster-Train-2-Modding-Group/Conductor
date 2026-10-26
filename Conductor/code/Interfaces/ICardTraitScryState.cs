using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Interfaces
{
    public interface ICardTraitScryState
    {
        /// <summary>
        /// When present on played card gets extra scry
        /// </summary>
        /// <param name="currentScry">Current Scry amount from card effect.</param>
        /// <param name="cardStatistics">CardStatistics object.</param>
        /// <returns>The new Scry Amount</returns>
        public int GetScryAmount(int currentScry, CardStatistics cardStatistics);

        /// <summary>
        /// When present on a scried card expands the scry pool.
        /// </summary>
        /// <returns>Additional scry.</returns>
        public int GetScryAmountOnScried();

        /// <summary>
        /// Called when present on a scried card (after a card is obtained) 
        /// </summary>
        /// <param name="coreGameManagers">CoreGameManagers</param>
        public IEnumerator OnScry(ICoreGameManagers coreGameManagers);
    }
}
