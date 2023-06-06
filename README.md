# Custom CyberGrind Music Explorer

An ULTRAKILL mod that allows you to add your own tracks to CyberGrind playlist.

![Untitled-2](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/50dfa9d9-514c-413c-95c7-2ca515ea5359)

## Disclaimer

Custom music in Cyber Grind playlist is an upcoming feature of ULTRAKILL, so you can use this mod until custom tracks
are officially supported.
This mod does not affect the folder with original soundtrack, so you can combine it with your custom tracks.

### **N.B.!**

At the current stage of developement, this mod doesn't support sub-folders: all the tracks should be
placed in one directory.

## Installation

1. Download the freshest **BepInEx** release from [here](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). It's
   recommended to choose **BepInEx_x64** release unless you're certain you have a 32-bit system

2. Extract the contents of **BepInEx** archive in your local **ULTRAKILL** folder. If you're not sure where this folder
   is located, find **ULTRAKILL** in your Steam Library > Right mouse click > **Properties** > **Local files** > *
   *Browse**
3. Download the CybergrindMusicExplorer
   archive [here](https://github.com/Flazhik/CybergrindMusicExplorer/releases/download/v1.1.0/CybergrindMusicExplorer.v1.1.0.zip), then
   extract its contents at **ULTRAKILL/BepInEx/plugins** (create *plugins* folder manually in case it's missing)
   
## Additional features
The mod also supports custom track normalization: you don't have to worry your custom track is going to be too quiet or to loud.
All the currently available tweakable options are available in ULTRAKILL Audio options.

![tweak](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/3edf177c-e022-40a1-bd1b-d88d46087208)

## Using the mod

1. Move your custom tracks to *ULTRAKILL/Cybergrind/Music* (please make sure all your tracks are in this folder with no
   sub-folders)
2. Launch the game and go to the blue Cyber Grind settings terminal
3. **Music** section of the terminal should now contain two folders for original score and your own tracks
4. Fill the playlist the way you want and enjoy

## Known issues

Please note that this mod is in its early stage of developement and contains a plenty of issues. These are some I'm
aware of and will try to get rid of in the next releases:

- Lack of subfolders support: please place all your custom tracks at the *ULTRAKILL/Cybergrind/Music*, do not create
  folders there
- A significant freeze while browsing a **Custom tracks** folder at the first time since game was launched
