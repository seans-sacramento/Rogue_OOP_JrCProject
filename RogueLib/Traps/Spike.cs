using RogueLib.Dungeon;
using RogueLib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SandBox01.Levels;

public class Spike : Trap
{
    public int Damage { get; set; }

    public Spike (Vector2 pos) : base('i', pos)
    {
        
    }
    public override void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, ConsoleColor.White);
    }
}
