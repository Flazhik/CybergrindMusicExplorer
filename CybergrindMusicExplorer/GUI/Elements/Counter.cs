using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using TMPro;
using UnityEngine;

namespace CybergrindMusicExplorer.GUI.Elements
{
    [UICustomElement("CGMECounter")]
    public class Counter : MonoBehaviour
    {
        public int Value { get; private set; }
        private int tmpValue;
        public event Action<int> OnChanged;

        public TextMeshProUGUI textValue;
        public CounterButton increase;
        public CounterButton decrease;

        private IEnumerator changeValueRoutine;

        public void Init(CounterButton increaseButton, CounterButton decreaseButton)
        {
            increase = increaseButton;
            decrease = decreaseButton;
            
            AddEvents(increase, v => tmpValue = ChangeValue(i => i + 1));
            AddEvents(decrease, v => tmpValue = ChangeValue(i => i - 1));
        }

        public void SetDefaultValue(int defaultValue)
        {
            Value = defaultValue;
            tmpValue = defaultValue;
            textValue.text = defaultValue.ToString();
        }

        private void AddEvents(CounterButton button, Action<int> callback)
        {
            button.OnDown += () =>
            {
                changeValueRoutine = ChangeValueRoutine(callback);
                StartCoroutine(changeValueRoutine);
            };
            button.OnUp += SaveValue;
            button.OnExit += SaveValue;
        }

        private void SaveValue()
        {
            if (changeValueRoutine != null)
                StopCoroutine(changeValueRoutine);
            
            Value = tmpValue;
            textValue.text = Value.ToString();
            OnChanged?.Invoke(Value);
        }

        [SuppressMessage("ReSharper", "IteratorNeverReturns")]
        private IEnumerator ChangeValueRoutine(Action<int> callback)
        {
            callback.Invoke(tmpValue);
            yield return new WaitForSecondsRealtime(0.3f);

            while (true)
            {
                callback.Invoke(tmpValue);
                yield return new WaitForSecondsRealtime(0.1f);
            }
        }

        private int ChangeValue(Func<int, int> operation)
        {
            var result = ValidateOperation(operation);
            textValue.text = result.ToString();
            return result;
        }

        private int ValidateOperation(Func<int, int> operation)
        {
            var result = operation.Invoke(tmpValue);
            if (result < 0 || result > 99)
                return tmpValue;

            return result;
        }
    }
}