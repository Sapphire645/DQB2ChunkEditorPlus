Many thanks to Mugafo (https://github.com/Mugafo/DQB2ChunkEditor) and his work! Without him this fork wouldn't exist. Many many thanks.

And of course to turtle-insect (https://github.com/turtle-insect/DQB2), the one who paved the way in DQB2 technical knowledge.

## DQB2 Chunk Editor Plus

This is my first time uploading to GitHub. I am unsure of the standard procedure. This fork adds more functionality to the original program.

For all undocumented blocks, here is a link with all IDs identified. https://www.tumblr.com/sapphire-rb/751291945137078272/all-of-dqb2s-blocks-with-their-ids

Do not try to set unidentified blocks (the ones that make the selection say '-1 not on list') because I don't know if it will work or not.

*Items are still unrecognized.*

**Note: Has not been fully tested yet. ALWAYS BACK UP YOUR SAVE.**

## Features

- Import and Export buttons for raw hex editing.
- Updated block list with new blocks.
- Changed the way the blocks display in the map for a better visual interface (In-game textures instead of block icons).
- Blocks seem to overflow at index 2048. Added functionality to read and insert blocks with their normal and overflowed indices.
- 'flatten island' option (Turtle's one wasn't working for me and I needed it for testing)
- Magic Pencil tool that will select a whole area in the current layer. By choosing a new block the entire area will be set to that block.
- 'Replace block' tool that will replace every instance of that block in the map for a different one. If there is a selection active with the Pencil it will only affect the selected area.
- New info added: What items do the blocks drop and the hammer requirement of the blocks. 'Used' property added as well (Not accurate)
- Added support for editing Gratitude Points, Time and all 30 Weathers (Not fully tested, gratitude does not work)
- Editing Virtual Chunk Grid (Move chunks around)
- Change text of signs and salutation stations
- Objects are recognized. Database is incomplete as of now.

## Overhaul! 
## Current Release 2.5:

<img src="./src/Images/Screenshots/25obj.png" data-canonical-src="./src/Images/Screenshots/25obj.png" width="479" height="258" /><img src="./src/Images/Screenshots/25grid.png" data-canonical-src="./src/Images/Screenshots/25grid.png" width="479" height="258" />

The newest 2.4 release will overhaul most of my old code. Here's some screenshots

<img src="./src/Images/Screenshots/Overhaul1.png" data-canonical-src="./src/Images/ScreenshotPlus.png" width="479" height="258" /><img src="./src/Images/Screenshots/Overhaul2.png" data-canonical-src="./src/Images/ScreenshotPlus.png" width="479" height="258" />

<img src="./src/Images/Screenshots/Overhaul3.png" data-canonical-src="./src/Images/ScreenshotPlus.png" width="479" />

Screenshots of old 2.4:

<img src="./src/Images/ScreenshotPlus.png" data-canonical-src="./src/Images/ScreenshotPlus.png" width="479" height="258" />

Screenshots of 2.1:

<img src="./src/Images/ScreenshotFill.png" data-canonical-src="./src/Images/ScreenshotFill.png" width="479" height="258" /> <img src="./src/Images/ScreenshotReplace.png" data-canonical-src="./src/Images/ScreenshotReplace.png" width="479" height="258" />

Screenshot of 2.3:

![image](https://github.com/user-attachments/assets/277beef3-b3e9-4fdd-8977-a978ef831f4a)

**Features to add:**
- Decorative items.
- Fill bucket mayhaps.
- Automatic backup.
- "Favourite" toolbar for saving and fast selection of blocks
- Sign and salutation station text editor
- Warp editor (if possible)
- Chunk remapping (if possible)
- Minimap to chunk tab (if possible)

**External info to add:**
- Colored block images are not implemented yet.
- Mapping of all chunks in each island (They are really big.)

## Island Chunk Maps
*Will be updated with all main islands. There are plans for a "Chunk selector" in the app itself with the save file minimaps, but as of now this will do.*
*Use the green BG one to mask to your own island.*

**-Isle of Awakening (STGDAT01)**

<img src="./src/Data/Masks/IoA_Map.png" data-canonical-src="./src/Data/Masks/IoA_Map.png">
<img src="./src/Data/Masks/IoA_Square.png" data-canonical-src="./src/Data/Masks/IoA_Square.png">
<img src="./src/Data/Masks/IoA_Mask.png" data-canonical-src="./src/Data/Masks/IoA_Mask.png">

**-Furrowfield (STGDAT02)**

<img src="./src/Data/Masks/Furrowfield_Map.png" data-canonical-src="./src/Data/Masks/Furrowfield_Map.png">
<img src="./src/Data/Masks/Furrowfield_Square.png" data-canonical-src="./src/Data/Masks/Furrowfield_Square.png">
<img src="./src/Data/Masks/Furrowfield_Mask.png" data-canonical-src="./src/Data/Masks/Furrowfield_Mask.png">

**-Khrumbul-Dun (STGDAT03)**

<img src="./src/Data/Masks/Khrumbul-Dun_Map.png" data-canonical-src="./src/Data/Masks/Khrumbul-Dun_Map.png">
<img src="./src/Data/Masks/Khrumbul-Dun_Square.png" data-canonical-src="./src/Data/Masks/Khrumbul-Dun_Square.png">
<img src="./src/Data/Masks/Khrumbul-Dun_Mask.png" data-canonical-src="./src/Data/Masks/Khrumbul-Dun_Mask.png">

**-Moonbrooke (STGDAT04)**

<img src="./src/Data/Masks/Moonbrooke_Map.png" data-canonical-src="./src/Data/Masks/Moonbrooke_Map.png">
<img src="./src/Data/Masks/Moonbrooke_Square.png" data-canonical-src="./src/Data/Masks/Moonbrooke_Square.png">
<img src="./src/Data/Masks/Moonbrooke_Mask.png" data-canonical-src="./src/Data/Masks/Moonbrooke_Mask.png">

**-Malhalla (STGDAT05)**

<img src="./src/Data/Masks/Malhalla_Map.png" data-canonical-src="./src/Data/Masks/Malhalla_Map.png">
<img src="./src/Data/Masks/Malhalla_Square.png" data-canonical-src="./src/Data/Masks/Malhalla_Square.png">
<img src="./src/Data/Masks/Malhalla_Mask.png" data-canonical-src="./src/Data/Masks/Malhalla_Mask.png">

**-Angler's Isle (STGDAT09)**

<img src="./src/Data/Masks/Angler_Map.png" data-canonical-src="./src/Data/Masks/Angler_Map.png">
<img src="./src/Data/Masks/Angler_Square.png" data-canonical-src="./src/Data/Masks/Angler_Square.png">
<img src="./src/Data/Masks/Angler_Mask.png" data-canonical-src="./src/Data/Masks/Angler_Mask.png">

**-Skelkatraz (STGDAT10)**

<img src="./src/Data/Masks/Skelkatraz_Map.png" data-canonical-src="./src/Data/Masks/Skelkatraz_Map.png">
<img src="./src/Data/Masks/Skelkatraz_Square.png" data-canonical-src="./src/Data/Masks/Skelkatraz_Square.png">
<img src="./src/Data/Masks/Skelkatraz_Mask.png" data-canonical-src="./src/Data/Masks/Skelkatraz_Mask.png">

## Patch Notes

**0.2.0:**
- Import and Export buttons for raw hex editing.
- Updated block list with new blocks.
- Changed the way the blocks display in the map for a better visual interface (In-game textures instead of block icons).
- Blocks seem to overflow at index 2048. Added functionality to read and insert blocks with their normal and overflowed indices.

**0.2.1:**
- Added a 'flatten island' option (Turtle's one wasn't working for me and I needed it for testing)
- Added a Magic Pencil tool that will select a whole area in the current layer. By choosing a new block the entire area will be set to that block.
- Added a 'Replace block' tool that will replace every instance of that block in the map for a different one. If there is a selection active with the Pencil it will only affect the selected area.
- Changed a water block that seemed to have the wrong name (It was *too* obviously wrong to ignore)
- Changed the way the chunk number is fetched.

**0.2.2:**
- Completed the whole block list from ID 0 to 2047. Added colors (Still missing images)
- New info added: What items do the blocks drop and the hammer requirement of the blocks. 'Used' property added as well (Not accurate)
- Fixed oversight on the way the chunk number is fetched when importing a file

**0.2.3:**
- Added placeholder filter option to filter blocks by name (Thank goodness)
- Added support for editing Gratitude Points, Time and Weather (Not fully tested, weather is not documented yet)

**0.2.3a:**
- Fixed import option (That was broken in 0.2.3)
- Added ability to change chunk count manually (since it sometimes doesn't work?)
- Tile update, some images were added, water and snow cover was updated, and item tiles were also better identified
- Clock displayed wrong max number (12000 instead of 1200)
- Changed 'replace' icon to an in-game one (finally)
- Save option will now display the proper file name, even if the name of the file was something completely different when it was opened.
- Better weather editor, all 30 weather tipes are named and can be chosen from a dropdown. Which weathers work where is untested.

**Mayor glitches:**
- Gratitude editor is still broken. I'm not sure where the problem is
- For some reason sometimes the blocks will not be recognised despite being in the dropdown? I have no idea why this happens I did not touch that... If this happens just save, close and reopen the program, the file won't be affected by this.
- Filtering seems to cause some chaos in the dropdown thing. Not recommended.
Main branch text below.

## DQB2 Chunk Editor 

Dragon Quest Builders 2 Map Chunk Editor based on the map flattener from https://github.com/turtle-insect/DQB2

### Requirements
[.NET 6 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/6.0/runtime)

### Save Data

Steam save files should be located in `C:\Users\<user>\Documents\My Games\DRAGON QUEST BUILDERS II\Steam\<steam id>\SD\`

B00, B01, B02 are the corresponding save slots for the data.

STGDAT01.BIN is the Isle of Awakening map data.

Backups are not made when editing, so make sure to keep a backup of the save folders.

### Information

Able to edit all chunks and layers. Some issues with unmapped blocks due to duplication. Still need to add color and block shapes as well. I have the ID's but still need to add functionality for it.

"Items" are more difficult than simple blocks and I am still researching the data structure. Looks like there are a lot of different things that go into those, placement direction, shadows, block tile effects.

<img src="./src/Images/Screenshot.png" data-canonical-src="./src/Images/Screenshot.png" width="958" height="517" />
