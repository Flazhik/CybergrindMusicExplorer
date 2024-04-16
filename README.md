![Version](https://img.shields.io/github/v/release/Flazhik/CybergrindMusicExplorer)
![Licence](https://img.shields.io/github/license/Flazhik/CybergrindMusicExplorer)

# Cyber Grind Music Explorer

### Enhanced music browser and player for Cyber Grind

![CGME](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/63593445-8bea-43b5-87eb-1cd7e00ed527)

## Disclaimer
Formerly the main focus of this mod was to allow to play custom music in Cyber Grind, but since this part is official now CGME will be developing as a QoL mod and an enhancement of existing music player.
#### _This mod doesn't affect leaderboards and doesn't alter your gameplay: your score will be saved_

- [Installation](#installation)
- [Basics](#basics)
- [Mod settings](#settings)
- Features
  - [Playback menu](#playback-menu)
  - [Downloader](#downloader)
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
   archive [here](https://github.com/Flazhik/CybergrindMusicExplorer/releases/download/v1.6.3/CybergrindMusicExplorer.v1.6.3.zip), then
   extract its contents at **ULTRAKILL/BepInEx/plugins** (create *plugins* folder manually in case it's missing)

You can also use r2modman for that. Both methods are described in the video below (click to open):
[![Watch the video](https://img.youtube.com/vi/oCmJJdCK8IE/hqdefault.jpg)](https://youtu.be/oCmJJdCK8IE)

## Basics

All the basic functionality is available by pressing **F4** while in Cyber Grind and ~ while in arena.

In case you forgot your CGME menu hotkey, open **Audio** section of ULTRAKILL options to check it.

## Settings
![CGME menu](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/999991ff-991d-427f-b2f4-6c038f0fa7fc)

All the settings for the mod are available by pressing CGME Menu hotkey (F4 by default). The main screen of the menu contains the following options:

**Playback**

1. **Show current track panel indefinitely**: once checked, this option will make the "Now playing" panel to be present the whole time the track is playing
2. **Volume boost for custom tracks**: if your track is still too quiet, you can always add up to 10dB to it. Please use it carefully
3. **Show current track alongside wave number and enemy counter**: Display "Now playing" section on a big panel next to wave number and enemy counter
4. **Display subtitles**: enable subtitles if present. Yes, the mod supports it, but more on that later

**Playlist editing**

1. **Enable tracks preview**: allows to listen to 5 seconds preview of a track by clicking on it in the terminal
2. **Prevent duplicate tracks**: prevents tracks duplication. Once checked, deletes existing duplicates
3. **Add downloaded tracks to playlist automatically**: all the tracks that are obtained via [Downloader](#downloader) will be added to playlist automatically

**Other**

**Menu upscale**: you can make both main and playback menus a little bit bigger

For other settings like key bindings and Themes, use the respective tab. 

## Playback menu
![Playback menu](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/b4b789ab-6c50-4b6d-97d2-8c7eb62acc33)

Allows you to switch between the tracks mid-game. Available by pressing **~** by default.

If the whole menu is an overkill for you, just switch between tracks using **Next track** hotkey.

## Downloader
![Downloader window](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/3ecbfbce-570e-4079-8baa-126dad5da171)
![YouCloud](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/439727a4-8220-4cac-ba8f-4b772fbc951c)

You can now download tracks directly from **YouTube** and **SoundCloud**.

Downloader section supports both playlists and individual tracks URLs. All you have to do is to paste track/playlist URL in the input field and download each track individually or download every track of playlist at once.
Downloaded tracks will be placed inside **_ULTRAKILL/Cybergrind/Music_** directory in _**YouTube**_ and _**SoundCloud**_ folders.

**Note that you'll have to restart CyberGrind in order for tracks to appear in Terminal!**

### **N.B.!**

This feature uses ffmpeg. It's a 3rd-party library which you'll be offered to download automatically from the official source. You're at liberty not to do it, but in this case Downloader functionality won't be available to you.
ffmpeg executables will be placed within the folder where **CybergrindMusicExplorer.dll** is located.

### What if for some reason you can't install ffmpeg by means of CGME?
You can perform it manually.

1. Download ffmpeg archive manually from [here](https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip)
2. Open the folder `ffmpeg-master-latest-win64-gpl\bin` inside it
3. Extract `ffmpeg.exe` and `ffprobe.exe` into the same folder where `CybergrindMusicExplorer.dll` is located.
   If you've installed the mod manually, it's `ULTRAKILL\BepInEx\plugins\CybergrindMusicExplorer`. If you're using r2modman the path is a little bit trickier: `C:\Users\{YourUsername}\AppData\Roaming\r2modmanPlus-local\ULTRAKILL\profiles\{ProfileName}\BepInEx\plugins\Flazhik-CybergrindMusicExplorer\CybergrindMusicExplorer`

Please also note that this feature is in early stage of development and may not work consistently.

**Known issues:**
- If you faced an issue where track is seemingly missing at the provided URL when it shouldn't be, refresh the list by pasting URL once again or restart CyberGrind
- Some videos e.g. the ones that contain self harm topics may not be available for downloading
- **Download all** process may stall, in this case please download the rest of the tracks manually or restart CyberGrind and try again
- Only 100 first tracks are available if your trying to download YouTube playlist

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
- **menu.mp3**: Music in Terminal
- **aww.mp3**: The sound of utter disappointment when you died
- **end.mp3**: Results screen music

![Effects replacement](https://github.com/Flazhik/CybergrindMusicExplorer/assets/2077991/c73a5fb5-86bd-4085-a43e-194b64f0859e)

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
- [Xabe.FFmpeg](https://github.com/tomaszzmuda/Xabe.FFmpeg) - licensed under [CC BY-NC-SA 3.0](https://ffmpeg.xabe.net/license.html) for non-commercial use

#### Extras:
For SoundCloud support, client_id was borrowed from [SoundCloudExplode](https://github.com/jerry08/SoundCloudExplode) by [jerry08](https://github.com/jerry08), licensed under [MIT License](https://github.com/jerry08/SoundCloudExplode/blob/master/LICENSE.txt)
