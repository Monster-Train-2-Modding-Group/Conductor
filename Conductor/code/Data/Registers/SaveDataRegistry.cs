using Conductor.Interfaces;
using System.Text.Json;
using System.Text.Json.Serialization;
using Unity.Jobs;

namespace Conductor.Data.Registers
{
    /// <summary>
    /// Class that manages SaveData across all mods.
    /// 
    /// </summary>
    public static class SaveDataRegistry
    {
        internal static IDictionary<string, IDictionary<string, ISaveData>> Data = new Dictionary<string, IDictionary<string, ISaveData>>();
        internal static string Magic = "ConductorSaveData";

        [Serializable]
        struct SaveDataEntry
        {
            [JsonInclude]
            public string Key;
            [JsonInclude]
            public string Payload;
        }
        [Serializable]
        struct ModSaveDataEntry
        {
            [JsonInclude]
            public string Guid = "";
            [JsonInclude]
            public List<SaveDataEntry> Entries = [];
            public ModSaveDataEntry() { }
        }
        [Serializable]
        struct SaveData
        {
            [JsonInclude]
            public string SaveDataMagic = Magic;
            [JsonInclude]
            public List<ModSaveDataEntry> Entries = [];
            public SaveData() { }
        }

        public static void Register(string guid, ISaveData data)
        {
            if (!Data.ContainsKey(guid))
                Data.Add(guid, new Dictionary<string, ISaveData>());
            Data[guid].Add(data.Key, data);
        }

        internal static void Reset()
        {
            Plugin.Logger.LogDebug("Resetting Save Data states");
            foreach (var guid_item in Data)
            {
                foreach (var item2 in guid_item.Value.Values)
                {
                    try
                    {
                        item2.Reset();
                    }
                    catch (Exception e)
                    {
                        Plugin.Logger.LogError($"Could not reset item {guid_item.Key} {item2.Key} due to exception {e.Message}");
                    }
                }
            }
        }

        internal static void Deserialize(string data)
        {
            Plugin.Logger.LogDebug("Loading Save Data from json");
            SaveData saveData;
            try
            {
                saveData = JsonSerializer.Deserialize<SaveData>(data);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Could not load save data due to exception\n{e.Message}");
                return;
            }
            foreach (var modEntry in saveData.Entries)
            {
                var guid = modEntry.Guid;
                foreach (var entry in modEntry.Entries)
                {
                    try
                    {
                        var modSaveData = Data.GetValueOrDefault(guid);
                        var saveDataItem = modSaveData?.GetValueOrDefault(entry.Key);
                        saveDataItem?.Deserialize(entry.Payload);
                    }
                    catch (Exception e)
                    {
                        Plugin.Logger.LogError($"Could not load save data item with guid {guid} key {entry.Key} due to exception\n{e.Message}");
                    }
                }
            }
        }

        internal static string? Serialize()
        {
            Plugin.Logger.LogDebug("Writing Save Data to json");
            SaveData data = new();
            foreach (var guid_entries in Data)
            {
                var guid = guid_entries.Key;
                ModSaveDataEntry mod_entry = new()
                {
                    Guid = guid
                };
                foreach (var key_entry in guid_entries.Value)
                {
                    var key = key_entry.Key;
                    var saveObject = key_entry.Value;
                    try
                    {
                        if (saveObject.ShouldSerialize())
                        {
                            SaveDataEntry entry = new()
                            {
                                Key = key_entry.Key,
                                Payload = saveObject.Serialize()
                            };
                            mod_entry.Entries.Add(entry);
                        }
                    }
                    catch (Exception e)
                    {
                        Plugin.Logger.LogError($"Failed to serialize {guid} {key} Save Data due to exception\n{e.Message}");
                    }
                }
                if (mod_entry.Entries.Count > 0)
                {
                    data.Entries.Add(mod_entry);
                }
            }
            if (data.Entries.IsNullOrEmpty())
            {
                return null;
            }
            string? json = null;
            try
            {
                json = JsonSerializer.Serialize<SaveData>(data);
            }
            catch (Exception e)
            {
                Plugin.Logger.LogError($"Could not save data due to exception\n{e.Message}");
            }
            return json;
        }
    }
}
