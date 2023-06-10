using UnityEngine.SceneManagement;

namespace CybergrindMusicExplorer.GUI
{
    public class GUIManager : MonoSingleton<GUIManager>
    {
        private static OptionsMenuDeployer optionsMenuDeployer;

        public static void Init()
        {
            SceneManager.sceneLoaded += OnSceneLoad;
        }

        private static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (SceneHelper.CurrentScene != "Endless")
                return;

            Prepare();
        }

        private static void Prepare()
        {
            var canvas = MonoSingleton<CanvasController>.Instance;
            optionsMenuDeployer = !canvas.TryGetComponent(out OptionsMenuDeployer deployer)
                ? canvas.gameObject.AddComponent<OptionsMenuDeployer>()
                : deployer;
        }
    }
}