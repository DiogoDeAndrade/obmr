# OBMR - One Button Multiplayer Runner

This is a game done for the 1st Game Design Club challenge (One Button) at the Universidade Lusófona Game Development degree.

![alt text](https://github.com/DiogoDeAndrade/obmr/raw/master/Screenshots/screen01.png "Title Screen")

## Instructions

The game is a multiplayer endless runner (not really endless, since every race is 1.5 mins long).
All player control is done through just one button:

* Players choose the button by pressing it on the title screen or the prepare screen.
  * Any button can be used, keyboard, mouse, gamepad, even the controllers from Buzz (which I used on the demo)
* Tap to jump
* If you jump and hold the button in the air, character will glide and charge a dash
  * If you release the button, the character will dash, damaging all other players in the way
  * It's possible to overload the dash, so release before the bar fills up
* Holding the button while on a platform will drop the player to a lower platform
* Holding the button on the lower platform will break him
* Hitting another player on the top of the head will deal damage to it
* If a player's health drops to zero, he'll respawn after a bit
* Jumping and dashing will accelerate the player
* The more time you spend ahead of the others, the more points you get
* Hitting obstacles will slow you down

## Tech stuff

This game was slightly harder than expected, due to the fact that the camera doesn't actually move and everything is done relative to the average speed of the characters, which makes everything much harder.
Other than that, the challenge was making the game fun by tweaking parameters and getting the "party game" feel right, which considering all tests went according to planned.

## Credits

* Code, art, game design done by Diogo de Andrade
* Sounds done with BXFR and taken from freesounds.com (CC license)
* Beat detection by Allan Pichardo (https://github.com/allanpichardo/Unity-Beat-Detection)
* Music "Infiltrators" by Nathaniel Wyvern (https://nathanielwyvern.bandcamp.com) - Used by kind permission
* Font "Neon Glow"  by Weknow Font Foundry
* Font "Technology" by Vladimir Nikolic
* Font "Neon 80's" by Essqué Productions

## Licenses

All code in this repo is made available through the [GPLv3] license.
The text and all the other files are made available through the 
[CC BY-NC-SA 4.0] license.

## Metadata

* Autor: [Diogo Andrade][]

[Diogo Andrade]:https://github.com/DiogoDeAndrade
[GPLv3]:https://www.gnu.org/licenses/gpl-3.0.en.html
[CC BY-NC-SA 4.0]:https://creativecommons.org/licenses/by-nc-sa/4.0/
[Bfxr]:https://www.bfxr.net/
