# NeosPimaxEyeTracker

A [NeosModLoader](https://github.com/zkxs/NeosModLoader) mod for [Neos VR](https://neos.com/)  
Integrates the Droolon Pi1 eye tracking module. Tracks the following, per eye and combined:
- Gaze
- Eye Squeeze
- Eye Widen
- Eye Openess
- Pupil dilation (iffy)

Related issue on the Neos Github:
https://github.com/Neos-Metaverse/NeosPublic/issues/1696

## Installation
1. Install [NeosModLoader](https://github.com/zkxs/NeosModLoader) and if using a face tracker, [mixNmatch_lipsNmouth](https://github.com/dfgHiatus/mixNmatch_lipsNmouth/releases/tag/v1.0.1).
2. Place [NeosPimaxIntegration.dll](https://github.com/dfgHiatus/NeosPimaxEyeTracker/releases/tag/v1.0.0b) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
3. Place [PimaxEyeTracker.dll](https://github.com/dfgHiatus/NeosPimaxEyeTracker/releases/tag/v1.0.0) into the same folder as Neos.exe. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR` for a default install. Additionally you could get the directory through Steam's "Manage" -> "Browse local files".
4. Start the game!

If you want to verify that the mod is working you can check your Neos logs, or create an EmptyObject with an AvatarRawEyeData Component (Found under Users -> Common Avatar System -> Face -> AvatarRawEyeData).

# Credits
- Utilizes: https://github.com/NGenesis/PimaxEyeTracker
- Inspired by: https://github.com/NGenesis/VRCPimaxEyeTracker

Thanks to those who helped me test this!
