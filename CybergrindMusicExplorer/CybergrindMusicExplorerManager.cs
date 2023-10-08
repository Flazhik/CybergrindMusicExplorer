namespace CybergrindMusicExplorer
{
    public class CybergrindMusicExplorerManager : MonoSingleton<CybergrindMusicExplorerManager>
    {
        private readonly PrefsManager prefsManager = MonoSingleton<PrefsManager>.Instance;

        public bool allowMusicBoost;

        public bool ShowCurrentTrackPanelIndefinitely
        {
            get => prefsManager.GetBoolLocal("cyberGrind.musicExplorer.showCurrentTrackPanelIndefinitely");
            set => prefsManager.SetBoolLocal("cyberGrind.musicExplorer.showCurrentTrackPanelIndefinitely", value);
        }

        public float CustomTracksBoost
        {
            get => prefsManager.GetFloatLocal("cyberGrind.musicExplorer.customTracksVolumeBoost");
            set => prefsManager.SetFloatLocal("cyberGrind.musicExplorer.customTracksVolumeBoost", value);
        }
        
        public bool PlayCalmTheme
        {
            get => prefsManager.GetBoolLocal("cyberGrind.musicExplorer.playCalmTheme", true);
            set => prefsManager.SetBoolLocal("cyberGrind.musicExplorer.playCalmTheme", value);
        }
        
        public bool PreventDuplicateTracks
        {
            get => prefsManager.GetBoolLocal("cyberGrind.musicExplorer.preventDuplicateTracks", false);
            set => prefsManager.SetBoolLocal("cyberGrind.musicExplorer.preventDuplicateTracks", value);
        }
        
        public int CalmThemeEnemiesThreshold
        {
            get => prefsManager.GetIntLocal("cyberGrind.musicExplorer.calmThemeEnemiesThreshold", 2);
            set => prefsManager.SetIntLocal("cyberGrind.musicExplorer.calmThemeEnemiesThreshold", value);
        }

        public int NextTrackBinding =>
            prefsManager.GetIntLocal("cyberGrind.musicExplorer.keyBinding.CGMENextTrack", 284);

        public int MenuBinding => prefsManager.GetIntLocal("cyberGrind.musicExplorer.keyBinding.CGMEMenu", 285);
        
        public int PlaybackMenuBinding => prefsManager.GetIntLocal("cyberGrind.musicExplorer.keyBinding.CGMEPlaybackMenu", 9);

        public void SetIntLocal(string key, int content)
        {
            prefsManager.SetIntLocal(key, content);
        }
    }
}