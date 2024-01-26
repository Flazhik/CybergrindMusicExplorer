using TMPro;
using UnityEngine;

namespace CybergrindMusicExplorer.Scripts.UI
{
    public class DownloadStatusBar : MonoBehaviour
    {
        [SerializeField]
        private GameObject progressBarGo;
        
        [SerializeField]
        public GameObject downloadButton;        
        
        [SerializeField]
        public GameObject retryButton;
        
        [SerializeField]
        public GameObject statusLabel;

        public CgmeProgressBar progressBar;
        
        private TextMeshProUGUI statusLabelText;

        private void Start()
        {
            progressBar = progressBarGo.GetComponent<CgmeProgressBar>();
            statusLabelText = statusLabel.GetComponent<TextMeshProUGUI>();
        }

        public void Downloading()
        {
            downloadButton.SetActive(false);
            progressBarGo.SetActive(true);
            retryButton.SetActive(false);
            statusLabelText.gameObject.SetActive(false);
        }
        
        public void Enqueued()
        {
            downloadButton.SetActive(false);
            progressBarGo.SetActive(false);
            statusLabel.SetActive(true);
            statusLabelText.text = "ENQUEUED";
            statusLabelText.color = Color.yellow;
        }
        
        public void Processing()
        {
            downloadButton.SetActive(false);
            progressBarGo.SetActive(false);
            statusLabel.SetActive(true);
            statusLabelText.text = "PROCESSING";
            statusLabelText.color = Color.yellow;
        }

        public void Downloaded()
        {
            downloadButton.SetActive(false);
            progressBarGo.SetActive(false);
            statusLabel.SetActive(true);
            statusLabelText.text = "DOWNLOADED";
            statusLabelText.color = Color.green;
        }
        
        public void Failed()
        {
            downloadButton.SetActive(false);
            progressBarGo.SetActive(false);
            retryButton.SetActive(true);
            statusLabelText.gameObject.SetActive(false);
        }
    }
}