using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CybergrindMusicExplorer.Scripts.UI
{
    public class DelayedInputField : MonoBehaviour
    {
        public event Action<string> ValueChanged;
        private Coroutine currentCoroutine;

        private void Awake()
        {
            gameObject.GetComponent<InputField>().onValueChanged.AddListener(value =>
            {
                if (currentCoroutine != null)
                    StopCoroutine(currentCoroutine);

                currentCoroutine = StartCoroutine(DelayedTrigger(value));
            });
        }

        private IEnumerator DelayedTrigger(string value)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            if (ValueChanged != null)
                ValueChanged.Invoke(value);
        }
    }
}