using CitiesHarmony.API;
using ICities;

namespace SortModSettings
{
	public class Mod : LoadingExtensionBase, IUserMod
	{
		public string Name => "Sort Mod Settings";
		public string Description => "Sorts the 'Mod Settings' by name.";

		public void OnEnabled()
		{
			HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
		}

		public void OnDisabled()
		{
			if (!HarmonyHelper.IsHarmonyInstalled)
				return;

			Patcher.UnpatchAll();
		}
	}
}
