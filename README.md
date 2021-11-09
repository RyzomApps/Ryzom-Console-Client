# RCC
Ryzom Console Client (RCC) is a lightweight application that lets you connect to the server of the MMORPG Ryzom, send commands and receive text messages in a fast and simple way, without having to open the main Ryzom game. It also offers various automations for management and other purposes.

## Installation
Get the exe file from the latest build. This exe file is a .NET-Core binary that also works on Mac and Linux.

## Usage
Take a look at the sample configuration that is created when RCC is started for the first time.

## Scripting
Use an automaton that inherits from RCC.Automata.Internal.AutomatonBase. It has an initialization method and is updated every few ticks by the server. Many so-called "impulses" of the Ryzom server are already included as methods that can be inherited by the automaton.

## Contributing
Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.
Please make sure to update tests as appropriate.

## License ##
See the [LICENSE](LICENSE.md) file for license rights and limitations (McRae General Public License).

## Sources

### Minecraft Console Client (https://github.com/ORelio/Minecraft-Console-Client)
released under CDDL-1.0 License http://opensource.org/licenses/CDDL-1.0

Copyright 2021 ORelio and Contributers

### Ryzom - MMORPG Framework (http://dev.ryzom.com/projects/ryzom/)
released under GNU Affero General Public License http://www.gnu.org/licenses/

Copyright 2010 Winch Gate Property Limited

### CryptSharp DES (https://gist.github.com/aannenko/d47c90b0f92177286bb9814850888920)
Copyright 2019 by aannenko

### CryptSharp SHA512 (http://www.zer7.com/software/cryptsharp)
Copyright 2013 by James F. Bellinger 

### CSharpDiscordWebhook (https://github.com/N4T4NM/CSharpDiscordWebhook)
released under MIT License https://github.com/N4T4NM/CSharpDiscordWebhook/blob/master/LICENSE

Copyright 2021 by N4T4NM
