# White Knuckle Wide Angle Camera Mod
A mod that implements stereographic projection to allow for much greater fields of view with minimal/more pleasant distortion.
![](https://github.com/BarackOBusiness/WKWideAngleCamera/blob/master/resources/preview.avif?raw=true "Example of 180° camera in motion climbing the interlude to pipeworks")

Specifics
---
The mod currently features the wide angle camera in question fully functionally. The FOV may be adjusted by the now expanded slider in the settings menu as usual. The FOV scales with sprint and consumable effects, and zooms to computer terminals appropriately.

It supports configuration in the BepInEx configuration file via the following parameters:
* Resolution of the cubemap, which generally scales the clarity to performance ratio of the camera (higher resolution is sharper, but more costly)
* Whether to render behind the player, which only needs to be enabled if your fov is so high that you can see behind you

As of right now there is one known issue where hand grab sprites diverge from their actual position at very high FOV
