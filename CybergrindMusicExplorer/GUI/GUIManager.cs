using UnityEngine.SceneManagement;

namespace CybergrindMusicExplorer.GUI
{
    public class GUIManager : MonoSingleton<GUIManager>
    {
        public static CgmeMenuDeployer GUIDeployer;

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
            GUIDeployer = !canvas.TryGetComponent(out CgmeMenuDeployer deployer)
                ? canvas.gameObject.AddComponent<CgmeMenuDeployer>()
                : deployer;
        }
    }
}