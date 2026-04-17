using RogueLib.Dungeon;
using RogueLib.Traps;
using RogueLib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueLib.Items;

public class Stairs: Item
{
    

    public Stairs (Vector2 pos) : base('=', pos)
    {
      
    }
    public override void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, ConsoleColor.Yellow);
    }   
}
