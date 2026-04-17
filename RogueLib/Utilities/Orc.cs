using RogueLib.Utilities;
using TileSet = System.Collections.Generic.HashSet<RogueLib.Utilities.Vector2>;

// -----------------------------------------------------------------------
// Orc — basic melee enemy.
// Wanders randomly (uses base Move).
// Deals standard damage; no special abilities.
// -----------------------------------------------------------------------
public class Orc : Enemy
{
    public override char Glyph => 'o';

    public Orc(Vector2 startPos)
    {
        Name = "Orc";
        Pos = startPos;
        _hp = 10;
        _maxHp = 10;
        _str = 4;
        _arm = 1;
        _color = ConsoleColor.Green;
    }

    public override void Move(TileSet walkables, Vector2 playerPos)
    {
        base.Move(walkables, playerPos);
    }

    public override int AttackPlayer(Player player)
    {
        return base.AttackPlayer(player);
    }
}