using RogueLib.Dungeon;
using RogueLib.Items;
using RogueLib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SandBox01.Levels;

public class Potion : Item
{
    public string PotionType { get; init; }
    public int NumberOfCharges { get; set; } = 1;

    public Potion (Vector2 pos) : base('d', pos)
    {
        
    }
    public override void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, ConsoleColor.Magenta);
    }
}
