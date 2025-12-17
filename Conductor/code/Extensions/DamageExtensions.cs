using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Extensions
{
    public static class DamageExtensions
    {
        internal static IDictionary<Damage.Type, string> DamageTypeToCue = new Dictionary<Damage.Type, string>();

        private static bool IsVanillaEnum(Damage.Type value)
        {
            if (EnumDefinitionCache<Damage.Type>.IsDefined(value))
            {
                Plugin.Logger.LogError($"Attempt to redefine vanilla {value.GetType().Name} {value}, you probably didn't mean to do this?");
                return true;
            }
            return false;
        }

        public static Damage.Type SetSoundEffect(this Damage.Type damageType, string cueName)
        {
            if (IsVanillaEnum(damageType)) return damageType;
            DamageTypeToCue.Add(damageType, cueName);
            return damageType;
        }
    }
}
