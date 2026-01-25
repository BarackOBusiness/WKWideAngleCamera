# White Knuckle Wide Angle Camera Mod
Adds novel projection techniques to White Knuckle to allow for much greater fields of view with more pleasant distortion profiles.
![](https://github.com/barackobusiness/wkwideanglecamera/blob/master/assets/gallery/preview.avif?raw=true)
*Note, the black flickering present in the bottom left corner of these and other showcase clips are not present in the release build of the mod, this video was taken earlier in development*

## Features
- Very wide FOV (in horizontal degrees), via two distinct camera projections, see below
- Configurable cubemap resolution, which generally scales the performance to clarity ratio of the camera (higher resolution increases sharpness but is more costly)
- Ability to render behind the player, which typically only needs to be enabled if your ocular venture takes you to such heights of view where gameplay is a secondary concern

## Gallery
![](https://github.com/barackobusiness/wkwideanglecamera/blob/master/assets/gallery/stereographic-250fov.avif?raw=true)\
*Walking along the haunted pier and climbing some buildings under stereographic projection at 250° hfov*

![](https://github.com/barackobusiness/wkwideanglecamera/blob/master/assets/gallery/panini-250fov.avif?raw=true)\
*Entering habitation yellow using panini projection at 250° hfov*

![](https://github.com/barackobusiness/wkwideanglecamera/blob/master/assets/gallery/stereographic-135fov.avif?raw=true)\
*A climb through the waste heap at 135° hfov with stereographic projection*

![](https://github.com/barackobusiness/wkwideanglecamera/blob/master/assets/gallery/default-135fov.avif?raw=true)\
*The same path taken at the same field of view but in the vanilla perspective/rectilinear projection.*

## Further Information
The default perspective projection of most games applies field of view by taking the vertical field of view as a parameter.
Therefore, to compare fairly between the this mod and the vanilla game, you will have to derive the normal camera's horizontal field of view from your fov setting.\
I've made [this calculator](https://www.desmos.com/calculator/awebrw5jve) to plug in your display parameters and get a corresponding number for comparison
