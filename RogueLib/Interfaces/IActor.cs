using System.Data;

namespace RogueLib.Dungeon;

public interface IActor
{
    void Update();
    void IsDead();
    char Glyph { get; }
}