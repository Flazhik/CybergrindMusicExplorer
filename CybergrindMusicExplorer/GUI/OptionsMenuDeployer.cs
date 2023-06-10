using System;
using CybergrindMusicExplorer.GUI.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace CybergrindMusicExplorer.GUI
{
    public class OptionsMenuDeployer : MonoBehaviour
    {
        private readonly OptionsManager optionsManager = OptionsManager.Instance;
        private readonly GameStateManager gameStateManager = GameStateManager.Instance;
        private readonly CameraController cameraController = MonoSingleton<CameraController>.Instance;
        private NewMovement newMovement = MonoSingleton<NewMovement>.Instance;
        private CybergrindMusicExplorerManager manager = MonoSingleton<CybergrindMusicExplorerManager>.Instance;
        private AudioMixerController mixer = MonoSingleton<AudioMixerController>.Instance;
        private GunControl gunControl;
        private CgmeBindingsController bindingsController;

        private GameObject canvas;
        private GameObject menu;
        private Toggle normalizationToggle;
        private Toggle infinitePanelToggle;
        private Slider boostSlider;
        private Text boostPercentage;
        private Button closeButton;


        public void Awake()
        {
            canvas = GameObject.Find("Canvas");
            menu = (GameObject)Instantiate(AssetsManager.Instance.GetAsset("assets/ui/cgme settings.prefab"),
                canvas.transform);

            bindingsController = menu.AddComponent<CgmeBindingsController>();
            menu.AddComponent<HudOpenEffect>();

            closeButton = menu.transform.Find("Close Settings").GetComponent<Button>();
            normalizationToggle = menu.transform.Find("Normalize track").GetComponent<Toggle>();
            infinitePanelToggle = menu.transform.Find("Infinite panel").GetComponent<Toggle>();

            boostSlider = menu.transform.Find("Boost Settings").Find("Slider").GetComponent<Slider>();
            boostPercentage = menu.transform.Find("Boost Settings").Find("Percentage").GetComponent<Text>();

            BindControls();

            gunControl = newMovement.GetComponentInChildren<GunControl>();
            menu.SetActive(false);
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && menu.activeSelf)
                Close();

            if (Input.GetKeyDown((KeyCode)manager.MenuBinding))
            {
                if (!menu.activeSelf && !optionsManager.paused)
                    Show();
                else
                    Close();
            }
        }

        public void Show()
        {
            Pause();
            menu.SetActive(true);
        }

        public void Close()
        {
            UnPause();
            menu.SetActive(false);
        }

        private void BindControls()
        {
            closeButton.onClick.AddListener(Close);
            normalizationToggle.isOn =
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.NormalizeSoundtrack;
            normalizationToggle.onValueChanged.AddListener(state =>
            {
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.NormalizeSoundtrack = state;
                var onOffText = state ? "on" : "off";
                HudMessageReceiver.Instance.SendHudMessage(
                    $"Soundtrack normalization will be switched {onOffText} after Cybergrind restart");
            });
            infinitePanelToggle.isOn =
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.ShowCurrentTrackPanelIndefinitely;
            infinitePanelToggle.onValueChanged.AddListener(state =>
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.ShowCurrentTrackPanelIndefinitely = state);

            boostSlider.onValueChanged.AddListener(value =>
            {
                boostPercentage.text = $"+{value}dB";
                manager.CustomTracksBoost = value;
                mixer.SetMusicVolume(mixer.musicVolume);
            });
            boostSlider.value = manager.CustomTracksBoost;
        }

        private void Pause()
        {
            if (newMovement == null)
            {
                newMovement = MonoSingleton<NewMovement>.Instance;
                gunControl = newMovement.GetComponentInChildren<GunControl>();
            }

            newMovement.enabled = false;
            cameraController.activated = false;
            gunControl.activated = false;
            gameStateManager.RegisterState(new GameState("cgmeMenu", new[] { menu })
            {
                cursorLock = LockMode.Unlock,
                cameraInputLock = LockMode.Lock,
                playerInputLock = LockMode.Lock
            });
            optionsManager.paused = true;
        }

        private void UnPause()
        {
            if (newMovement == null)
            {
                newMovement = MonoSingleton<NewMovement>.Instance;
                gunControl = newMovement.GetComponentInChildren<GunControl>();
            }

            Time.timeScale = MonoSingleton<TimeController>.Instance.timeScale *
                             MonoSingleton<TimeController>.Instance.timeScaleModifier;
            optionsManager.paused = false;
            cameraController.activated = true;
            newMovement.enabled = true;
            gameStateManager.PopState("cgmeMenu");
            gunControl.activated = true;
        }
    }
}