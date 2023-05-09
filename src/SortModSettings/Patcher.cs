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
		private static bool _patched;

		public static void PatchAll()
		{
			if (_patched)
				return;

			MethodInfo addUserModsOriginal = typeof(OptionsMainPanel)
				.GetMethod("AddUserMods", BindingFlags.NonPublic | BindingFlags.Instance);
			MethodInfo addUserModsTranspiler = typeof(Patcher)
				.GetMethod(nameof(AddUserModsTranspiler), BindingFlags.Public | BindingFlags.Static);

			Harmony harmony = new(_harmonyId);
			harmony.Patch(addUserModsOriginal, null, null, new HarmonyMethod(addUserModsTranspiler));

			_patched = true;
		}

		public static void UnpatchAll()
		{
			if (!_patched)
				return;

			Harmony harmony = new(_harmonyId);
			harmony.UnpatchAll(_harmonyId);

			_patched = false;
		}

		public static IEnumerable<CodeInstruction> AddUserModsTranspiler(IEnumerable<CodeInstruction> codeInstructions)
		{
			MethodInfo hookOperand = typeof(PluginManager)
				.GetMethod("GetPluginsInfo", BindingFlags.Public | BindingFlags.Instance);
			MethodInfo replacementMethod = typeof(Patcher)
				.GetMethod(nameof(GetPluginsInfoInOrder), BindingFlags.Public | BindingFlags.Static);

			var instructions = codeInstructions.ToList();
			for (int i = 0; i < instructions.Count; i++)
			{
				CodeInstruction instruction = instructions[i];
				if (instruction.opcode == OpCodes.Callvirt && instruction.operand == hookOperand)
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
				.Where(p => p?.userModInstance is IUserMod)
				.OrderBy(p => ((IUserMod)p.userModInstance).Name);
	}
}
