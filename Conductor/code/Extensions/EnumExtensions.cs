using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Extensions
{
    internal static class EnumDefinitionCache<TEnum> where TEnum : Enum
    {
        internal static ISet<long> DefinedValues;

        static EnumDefinitionCache()
        {
            DefinedValues = Enum.GetValues(typeof(TEnum)).Cast<object>().Select(Convert.ToInt64).ToHashSet();
        }

        internal static bool IsDefined(TEnum value) => DefinedValues.Contains(Convert.ToInt64(value));
    }
}
