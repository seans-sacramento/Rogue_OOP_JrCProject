using RogueLib.Dungeon;
using RogueLib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SandBox01.Levels;

public class Armor : Item
{
    public string ArmorType { get; init; }
    public int Defense { get; set; }

    public Armor (Vector2 pos) : base('T', pos)
    {
        
    }
    public override void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, ConsoleColor.White);
    }
}
