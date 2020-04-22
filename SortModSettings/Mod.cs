using ColossalFramework.UI;
using Harmony;
using ICities;
using System.Linq;
using System.Reflection;

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

            var createCategoriesOriginal = typeof(OptionsMainPanel).GetMethod("CreateCategories", BindingFlags.Instance | BindingFlags.NonPublic);
            var createCategoriesPostfix = typeof(Mod).GetMethod(nameof(CreateCategoriesPostfix), BindingFlags.Static | BindingFlags.Public);
            _harmonyInstance.Patch(createCategoriesOriginal, null, new HarmonyMethod(createCategoriesPostfix));
        }

        public void OnDisabled()
        {
            _harmonyInstance.UnpatchAll(_harmonyId);
            _harmonyInstance = null;
        }

        public static void CreateCategoriesPostfix(OptionsMainPanel __instance)
        {
            var categories = typeof(OptionsMainPanel)
                .GetField("m_Categories", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(__instance) as UIListBox;

            var m_FilteredItems = typeof(UIListBox)
                .GetField("m_FilteredItems", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(categories) as int[];

            var selectedIndex = categories.selectedIndex;

            //Spaces, Graphics, Gameplay etc.
            var itemsToIgnoreCount = 10;

            var defaultCategories = categories
                .items
                .Take(itemsToIgnoreCount);
            var modCategories = categories
                .items
                .Skip(itemsToIgnoreCount)
                .Where(c => !string.IsNullOrEmpty(c))
                .OrderBy(c => c);
            categories.items = defaultCategories //this will change selected index
                .Concat(modCategories)
                .ToArray();

            categories.selectedIndex = selectedIndex;
        }
    }
}
