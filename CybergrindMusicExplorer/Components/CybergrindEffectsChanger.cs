using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CybergrindMusicExplorer.Components.TracksLoader;
using static CybergrindMusicExplorer.Util.PathsUtils;

namespace CybergrindMusicExplorer.Components
{
    public class CybergrindEffectsChanger: MonoSingleton<CybergrindEffectsChanger>
    {
        private static bool _initialized;

        public void Start()
        {
            if (!_initialized)
            {
                SceneManager.sceneLoaded += OnSceneLoad;
                CreateEffectsFolder();
            }

            _initialized = true;
        }

        private static void OnSceneLoad(Scene scene, LoadSceneMode mode)
        {
            if (SceneHelper.CurrentScene != "Endless")
                return;

            Prepare(scene);
        }

        private static void CreateEffectsFolder()
        {
            if (!SpecialEffectsDirectory.Exists)
                SpecialEffectsDirectory.Create();
        }

        private static void Prepare(Scene scene)
        {
            Instance.StartCoroutine(Load(scene));
        }
        
        private static IEnumerator Load(Scene scene)
        {
            var crowdReaction = CrowdReactions.Instance;
            yield return OverrideEndTrack(scene);
            yield return OverrideCheer(crowdReaction);
            yield return OverrideLongerCheer(crowdReaction);
            yield return OverrideAww(crowdReaction);
            yield return null;
        }

        private static IEnumerator OverrideEndTrack(Scene scene)
        {
            var file = SpecialTrack("end.mp3");
            if (!file.Exists)
                yield break;

            AudioClip end = null;
            yield return LoadTrack(file, data => end = data);
            
            var endMusic = scene.GetRootGameObjects().First(o => o.name == "EndMusic");
            endMusic.GetComponent<AudioSource>().clip = end;
            yield return null;
        }
        
        private static IEnumerator OverrideCheer(CrowdReactions reactions)
        {
            var file = SpecialTrack("cheer.mp3");
            if (!file.Exists)
                yield break;

            yield return LoadTrack(file, cheer => reactions.cheer = cheer);
            yield return null;
        }
        
        private static IEnumerator OverrideLongerCheer(CrowdReactions reactions)
        {
            var file = SpecialTrack("cheer_long.mp3");
            if (!file.Exists)
                yield break;

            yield return LoadTrack(file, cheerLong => reactions.cheerLong = cheerLong);
            yield return null;
        }
        
        private static IEnumerator OverrideAww(CrowdReactions reactions)
        {
            var file = SpecialTrack("aww.mp3");
            if (!file.Exists)
                yield break;
            
            yield return LoadTrack(file, aww => reactions.aww = aww);
            yield return null;
        }

        private static IEnumerator LoadTrack(FileInfo file, Action<AudioClip> callback)
        {
            yield return LoadCustomSong(file.FullName, AudioType.MPEG, false, callback);
        }

        private static FileInfo SpecialTrack(string fileName)
        {
            return new FileInfo(Path.Combine(SpecialEffectsDirectory.FullName, fileName));
        }
    }
}