using CybergrindMusicExplorer.GUI.Controllers;
using UnityEngine;

namespace CybergrindMusicExplorer.GUI
{
    public class CgmeMenuDeployer : MonoBehaviour
    {
        [PrefabAsset("assets/ui/cgmemenu.prefab")]
        private static GameObject menuPrefab;
        
        [PrefabAsset("assets/ui/playbackwindow.prefab")]
        private static GameObject playbackPrefab;
        
        public GameObject playbackWindow;
        private GameObject menuWindow;
        
        private GameObject canvas;

        private readonly OptionsManager optionsManager = OptionsManager.Instance;
        private readonly GameStateManager gameStateManager = GameStateManager.Instance;
        private readonly CameraController cameraController = CameraController.Instance;
        private readonly CybergrindMusicExplorerManager manager = CybergrindMusicExplorerManager.Instance;
        private readonly NewMovement newMovement = NewMovement.Instance;
        private readonly GunControl gunControl = GunControl.Instance;

        private void Awake()
        {
            canvas = GameObject.Find("Canvas");
            menuWindow = Instantiate(menuPrefab, canvas.transform);
            playbackWindow = Instantiate(playbackPrefab, canvas.transform);

            if (!menuWindow.TryGetComponent(out CgmeMenuController c))
            {
                var menuController = menuWindow.AddComponent<CgmeMenuController>();
                menuWindow.AddComponent<HudOpenEffect>();
                playbackWindow.AddComponent<HudOpenEffect>();

                menuController.OnClose += () =>
                {
                    if (menuWindow.activeSelf)
                        CloseOptionsMenu();
                };
            }

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
        
        private void Pause(string stateKey, GameObject window)
        {
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

            Time.timeScale = MonoSingleton<TimeController>.Instance.timeScale *
                             MonoSingleton<TimeController>.Instance.timeScaleModifier;
            optionsManager.paused = false;
            cameraController.activated = true;
            newMovement.enabled = true;
            gunControl.activated = true;
        }

        private bool GameIsPaused() => gameStateManager.IsStateActive("pause");
    }
}