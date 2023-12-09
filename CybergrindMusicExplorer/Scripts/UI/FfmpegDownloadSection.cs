using System;
using CybergrindMusicExplorer.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace CybergrindMusicExplorer.Scripts.UI
{
    public class FfmpegDownloadSection : MonoBehaviour
    {
        [SerializeField] public GameObject menu;
        [SerializeField] public GameObject warning;
        
        [SerializeField] public GameObject downloadButton;
        [SerializeField] public GameObject progressBar;
        [SerializeField] public GameObject progressText;

        private void Awake()
        {
            CheckFfmpegPresence();
            downloadButton.GetComponent<Button>().onClick.AddListener(() => Ffmpeg.InstallFfmpeg(this));
        }

        public void StartDownloading()
        {
            downloadButton.SetActive(false);
            progressBar.SetActive(true);
            progressText.SetActive(true);
            ChangeStatusMessage("Downloading...");
        }

        public void Unzipping()
        {
            progressBar.SetActive(false);
            ChangeStatusMessage("Unzipping...");
        }

        public void DownloadFailed()
        {
            progressBar.SetActive(false);
            ChangeStatusMessage("Download failed! Try again later.");
        }

        public void DownloadingStatusUpdate(int percentage) => progressBar.GetComponent<CgmeProgressBar>().UpdateValue(percentage);
        
        public void CheckFfmpegPresence()
        {
            if (Ffmpeg.FfmpegInstalled())
            {
                menu.SetActive(true);
                warning.SetActive(false);
                return;
            }
            
            menu.SetActive(false);
            warning.SetActive(true);
        }

        private void ChangeStatusMessage(string msg) => progressText.GetComponent<Text>().text = msg;
        
    }
}