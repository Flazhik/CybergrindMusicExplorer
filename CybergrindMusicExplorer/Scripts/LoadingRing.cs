using UnityEngine;

namespace CybergrindMusicExplorer.Scripts
{
    public class LoadingRing : MonoBehaviour
    {
        private void Update()
        {
            transform.Rotate(0, 0, -60 * Time.fixedUnscaledDeltaTime);
        }
    }
}
