using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CybergrindMusicExplorer.Scripts.UI
{
    public class TerminalBrowserConfirmationWindow: MonoBehaviour
    {
        [SerializeField] private GameObject warningMessage;
        [SerializeField] private GameObject acceptButton;
        [SerializeField] private GameObject declineButton;

        private void Awake()
        {
            declineButton.GetComponent<Button>().onClick.AddListener(() => gameObject.SetActive(false));
        }

        public void ShowWarning(string message, UnityAction action)
        {
            gameObject.SetActive(true);
            warningMessage.GetComponent<TextMeshProUGUI>().text = message;
            acceptButton.GetComponent<Button>().onClick.RemoveAllListeners();
            acceptButton.GetComponent<Button>().onClick.AddListener(action);
            acceptButton.GetComponent<Button>().onClick.AddListener(() => gameObject.SetActive(false));
        }
    }
}