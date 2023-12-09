using System;
using UnityEngine;

namespace CybergrindMusicExplorer.Scripts.UI
{
    public class ShopButtonCallback: MonoBehaviour
    {
        public GameObject callbackObject;

        public void SetCallback(Action callback)
        {
            callbackObject = Instantiate(new GameObject(), transform);
            callbackObject.AddComponent<InternalCallback>().SetCallback(callback);
        }

        private class InternalCallback: MonoBehaviour
        {
            private Action callback;
            
            private void OnEnable()
            {
                callback?.Invoke();
                gameObject.SetActive(false);
            }

            public void SetCallback(Action action)
            {
                callback = action;
            }
        }
    }
}