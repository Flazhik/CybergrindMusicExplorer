using HarmonyLib;
using TMPro;
using UnityEngine;
using static UnityEngine.Object;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(OptionsMenuToManager))]
    public class OptionsMenuToManagerPatch
    {
        private static Transform _optionsMenu;
        private static Transform _audioSettingsContent;
        private static Transform _sectionPrefab;
        private static Transform _note;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OptionsMenuToManager), "OpenOptions")]
        public static bool OptionsMenuToManager_OnEnable_Prefix(OptionsMenuToManager __instance)
        {
            _note.GetComponent<TextMeshProUGUI>().text = GetNote();
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OptionsMenuToManager), "Start")]
        public static bool OptionsMenuToManager_Start_Prefix(OptionsMenuToManager __instance)
        {
            _optionsMenu = __instance.optionsMenu.transform;
            Prepare();

            var generalSettings = CreateSection(_audioSettingsContent, "-- GENERAL --");
            generalSettings.SetSiblingIndex(0);
            CreateSection(_audioSettingsContent, "-- CYBERGRIND MUSIC EXPLORER --");
            _note = CreateSection(_audioSettingsContent, GetNote());
            _note.GetComponent<TextMeshProUGUI>().fontSize = 16;
            return true;
        }

        private static void Prepare()
        {
            _audioSettingsContent = _optionsMenu.Find("Audio Options")
                .Find("Image");
            _sectionPrefab = _optionsMenu.Find("HUD Options")
                .Find("Scroll Rect (1)")
                .Find("Contents")
                .Find("-- HUD Elements -- ");
            _audioSettingsContent.GetComponent<RectTransform>().offsetMax = new Vector2(300f, 160f);
        }

        private static Transform CreateSection(Transform parent, string text)
        {
            var section = Instantiate(_sectionPrefab, parent);
            section.name = text;
            section.GetComponent<TextMeshProUGUI>().text = text;
            return section;
        }

        private static string GetNote()
        {
            return
                $"Please press [<color=orange>{((KeyCode)MonoSingleton<CybergrindMusicExplorerManager>.Instance.MenuBinding).ToString()}</color>] in Cybergrind to open CGME options";
        }
    }
}