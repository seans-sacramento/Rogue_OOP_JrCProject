using RogueLib.Dungeon;
using RogueLib.Items;
using RogueLib.Utilities;
using RogueLib.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using SandBox01.Levels;

namespace RogueLib.Items;

public class HealthPotion : Potion
{
    public int HealAmount { get; } = 5;
    public override string PotionType => "Health Potion";
  

    public HealthPotion(Vector2 pos) : base( pos)
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
