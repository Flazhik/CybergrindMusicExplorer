![Version](https://img.shields.io/github/v/release/Flazhik/CybergrindMusicExplorer)
![Licence](https://img.shields.io/github/license/Flazhik/CybergrindMusicExplorer)

# Custom Cyber Grind Music Explorer

### Add your custom tracks in Cyber Grind playlist and have a complete control over the playback.

![CGME](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/63593445-8bea-43b5-87eb-1cd7e00ed527)

## Disclaimer

Custom music in Cyber Grind playlist is an upcoming feature of ULTRAKILL, so you can use this mod until custom tracks are officially supported.
#### It doesn't interfere with leaderboards and doesn't alter your gameplay in any way apart from custom music.
This mod does not affect the folder with original soundtrack, so you can combine it with your custom tracks.

- [Installation](#installation)
- [How to use](#using-the-mod)
- [Mod settings](#settings)
- Additional Features
  - [Playback menu](#playback-menu)
  - [Calm & Battle themes](#calm-and-battle-themes)
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
   archive [here](https://github.com/Flazhik/CybergrindMusicExplorer/releases/download/v1.5.0/CybergrindMusicExplorer.v1.5.0.zip), then
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
![CGME menu](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/a5befff1-6d7f-4ed7-a768-468cb5dc4e7d)

All the settings for the mod are available by pressing CGME Menu hotkey (F4 by default). The main screen of the menu contains the following options:
1. **Show current track panel indefinitely**: once checked, this option will make the "Now playing" panel to be present the whole time the track is playing.
2. **Display subtitles**: enable subtitles if present. Yes, the mod supports it, but more on that later.
3. **Volume boost for custom tracks**: if your track is still too quiet, you can always add up to 10dB to it. Please use it carefully.
4. **Prevent duplicate tracks**: prevents tracks duplication. Once checked, deletes existing duplicates.

For other settings like key bindings and Themes, use the respective tab. 

## Playback menu
![Playback menu](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/b4b789ab-6c50-4b6d-97d2-8c7eb62acc33)

Allows you to switch between the tracks mid-game. Available by pressing **Tab** by default.

If the whole menu is an overkill for you, just switch between tracks using **Next track** hotkey.

## Calm and battle themes

You know how in original ULTRAKILL campaign there's a light, calm theme playing when there are no enemies around, followed by an intense variation once the battle has started?
Now you can enable it in Cyber Grind for original ULTRAKILL soundtrack and create your own tracks.


But unlike the campaign mode, you can decide when to play the calm theme by yourself (otherwise it would've been stupid since enemies are almost always present in Cyber Grind).
Go to <b>Themes</b> tab and you'll see three controls:


1. **Play calm theme**: disable if you're not interested in this feature
2. **Enemies threshold for calm theme to play:** the battle theme will play unless there's less or equal enemies currently alive
3. **Play battle theme if more that N of these enemies are alive:** enemies threshold may be a reliable way to control when the calm theme is playing, but what if, say, these two enemies who's left are Mindflayer and Insurrectionist? Is it really a good occasion to switch to the calm theme?
   That's why you can change a threshold for each enemy individually or disable it.

![Example of the calm theme settings](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/f2494922-9664-4aa9-b2fe-b55f9ca759eb)

_"Play calm theme when there's 4 or less enemies around. Unless there's 1 or more Insurrectionists or >= 2 Mindflayers"_

Alright, but what about custom tracks?

Well, to create such a track, use the same technique as for intros and loops: postfixes in your track file name.
Any track is treated as a "battle theme" by default. To add its calm variation, add another, similarly named track with **_calm** postfix. Again, these files must have the same extension!

And what about segmented tracks? They already contain two files! How do I even make a calm variation to its intro and loop? Can I add a calm theme only to loop or only to intro specifically?
Yes, you can. Customize it the way you want. Calm loop part can either have a **_calm** or **_calmloop** postfix, but intro has to end with **_calmintro**.
More on that in the Manual section of the menu.

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