# Custom CyberGrind Music Explorer

### Add your custom tracks in Cyber Grind playlist.

![CGME](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/a8008a48-7c1d-4277-a5b6-02f25a5b912e)

## Disclaimer

Custom music in Cyber Grind playlist is an upcoming feature of ULTRAKILL, so you can use this mod until custom tracks are officially supported.
#### It doesn't interfere with leaderboards and doesn't alter your gameplay in any way apart from custom music.
This mod does not affect the folder with original soundtrack, so you can combine it with your custom tracks.

- [Installation](#installation)
- [How to use](#using-the-mod)
- [Mod settings](#settings)
- Additional Features
  - [Playback menu](#playback-menu)
  - [Segmented tracks](#segmented-tracks)
  - [Effects replacement](#effects-replacement)
  - [Subtitles support](#subtitles-support)
- [Additional credits](#additional-credits)

# Installation

1. Download the **BepInEx** release from [here](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.21). It's
   recommended to choose **BepInEx_x64** release unless you're certain you have a 32-bit system
2. Extract the contents of **BepInEx** archive in your local **ULTRAKILL** folder. If you're not sure where this folder
   is located, find **ULTRAKILL** in your Steam Library > Right mouse click > **Properties** > **Local files** > **Browse**
3. Download the CybergrindMusicExplorer
   archive [here](https://github.com/Flazhik/CybergrindMusicExplorer/releases/download/v1.4.0/CybergrindMusicExplorer.v1.4.0.zip), then
   extract its contents at **ULTRAKILL/BepInEx/plugins** (create *plugins* folder manually in case it's missing)

You can also use r2modman for that. Both methods are described in the video below (click to open):

[![Watch the video](https://img.youtube.com/vi/oCmJJdCK8IE/hqdefault.jpg)](https://youtu.be/oCmJJdCK8IE)

## Using the mod

1. Move your custom tracks/folders to *ULTRAKILL/Cybergrind/Music*
2. Launch the game and go to the blue Cyber Grind settings terminal
3. **Music** section of the terminal should now contain two folders for original score and your own tracks
4. Fill the playlist the way you want and enjoy
5. Optional: Music Explorer settings menu is available by pressing F4 (by default)

## Settings
![CGME menu](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/f8a62d24-1138-4479-8cee-7421c8adef92)

All the settings for the mod are available by pressing CGME Menu hotkey (F4 by default). The menu contains the following options:
1. **Show current track panel indefinitely**: once checked, this option will make the "Now playing" panel to be present the whole time the track is playing.
2. **Display subtitles**: enable subtitles if present. Yes, the mod supports it, but more on that later.
3. **Volume boost for custom tracks**: if your track is still too quiet, you can always add up to 10dB to it. Please use it carefully.
4. **Hotkey configuration**: configures hotkeys for the main CGME menu, next track and Playback menu.

## Playback menu
![Playback menu](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/b4b789ab-6c50-4b6d-97d2-8c7eb62acc33)

Allows you to switch between the tracks mid-game. Available by pressing **Tab** by default.

If the whole menu is an overkill for you, just switch between tracks using **Next track** hotkey.

## Segmented tracks
You can also add multi-segmented looped tracks with an intro and loop parts.
In order to make such a track, place your intro and loop files in the same folder and add _intro and _loop postfixes into their file names respectively.
Please note that these parts must have the same extension!

![Looped track example](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/204979ac-2fe7-44b8-9c99-1892610a98b8)

Like this.

If everything's been done correctly, it now should be displayed as a single track.

![Looped track example numero dos](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/1451f1d2-ca84-4520-850c-e6be6180c097)

## Effects replacement
Create a folder called CGME inside your *ULTRAKILL/Cybergrind/Music* directory (or use the one created by the mod), and you'll be able to replace some of the sound effects in Cybergrind.
This folder is ignored by Music terminal.
Please note that these files must have an .mp3 extension.

Here's the list of file names and the references to the sound effects these files are intended to replace:

- **cheer.mp3**: The sound of cheering when you perform a parry
- **cheer_long.mp3**: The sound of cheering when you finished a wave
- **aww.mp3**: The sound of utter dissapointment when you died
- **end.mp3**: Results screen music

![Effects replacement](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/3c2d8bca-ad1e-450c-8536-bf8b064ed1d2)

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

## Additional Credits:

Created by [Flazhik](https://github.com/Flazhik)
#### Cyber Grind Music Explorer uses following libraries:
- [TagLibSharp](https://github.com/mono/taglib-sharp) - Licensed under [LGPL-2.1](https://github.com/mono/taglib-sharp/blob/main/COPYING)
- [SubtitlesParser](https://github.com/AlexPoint/SubtitlesParser) - by [AlexPoint](https://github.com/AlexPoint), licensed under [MIT License](https://github.com/AlexPoint/SubtitlesParser/blob/master/LICENSE)