using ColossalFramework.UI;
using ICities;
using System.Reflection;
using System.Linq;
using Harmony;

namespace SortModSettings
{
    public class Mod : LoadingExtensionBase, IUserMod
    {
        private static int itemsToIgnoreCount = 10; //these are the regular menu items like graphics, gameplay etc.
        private bool patchesApplied = false;

        public string Name => "Sort Mod Settings";
        public string Description => "Sorts the 'Mod Settings' by name.";

        public void OnEnabled()
        {
            if (patchesApplied)
            {
                return;
            }

            var harmony = HarmonyInstance.Create("egi.citiesskylinesmods.sortmodsettings");
            ApplyHarmonyPatches(harmony);
        }

        public void OnDisabled()
        {
            //TODO: can be done once harmony will support the unpatch feature in the next version.
        }

        public void ApplyHarmonyPatches(HarmonyInstance harmony)
        {
            var createCategories = typeof(OptionsMainPanel).GetMethod("CreateCategories", BindingFlags.Instance | BindingFlags.NonPublic);
            var createCategoriesPostfix = typeof(Mod).GetMethod(nameof(CreateCategoriesPostfix), BindingFlags.Static | BindingFlags.Public);
            harmony.Patch(createCategories, null, new HarmonyMethod(createCategoriesPostfix));

            var setContainerCategory = typeof(OptionsMainPanel).GetMethod("SetContainerCategory", BindingFlags.Instance | BindingFlags.NonPublic);
            var setContainerCategoryPrefix = typeof(Mod).GetMethod(nameof(SetContainerCategoryPrefix), BindingFlags.Static | BindingFlags.Public);
            harmony.Patch(setContainerCategory, new HarmonyMethod(setContainerCategoryPrefix), null);

            patchesApplied = true;
        }

        public static void CreateCategoriesPostfix(OptionsMainPanel __instance)
        {
            var categories = __instance.GetType().GetField("m_Categories", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as UIListBox;

            var defaultCategories = categories.items.Take(itemsToIgnoreCount);
            var modCategories = categories.items.Skip(itemsToIgnoreCount).Where(c => !string.IsNullOrEmpty(c)).OrderBy(c => c);
            categories.items = defaultCategories.Concat(modCategories).ToArray();
        }

        public static bool SetContainerCategoryPrefix(OptionsMainPanel __instance, int index)
        {
            //default behaviour for the regular menu items like graphics, gameplay etc.
            if (index < itemsToIgnoreCount)
            {
                return true;
            }

            var categories = __instance.GetType()
                .GetField("m_Categories", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(__instance) as UIListBox;

            var categoryContainer = __instance.GetType()
                .GetField("m_CategoriesContainer", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(__instance) as UITabContainer;

            var selectedModName = categories.items[index];
            var selectedModIndexAndContainer = categoryContainer.components.FirstOrDefault(c => c.name == selectedModName);

            //default behaviour if name was not found
            if (selectedModIndexAndContainer == null)
            {
                return true;
            }

            var selectedModIndex = categoryContainer.components.IndexOf(selectedModIndexAndContainer);
            categoryContainer.selectedIndex = selectedModIndex;

            return false;
        }
    }
}
