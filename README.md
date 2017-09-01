### AudioVolumeManager

This is a command line utility for saving restoring audio volume profiles on windows machines.


### Why do I need an audio volume manager?

There are several ways to play sound on the computer. You usually have speakers and headphones.  Switching between these devices often requires the incredible effort to adjust the volume slider. This tool takes over this task for you.

### Usage

USAGE: AudioVolumeManager.exe [--help] [--list] [--add <name> <volume>] [--remove <id>] [--apply <id>]

OPTIONS:

    --list                List all stored profiles.
    --add <name> <volume> Add a new profile with a name and audio volume.
    --remove <id>         Remove a profile by given id.
    --apply <id>          Apply a profile by given id.
    --help                display this list of options.

### Known issues

* Incorrect use of the parameters can lead to exceptions


### Building

Clone the repository and build the solution in Visual Studio. In the next version a build-script will be available.

For those who only want to use the application: Load the .exe-file from the [**Releases**](https://github.com/oopbase/AudioVolumeManager/releases) section.