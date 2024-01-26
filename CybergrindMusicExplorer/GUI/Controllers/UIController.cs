using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CybergrindMusicExplorer.GUI.Attributes;
using CybergrindMusicExplorer.GUI.Elements;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static CybergrindMusicExplorer.Util.KeyUtils;
using Object = UnityEngine.Object;

namespace CybergrindMusicExplorer.GUI.Controllers
{
    public abstract class UIController : MonoBehaviour
    {
        private const BindingFlags Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        
        private static readonly Color NormalColor = new Color32(0x22, 0x22, 0x22, 0xFF);
        private static readonly Color SelectedColor = new Color32(0xEB, 0x5F, 0x00, 0xFF);

        private static readonly Dictionary<Type, Action<Transform, UIController>> CustomComponentInjectors =
            new Dictionary<Type, Action<Transform, UIController>>
        {
            {
                typeof(ControlBinding), (transform, controller) => {
                    var binding = transform.gameObject.AddComponent<ControlBinding>();
                    binding.button = transform.GetComponent<Button>();
                    binding.value = transform.Find("Text").GetComponent<TextMeshProUGUI>();
                }
            },
            {
                typeof(Counter), (transform, controller) => {
                    var counter = transform.gameObject.AddComponent<Counter>();
                    counter.textValue = transform.Find("Value").GetComponent<TextMeshProUGUI>();
                    counter.Init(transform.Find("Increase").gameObject.AddComponent<CounterButton>(),
                        transform.Find("Decrease").gameObject.AddComponent<CounterButton>());
                }
            }
        };
        
        protected readonly CybergrindMusicExplorerManager Manager = CybergrindMusicExplorerManager.Instance;
        private GameObject currentKey;

        protected void Awake()
        {
            CheckType(GetType());
            BindCustomControllers(gameObject);
        }

        private void CheckType(IReflect type)
        {
            type.GetFields(Flags)
                .ToList()
                .ForEach(ProcessField);
        }
        
        protected void BindCustomControllers(GameObject go)
        {
            var customElementTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.GetCustomAttribute<UICustomElement>() != null)
                .ToDictionary(type => type, type => type.GetCustomAttribute<UICustomElement>());

            foreach (var child in go.transform.GetComponentsInChildren<Transform>(true))
            {
                customElementTypes
                    .Where(type => child.name.StartsWith(type.Value.GameObjectName))
                    .ToList()
                    .ForEach(type =>
                    {
                        if (CustomComponentInjectors.TryGetValue(type.Key, out var injector))
                            injector.Invoke(child, this);
                    });
            }
        }

        private void ProcessField(FieldInfo field)
        {
            if (field.FieldType.IsArray
                || !field.FieldType.IsSubclassOf(typeof(Object))
                || field.IsStatic)
                return;

            var uiTag = field.GetCustomAttribute<UIElement>();
            if (uiTag == null)
                return;
            
            PopulateUiElement(uiTag, field);
        }
        
        private void PopulateUiElement(UIElement uiTag, FieldInfo field)
        {
            var t = gameObject.transform.Find(uiTag.Path);
            var type = field.FieldType;

            if (field.FieldType.IsSubclassOf(typeof(MonoBehaviour)))
            {
                if (CustomComponentInjectors.TryGetValue(field.FieldType, out var injector))
                    injector.Invoke(t, this);
                field.SetValue(this, t.GetComponent(type));
            } else if (field.FieldType.IsAssignableFrom(typeof(GameObject)))
            {
                field.SetValue(this, t.gameObject);
            }

            if (field.GetCustomAttribute<HudEffect>() != null)
                t.gameObject.AddComponent<HudOpenEffect>();
            
            var componentTag = field.GetCustomAttribute<CustomComponent>();
            if (componentTag != null)
                t.gameObject.AddComponent(componentTag.ComponentType);
        }

        private void OnGUI()
        {
            if (currentKey == null)
                return;

            var current = Event.current;
            if (current.keyCode == KeyCode.Escape
                || Input.GetKey(KeyCode.Mouse0)
                || (currentKey.name != "CGMEMenu" && current.keyCode == (KeyCode)Manager.MenuBinding))
            {
                currentKey.GetComponent<Image>().color = NormalColor;
                currentKey = null;
            }

            if (current.isKey || current.isMouse || current.button > 2 || current.shift)
            {
                KeyCode keyCode;
                if (current.isKey)
                    keyCode = current.keyCode;
                else if (Input.GetKey(KeyCode.LeftShift))
                    keyCode = KeyCode.LeftShift;
                else if (Input.GetKey(KeyCode.RightShift))
                    keyCode = KeyCode.RightShift;
                else if (current.button <= 6)
                {
                    keyCode = (KeyCode)(323 + current.button);
                }
                else
                {
                    currentKey.GetComponent<Image>().color = NormalColor;
                    currentKey = null;
                    return;
                }
                
                currentKey.GetComponentInChildren<TextMeshProUGUI>().text = ToHumanReadable(keyCode);
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.SetIntLocal(
                    "cyberGrind.musicExplorer.keyBinding." + currentKey.name, (int)keyCode);
                currentKey.GetComponent<Image>().color = NormalColor;
                currentKey = null;
            }
            else if (Input.GetKey(KeyCode.Mouse3) || Input.GetKey(KeyCode.Mouse4) || Input.GetKey(KeyCode.Mouse5) ||
                     Input.GetKey(KeyCode.Mouse6))
            {
                var keyCode = KeyCode.Mouse3;
                if (Input.GetKey(KeyCode.Mouse4))
                    keyCode = KeyCode.Mouse4;
                else if (Input.GetKey(KeyCode.Mouse5))
                    keyCode = KeyCode.Mouse5;
                else if (Input.GetKey(KeyCode.Mouse6))
                    keyCode = KeyCode.Mouse6;

                Manager.SetIntLocal("cyberGrind.musicExplorer.keyBinding." + currentKey.name, (int)keyCode);
                currentKey.GetComponentInChildren<TextMeshProUGUI>().text = ToHumanReadable(keyCode);
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.SetIntLocal(
                    "cyberGrind.musicExplorer.keyBinding." + currentKey.name, (int)keyCode);

                currentKey.GetComponent<Image>().color = NormalColor;
                currentKey = null;
            }
            else
            {
                if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
                    return;
                var keyCode = KeyCode.LeftShift;
                if (Input.GetKey(KeyCode.RightShift))
                    keyCode = KeyCode.RightShift;
                currentKey.GetComponentInChildren<TextMeshProUGUI>().text = ToHumanReadable(keyCode);
                MonoSingleton<CybergrindMusicExplorerManager>.Instance.SetIntLocal(
                    "cyberGrind.musicExplorer.keyBinding." + currentKey.name, (int)keyCode);

                currentKey.GetComponent<Image>().color = NormalColor;
                currentKey = null;
            }
        }
        
        public void ChangeKey(GameObject stuff)
        {
            currentKey = stuff;
            stuff.GetComponent<Image>().color = SelectedColor;
        }
    }
}