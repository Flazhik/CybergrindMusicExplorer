using UnityEngine;

namespace CybergrindMusicExplorer
{
    public class CybergrindMusicExplorerManager: MonoSingleton<CybergrindMusicExplorerManager>
    {
        private readonly PrefsManager prefsManager = MonoSingleton<PrefsManager>.Instance;

        public bool NormalizeSoundtrack
        {
            get => prefsManager.GetBoolLocal("cyberGrind.musicExplorer.normalizeAudio", true);
            set => prefsManager.SetBoolLocal("cyberGrind.musicExplorer.normalizeAudio", value);
        }

        public bool ShowCurrentTrackPanelIndefinitely {
            get => prefsManager.GetBoolLocal("cyberGrind.musicExplorer.showCurrentTrackPanelIndefinitely");
            set => prefsManager.SetBoolLocal("cyberGrind.musicExplorer.showCurrentTrackPanelIndefinitely", value);
        }
    }
}