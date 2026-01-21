# White Knuckle Wide Angle Camera Mod
A mod that implements stereographic projection to allow for much greater fields of view with minimal/more pleasant distortion.
![](https://github.com/BarackOBusiness/WKWideAngleCamera/blob/master/resources/preview.avif?raw=true "Example of 180° camera in motion climbing the interlude to pipeworks")

Specifics
---
The mod currently features the wide angle camera in question fully functionally.
It supports configuration in the BepInEx configuration file via the following parameters:
* The vertical field of view of the camera in degrees
* Resolution of the cubemap, which generally scales the performance impact of this mod
* Whether to render behind the player, which only needs to be enabled if your fov is so high that you can see behind you

It is missing a few quality of life fixes:
* The FOV remains the same in computer terminals
* The FOV does not scale with sprint and consumable effects
* At higher FOV the hand grab sprites diverge from their actual position
