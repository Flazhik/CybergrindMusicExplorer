using System.Collections;
using CybergrindMusicExplorer.GUI.Attributes;
using UnityEngine;

namespace CybergrindMusicExplorer.GUI.Controllers
{
    public class TerminalBrowserController : UIController
    {
        [UIElement("RemoveButton")] [HudEffect] private GameObject removeButton;
        [UIElement("ConfirmationWindow")] [HudEffect] private GameObject confirmationWindow;
        [UIElement("ConfirmationWindow/Border/Accept")] private GameObject acceptButton;
        [UIElement("ConfirmationWindow/Border/Reject")] private GameObject rejectButton;

        private new void Awake()
        {
            base.Awake();
            StartCoroutine(Startup());
        }

        private IEnumerator Startup()
        {
            BindControls();
            yield break;
        }

        private void BindControls()
        {
            AddShopButton(acceptButton);
            AddShopButton(rejectButton);
            AddShopButton(removeButton);
        }

        private static void AddShopButton(GameObject go)
        {
            go.AddComponent<ShopButton>().deactivated = true;
        }
    }
}