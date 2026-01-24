# White Knuckle Wide Angle Camera Mod
Adds novel projection techniques to White Knuckle to allow for much greater fields of view with more pleasant distortion profiles.
![](https://github.com/barackobusiness/wkwideanglecamera/blob/master/assets/gallery/preview.avif?raw=true)

## Features
- Very wide FOV (in horizontal degrees), via two distinct camera projections, see below
- Configurable cubemap resolution, which generally scales the performance to clarity ratio of the camera (higher resolution increases sharpness but is more costly)
- Ability to render behind the player, which typically only needs to be enabled if your ocular venture takes you to such heights of view where gameplay is a secondary concern

## Gallery
![](https://github.com/barackobusiness/wkwideanglecamera/blob/master/assets/gallery/stereographic-250fov.avif?raw=true)
*Walking along the haunted pier and climbing some buildings under stereographic projection at 250° hfov*

![](https://github.com/barackobusiness/wkwideanglecamera/blob/master/assets/gallery/panini-250fov.avif?raw=true)
*Entering habitation yellow using panini projection at 250° hfov*

![](https://github.com/barackobusiness/wkwideanglecamera/blob/master/assets/gallery/default-250fov.avif?raw=true)
*Unpleasant distortion visible at the same field of view in the default (perspective/rectilinear) camera projection*

## Further Information
The default perspective projection of most games applies field of view by taking the vertical field of view as a parameter.
Therefore, to compare fairly between the two, you should multiply your perspective fov by your display's aspect ratio.\
For example: 90° × 16:9 == 90° × (16/9) = 160° which becomes the angle from edge to edge horizontally of your screen.\
Above 140° you should find that stereographic and panini projections map the environment to your periphery with much less distortion.
