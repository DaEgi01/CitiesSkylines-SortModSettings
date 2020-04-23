using ColossalFramework;
using ColossalFramework.Plugins;
using Harmony;
using ICities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SortModSettings
{
    public class Mod : LoadingExtensionBase, IUserMod
    {
        private readonly string _harmonyId = "egi.citiesskylinesmods.sortmodsettings";
        private HarmonyInstance _harmonyInstance;

        public string Name => "Sort Mod Settings";
        public string Description => "Sorts the 'Mod Settings' by name.";

        public void OnEnabled()
        {
            _harmonyInstance = HarmonyInstance.Create(_harmonyId);

            var addUserModsOriginal = typeof(OptionsMainPanel).GetMethod("AddUserMods", BindingFlags.NonPublic | BindingFlags.Instance);
            var addUserModsTranspiler = typeof(Mod).GetMethod(nameof(AddUserModsTranspiler), BindingFlags.Public | BindingFlags.Static);
            _harmonyInstance.Patch(addUserModsOriginal, null, null, new HarmonyMethod(addUserModsTranspiler));
        }

        public void OnDisabled()
        {
            _harmonyInstance.UnpatchAll(_harmonyId);
            _harmonyInstance = null;
        }

        public static IEnumerable<CodeInstruction> AddUserModsTranspiler(IEnumerable<CodeInstruction> codeInstructions)
        {
            var hookOpCode = OpCodes.Callvirt;
            var hookOperand = typeof(PluginManager)
                .GetMethod("GetPluginsInfo", BindingFlags.Public | BindingFlags.Instance);

            var replacementMethod = typeof(Mod)
                .GetMethod(nameof(GetPluginsInfoInOrder), BindingFlags.Public | BindingFlags.Static);

            var instructions = codeInstructions.ToList();
            for (int i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                if (instruction.opcode == hookOpCode && instruction.operand == hookOperand)
                {
                    instruction.opcode = OpCodes.Call;
                    instruction.operand = replacementMethod;

                    instructions.RemoveAt(i - 1);
                    break;
                }
            }

            return instructions;
        }

        public static IEnumerable<PluginManager.PluginInfo> GetPluginsInfoInOrder() => 
            Singleton<PluginManager>
                .instance
                .GetPluginsInfo()
                .OrderBy(p => ((IUserMod)p.userModInstance).Name);
    }
}
