using System;
using System.Collections.Generic;
using RogueLib.Utilities;
using System.Text;
using RogueLib.Dungeon;

namespace RogueLib.Items;

public abstract class Item : IDrawable
{
    public Vector2 Pos { get; set; }

    public char Glyph { get; set; }

    public Item(char glyph, Vector2 pos)
    {
        Glyph = glyph;
        Pos = pos;
    }

    public abstract void Draw(IRenderWindow disp);
}
