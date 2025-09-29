using Conductor.Triggers;
using HarmonyLib;
using ShinyShoe.Logging;
using System.Collections;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.ApplyDamage))]
    class OnOtherMonsterHitPatch
    {
        private struct CharacterHpState
        {
            public int hp;
            public int armor;
            public int damageShields;
            public int reanimate;
        }

        private static CharacterHpState hpState = new();
        public static List<CharacterState> outCharacters = [];

        public static void Prefix(CharacterState __instance)
        {
            hpState.hp = __instance.GetHP();
            hpState.armor = __instance.GetStatusEffectStacks("armor");
            hpState.damageShields = __instance.GetStatusEffectStacks("damage shield");
            hpState.reanimate = __instance.GetStatusEffectStacks("undying");
        }

        public static IEnumerator Postfix(IEnumerator enumerator, CharacterState __instance, CombatManager ___combatManager)
        {
            while (enumerator.MoveNext())
                yield return enumerator.Current;

            if (__instance == null)
                yield break;

            if (__instance.GetTeamType() == Team.Type.Heroes)
                yield break;

            var lastAttackerCharacter = __instance.GetLastAttackerCharacter();
            if (lastAttackerCharacter == null || lastAttackerCharacter.IsDead || lastAttackerCharacter.IsDestroyed)
                yield break;

            // Test if the character was hit. If the HP or Armor changes then it should trigger OnHit
            if (__instance.GetHP() == hpState.hp && __instance.GetStatusEffectStacks("armor") == hpState.armor && __instance.GetStatusEffectStacks("damage shield") == hpState.armor && __instance.GetStatusEffectStacks("undying") == hpState.reanimate)
                yield break;

            outCharacters.Clear();
            RoomState? room = __instance.GetSpawnPoint(true)?.GetRoomOwner();
            if (room == null)
            {
                Log.Error(LogGroups.Gameplay, "Could not find room associated with character.", LogOptions.None, "OnOtherMonsterHitPatch CharacterState.ApplyDamagePostfix");
                yield break;
            }

            room.AddCharactersToList(outCharacters, Team.Type.Monsters);

            foreach (CharacterState character in outCharacters)
            {
                if (character == __instance)
                {
                    continue;
                }
                yield return ___combatManager.QueueAndRunTrigger(character, CharacterTriggers.Vengeance, fireTriggersData: new CharacterState.FireTriggersData
                {
                    // This hack is used to get the LastAttackerCharacter in CardEffects.
                    // Set TargetMode.LastAttackedCharacter, unintuitive, but that's how the code for overrideTargetCharacter was written.
                    overrideTargetCharacter = __instance.GetLastAttackerCharacter(),
                });
            }
        }
    }
}
