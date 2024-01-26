using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CybergrindMusicExplorer.Scripts.UI
{
    public delegate void EndSliderDragEvent(float val);
    
    [RequireComponent(typeof(Slider))]
    public class SliderAndValue : MonoBehaviour, IPointerUpHandler 
    {
        public event EndSliderDragEvent EndDrag;
        public TextMeshProUGUI value;
        public Slider.SliderEvent OnValueChanged => gameObject.GetComponent<Slider>().onValueChanged;
        public float Value
        {
            set => gameObject.GetComponent<Slider>().value = value;
        }
        private float SliderValue => gameObject.GetComponent<Slider>().value;

        public void OnPointerUp(PointerEventData data)
        {
            if (EndDrag != null) 
                EndDrag(SliderValue);
        }
    }
}