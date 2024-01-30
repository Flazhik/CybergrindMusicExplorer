using UnityEngine;
using UnityEngine.UI;

namespace CybergrindMusicExplorer.Scripts.UI.Manual
{
    public class ManualSectionButton: MonoBehaviour
    {
        [SerializeField] private GameObject contents;
        [SerializeField] private GameObject contentsSections;
        [SerializeField] private GameObject viewport;
        [SerializeField] private ScrollRect scrollArea;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() =>
            {
                contentsSections.SetActive(true);
                transform.parent.gameObject.SetActive(false);
                foreach (Transform child in viewport.transform)
                    Destroy(child.gameObject);
                var help = Instantiate(contents, viewport.transform);
                scrollArea.content = help.GetComponent<RectTransform>();
                contents.SetActive(true);
            });
        }
    }
}