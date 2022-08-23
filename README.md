# RCC (Ryzom Console Client)

[![nightly-release](https://github.com/RyzomApps/RCC/actions/workflows/dotnet.yml/badge.svg)](https://github.com/RyzomApps/RCC/actions/workflows/dotnet.yml)
[![last commit](https://img.shields.io/github/last-commit/RyzomApps/RCC)](https://github.com/RyzomApps/Ryzom-Console-Client/commits/main)
[![reddit](https://img.shields.io/reddit/subreddit-subscribers/Ryzom)](https://old.reddit.com/r/Ryzom/)
![repo size](https://img.shields.io/github/languages/code-size/RyzomApps/RCC.svg?label=repo%20size)
[![downloads](https://img.shields.io/github/downloads/RyzomApps/RCC/total)](https://github.com/RyzomApps/RCC/releases)

Ryzom Console Client (RCC) is a lightweight application that lets you connect to the server of the MMORPG Ryzom, send commands and receive text messages in a fast and simple way, without having to open the official Ryzom game client. It also offers an [API](https://github.com/RyzomApps/Ryzom-Console-Client-API) to create plugins for management and other purposes.

## Installation
Get the executable for windows from the latest [nightly-release](https://github.com/RyzomApps/Ryzom-Console-Client/releases/tag/nightly-release) or build the source files yourself. The application is a .NET-Core binary that should also work on Mac and Linux.

## Usage
Take a look at the sample configuration (client.cfg) that is created when RCC is started for the first time.

## Scripting
Write your own plugin that inherits from API.Plugins.Plugin. An initialization method is provided and the plugin is updated every few ticks by the client. Many so-called "impulses" of the Ryzom server are already included as methods that can be inherited by the plugin.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
Please make sure to update tests as appropriate.

## License ##
See the [LICENSE](LICENSE.md) file for license rights and limitations (McRae General Public License).

## Warranty ##

Please note: all tools/scripts in this repo are released for use "AS IS" without any warranties of any kind, including, but not limited to their installation, use, or performance. We disclaim any and all warranties, either express or implied, including but not limited to any warranty of noninfringement, merchantability, and/or fitness for a particular purpose. We do not warrant that the technology will meet your requirements, that the operation thereof will be uninterrupted or error-free, or that any errors will be corrected.

Any use of these scripts and tools is at your own risk. There is no guarantee that they have been through thorough testing in a comparable environment and we are not responsible for any damage or data loss incurred with their use.

You are responsible for reviewing and testing any scripts you run thoroughly before use in any non-testing environment.

## Sources

### Minecraft Console Client (https://github.com/ORelio/Minecraft-Console-Client)
released under CDDL-1.0 License http://opensource.org/licenses/CDDL-1.0

Copyright 2021 ORelio and Contributers

### Ryzom - MMORPG Framework (http://dev.ryzom.com/projects/ryzom/)
released under GNU Affero General Public License http://www.gnu.org/licenses/

Copyright 2010 Winch Gate Property Limited

### Bukkit - A Minecraft Server API (https://github.com/Bukkit/Bukkit)
released under GNU General Public License v3.0 http://www.gnu.org/licenses/

Copyright 2021 Bukkit Team

### CryptSharp DES (https://gist.github.com/aannenko/d47c90b0f92177286bb9814850888920)
Copyright 2019 by aannenko

### CryptSharp SHA512 (http://www.zer7.com/software/cryptsharp)
Copyright 2013 by James F. Bellinger 

### CSharpDiscordWebhook (https://github.com/N4T4NM/CSharpDiscordWebhook)
released under MIT License https://mit-license.org

Copyright 2021 by N4T4NM

### L2 .NET (https://github.com/devmvalvm/L2Net)
released under GNU General Public License v2.0 http://www.gnu.org/licenses/

Copyright 2018 devmvalvm
