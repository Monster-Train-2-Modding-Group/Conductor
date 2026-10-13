using System.Collections;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Like CardEffectAdjustRoomCapacity except it can target a specific floor
    /// 
    /// Parameters:
    ///     param_int - Amount to adjust capacity by.
    ///     param_int_2 - Which room index to adjust capacity. (0 = bottom floor, 1 = middle floor, 2 = top floor).
    ///     param_bool - Require no enemy units on floor.
    /// </summary>
    public class CardEffectAdjustSelectRoomCapacity : CardEffectBase
    {
        public override bool CanPlayAfterBossDead => false;
        public override bool CanApplyInPreviewMode => false;
        public override bool CanPlayInEngineRoom => false;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Capacity Amount"),
                [CardEffectFieldNames.AdditionalParamInt.GetFieldName()] = new PropDescription("Room Index to Increase Capacity for."),
                [CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("Only Trigger If No Enemies", "If enabled, this effect will only occur if there are no hero units in the room.")
            };
        }

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            RoomState room = coreGameManagers.GetRoomManager().GetRoom(cardEffectState.GetAdditionalParamInt());
            if (room == null || room.GetIsPyreRoom())
            {
                return false;
            }
            if (cardEffectState.GetParamBool() && room.HasCharacters(Team.Type.Heroes))
            {
                return false;
            }
            return true;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            int selectedRoom = cardEffectState.GetAdditionalParamInt();
            RoomManager roomManager = coreGameManagers.GetRoomManager();
            RoomUI roomUI = roomManager.GetRoomUI();
            RoomState room = roomManager.GetRoom(selectedRoom);
            Team.Type team = cardEffectState.GetTargetTeamType();
            int adjustAmount = cardEffectState.GetParamInt();
            bool onlyTriggerIfNoEnemies = cardEffectState.GetParamBool();
            CardState? parentCardState = cardEffectState.GetParentCardState();
            if (parentCardState != null)
            {
                foreach (CardTraitState traitState in parentCardState.GetTraitStates())
                {
                    if (traitState is CardTraitScalingAdjustCapacity cardTraitScalingAdjustCapacity)
                    {
                        adjustAmount += cardTraitScalingAdjustCapacity.GetAdditionalCapacityAdjustment(coreGameManagers.GetCardStatistics(), forPreviewText: false);
                    }
                }
            }
            roomUI.DisableScrolling();
            yield return roomUI.SetSelectedRoom(selectedRoom);
            SpawnPointGroup.CapacityAdjustmentError error;
            if (onlyTriggerIfNoEnemies && room.HasCharacters(Team.Type.Heroes))
            {
                roomUI.EnableScrolling();
                yield break;
            }
            else if (!room.CanAdjustCapacity(team, ref adjustAmount, out error))
            {
                PopupNotificationManager? popupNotificationManager = sysManagers.GetPopupNotificationManager();
                if (popupNotificationManager != null)
                {
                    popupNotificationManager.ShowNotification(new PopupNotificationUI.NotificationData
                    {
                        text = $"CardEffectAdjustRoomCapacity_{error}".Localize(),
                        colorType = ColorDisplayData.ColorType.Negative,
                        source = PopupNotificationUI.Source.Room
                    }, 0f, room);
                }
            }
            else
            {
                yield return room.AdjustCapacity(cardEffectState.GetTargetTeamType(), adjustAmount);
                yield return CoreUtil.WaitForSecondsOrBreak(coreGameManagers.GetSaveManager().GetActiveTiming().CharacterTriggerPostFire, "C:\\jenkins\\workspace\\Shiny_Shoe_titan_production\\TitanClient\\Assets\\Scripts\\Cards\\CardEffects\\CardEffectAdjustRoomCapacity.cs", 89);
            }

            roomUI.EnableScrolling();
        }
    }
}
