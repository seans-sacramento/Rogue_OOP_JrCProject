using RogueLib.Dungeon;
using RogueLib.Items;
using RogueLib.Utilities;
using RogueLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using SandBox01.Levels;

namespace RogueLib.Items;

public class StrengthPotion : Potion
{
    public int StrAmount { get; } = 5;
    public override string PotionType => "Strength Potion";
  

    public StrengthPotion(Vector2 pos) : base( pos)
    {
        
    }
    public override void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, ConsoleColor.Blue);
    }
    public void Drink()
    {

        //if drink do potion effect
        //currently only health potions but can be expanded
        //- potion uses
        //if potion uses = 0 remove from inventory
    }
}
