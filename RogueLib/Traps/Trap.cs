using System;
using System.Collections.Generic;
using RogueLib.Utilities;
using System.Text;
using RogueLib.Dungeon;

namespace RogueLib.Traps;

public abstract class Trap : IDrawable
{
    public Vector2 Pos { get; set; }

    public char Glyph { get; set; }

    public Trap(char glyph, Vector2 pos)
    {
        Glyph = glyph;
        Pos = pos;
    }

    public abstract void Draw(IRenderWindow disp);
}
