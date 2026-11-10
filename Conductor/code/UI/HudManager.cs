namespace Conductor.UI
{
    public static class HudManager
    {
        internal static Dictionary<string, ClassMechanicHud> ClassHUDs = [];

        [Obsolete("Use Conductor.MonsterTrain2 package instead.")]
        public static void AddHUD(string key, string name, ClassMechanicHud ui)
        {
            ClassHUDs.Add($"{key}/{name}", ui);
        }

        [Obsolete("Use Conductor.MonsterTrain2 package instead.")]
        public static ClassMechanicHud? GetHUD(string key, string name)
        {
            if (ClassHUDs.TryGetValue($"{key}/{name}", out var hud))
            {
                return hud;
            }
            return null;
        }
    }
}
