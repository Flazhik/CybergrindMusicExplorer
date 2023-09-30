using CybergrindMusicExplorer.GUI.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace CybergrindMusicExplorer.GUI
{
    public class GUIDeployer : MonoBehaviour
    {
        public GameObject menuWindow;
        public GameObject playbackWindow;

        private readonly OptionsManager optionsManager = OptionsManager.Instance;
        private readonly GameStateManager gameStateManager = GameStateManager.Instance;
        private readonly CameraController cameraController = MonoSingleton<CameraController>.Instance;
        private NewMovement newMovement = MonoSingleton<NewMovement>.Instance;
        private CybergrindMusicExplorerManager manager = MonoSingleton<CybergrindMusicExplorerManager>.Instance;
        private AudioMixerController mixer = MonoSingleton<AudioMixerController>.Instance;
        private GunControl gunControl;

        private GameObject canvas;
        private Toggle infinitePanelToggle;
        private Toggle displaySubtitlesToggle;
        private Slider boostSlider;
        private Text boostPercentage;
        private Button closeButton;


        public void Awake()
        {
            canvas = GameObject.Find("Canvas");
            menuWindow = (GameObject)Instantiate(AssetsManager.Instance.GetAsset("assets/ui/cgme settings.prefab"),
                canvas.transform);
            playbackWindow = (GameObject)Instantiate(AssetsManager.Instance.GetAsset("assets/ui/playbackwindow.prefab"),
                canvas.transform);
            
            menuWindow.AddComponent<CgmeBindingsController>();
            menuWindow.AddComponent<HudOpenEffect>();
            playbackWindow.AddComponent<HudOpenEffect>();

            closeButton = menuWindow.transform.Find("Close Settings").GetComponent<Button>();
            infinitePanelToggle = menuWindow.transform.Find("Infinite panel").GetComponent<Toggle>();
            displaySubtitlesToggle = menuWindow.transform.Find("Display subtitles").GetComponent<Toggle>();

            boostSlider = menuWindow.transform.Find("Boost Settings").Find("Slider").GetComponent<Slider>();
            boostPercentage = menuWindow.transform.Find("Boost Settings").Find("Percentage").GetComponent<Text>();

            BindControls();

            gunControl = newMovement.GetComponentInChildren<GunControl>();
            menuWindow.SetActive(false);
        }

        public void Update()
        {
            if (GameIsPaused())
                return;
            
            // On Escape
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (menuWindow.activeSelf)
                    CloseOptionsMenu();
                
                if (playbackWindow.activeSelf)
                    ClosePlaybackMenu();
            }

            // On CGME menu binding
            if (Input.GetKeyDown((KeyCode)manager.MenuBinding))
            {
                if (playbackWindow.activeSelf)
                    return;
                
                if (!menuWindow.activeSelf)
                    OpenOptionsMenu();
                else
                    CloseOptionsMenu();
            }

            // On playback menu binding
            if (!Input.GetKeyDown((KeyCode)manager.PlaybackMenuBinding))
                return;
            
            if (!playbackWindow.activeSelf)
                OpenPlaybackMenu();
            else
                ClosePlaybackMenu();
        }

        public void OpenOptionsMenu()
        {
            menuWindow.SetActive(true);
            Pause("cgmeMenu", menuWindow);
        }

        public void CloseOptionsMenu()
        {
            menuWindow.SetActive(false);
            UnPause("cgmeMenu");
        }
        
        public void OpenPlaybackMenu()
        {
            playbackWindow.SetActive(true);
            Pause("cgmePlayback", playbackWindow);
        }
        
        public void ClosePlaybackMenu()
        {
            playbackWindow.SetActive(false);
            UnPause("cgmePlayback");
        }

        private void BindControls()
        {
            closeButton.onClick.AddListener(CloseOptionsMenu);

            infinitePanelToggle.isOn =
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.ShowCurrentTrackPanelIndefinitely;
            infinitePanelToggle.onValueChanged.AddListener(state =>
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.ShowCurrentTrackPanelIndefinitely = state);

            displaySubtitlesToggle.isOn =
                MonoSingleton<PrefsManager>.Instance.GetBool("subtitlesEnabled");
            displaySubtitlesToggle.onValueChanged.AddListener(state =>
                MonoSingleton<OptionsMenuToManager>.Instance.SetSubtitles(state));

            boostSlider.onValueChanged.AddListener(value =>
            {
                boostPercentage.text = $"+{value}dB";
                manager.CustomTracksBoost = value;
                mixer.SetMusicVolume(mixer.musicVolume);
            });
            boostSlider.value = manager.CustomTracksBoost;
        }

        private void Pause(string stateKey, GameObject window)
        {
            if (newMovement == null)
            {
                newMovement = MonoSingleton<NewMovement>.Instance;
                gunControl = newMovement.GetComponentInChildren<GunControl>();
            }

            newMovement.enabled = false;
            cameraController.activated = false;
            gunControl.activated = false;
            gameStateManager.RegisterState(new GameState(stateKey, new[] { window })
            {
                cursorLock = LockMode.Unlock,
                cameraInputLock = LockMode.Lock,
                playerInputLock = LockMode.Lock
            });
            
            optionsManager.paused = true;
        }

        private void UnPause(string stateKey)
        {
            gameStateManager.PopState(stateKey);
            
            if (menuWindow.activeSelf || playbackWindow.activeSelf)
                return;
            
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
            gunControl.activated = true;
        }

        private bool GameIsPaused()
        {
            return gameStateManager.IsStateActive("pause");
        }
    }
}