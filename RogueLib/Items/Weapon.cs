using RogueLib.Dungeon;
using RogueLib.Items;
using RogueLib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueLib.Items;

public class Weapon : Item
{
    public string WeaponType { get; init; }
    public int AttackDamage { get; set; }

    public Weapon (Vector2 pos) : base('l', pos)
    {
        
    }
    public override void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, ConsoleColor.Red);
    }
}
