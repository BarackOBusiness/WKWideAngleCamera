# Changelog
v2.0.1
---
- Updated incorrect information and comparisons in the README

v2.0.0
---
- Added panini projection; a cylindrical mapping to the view plane
- The resolution option was changed to presets, this was done to prevent breakages caused by setting the resolution to a non-power of two.
- Fixed critical issue where the view plane was larger than the camera bounds, why is the player scaled to 1.5. why.
- Fixed black flashing issue that occurred as a result of fixing the above issue
- Fixed shader calculations so that the fov option now properly sets horizontal field of view

v1.1.1
---
- Matched camera field of view change speed to vanilla game
- Fixed upgrade terminal animation smoothing

v1.1.0
---
- Camera now respects settings menu field of view slider, removed fov config option
- Field of view now responds to sprinting
- Field of view now responds to consumable effects
- Field of view zooms to fit upgrade terminals

v1.0.0
---
- Implemented the stereographic camera projection
- Configurable cubemap resolution, defaults to 512px
- Configurable vertical field of view, defaults to 135
- Configurable backface rendering, defaults off
