using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Conductor.Patches
{
    public static class TranspilerUtilities
    {
        public static int LdLocIndex(this CodeInstruction ci)
        {
            if (ci == null)
                return -1;

            var op = ci.opcode;

            // operand could be a byte, int, or LocalBuilder
            if (op == OpCodes.Ldloc_S || op == OpCodes.Ldloc)
            {
                
                if (ci.operand is byte b) return b;
                if (ci.operand is int i) return i;
                if (ci.operand is LocalBuilder lb) return lb.LocalIndex;
            }

            // Also check for dedicated short forms (ldloc_0 .. ldloc_3)
            if (op == OpCodes.Ldloc_0) return 0;
            if (op == OpCodes.Ldloc_1) return 1;
            if (op == OpCodes.Ldloc_2) return 2;
            if (op == OpCodes.Ldloc_3) return 3;

            return -1;
        }
    }
}
