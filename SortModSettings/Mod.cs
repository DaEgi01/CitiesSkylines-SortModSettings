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

        private HarmonyInstance harmony;
        private MethodInfo createCategoriesOriginal;
        private MethodInfo createCategoriesPostfix;
        private MethodInfo setContainerCategoryOriginal;
        private MethodInfo setContainerCategoryPrefix;

        public string Name => "Sort Mod Settings";
        public string Description => "Sorts the 'Mod Settings' by name.";

        public void OnEnabled()
        {
            harmony = HarmonyInstance.Create("egi.citiesskylinesmods.sortmodsettings");

            createCategoriesOriginal = typeof(OptionsMainPanel).GetMethod("CreateCategories", BindingFlags.Instance | BindingFlags.NonPublic);
            createCategoriesPostfix = typeof(Mod).GetMethod(nameof(CreateCategoriesPostfix), BindingFlags.Static | BindingFlags.Public);
            harmony.Patch(createCategoriesOriginal, null, new HarmonyMethod(createCategoriesPostfix));

            setContainerCategoryOriginal = typeof(OptionsMainPanel).GetMethod("SetContainerCategory", BindingFlags.Instance | BindingFlags.NonPublic);
            setContainerCategoryPrefix = typeof(Mod).GetMethod(nameof(SetContainerCategoryPrefix), BindingFlags.Static | BindingFlags.Public);
            harmony.Patch(setContainerCategoryOriginal, new HarmonyMethod(setContainerCategoryPrefix), null);
        }

        public void OnDisabled()
        {
            harmony.RemovePatch(createCategoriesOriginal, createCategoriesPostfix);
            harmony.RemovePatch(setContainerCategoryOriginal, setContainerCategoryPrefix);
        }

        public static void CreateCategoriesPostfix(OptionsMainPanel __instance)
        {
            var categories = __instance.GetType().GetField("m_Categories", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance) as UIListBox;

            var defaultCategories = categories.items.Take(itemsToIgnoreCount);
            var modCategories = categories.items.Skip(itemsToIgnoreCount).Where(c => !string.IsNullOrEmpty(c)).OrderBy(c => c);
            categories.items = defaultCategories.Concat(modCategories).ToArray();
        }

        public static bool SetContainerCategoryPrefix(OptionsMainPanel __instance, UIListBox ___m_Categories, UITabContainer ___m_CategoriesContainer, int index)
        {
            //default behaviour for the regular menu items like graphics, gameplay etc.
            if (index < itemsToIgnoreCount)
                return true;

            var selectedModName = ___m_Categories.items[index];
            var selectedModIndexAndContainer = ___m_CategoriesContainer.components.FirstOrDefault(c => c.name == selectedModName);

            //default behaviour if name was not found
            if (selectedModIndexAndContainer == null)
                return true;

            var selectedModIndex = ___m_CategoriesContainer.components.IndexOf(selectedModIndexAndContainer);
            ___m_CategoriesContainer.selectedIndex = selectedModIndex;

            return false;
        }
    }
}
