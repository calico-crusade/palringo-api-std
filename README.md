[![NuGet](https://img.shields.io/nuget/dt/PalApi.svg?style=for-the-badge)](https://www.nuget.org/packages/PalApi/)
[![NuGet](https://img.shields.io/nuget/v/PalApi.svg?style=for-the-badge)](https://www.nuget.org/packages/PalApi/)
[![license](https://img.shields.io/github/license/calico-crusade/palringo-api-std.svg?style=for-the-badge)](https://github.com/calico-crusade/palringo-api-std/blob/master/LICENSE)
[![GitHub last commit](https://img.shields.io/github/last-commit/calico-crusade/palringo-api-std.svg?style=for-the-badge)](https://github.com/calico-crusade/palringo-api-std/commits/master)

# New project
I embarked on expanding the scope of the "bot api" and created a new project that allows for multiple platforms to be supported for the same plugin set! Check it out [BotsDotNet](https://github.com/calico-crusade/BotsDotNet). I will be supporting this library for a while, however, most of my attention will be on the BotsDotNet library. New features will not be added unless there is resounding support. However, I will add them to BotsDotNet!

# PalringoAPI for .net standard 2.0
A complete rewrite of the original [PalringoAPI](https://github.com/calico-crusade/PalringoApi) in .net standard 2.0

# PalApi Version 1.0.16
Please do not use V 1.0.16 of the PalApi on Nuget. Update to 1.0.17+ if you are using it. The wrong binary was uploaded and a lot of the features are no longer available in this version. The package is no longer listed on Nuget to avoid people using it accidentally.

# Where can I use it?
I have tested it on Windows (.net Framework 4.5.1+ & Core 2.0) and on Mac (Core 2.0). Anyone want to test on linux for me? ;)

# How do I use it?
It is pretty simple to use. You can install it via Nuget (see below). There are 2 different packages available.

```
PM> Install-Package PalApi
```
The primary package that contains all of the useful stuff. Networking, Plugins, all of it. 

```
PM> Install-Package PalApi.Storage
```
This package is an extra-addon that is like a mini version of EntityFramework but with my own spin on it. Mostly because I was bored one day.

# Documentation
You can check the Examples libraries in the source. It should be pretty simple. Any questions? Be sure to reach out: [You can contact me via Pal](http://chat.palringo.com/u/43681734)

# Legal & Stuff
* By using this software you agree to the [Palringo Terms of Service](https://palringo.com/en/us/terms-and-conditions)
* All of the logos and name usage adhere to the [Brand Guidelines](https://www.palringo.com/en/gb/brand-guidelines) set forth by Palringo
* Join the [Bot Approval](https://chat.palringo.com/bot+approval) group on Palringo for any questions regarding the use and to get your bot approved!
* Software is provided as is, I am not liable or responsible for any outcome from using this library.
