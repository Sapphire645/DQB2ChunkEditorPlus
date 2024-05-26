
**WILL BE UPLOADED SOON, PROGRAM IS MISSING SOME IMAGES**

## DQB2 Chunk Editor Plus

This is my first time uploading to GitHub. I am unsure of the standard procedure. This fork adds more functionality to the original program.
I assume since it hasn't been updated for years that it will not be.

**Added functionality:**
- Import and Export buttons for raw hex editing.
- Updated block list with new blocks.
- Changed the way the blocks display in the map for a better visual interface (In-game textures instead of block icons).
- Blocks seem to overflow at index 2048. Added functionality to read and insert blocks with their normal and overflowed indices.
  

**To add:**
- Optimize images from the dropdown menu (lags a little).
- Better filtering for the dropdown menu.
- Decorative items.
- More tools for editing (perhaps a bucket fill option or something along those lines).
- Automatic backup.
- Importing, then saving saves over the imported file. A 'save as' option needs to be implemented.

**External info to add:**
- Full block descriptions.
- Mapping of all chunks in each island (They are really big.)

  
**Note: Has not been fully tested yet. ALWAYS BACK UP YOUR SAVE.**


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
