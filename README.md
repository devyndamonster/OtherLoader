# OtherLoader
#### A whole new loader!

OtherLoader is an asset loader for H3VR, which functions similarly to other loaders like LSIIC.

#### Why should you use OtherLoader?
- It supports custom ammo
- It supports custom item spawner categories
- It allows modded items to use Deli's dependany system
- It doesn't spam the console like LSIIC

## Installation
1. Install the [Deli Mod Loader](https://github.com/Deli-Counter/Deli)
    - Deli requires the x64 version of BepInEx, you can find installation instructions for that [here]
2. Drag the OtherLoader Deli file into the `Deli/mods` folder
    - Custom items packaged as a .Deli file belong in the `Deli/mods` folder
    - Legacy custom items (the ones made for LSIIC) go into the `Deli/mods/legacy/LegacyVirtualObjects`

## Making mods compatible with OtherLoader
OtherLoader works similarly to LSIIC, in that it loads asset bundles into the game. Any custom item that was made for LSIIC can also be used with OtherLoader by simply packaging the asset bundles into a `.Deli` file.

Once you have your asset bundle inside a `.Deli` file, you just need to point to it with a Deli manifest file. Here's an example of what that file might look like:
```json
{
  "guid": "h3vr.otherloader.example",
  "version": "0.1.0",
  "require": "0.3.1",
  "dependencies": {
      "h3vr.otherloader.deli": "0.1.2"
  },
  "name": "ExampleAsset",
  "description": "Example",
  "authors": [ 
    "Devyn Myers"
  ],
  "assets": {
    "runtime": {
        "YourAssetBundleName": "h3vr.otherloader.deli:item",
        "AnotherAssetBundleName": "h3vr.otherloader.deli:item"
    }
  }
}
```
