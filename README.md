# SWF Player for UWP

This is not finished, currently figuring out why injected key input isn't working on Windows Moblie.

## What is it?
A simple SWF player for Windows Mobile and Desktop, uses [swf2js](https://github.com/swf2js/swf2js) as the flash renderer 

## Current Progress
- Load SWF files
- Control with keyboard and mouse, or Touch (if point and click orientated)

## Planned
- Adaptive controls onscreen depending if touchscreen/mobile or desktop
- Keyboard controls for Touchscreen/Mobile devices
- History of loaded files

## Issues
- ActionScript 1/2 files are only supported at this time
- Some SWF games have errors in physics (Interactive Buddy for example)
- Performance isn't the best on mobile due to JS capabilities in WebView component
