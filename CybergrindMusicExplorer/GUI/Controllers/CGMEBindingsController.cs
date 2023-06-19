using UnityEngine;
using UnityEngine.UI;
using static ControlsOptions;

namespace CybergrindMusicExplorer.GUI.Controllers
{
    public class CgmeBindingsController : MonoBehaviour
    {
        private static readonly Color NormalColor = new Color32(0x22, 0x22, 0x22, 0xFF);
        private static readonly Color SelectedColor = new Color32(0xEB, 0x5F, 0x00, 0xFF);

        private CybergrindMusicExplorerManager manager;

        private GameObject currentKey;
        private Transform optionsMenu;
        private Transform nextTrack;

        public void Awake()
        {
            manager = MonoSingleton<CybergrindMusicExplorerManager>.Instance;

            nextTrack = transform
                .Find("CGMENextTrack")
                .Find("CGMENextTrack");
            nextTrack.GetComponent<Button>().onClick.AddListener(() => ChangeKey(nextTrack.gameObject));
            nextTrack.Find("Text").GetComponent<Text>().text = GetKeyName((KeyCode)manager.NextTrackBinding);

            optionsMenu = transform
                .Find("CGMEMenu")
                .Find("CGMEMenu");
            optionsMenu.GetComponent<Button>().onClick.AddListener(() => ChangeKey(optionsMenu.gameObject));
            optionsMenu.Find("Text").GetComponent<Text>().text = GetKeyName((KeyCode)manager.MenuBinding);
        }

        // That's a dirty copy, bind I'm not going to cut ControlsOptions open
        private void OnGUI()
        {
            if (currentKey == null)
                return;

            var current = Event.current;
            if (current.keyCode == KeyCode.Escape || Input.GetKey(KeyCode.Mouse0))
            {
                currentKey.GetComponent<Image>().color = NormalColor;
                currentKey = null;
            }

            if (current.isKey || current.isMouse || current.button > 2 || current.shift)
            {
                KeyCode keyCode;
                if (current.isKey)
                    keyCode = current.keyCode;
                else if (Input.GetKey(KeyCode.LeftShift))
                    keyCode = KeyCode.LeftShift;
                else if (Input.GetKey(KeyCode.RightShift))
                    keyCode = KeyCode.RightShift;
                else if (current.button <= 6)
                {
                    keyCode = (KeyCode)(323 + current.button);
                }
                else
                {
                    currentKey.GetComponent<Image>().color = NormalColor;
                    currentKey = null;
                    return;
                }

                if (manager == null)
                    manager = MonoSingleton<CybergrindMusicExplorerManager>.Instance;
                currentKey.GetComponentInChildren<Text>().text = GetKeyName(keyCode);
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.SetIntLocal(
                    "cyberGrind.musicExplorer.keyBinding." + currentKey.name, (int)keyCode);
                currentKey.GetComponent<Image>().color = NormalColor;
                currentKey = null;
            }
            else if (Input.GetKey(KeyCode.Mouse3) || Input.GetKey(KeyCode.Mouse4) || Input.GetKey(KeyCode.Mouse5) ||
                     Input.GetKey(KeyCode.Mouse6))
            {
                var keyCode = KeyCode.Mouse3;
                if (Input.GetKey(KeyCode.Mouse4))
                    keyCode = KeyCode.Mouse4;
                else if (Input.GetKey(KeyCode.Mouse5))
                    keyCode = KeyCode.Mouse5;
                else if (Input.GetKey(KeyCode.Mouse6))
                    keyCode = KeyCode.Mouse6;
                if (manager == null)
                    manager = MonoSingleton<CybergrindMusicExplorerManager>.Instance;
                manager.SetIntLocal("cyberGrind.musicExplorer.keyBinding." + currentKey.name, (int)keyCode);
                currentKey.GetComponentInChildren<Text>().text = GetKeyName(keyCode);
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.SetIntLocal(
                    "cyberGrind.musicExplorer.keyBinding." + currentKey.name, (int)keyCode);

                currentKey.GetComponent<Image>().color = NormalColor;
                currentKey = null;
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                    return;
                var keyCode = KeyCode.LeftShift;
                if (Input.GetKey(KeyCode.RightShift))
                    keyCode = KeyCode.RightShift;
                currentKey.GetComponentInChildren<Text>().text = GetKeyName(keyCode);
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.SetIntLocal(
                    "cyberGrind.musicExplorer.keyBinding." + currentKey.name, (int)keyCode);

                currentKey.GetComponent<Image>().color = NormalColor;
                currentKey = null;
            }
        }

        public void ChangeKey(GameObject stuff)
        {
            currentKey = stuff;
            stuff.GetComponent<Image>().color = SelectedColor;
        }
    }
}