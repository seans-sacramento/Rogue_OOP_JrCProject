using RogueLib.Dungeon;
using RogueLib.Traps;
using RogueLib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SandBox01.Levels;

public class Spike : Trap
{
    public static int Damage { get; set; } = 1;

    public Spike (Vector2 pos, int damage) : base('i', pos)
    {
        Damage = damage;
    }
    public override void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, ConsoleColor.White);
    }
}
