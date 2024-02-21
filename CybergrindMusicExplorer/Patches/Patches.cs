using HarmonyLib;
using static CybergrindMusicExplorer.Util.Patching.PatchRequest;

namespace CybergrindMusicExplorer.Patches
{
    public static class Patches
    {
        private static readonly Harmony Harmony = new Harmony("Flazhik.ULTRAKILL.CybergrindMusicExplorer");

        public static void PatchOnStartOptionsMenu()
        {
            PatchMethod(typeof(OptionsMenuToManager), "Start")
                .WithPrefix(typeof(OptionsMenuToManagerPatch), "OptionsMenuToManager_Start_Prefix")
                .Using(Harmony)
                .Once();
        }

        public static void PatchOnEnableOptionsMenu()
        {
            PatchMethod(typeof(OptionsMenuToManager), "OpenOptions")
                .WithPrefix(typeof(OptionsMenuToManagerPatch), "OptionsMenuToManager_OnEnable_Prefix")
                .Using(Harmony)
                .Once();
        }

        public static void PatchAudioMixer()
        {
            PatchMethod(typeof(AudioMixerController), "SetMusicVolume")
                .WithPrefix(typeof(AudioMixerControllerPatch), "AudioMixerController_SetMusicVolume_Prefix")
                .Using(Harmony)
                .Once();
        }

        public static void PatchMusicPlayer()
        {
            PatchMethod(typeof(CustomMusicPlayer), "OnEnable")
                .WithPrefix(typeof(CustomMusicPlayerPatch), "CustomMusicPlayer_OnEnable_Prefix")
                .Using(Harmony)
                .Once();
        }

        public static void PatchMusicManager()
        {
            if (SceneHelper.CurrentScene != "Endless")
                return;
            
            PatchMethod(typeof(MusicManager), "StartMusic")
                .WithPostfix(typeof(MusicManagerPatch), "MusicManager_StartMusic_Postfix")
                .Using(Harmony)
                .Once();

            PatchMethod(typeof(MusicManager), "PlayBattleMusic")
                .WithPrefix(typeof(MusicManagerPatch), "MusicManager_PlayBattleMusic_Prefix")
                .WithTranspiler(typeof(MusicManagerPatch), "MusicManager_PlayBattleMusic_Transpiler")
                .Using(Harmony)
                .Once();

            PatchMethod(typeof(MusicManager), "PlayCleanMusic")
                .WithPrefix(typeof(MusicManagerPatch), "MusicManager_PlayCleanMusic_Prefix")
                .WithTranspiler(typeof(MusicManagerPatch), "MusicManager_PlayCleanMusic_Transpiler")
                .Using(Harmony)
                .Once();
        }

        public static void PatchMusicChanger()
        {
            if (SceneHelper.CurrentScene != "Endless")
                return;
            
            PatchMethod(typeof(MusicChanger), "Change")
                .WithTranspiler(typeof(MusicChangerPatch), "MusicChanger_Change_Transpiler")
                .Using(Harmony)
                .Once();
        }

        public static void PatchFinalCyberRank()
        {
            PatchMethod(typeof(FinalCyberRank), "GameOver")
                .WithPrefix(typeof(FinalCyberRankPatch), "FinalCyberRank_GameOver_Prefix")
                .Using(Harmony)
                .Once();
        }

        public static void PatchCustomMusicPlaylistEditor()
        {
            PatchMethod(typeof(CustomMusicPlaylistEditor), "GetSongMetadataFromFilepath")
                .WithPrefix(typeof(CustomMusicPlaylistEditorPatch), "CustomMusicPlaylistEditor_GetSongMetadataFromFilepath_Prefix")
                .Using(Harmony)
                .Once();
            
            PatchMethod(typeof(CustomMusicPlaylistEditor), "LoadPlaylist")
                .WithPrefix(typeof(CustomMusicPlaylistEditorPatch), "CustomMusicPlaylistEditor_LoadPlaylist_Prefix")
                .Using(Harmony)
                .Once();
            
            PatchMethod(typeof(CustomMusicPlaylistEditor), "Select")
                .WithPostfix(typeof(CustomMusicPlaylistEditorPatch), "CustomMusicPlaylistEditor_Select_Postfix")
                .Using(Harmony)
                .Once();
        }        
        
        public static void PatchPlaylist()
        {
            PatchMethod(typeof(Playlist), "Add")
                .WithPrefix(typeof(PlaylistPatch), "Playlist_Add_Prefix")
                .Using(Harmony)
                .Once();
            
            PatchMethod(typeof(Playlist), "get_currentPath")
                .WithPrefix(typeof(PlaylistPatch), "Playlist_get_currentPath_Prefix")
                .Using(Harmony)
                .Once();
        }
        
        public static void PatchDirectoryTree()
        {
            PatchMethod(typeof(FileDirectoryTree), "GetFilesRecursive")
                .WithPrefix(typeof(FileDirectoryTreePatch), "FileDirectoryTree_GetFilesRecursive_Prefix")
                .Using(Harmony)
                .Once();
            
            PatchMethod(typeof(FileDirectoryTree), "Refresh")
                .WithPrefix(typeof(FileDirectoryTreePatch), "FileDirectoryTree_Refresh_Prefix")
                .Using(Harmony)
                .Once();
        }

        public static void PatchWaveMenu()
        {
            PatchMethod(typeof(WaveMenu), "Start")
                .WithPostfix(typeof(WaveMenuPatch), "WaveMenu_Start_Postfix")
                .Using(Harmony)
                .Once();
        }
        
        public static void PatchScreenZone()
        {
            PatchMethod(typeof(ScreenZone), "Update")
                .WithPrefix(typeof(ScreenZonePatch), "ScreenZone_Update_Prefix")
                .Using(Harmony)
                .Once();
        }
    }
}