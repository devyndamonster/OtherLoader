# OtherLoader
#### A whole new loader!

OtherLoader is an asset loader for H3VR, which functions similarly to other loaders like LSIIC.

#### Why should you use OtherLoader?
- It supports custom ammo
- It supports custom item spawner categories
- It allows modded items to use Deli's dependany system
- It doesn't spam the console like LSIIC

## Installation
1. Install version 0.4.1 of [Deli Mod Loader](https://github.com/Deli-Counter/Deli)
    - Deli requires the x64 version of BepInEx
2. Extract the OtherLoader `.zip` file into the BepInEx plugins folder

## Making a mod for OtherLoader

To make your item mod use OtherLoader, you must package it as a Deli mod. Below is an example manifest file for a mod using OtherLoader

```
{
  "guid": "h3vr.otherloader.example",
  "version": "0.1.0",
  "require": "0.4.1",
  "dependencies": {
      "h3vr.otherloader.deli": "0.3.0"
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

## Using legacy mods with OtherLoader
Legacy mods (mods loaded with LSIIC) can be loaded by placing the virtual object files into the `H3VR/LegacyVirtualObjects` folder


