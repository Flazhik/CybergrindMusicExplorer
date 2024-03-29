using TMPro;
using UnityEngine;

namespace CybergrindMusicExplorer.Scripts.UI
{
    public class CgmeProgressBar : MonoBehaviour
    {
        [SerializeField]
        private GameObject bar;

        [SerializeField] private TextMeshProUGUI textValue;
        
        private void Start()
        {
            UpdateValue(0);
        }

        public void UpdateValue(int newValue)
        {
            textValue.text = $"{newValue.ToString()}%";
            bar.transform.localScale = new Vector3((float)newValue / 100, 1, 1);
        }
    }
}