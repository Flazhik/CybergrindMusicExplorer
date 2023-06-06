using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static UnityEngine.Object;

namespace CybergrindMusicExplorer.Patches
{
    [HarmonyPatch(typeof(OptionsMenuToManager))]
    public class OptionsMenuToManagerPatch
    {
        private static Transform _optionsMenu;
        private static Transform _audioSettingsContent;
        private static Transform _checkboxPrefab;
        private static Transform _sectionPrefab;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(OptionsMenuToManager), "Start")]
        public static bool OptionsMenuToManager_Start_Prefix(OptionsMenuToManager __instance)
        {
            _optionsMenu = __instance.optionsMenu.transform;
            Prepare();
            
            var generalSettings = CreateSection(_audioSettingsContent, "-- GENERAL --");
            generalSettings.SetSiblingIndex(0);
            CreateSection(_audioSettingsContent, "-- CYBERGRIND MUSIC EXPLORER --");

            var normalizeCheckbox = Instantiate(_checkboxPrefab, _audioSettingsContent);
            var normalizationToggle = CreateToggle(
                normalizeCheckbox,
                "Normalise Checkbox",
                "Normalize sound",
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.NormalizeSoundtrack,
                state => MonoSingleton<CybergrindMusicExplorerManager>.Instance.NormalizeSoundtrack = state);
            
            // TODO: I can't switch off normalization on the fly yet. Eh, maybe later.
            normalizationToggle.onValueChanged.AddListener(state =>
            {
                if (SceneHelper.CurrentScene != "Endless")
                    return;
                var onOffText = state ? "on" : "off";
                HudMessageReceiver.Instance.SendHudMessage(
                    $"Soundtrack normalization will be switched {onOffText} after Cybergrind restart");
            });
            
            var indefinitePanelCheckbox = Instantiate(_checkboxPrefab, _audioSettingsContent);
            CreateToggle(
                indefinitePanelCheckbox,
                "Indefinite Panel Checkbox",
                "Show current song panel indefinitely",
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.ShowCurrentTrackPanelIndefinitely,
                state => MonoSingleton<CybergrindMusicExplorerManager>.Instance.ShowCurrentTrackPanelIndefinitely = state);
            return true;
        }

        private static Toggle CreateToggle(Transform toggle, string name, string text, bool defaultState, UnityAction<bool> action)
        {
            toggle.name = name;
            toggle.gameObject.SetActive(true);
            toggle.Find("Text").GetComponent<Text>().text = text;
            var toggleComponent = toggle.Find("Toggle").GetComponent<Toggle>();
            toggleComponent.onValueChanged.RemoveAllListeners();
            toggleComponent.isOn = defaultState;
            toggleComponent.onValueChanged.AddListener(action);
            return toggleComponent;
        }

        private static void Prepare()
        {
            _audioSettingsContent = _optionsMenu.Find("Audio Options")
                    .Find("Image");
            _checkboxPrefab = _audioSettingsContent.Find("Subtitles Checkbox");
            _sectionPrefab = _optionsMenu.Find("HUD Options")
                    .Find("Scroll Rect (1)")
                    .Find("Contents")
                    .Find("-- HUD Elements -- ");
            _audioSettingsContent.GetComponent<RectTransform>().offsetMax = new Vector2(300f, 240f);
        }

        private static Transform CreateSection(Transform parent, string text)
        {
            var section = Instantiate(_sectionPrefab, parent);
            section.name = text;
            section.GetComponent<Text>().text = text;
            return section;
        }
    }
}