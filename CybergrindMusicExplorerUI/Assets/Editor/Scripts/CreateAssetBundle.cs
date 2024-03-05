using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
public class CreateAssetBundle : MonoBehaviour
{
    [MenuItem("AssetsBundle/Build AssetBundles")]
    static void BuildAllAssetBundles () {
        string dir = "C:\\Users\\User\\Documents\\GitHub\\CybergrindMusicExplorer\\CybergrindMusicExplorer\\Resources";
        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.UncompressedAssetBundle,BuildTarget.StandaloneWindows64);
    }
}
