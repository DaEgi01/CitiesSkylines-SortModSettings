using ColossalFramework;
using ColossalFramework.Plugins;
using HarmonyLib;
using ICities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace SortModSettings
{
	public static class Patcher
	{
		private const string _harmonyId = "egi.citiesskylinesmods.sortmodsettings";
		private static bool _patched = false;

		public static void PatchAll()
		{
			if (_patched)
				return;

			var addUserModsOriginal = typeof(OptionsMainPanel).GetMethod("AddUserMods", BindingFlags.NonPublic | BindingFlags.Instance);
			var addUserModsTranspiler = typeof(Patcher).GetMethod(nameof(AddUserModsTranspiler), BindingFlags.Public | BindingFlags.Static);

			var harmony = new Harmony(_harmonyId);
			harmony.Patch(addUserModsOriginal, null, null, new HarmonyMethod(addUserModsTranspiler));

			_patched = true;
		}

		public static void UnpatchAll()
		{
			if (!_patched)
				return;

			var harmony = new Harmony(_harmonyId);
			harmony.UnpatchAll(_harmonyId);

			_patched = false;
		}

		public static IEnumerable<CodeInstruction> AddUserModsTranspiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			var hookOpCode = OpCodes.Callvirt;
			var hookOperand = typeof(PluginManager)
				.GetMethod("GetPluginsInfo", BindingFlags.Public | BindingFlags.Instance);

			var replacementMethod = typeof(Patcher)
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
				.Where(p => p?.userModInstance as IUserMod != null)
				.OrderBy(p => ((IUserMod)p.userModInstance).Name);
	}
}
