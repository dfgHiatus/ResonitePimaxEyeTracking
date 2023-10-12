# ResonitePimaxEyeTracker

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/)  
Integrates the Droolon Pi1 eye-tracking module. Tracks the following, per eye and combined:
- Gaze
- Eye Widen
- Eye Openness

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) and if using a face tracker, [mixNmatch_lipsNmouth](https://github.com/dfgHiatus/mixNmatch_lipsNmouth/releases/latest).
2. Place [ResonitePimaxIntegration.dll](https://github.com/dfgHiatus/ResonitePimaxEyeTracker/releases/latest) into your `rml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install. You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create the folder for you.
3. Place [PimaxEyeTracker.dll](https://github.com/dfgHiatus/ResonitePimaxEyeTracker/releases/latest) into the same folder as Resonite.exe. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite` for a default install. Additionally, you could get the directory through Steam's "Manage" -> "Browse local files".
4. Start the game!

If you want to verify that the mod is working you can check your Resonite logs, or create an EmptyObject with an AvatarRawEyeData Component (Found under Users -> Common Avatar System -> Face -> AvatarRawEyeData).

# Credits
- Utilizes: https://github.com/NGenesis/PimaxEyeTracker
- Inspired by: https://github.com/NGenesis/VRCPimaxEyeTracker

Thanks to those who helped me test this!
