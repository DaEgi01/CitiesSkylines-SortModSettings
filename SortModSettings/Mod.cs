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

        public string Name => "Sort Mod Settings";
        public string Description => "Sorts the 'Mod Settings' by name.";

        public override void OnLevelLoaded(LoadMode mode)
        {
            var optionsMainPanel = UIView.library.Get<OptionsMainPanel>("OptionsPanel");
            SortCategories(optionsMainPanel);
            ReplaceSetContainerCategoryMethod(optionsMainPanel);
        }

        private void SortCategories(OptionsMainPanel optionsMainPanel)
        {
            var categories = optionsMainPanel.GetType()
                                             .GetField("m_Categories", BindingFlags.Instance | BindingFlags.NonPublic)
                                             .GetValue(optionsMainPanel) as UIListBox;

            var defaultCategories = categories.items.Take(itemsToIgnoreCount);
            var modCategories = categories.items.Skip(itemsToIgnoreCount).Where(c => !string.IsNullOrEmpty(c)).OrderBy(c => c);
            categories.items = defaultCategories.Concat(modCategories).ToArray();
        }

        private void ReplaceSetContainerCategoryMethod(OptionsMainPanel optionsMainPanel)
        {
            var harmony = HarmonyInstance.Create("egi.citiesskylinesmods.sortmodsettings");
            var original = optionsMainPanel.GetType().GetMethod("SetContainerCategory", BindingFlags.Instance | BindingFlags.NonPublic);
            var replacement = typeof(Mod).GetMethod(nameof(SetContainerCategory), BindingFlags.Static | BindingFlags.Public);
            harmony.Patch(original, new HarmonyMethod(replacement), null);
        }

        public static bool SetContainerCategory(OptionsMainPanel __instance, int index)
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
