# Custom CyberGrind Music Explorer

An ULTRAKILL mod that allows you to add your own tracks to CyberGrind playlist.

![Untitled-2](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/50dfa9d9-514c-413c-95c7-2ca515ea5359)

## Disclaimer

Custom music in Cyber Grind playlist is an upcoming feature of ULTRAKILL, so you can use this mod until custom tracks
are officially supported.
This mod does not affect the folder with original soundtrack, so you can combine it with your custom tracks.

- [Installation](#installation)
- [How to use](#using-the-mod)
- Additional Features
  - [Segmented tracks](#segmented-tracks)
  - [Effects replacement](#effects-replacement)
  - [Subtitles support](#subtitles-support)
- [Credits](#additional-credits)

# Installation

1. Download the freshest **BepInEx** release from [here](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). It's
   recommended to choose **BepInEx_x64** release unless you're certain you have a 32-bit system

2. Extract the contents of **BepInEx** archive in your local **ULTRAKILL** folder. If you're not sure where this folder
   is located, find **ULTRAKILL** in your Steam Library > Right mouse click > **Properties** > **Local files** > **Browse**
3. Download the CybergrindMusicExplorer
   archive [here](https://github.com/Flazhik/CybergrindMusicExplorer/releases/download/v1.3.1/CybergrindMusicExplorer.v1.3.1.zip), then
   extract its contents at **ULTRAKILL/BepInEx/plugins** (create *plugins* folder manually in case it's missing)

# Using the mod

1. Move your custom tracks/folders to *ULTRAKILL/Cybergrind/Music*
2. Launch the game and go to the blue Cyber Grind settings terminal
3. **Music** section of the terminal should now contain two folders for original score and your own tracks
4. Fill the playlist the way you want and enjoy
5. Optional: Music Explorer settings menu is available by pressing F4 (by default)

# Segmented tracks
You can now upload your multi-segmented looped tracks with an intro and loop parts.
In order to make such a track, place your intro and loop files in the same folder and add _intro and _loop postfixes into their file names respectively.
Please note that these parts must have the same extension!

![pendulum](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/204979ac-2fe7-44b8-9c99-1892610a98b8)
Like this

If everything's been done correctly, it now should be displayed as a single track.

![single pendulum](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/1451f1d2-ca84-4520-850c-e6be6180c097)

## Effects replacement
Create a folder called CGME inside your *ULTRAKILL/Cybergrind/Music* directory (or use the one created by the mod), and you'll be able to replace some of the sound effects in Cybergrind.
This folder is ignored by Music terminal.
Please note that these files must have an .mp3 extension.

Here's the list of file names and the references to the sound effects these files are intended to replace:

- **cheer.mp3**: The sound of cheering when you perform a parry
- **cheer_long.mp3**: The sound of cheering when you finished a wave
- **aww.mp3**: The sound of utter dissapointment when you died
- **end.mp3**: Results screen music

![instr](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/3c2d8bca-ad1e-450c-8536-bf8b064ed1d2)

## Subtitles support
If a regular Cyber Grind experience is too boring for you, you can turn it into a bloody karaoke (he-he, "bloody").
Place subtitles file named exactly as your track in the same folder where this track is located. Currently, only .srt and .vtt files are supported.
The hierarchy and naming should be something like this:

```
ULTRAKILL/
├─ Cybergrind/
├─ Music/
│  ├─ Darude/
│  │  ├─ Darude - Sandstorm.mp3
│  │  ├─ Darude - Sandstorm.srt
```

Please note that if your track has into and loop segments, there must be two separate subtitles files

```

ULTRAKILL/
├─ Cybergrind/
│  ├─ Music/
│  │  ├─ Darude/
│  │  │  ├─ Darude - Sandstorm_loop.mp3
│  │  │  ├─ Darude - Sandstorm_loop.srt
│  │  │  ├─ Darude - Sandstorm_intro.mp3
│  │  │  ├─ Darude - Sandstorm_intro.srt
```

## Additional features
- Custom track boost: if your track is still too quiet, you can always add up to 10dB to it. Please use it carefully
- Next track hotkey: Skip the track you're not in the mood to listen to
- Enabling/disabling subtitles (also affects the similar setting in campaign!)
- All these feature are available in Cybergrind by pressing F4

# Additional Credits:
### Cyber Grind Music Explorer uses following libraries:

- [TagLibSharp](https://github.com/mono/taglib-sharp) - Licensed under [LGPL-2.1](https://github.com/mono/taglib-sharp/blob/main/COPYING)
- [SubtitlesParser](https://github.com/AlexPoint/SubtitlesParser) - by [AlexPoint](https://github.com/AlexPoint), licensed under [MIT License](https://github.com/AlexPoint/SubtitlesParser/blob/master/LICENSE)