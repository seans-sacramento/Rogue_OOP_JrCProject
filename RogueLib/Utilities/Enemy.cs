using RogueLib.Dungeon;
using RogueLib.Utilities;
using TileSet = System.Collections.Generic.HashSet<RogueLib.Utilities.Vector2>;

// -----------------------------------------------------------------------
// Enemy — base class for all dungeon monsters.
//
// Movement:   call Move() each turn; default wanders randomly.
// Combat:     bump-combat is driven by Level.MovePlayer() — when the
//             player steps onto an enemy tile, AttackPlayer() is called
//             instead of moving the player.  Enemies retaliate via
//             TakeDamage() on the player, also from MovePlayer().
// Inheritance: override Glyph, starting stats, Move(), and
//             AttackPlayer() in each child class.
// -----------------------------------------------------------------------
public abstract class Enemy : IActor, IDrawable
{
    // ---- identity ----
    public abstract char Glyph { get; }
    public string Name { get; protected set; } = "Enemy";
    public Vector2 Pos;

    // ---- stats ----
    protected int _hp;
    protected int _maxHp;
    protected int _str;   // damage dealt to player
    protected int _arm;   // damage reduction when hit

    public bool Alive => _hp > 0;

    protected ConsoleColor _color = ConsoleColor.Red;
    protected Random _rng = new Random();

    // ---- directions the enemy can move ----
    protected static readonly Vector2[] Directions =
        { Vector2.N, Vector2.S, Vector2.E, Vector2.W };

    // -----------------------------------------------------------------------
    // IActor
    // -----------------------------------------------------------------------
    public virtual void Update() { }   // called each turn by Level.Update()

    // -----------------------------------------------------------------------
    // Movement — default: wander randomly into any free walkable tile.
    // Override in child classes for smarter behaviour (chase, patrol, etc.)
    // -----------------------------------------------------------------------
    public virtual void Move(TileSet walkables, Vector2 playerPos)
    {
        // shuffle directions so movement feels random
        var dirs = Directions.OrderBy(_ => _rng.Next()).ToArray();

        foreach (var dir in dirs)
        {
            var next = Pos + dir;
            if (walkables.Contains(next))
            {
                walkables.Add(Pos);       // vacate current tile
                Pos = next;
                walkables.Remove(Pos);    // occupy new tile
                break;
            }
        }
    }

    // -----------------------------------------------------------------------
    // Combat
    // -----------------------------------------------------------------------

    // Called by Level when the player bumps into this enemy.
    // Returns the damage the player dealt so the level can log it.
    public int ReceiveAttackFromPlayer(Player player)
    {
        // simple d6 + player str damage formula, mitigated by armour
        int dmg = Math.Max(1, Roll(1, 6) + (player.Str / 4) - _arm);
        TakeDamage(dmg);
        return dmg;
    }

    // Called by this enemy when it retaliates (after player bumps it).
    // Returns damage dealt to player so the level can apply it.
    public virtual int AttackPlayer(Player player)
    {
        int dmg = Math.Max(1, Roll(1, 6) + (_str / 4) - (player.Arm / 2));
        return dmg;   // Level applies the damage to the player
    }

    public virtual void TakeDamage(int amount)
    {
        _hp -= amount;
        if (_hp < 0) _hp = 0;
    }

    // -----------------------------------------------------------------------
    // IDrawable
    // -----------------------------------------------------------------------
    public virtual void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, _color);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------
    protected int Roll(int count, int sides)
    {
        int total = 0;
        for (int i = 0; i < count; i++)
            total += _rng.Next(1, sides + 1);
        return total;
    }

    // Chebyshev distance — useful for "is player adjacent?" checks
    protected int DistanceTo(Vector2 target)
        => Math.Max(Math.Abs(Pos.X - target.X), Math.Abs(Pos.Y - target.Y));
}