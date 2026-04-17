using RogueLib;
using RogueLib.Dungeon;
using RogueLib.Engine;
using RogueLib.Utilities;

namespace RlGameNS;


public class MyGame : Game {
   
   private void init() {
      // To create a new game just need to 
      // 'inject' an IRenderWindow to draw the game one
      // 'inject' a Player, the player lives outside or the Scene's because the 
      // player visits all the scenes and takes their inventory with them. 
      // you must load the first leveel, and your level or your game must manage 
      // the level switching. 
      
      _window       = new ScreenBuff();
      _player       = new Rogue();
      _currentLevel = new Level(_player, DungeonConfig.map2, this);      
   }

   public MyGame() {
      // init level on construction 
      init();
   }       
}