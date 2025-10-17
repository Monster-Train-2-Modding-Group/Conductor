using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using static TargetHelper;

namespace Conductor.Interfaces
{
    /// <summary>
    /// Class implementing a custom unit target mode.
    /// 
    /// Note that class should only be used for implementing completely new, reusable target modes.
    /// 
    /// If a TargetMode is just a subset of a existing target mode (i.e. All specific subtype in room)
    /// It is best to not implement it as a target mode but by using Target Mode Room with ParamSubtype set.
    /// </summary>
    public abstract class CharacterTargetSelector : ITargetSelector
    {
        private static MethodInfo MethodApplyTargetFilters = AccessTools.Method(typeof(TargetHelper), "ApplyTargetFilters", 
            [typeof(List<CharacterState>),typeof(List<string>),typeof(CardEffectData.HealthFilter),typeof(bool),typeof(bool),typeof(bool),typeof(bool), typeof(SubtypeData),typeof(List<SubtypeData>),typeof(bool),typeof(bool?)]);
        private static readonly List<string> statusEffectFilterEmpty = new List<string>();
        private static readonly List<SubtypeData> excludedSubtypesListEmpty = new List<SubtypeData>();

        /// <summary>
        /// The associated CardTargetMode used for CardUpgradeMaskData filtering.
        /// </summary>
        public virtual CardTargetMode CardTargetMode { get; } = CardTargetMode.Other;

        /// <summary>
        /// Does the target mode target a room.
        /// </summary>
        public virtual bool TargetsRoom { get; } = false;
        /// <summary>
        /// Does the target mode target characters in multiple rooms.
        /// </summary>
        public virtual bool TargetsMultipleRooms { get; } = false;
         

        /// <see cref=">ITargetSelector"/>
        public virtual void PreCollectTargets(CardEffectState effectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, bool isTesting)
        {

        }

        /// <summary>
        /// Function should be implemented if the target mode doesn't need to filter targets from a room.
        /// You should select which characters on the train to target here. Note that you must also check
        /// if the characters can actually be targeted (accounting for health filters, subtype, status, etc.)
        /// this can be done by calling ApplyTargetFilters
        /// </summary>
        /// <param name="data">Collect Targets Data object</param>
        /// <param name="coreGameManagers">Core Game Managers object</param>
        /// <param name="targetsOut">Selected characters for the target mode.</param>
        /// <returns>True if done processing targets, FilterTargets will not be called.</returns>
        public virtual bool CollectTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, List<CharacterState> targetsOut)
        {
            return false;
        }

        /// <summary>
        /// Function should ONLY be implemented if the target mode targets characters in multiple rooms (that is the custom target mode has the property targets_multiple_rooms set to true)
        /// Return ALL characters in the rooms that you are targeting, not just the ones being targeted by the target mode.
        /// The default behavior selects all characters in the current room. Again if you are just targeting characters in a singular room you don't need to implement this as it won't get called.
        /// (See the function of same name in TargetHelper)
        /// </summary>
        /// <param name="card">Card current;y being played</param>
        /// <param name="roomManager">RoomManager instance</param>
        /// <param name="room">Current room being focused</param>
        /// <param name="previewTargets">Selected characters for the target mode preview. Should be all targets for team in the rooms being targeted. Default implemnention puts all characters in current room in this parameter</param>
        public virtual void CollectPreviewTargets(CardState? card, RoomManager roomManager, RoomState room, List<CharacterState> previewTargets)
        {
            room.AddCharactersToList(previewTargets, Team.Type.Heroes | Team.Type.Monsters);
        }

        /// <summary>
        /// Function should be implemented if the target mode chooses a subset of characters from within a room.
        /// CollectTargets must return false and then you can pick a target from allTargets.
        /// 
        /// Note that difference between this and CollectTargets is the that allValidTargets list is pre-filtered.
        /// </summary>
        /// <param name="data">Collect Targets Data object</param>
        /// <param name="coreGameManagers">Core Game Managers object</param>
        /// <param name="allValidTargets">All targets in room that passes filters (subtype, health, status, etc). Do not keep a reference to this list.</param>
        /// <param name="chosenTargets">Selected characters for the target mode</param>
        public virtual void FilterTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, IReadOnlyList<CharacterState> allValidTargets, List<CharacterState> chosenTargets)
        {

        }

        /// <summary>
        /// For use in CollectTargets to apply the target filtering parameters onto the potential list of targets.
        /// 
        /// Shortcut that simply calls the private method TargetHelper.ApplyTargetFilters.
        /// </summary>
        /// <param name="data">Collect Targets Data object</param>
        /// <param name="targets">List of potential targets</param>
        protected void ApplyTargetFilters(CollectTargetsData data, List<CharacterState> targets)
        {
            DoApplyTargetFilters(data, targets);
        }

        internal static void DoApplyTargetFilters(CollectTargetsData data, List<CharacterState> targets)
        {
            if (MethodApplyTargetFilters == null)
            {
                Plugin.Logger.LogError("-------------------------------------------------------------------------------------------");
                Plugin.Logger.LogError("Could not find an applicable ApplyTargetFilters Method, perhaps it was modified. PLEASE FIX");
                Plugin.Logger.LogError("-------------------------------------------------------------------------------------------");
                return;
            }
            MethodApplyTargetFilters?.Invoke(null, [targets, data.targetModeStatusEffectsFilter ?? statusEffectFilterEmpty, data.targetModeHealthFilter, data.targetIgnoreBosses, data.ignorePyre, data.inCombat, data.ignoreDead, data.targetSubtype, data.targetExcludedSubtypesFilter ?? excludedSubtypesListEmpty, data.includeUntouchable, data.mustHaveEquipment]);
        }
    }
}
