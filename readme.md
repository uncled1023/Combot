# About Combot

Created by: Chris Woodward

Combot is designed to provide an all-in-one solution for those who wish to run an IRC bot easily.  It includes many useful features as well as fun games.  It can even be used as your personal client!

## Feature Set

* Auto-Reconnect Available
* Server Specific Configurations
* Nickname and Channel Blacklists
* Simple Yet Powerful Command Structure
* Hot loaded Modules
* Bot Configuration Access
* Automatic Invite Accepts
* Channel Moderation Control
* Message and Highlight Spam Control
* Messaging System
* Weather Information
* Youtube search
* Searx Search
* URL Information Display
* Channel Introductions
* Wolfram Alpha
* Nick Quotes
* Decision Maker
* Channel Rules
* Nick Last Seen
* SED
* Custom Command Repsponses
* Channel and Nick Relaying

## Requirements

* MySQL v5.5 or greater
* .NET Framework v4.5.1 or Mono Runtime >3.2.8
* Git (If using the GitVersionTask Nuget Package)

## Installation - Windows

1) Download the Release zip file from the latest release in https://git.teknik.io/Uncled1023/Combot/releases and extract the files to a directory of your choice.<br>
2) Copy Combot.Servers.Default.json to Combot.Servers.json and configure the bot for the correct servers, channels, owner information, and MySQL database.<br>
3) Run Combot-Interface.exe or Combot-CLI.exe

## Installation - Linux (Ubuntu)

1) Download the Release zip file from the latest release in https://git.teknik.io/Uncled1023/Combot/releases and extract the files to a directory of your choice.<br>
2) Copy Combot.Servers.Default.json to Combot.Servers.json and configure the bot for the correct servers, channels, owner information, and MySQL database.<br>
3) Install Mono-Complete: `sudo apt-get mono-complete`<br>
4) Run Combot-CLI.exe: `mono Combot-CLI.exe`

## Building

To fully maximise the potential of Combot's ease of development, Visual Studios 13 should be installed.  If you have Visual Studios installed, you can add **Module Template.zip** to your project templates to easily create new modules.  If you do not want to use a template, you can copy an existing module and work from there.

If you do not have git installed on your machine, or do not want to add the git versioning, then go to the Nuget Package Manager within Visual Studios and Uninstall the **GitVersionTask** package.

## Bugs/Feature Requests

Please report all bugs you find to me so I can fix them as soon as possible.  Also if you have any feature requests, feel free to send them to me as well.

## Contact Info

Email: admin@teknik.io<br>
IRC: (irc.teknik.io)#Combot<br>
IRC: (irc.rizon.net)#Combot<br>
Nick: Uncled1023
