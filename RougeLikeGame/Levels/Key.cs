using RogueLib.Dungeon;
using RogueLib.Utilities;

namespace SandBox01.Levels;

// Daniel Guerrero
// Key is a new item the player must pick up before they can leave through the exit.
// It follows the exact same pattern as Gold — extend Item and override Draw.
public class Key : Item
{
    public Key(Vector2 pos) : base('!', pos) { }

    public override void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, ConsoleColor.Cyan);
    }
}
