using RogueLib.Dungeon;
using RogueLib.Items;
using RogueLib.Utilities;
using RogueLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace SandBox01.Levels;

public class Potion : Item, IDrinkable
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
    public void Drink()
    {
        //if drink do potion effect
        //currently only health potions but can be expanded
        //- potion uses
        //if potion uses = 0 remove from inventory
    }
}
