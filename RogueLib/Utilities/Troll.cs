using RogueLib.Utilities;
using TileSet = System.Collections.Generic.HashSet<RogueLib.Utilities.Vector2>;

// -----------------------------------------------------------------------
// Troll — heavy enemy.
// Chases the player when within 6 tiles (Chebyshev), wanders otherwise.
// Hits harder and regenerates 1 HP per turn.
// -----------------------------------------------------------------------
public class Troll : Enemy
{
    public override char Glyph => 'T';

    private const int ChaseRadius = 6;

    public Troll(Vector2 startPos)
    {
        Name = "Troll";
        Pos = startPos;
        _hp = 20;
        _maxHp = 20;
        _str = 6;
        _arm = 2;
        _color = ConsoleColor.DarkCyan;
    }

    // Chase the player when close; wander when far away.
    public override void Move(TileSet walkables, Vector2 playerPos)
    {
        if (DistanceTo(playerPos) <= ChaseRadius)
            ChasePlayer(walkables, playerPos);
        else
            base.Move(walkables, playerPos);
    }

    // Heavier hit — 2d6 instead of 1d6.
    public override int AttackPlayer(Player player)
    {
        int dmg = Math.Max(1, Roll(2, 6) + (_str / 4) - (player.Arm / 2));
        return dmg;
    }

    // Regenerate 1 HP per turn (called by Level.Update via base Update).
    public override void Update()
    {
        base.Update();
        if (Alive && _hp < _maxHp)
            _hp++;
    }

    // -----------------------------------------------------------------------
    // Move one step toward the player along the axis with the greater gap.
    // -----------------------------------------------------------------------
    private void ChasePlayer(TileSet walkables, Vector2 playerPos)
    {
        int dx = playerPos.X - Pos.X;
        int dy = playerPos.Y - Pos.Y;

        var preferred = new List<Vector2>();

        if (Math.Abs(dx) >= Math.Abs(dy))
        {
            if (dx != 0) preferred.Add(new Vector2(Math.Sign(dx), 0));
            if (dy != 0) preferred.Add(new Vector2(0, Math.Sign(dy)));
        }
        else
        {
            if (dy != 0) preferred.Add(new Vector2(0, Math.Sign(dy)));
            if (dx != 0) preferred.Add(new Vector2(Math.Sign(dx), 0));
        }

        foreach (var dir in preferred)
        {
            var next = Pos + dir;
            if (walkables.Contains(next))
            {
                walkables.Add(Pos);
                Pos = next;
                walkables.Remove(Pos);
                return;
            }
        }

        // both direct paths blocked — fall back to random wander
        base.Move(walkables, playerPos);
    }
}