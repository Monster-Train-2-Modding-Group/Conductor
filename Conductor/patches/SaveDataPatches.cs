using Conductor.Data.Registers;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.ResetSave))]
    public class SaveManager_ResetSave_Patch
    {
        public static void Prefix()
        {
            SaveDataRegistry.Reset();
        }
    }

    [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.OnStartGame))]
    public class PlayerManager_OnStartGame_Patch
    {
        public static void Prefix()
        {
            SaveDataRegistry.Reset();
        }
    }

    [HarmonyPatch(typeof(SaveManager), "LoadDeckFromFile")]
    public class SaveManager_LoadDeckFromFile_Patch
    {
        internal readonly static PropertyInfo ActiveSaveDataProperty = AccessTools.Property(typeof(SaveManager), "ActiveSaveData");
        internal readonly static FieldInfo PermanentlyDisabledAbilitiesField = AccessTools.Field(typeof(SaveData), "permanentlyDisabledAbilities");
        public static void Postfix(SaveManager __instance)
        {
            var saveData = ActiveSaveDataProperty.GetValue(__instance) as SaveData;
            var permanentlyDisabledAbilities = PermanentlyDisabledAbilitiesField.GetValue(saveData) as List<string>;
            var saveDataJson = permanentlyDisabledAbilities.FirstOrDefault(s => s.Contains(SaveDataRegistry.Magic));
            if (saveDataJson != null)
            {
                SaveDataRegistry.Deserialize(saveDataJson);
            }
        }
    }

    [HarmonyPatch(typeof(SaveManager), nameof(SaveManager.Save))]
    public class SaveManager_Save_Patch
    {
        public static void Prefix(SaveManager __instance, bool writeToFile, bool saveReplayOnly)
        {
            if (__instance.PreviewMode || saveReplayOnly)
                return;

            var saveData = SaveManager_LoadDeckFromFile_Patch.ActiveSaveDataProperty.GetValue(__instance) as SaveData;
            var permanentlyDisabledAbilities = SaveManager_LoadDeckFromFile_Patch.PermanentlyDisabledAbilitiesField.GetValue(saveData) as List<string>;
            if (permanentlyDisabledAbilities == null)
            {
                Plugin.Logger.LogError("Could not find ActiveSaveData or PermanentlyDisabledAbilities to save the data!");
                return;
            }
            permanentlyDisabledAbilities.RemoveAll(s => s.Contains(SaveDataRegistry.Magic));
            var saveDataJson = SaveDataRegistry.Serialize();
            if (saveDataJson != null)
            {
                permanentlyDisabledAbilities.Add(saveDataJson);
            }
        }
    }
}
