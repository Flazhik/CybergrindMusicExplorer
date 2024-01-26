using TMPro;
using UnityEngine;

namespace CybergrindMusicExplorer.Scripts.UI
{
    public class Paginator: MonoBehaviour
    {
        [SerializeField] public GameObject previousButton;
        [SerializeField] public GameObject nextButton;
        [SerializeField] private GameObject pageText;

        public void Refresh(int currentPage, int pagesCount) =>
            pageText.GetComponent<TextMeshProUGUI>().text = $"{currentPage}/{pagesCount}";
    }
}