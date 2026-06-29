using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Interfaces
{
    /// <summary>
    /// Interface for Save Data. Instances can be registered with the SaveDataRegistry to be able to load/save data at appropriate times.
    /// 
    /// Data should be serialized/deserialized in JSON.
    /// </summary>
    public interface ISaveData
    {
        /// <summary>
        /// Identifier for this Data. Must be unique. No two items within a mod can have the same key.
        /// </summary>
        public string Key { get; }
        /// <summary>
        /// Should the data be serialized.
        /// This function returns a flag if there's data that needs to be saved.
        /// </summary>
        /// <returns></returns>
        public bool ShouldSerialize();
        /// <summary>
        /// Returns a serialized string with the data.
        /// 
        /// Recommended to use the DTO pattern, defining a simple struct to hold the values you wish to save.
        /// When this function is called transfer the properties you wish to serialze to to the struct
        /// and then return JsonSerializer.Serialize(state);
        /// 
        /// The string return need not be JSON however.
        /// See PlayerResource.Serialize for an example.
        /// </summary>
        /// <returns>String containing the serialized save data.</returns>
        public string Serialize();
        /// <summary>
        /// Given a previously serialized string deserializes it in place.
        /// </summary>
        /// <param name="data">String containing serialized data</param>
        public void Deserialize(string data);
        /// <summary>
        /// Called when the data should reset as part of starting a new run.
        /// </summary>
        public void Reset();

    }
}
