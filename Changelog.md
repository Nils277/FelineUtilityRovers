# v1.0.4  *-Keep driving-*
*[Release: 13.OCTOBER.2017]*

**Bug Fixes:**

* Fixed collider of thin Cargo Bay
* Fixed label of the small freight kontainer
* Removed debug logs from the Texture Switch
* Fixed a rare error in the resource switch when a module of another mod cannot be loaded

# v1.0.3  *-All about functions-*
*[Released: 08.Oktober.2017]* 

**Bug Fixes:**
* Fixed category in function filter

# v1.0.2  *-Fixing the wings-*
*[Released: XX.OKTOBER.2017]*

**Update:**
* Recompile for KSP 1.3.1 [PENDING]
        
**Enhancements:**
* The textures of the resource storages do now show what they have stored
* Reduced strenght of 'Cancel lateral speed' option for the engines. (It cause unwanted rotation)
        
**New Parts:**
* Added Drill
* Added ISRU
* Added thin version of Flatbed
* Added thin version of Cargo Bay
        
**Bug Fixes:**
* Added missing texts for the freight canister
* Fixed engine categories (this time really, take two)
* Fixed a bug where a resource container removed non switchable resources from the part
        
**Mod Support:**
* Added basic support for MKS
* Added support for kOS
        
**Misc:**
* Adapted licenses (there are now two separate licenses for code and assets)

        
# v1.0.1  *-Fixing the wings-*
*[Released: 14.SEPTEMBER.2017]*

**Bug Fixes:**
* Fixed engine categories (this time really)
* Removed debug info from right click menu of the hover engines
* Fixed inconsistens behaviour when hover mode cannot be enabled
* Fixed headlights of the Cockpit

# v1.0.0 *-The legend of the flying Lynx-*
*[Released: 13.SEPTEMBER.2017]*

**Enhancements:**
* Extended KSPedia entries to explain the latest features (needs translation)
* Rebalanced the fuelcell for the roof
* Added airlocks to the Docking Module for rescue contracts

**New Parts:**
* Added Hover-Engines
* Added Boat-Engine
* Added Skids

**Bug Fixes:**
* The Engine Category is now available for the engines
* The adapters for the rover types are now passable for CLS
* Fixed compatibility issues with KJR


# v0.6.1 *-Another day another tweak-*
*[Released: 18.JULY.2017]*

**New Parts:**
* Added Freight Canister  

**Enhancements:**
* Rebalanced all attachable Cargo Parts
* The legs are now added to the Gear Action Group automatically
* Tweaked displayed amount of resources for the switchable tanks

**Bug Fixes:**
* Fixed text for the extendible docking port for several languages

# v0.6.0 *-Make a wish-*
*[Released: 17.JULY.2017]*

**Enhancements:**
* Enhanced performance of all animated parts
* The life support parts now have more storage options for Kerbalism
* Rear Airlock and Rear Fueltank can now switch between head and rear lights

**Localization:**
* Added chinese translation ![](http://i.imgur.com/JyqfJ1P.png). Thanks to [ssd21345](http://forum.kerbalspaceprogram.com/index.php?/profile/146209-ssd21345/), [vosskftw](http://forum.kerbalspaceprogram.com/index.php?/profile/175031-vosskftw/) and [Levin845](http://forum.kerbalspaceprogram.com/index.php?/profile/176530-levin845/)
* Added (brazilian) portuguese translation ![](http://i.imgur.com/THVD1ot.jpg). Thanks to [FellipeC](http://forum.kerbalspaceprogram.com/index.php?/profile/77983-fellipec/)
* Added german translation ![](http://i.imgur.com/SuHOnKm.png)
* Updated spanish translations for new parts. Thanks to [bice](http://forum.kerbalspaceprogram.com/index.php?/profile/152599-bice/)
* Added some missing translations for Kerbalism

**New Parts:**
* Added New Cockpit
* Added Thruster
* Added Thruster for sides
* Added Adapter for Buffalo and Malemute
* Added Legs
* Added Flatbed ramp
* Added height-adjustable docking bay (2x)
* Added Utility Module
* Added attachable Cargobags (3x)
* Added Fuelcell for Roof
* Added Thin Fuel Tank
        
**Bug Fixes:**
* Fixed multiple errors in the english texts
* Fixed multiple logged warnings for configs

# v0.5.2 *-Speaking russian-*
*[Released: 26.JUNE.2017]* 

**Enhancements:**
* The floor of the service bay can now be removed
* The config now has the option "showInOneCategoryOnly" to show parts only in one function filter

**Localization:**
* Added translation for russian ![](http://i.imgur.com/mFRcn0a.png). Thanks to [Tirathangil](https://github.com/Tirathangil)

**Bug Fixes:**
* The Fuelcell kanister now has the correct name
* Corrected a weird part name from "Bellowed Joint" to "Joint with folding bellow"

# v0.5.1  *-Speaking spanish-*
*[Released: 08.JUNE.2017]*

**Localization:**
* Added localization for spanish ![](http://i.imgur.com/cXO4NUi.png) . Thanks to [bice](http://forum.kerbalspaceprogram.com/index.php?/profile/152599-bice/)


# v0.5.0 *-Supporting localization-*
*[Released: 29.MAY.2017]*

**Update:**
* Recompile for KSP 1.3
* Adapted to new localization system (no translations yet)

**Bug Fixes:**
* Fixed wrong name in readme (it was labeled *KPBS_settings.cfg*)
* Fixed resourced for USI-LS it used Oxygen and Water instead of Mulch and Fertilizer
* Fixed lighs (they were able to illuminate Kerbin from orbit)
* Fixed category for the SEP Canister (it's in Science now)


# v0.4.1  *-Fixing Life Support-* 
*[Released: 04.MAR.2017]*

**Bug Fixes:**
* Fixed the missing resources for the life support containers
* Fixed the duplicate converter for the fuelcell when Kerbalism is installed


# v0.4.0 *-The FREIGHT update-*
*[Released: 03.03.2017]*

**Enhancements:**
* The switch for the resources is not visible when only one resource is available
* Moved the front AttachNode of the Cockpit to the height of the hitch nodes
* Renamed some parts for a more reasonable placement in the Editor
* Better clarity of the possible resource in the desctiption of the resource switch
* Resources for the Container are now configured with templates
* The bumper has an attach note for the adapter/decoupler now
* Increase the weight of the canisters and wheels for a slighly lower center of mass

**Mod Support:**
* Added support for Raster Prop Monitor
* Added support for ASET Props
* Added support for Surface Experiment Pack
* Added support for RealFuels
* Added support for Pathfinder
* Added support for Kerbal Engineer (Redux)
* Added resource support for Extraplanetary Launchpads
* Added resource support for OSE Workshop
* Added resource support for MKS
* Added resource support for Deepfreeze
* Added resource support for NearFuture Propulsion
* Added resource support for NearFuture Electrical
* The fuel-cell canister is now recognized as power producer by AmpYear and BonVoyage

**Bug Fixes:**
* Fixed NRE in category filter
* Fixed a bug where the KIS Container was available when KIS is not installed
* Fixed stack nods for wheels on the KIS Freight Container
* Fixed distortion for the hitch/joint after docking/attaching parts
* Fixed NRE in ModuleKerbetrotterInternalUpdater
* Fixed error where the bellowed hitch is not updating when reverting flight
* Fixed bug where the hitch/joint behaves weird when placed/moved by KIS

# v0.3.1  *-The KIS update-*
*[Released: 21.02.2017]*

**Misc:**
* Kerbals cannot level up in the mobile lab anymore
* Added surface scanner ability to the mobile lab
* Removed Ore from the FuelTank. It is carried by the Freight Container now
* Straightened roof of cargo ramp
* The front piece now has switchable lights
* Adjusted crash tolerance of many parts

**New Parts:**
* Added small freight container
 * Added big freight container
* Added small flatbed
* Added big flatbed
* Added Solar Panels (2x)
* Added bumper (for Freight and Flatbed)

**Mod Support:**
* Added support for KIS
* Added support for JSI Advanced Transparent Pods
* Added support for WheelSounds
* Added support for BonVoyage

**Bug Fixes:**
* Fixed the collider of the ladders
* Corrected name of the cargo ramp (removed the "big")


# v0.2.0  *-The LIFE SUPPORT update-*
*[Released: 03.02.2017]*

**Enhancements:**
* Rebalanced the EC of the unmanned control units

**New Parts:**
* Added Canister with Fuel-Cells

**Mod Support:**
* Added support for Snacks!
* Added support for TAC-LS
* Added support for USI-LS
* Added support for Kerbalism
* Added support for IFI Life-Support
* Added support for KeepFit
* Added support for RemoteTech


# v0.1.1  *-Bugfix update-*
*[Released: 27.01.2017]*

**Enhancements:**
* The windows look more like glass now

**Bug Fixes:**
* Corrected texture on the Bellowed Joint
* Fixed compatibility problem with RemoteTech


# v0.1.0  *-Going online-*
*[Released: 21.01.2017]*

**Inital release.**
