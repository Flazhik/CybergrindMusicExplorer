using System.Collections;
using UnityEngine;
using static CybergrindMusicExplorer.Util.ReflectionUtils;

namespace CybergrindMusicExplorer.Components
{
    public class ScreenZoneHelper: MonoBehaviour
    {
        public bool inZone;
        
        private Coroutine currentRoutine;
        private ScreenZone screenZone;
        private AudioSource terminalMusic;
        private bool stop;
        private AudioSource preview;

        private void Awake()
        {
            screenZone = GetComponent<ScreenZone>();
            terminalMusic = (AudioSource)GetPrivate(screenZone, typeof(ScreenZone), "music");
            inZone = (bool)GetPrivate(screenZone, typeof(ScreenZone), "inZone");
            preview = gameObject.AddComponent<AudioSource>();
        }

        private void Update()
        {
            if (preview.clip == null)
                return;
            
            if (inZone)
            {
                if (preview.pitch < 1.0)
                    preview.pitch = Mathf.MoveTowards(preview.pitch, 1f, Time.deltaTime);

                if (!stop)
                    return;
                
                if (preview.volume == 0f)
                {
                    preview.Stop();
                    preview.clip = null;
                    terminalMusic.Play();
                }
                else
                    preview.volume = Mathf.MoveTowards(preview.volume, 0.0f, Time.deltaTime);
            }
            else if (preview.pitch > 0.0)
                preview.pitch = Mathf.MoveTowards(preview.pitch, 0.0f, Time.deltaTime);
        }

        public void PlayPreview(AudioClip clip)
        {
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);

            currentRoutine = StartCoroutine(PreviewCoroutine(clip));
        }

        private IEnumerator PreviewCoroutine(AudioClip clip)
        {
            stop = false;
            terminalMusic.Pause();
            preview.clip = clip;
            preview.volume = 1;
            preview.Play();
            yield return new WaitForSeconds(5);
            stop = true;
        }
    }
}