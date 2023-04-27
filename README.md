# C# Scripts for RazorEnhanced

This is a collection of C# scripts for [RazorEnhanced](http://razorenhanced.net/)
The best place to clone this repo is inside the folder Scripts of RE.

## Disclaimer

Most of these scripts are tested only on [Demise Server](https://www.uogdemise.com/) but feel free to fix it for your server and send me a PR
Some scripts are under development or just for reference purpose. Do not expect that all is perfect!

## Use the scripts

Clone this repo into RazorEnhanced _Scripts_ folder and add all C# files you need

## Development

The develpment of those script is made on Visual Studio 2019 Community Edition and this is why I provide also a VS solution.
Develop C# scripts is more more complex compared to Python scripting and this is why I'm not going to provide here personal support on how setup the enviroment but feel free to ask in RE Discord Channel.

There are many way to develop a C# script but the most comfortable (for me) is the following: 

1. Clone this repo somewhere in your pc
2. Clone RazorEnhanced [source code](https://github.com/RazorEnhanced/RazorEnhanced) in an other folder
3. Compile it in debug with Visual Studio 
4. Locate the folder _bin\Win32\Debug\Scripts_, and make inside it a junction (symbolic links works only on NTFS not ExFAT) to this script repo (ref. #1). Execute this command in CMD (not PowerShell):
      * `mklink /J CSharp <full path of this cloned repo)`
5. Now open _Scripts.sln_ file from _bin\Win32\Debug\Scripts_ and never from the folder where you cloned the repo
6. Mantain RazorEnhanced source code updated `git pull` to be sure to have the latest features 

Note: 
1. You don't need to run the compiled version of Razor but you can use the installed and include scripts from there
2. If you want run your builded copy of RE, consider that the build procedure deletes the Scripts forlder. The better thing to do is create a batch file that generates the junction and run it before run UO.
3. Junktions works only on NTFS
4. Use Command Prompt (cmd) and not Power Shell
5. Remember to add quotes "" if your path contains a space
6. Be careful to not have too much long paths because Windows limitations

## Debug

Debug with Visual Studio is very simple:

1. Run RazorEnhanced
2. Open _Scripts.sln_
3. Press `CTRL + ALT + P`
4. Locate _RazorEnhanced.exe_ if you are running OSI Client or `ClassicUO.exe` if you are using Classic UO
5. Then Attach to the process
6. Place your breakpoints
7. Run the script on RazorEnhanced and enjoy your breakpoint

## Special RazorEnhanced C# scripting features
C# scripting supports a list of directives that can enable extra features.
All those directives MUST be added before the keyword _namespace_.

### _//#import_ directive
Some C# files uses classes written in other C# files and RazorEnhanced implements a "custom C# way" to link files using a special directive
* `//#import <relative path of the imported C# file>` 
* `//#import "absolute path of the imported C# file`

So be careful to have all required file in the correct path before run the script.

### _//#forcerelease_ directive
If you want to do huge amount of math or other time consuming operations, using this directive, the script will be compiled in _RELEASE_ instead of _DEBUG_ having it run more faster.
_RELEASE_ is not debuggable.
* `//#forcerelease`

## Notes

An important file to make scripts work is _Assemblies.cfg_. It contains the list of all the DLLs that are required to run your scripts.
If you want to use a custom DLL place it in RazorEnhanced main folder and add it's name into this file. You can also add the full path using quotes


## Contributes

This is the list of great guys that contributes to this repository

* Dynamic aka FunkyPants

## Links

* [RazorEnhaced](https://razorenhanced.net/)
* [Github official repo](https://github.com/RazorEnhanced/RazorEnhanced)
* [RazorEnhanced APIs](https://razorenhanced.github.io/doc/api/)
* [RazorEnhanced Discord](https://github.com/RazorEnhanced/awesome-razor-enhanced)
* [Other links](https://github.com/RazorEnhanced/awesome-razor-enhanced)
