using Conductor.Extensions;
using Conductor.Triggers;
using HarmonyLib;
using ShinyShoe;
using ShinyShoe.Logging;
using System.Collections;
using UnityEngine.TextCore.Text;
using static CardManager;
using static CharacterState;

namespace Conductor.Patches
{
    /// <summary>
    /// TODO rework this patch
    /// </summary>
    [HarmonyPatch(typeof(CharacterState), nameof(CharacterState.ApplyDamage))]
    class CharacterState_ApplyDamage_Patch
    {
        static bool alreadyLoggedWarning = false;

        public static IEnumerator Postfix(IEnumerator enumerator, CharacterState __instance, int damage, ApplyDamageParams damageParams, AllGameManagers ___allGameManagers)
        {
            bool queueAndRunTriggerFound = false;
            while (enumerator.MoveNext())
            {
                var currentType = enumerator.Current.GetType();
                if (currentType.DeclaringType == typeof(CombatManager) && currentType.Name.Contains("QueueAndRunTrigger"))
                {
                    yield return enumerator.Current;
                    if (queueAndRunTriggerFound)
                    {
                        if (!alreadyLoggedWarning)
                        {
                            Plugin.Logger.LogError("Found multiple calls to QueueAndRunTrigger, so skipping HandleOnHitTriggers, Patch may need to be redone.");
                            alreadyLoggedWarning = true;
                        }
                        continue;
                    }
                    yield return HandleOnHitTriggers(__instance, damage, damageParams, ___allGameManagers.GetCoreManagers());
                    if (__instance.IsPyreHeart())
                    {
                        yield return HandleOnPyreDamageTriggers(__instance, damage, damageParams, ___allGameManagers.GetCoreManagers());
                    }
                    queueAndRunTriggerFound = true;
                    continue;
                }
                yield return enumerator.Current;
            }
        }

        public static IEnumerator HandleOnHitTriggers(CharacterState character, int damage, ApplyDamageParams damageParams, ICoreGameManagers coreGameManagers)
        {
            List<CharacterState> outCharacters;
            using (GenericPools.GetList(out outCharacters))
            {
                RoomState? room = character.GetCurrentRoom(true);
                if (room == null)
                {
                    Plugin.Logger.LogError($"Character {character} not found in a room?");
                    yield break;
                }
                room.AddCharactersToList(outCharacters, Team.Type.Heroes | Team.Type.Monsters);

                var data = new TriggerOnCharacterHitParams
                {
                    DamagedCharacter = character,
                    OriginalDamage = damage,
                    DamageParams = damageParams,
                    Room = room,
                    RoomIndex = room.GetRoomIndex(),
                    CoreGameManagers = coreGameManagers
                };

                foreach (CharacterState allied in outCharacters)
                {
                    data.Character = allied;
                    foreach (var trigger_test in CharacterTriggerExtensions.TriggersOnCharacterHit)
                    {
                        if (trigger_test.Value(data, out var queueTriggerParams))
                        {
                            coreGameManagers.GetCombatManager().QueueCustomTrigger(allied, trigger_test.Key, queueTriggerParams);
                        }
                    }
                }

                yield return coreGameManagers.GetCombatManager().RunTriggerQueue();
            }
        }

        public static IEnumerator HandleOnPyreDamageTriggers(CharacterState pyre, int damage, ApplyDamageParams damageParams, ICoreGameManagers coreGameManagers)
        {
            var data = new TriggerOnPyreDamageParams
            {
                Pyre = pyre,
                Damage = damage,
                DamageParams = damageParams,
                CoreGameManagers = coreGameManagers,
            };
            
            var monsterManager = coreGameManagers.GetMonsterManager();
            var heroManager = coreGameManagers.GetHeroManager();
            var combatManager = coreGameManagers.GetCombatManager();

            for (int i = 0; i < monsterManager.GetNumCharacters(); i++)
            {
                var character = monsterManager.GetCharacter(i);
                data.Character = character;
                foreach (var trigger_test in CharacterTriggerExtensions.TriggersOnPyreDamage)
                {
                    if (trigger_test.Value(data, out var queueTriggerParams))
                    {
                        combatManager.QueueCustomTrigger(character, trigger_test.Key, queueTriggerParams);
                    }
                }
            }
            for (int i = 0; i < heroManager.GetNumCharacters(); i++)
            {
                var character = heroManager.GetCharacter(i);
                data.Character = character;
                foreach (var trigger_test in CharacterTriggerExtensions.TriggersOnPyreDamage)
                {
                    if (trigger_test.Value(data, out var queueTriggerParams))
                    {
                        combatManager.QueueCustomTrigger(character, trigger_test.Key, queueTriggerParams);
                    }
                }
            }
            yield return combatManager.RunTriggerQueue();
        }
    }
}
