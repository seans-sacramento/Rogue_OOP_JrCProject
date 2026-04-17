using RogueLib.Dungeon;
using RogueLib.Traps;
using RogueLib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SandBox01.Levels;

public class Gas : Trap
{
    
    public Gas (Vector2 pos, int damage) : base('~', pos)
    {
        
    }
    public override void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, ConsoleColor.DarkMagenta);
    }
}
