# EngagementBaiting
This is a Celeste mod built for an experiment testing the effect that positive and negative reinforcement upon player death can have on the player's frustration and their skill.


## Installation

This mod is made to work with [Everest](https://everestapi.github.io/).

To install this mod, download the latest release from the [releases](https://www.github.com/PringlesGang/EngagementBaiting/releases) tab, and extract the mod in the `Mods` folder of your Celeste install directory.


## Logging

The logger automatically starts, and tries to backup an experiment if it was still in progress.
Once a backup is made, a new experiment is automatically started, and a corresponding ID is generated, faintly visible at all times at the top-left of the screen.

The following data is automatically logged for the player:
- The player's path throughout the level
- The death positions of the player
- The screen transitions
- The level endings
- The death screens shown

Upon pressing `LCtrl + LShift + L`, the current logs are automatically archived in a new directory in `./Logs/Archived` according to the current timestamp and the experiment ID.

Within each archived experiment folder, there is:
- At most one `PlayerPositions.csv` file holding the player positions over time
- A number of `.log` files holding other logged data at key moments for each level played separately
- A `.txt` file holding the ID of the experiment as well


## Levels

Two levels are provided.
The first is a tutorial level, meant to quickly teach the player the basic mechanics of the game necessary for the experiment.
The second is the main experiment level. A longer, more difficult level specifically made to have the player come into contact with the death screen multiple times.


## Graphs

Graphs pertaining to all experiments can be generated automatically by running the [`./Source/plotter.py`](./Source/plotter.py) Python script from this mod's root directory.
This will generate the graphs in the `./Logs/Graphs` directory.


## Death screen

The core feature of this mod is a custom death screen that shows in place of the regular screen wipe.
These screens show either positive, negative, or no reinforcement based on an in-game setting.
The positive and negative messages shown can be found and edited in [`./Assets/positive_feedback.txt`](./Assets/positive_feedback.txt) and [`./Assets/negative_feedback.txt`](./Assets/negative_feedback.txt) respectively.

Alongside the positive/neutral/negative setting of the death screen, the duration of the screen and the fade time can be adjusted separately in the in-game settings.
