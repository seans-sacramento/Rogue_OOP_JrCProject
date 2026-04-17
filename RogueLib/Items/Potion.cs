using RogueLib.Dungeon;
using RogueLib.Items;
using RogueLib.Utilities;
using RogueLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace RogueLib.Items;

public abstract class Potion : Item, IDrinkable
{
    public abstract string PotionType { get; }
    public int NumberOfCharges { get; set; } = 1;

    public Potion (Vector2 pos) : base('d', pos)
    {
        
    }
    public override void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, ConsoleColor.Red);
    }
    public void Drink()
    {
       
        //if drink do potion effect
        //currently only health potions but can be expanded
        //- potion uses
        //if potion uses = 0 remove from inventory
    }
}
