# MHServerEmu
MHServerEmu is a server emulator for Marvel Heroes.
- this is a modded fork , the original can be found at https://github.com/crypto137/MHServerEmu
- major thanks to the original developers and contributors 

## FORKED CHANGES
a focus on QualityOfLife looting features , these are work in progress and not optimized for multiplayer .
most of these modded features have options in the config.ini , or are controlled with in game commands , last few are patch based and client side edits . 

- loot filtering by rarity for more item types ( ring , medal , insignia , teamup , catalyst ) controlled by player commands ( !filter set ring epic)
- loot filtering by character exclusivity ( optionaly remove dropped items for characters that your not currently , SoloSelfFound-ish )
- auto pickup of "currency" items ( eternity splinters , cosmic worldshards ) default is screenwide (1400)
- auto pickup and stashing of crafting ingredients ( treating them more like currency items )
- throwable environment props more easily cancellable ( any ability cancels ) or may disable interaction completely ( most characters dont want to be throwing props or getting locked in the animation )
- chest items auto open in player inventory ( common chests are by default opened , rare "chests" like cards are not tho could be configured by whitelist in the config )
- orb pickup radius increased to full screen pickup ( a patch .json mod ) 
- "Power Not Ready" red onscreen message removed ( edited Client localization file ) 

## FORKED INSTALL
- backup any existing MHServerEmu and accounts .db 
- build the server using Build.bat
- for increased Orb pickup radius = copy the patch into the built serveremu ( MHServerEmu\bin\x64\Release\net8.0\Data\Game\Patches )
- to remove the "Power Not Ready" message  = copy the .string file in the /client folder to your game installation folder ( C:\Steam\steamapps\common\Marvel Heroes\Data\Game\Loco\eng.all)
- adjust the settings in the config.ini of server to you preference defaults are all enabled 
- start server
- commands in game : `!filter list` , for example show only cosmic+ rings = `!filter set ring epic`  

## FORKED NOTES
the modded features are implmented primarily with singleplayer self-hosted server in mind , it has not been optimized or hardened for multiplayer and so it may be cheatable dupeable or cause lag  

# MHServerEmu ORIGINAL README BEGINS //=========================================

The only currently supported version of the game client is **1.52.0.1700** (also known as **2.16a**) released on September 7th, 2017.

We post development progress reports on our [blog](https://crypto137.github.io/MHServerEmu/). You can find additional information on various topics in the [documentation](./docs/Index.md). If you would like to discuss this project and/or help with its development, feel free to join our [Discord](https://discord.gg/hjR8Bj52t3).

## Download

We provide two kinds of builds: stable and nightly.

|                      | Stable         | Nightly               |
| -------------------- | -------------- | --------------------- |
| **Update Frequency** | Quarterly      | Daily                 |
| **Features**         | Fewer          | More                  |
| **Stability**        | High           | Medium                |
| **Platforms**        | Windows        | Windows / Linux       |
| **Configuration**    | Pre-Configured | Just the Server Files |

If you are setting the server up for the first time and/or unsure which one to use, we recommend you to start with a stable build. See [Initial Setup](./docs/Setup/InitialSetup.md) for information on how to set the server up.

You can always upgrade from stable to nightly simply by downloading the latest nightly build and overwriting your stable files.

### Stable

[![Stable Release](https://img.shields.io/github/v/release/Crypto137/MHServerEmu?include_prereleases)](https://github.com/Crypto137/MHServerEmu/releases)

### Nightly

[![Nightly Release (Windows x64)](https://github.com/Crypto137/MHServerEmu/actions/workflows/nightly-release-windows-x64.yml/badge.svg)](https://nightly.link/Crypto137/MHServerEmu/workflows/nightly-release-windows-x64/master?preview) [![Nightly Release (Linux x64)](https://github.com/Crypto137/MHServerEmu/actions/workflows/nightly-release-linux-x64.yml/badge.svg)](https://nightly.link/Crypto137/MHServerEmu/workflows/nightly-release-linux-x64/master?preview)

## FAQ

**Is the game fully playable?**

All systems and content that were in the game when it was shut down in 2017 have been restored.

**Where can I download the game client?**

We do not provide download links for the game client for legal reasons. If you have played the game through Steam when it was live, you should be able to download it in your Steam library.

**How to update the server?**

Download the latest stable or nightly build and overwrite your existing files. Nightly builds can be potentially unstable, so it is recommended to back up your account database file located in `MHServerEmu\Data\Account.db` before updating.

**Are you going to support other versions of the game, like the ones from before the Biggest Update Ever (BUE) came out?**

Yes, we do plan to implement support for other versions, including the final pre-BUE version (1.48) from late 2016. Currently there are no timeframes for when this is going to happen. The current work-in-progress 1.48 code is available on the [v48](https://github.com/Crypto137/MHServerEmu/tree/v48) branch.

Some early work has also been done to support version 1.10 from mid 2013. You can find the code for it in the [MHServerEmu2013](https://github.com/Crypto137/MHServerEmu2013) repository.

**Are you going to add new content to the game (heroes, team-ups, powers, etc.)?**

The scope of this project is restoring the game to its original state. We do not have any plans to create custom content. However, all of our research on the game is completely open-source, and it can be potentially used by others in such endeavors.

**Are you going to make improvements to the game client (e.g. upgrade graphics)?**

No, we do not touch the client side of the game in any way. This project is a recreation of only the server backend needed to run the game.

**I have problems with setting the server up.**

Feel free to join our [Discord](https://discord.gg/hjR8Bj52t3) and ask for help in the `#setup-help` channel.
