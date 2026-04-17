using RogueLib.Dungeon;
using RogueLib.Engine;
using RogueLib.Utilities;
using SandBox01.Levels;
using TileSet = System.Collections.Generic.HashSet<RogueLib.Utilities.Vector2>;

namespace RlGameNS;

// -----------------------------------------------------------------------
// The Level is the model, all the game world objects live in the model. 
// player input updates the model, the model updates the view, and the 
// controller runs the whole thing. 
//
// Scene is the base class for all game scenes (levels). Scene is an 
// abstract class that implements IDrawable and ICommandable. 
// 
// A dungeon level is a collection or rooms and tunnels in a 78x25 grid. 
// each tile is at a point, or grid location, represented by a Vector2. 
// 
// *TileSets* are HashSets of grid points, TileSets can be used to tell 
// GameScreen what tiles to draw. TileSets can be combined with Union and 
// Intersect to create complex tile sets.
// -----------------------------------------------------------------------
public class Level : Scene
{
    // ---- level config ---- 
    protected string? _map;
    protected int _senseRadius = 4; // change this value to 400 to see the whole map

    // --- Tile Sets -----
    // used to keep track of state of tiles on the map
    protected TileSet _walkables; // walkable tiles 
    protected TileSet _floor;
    protected TileSet _tunnel;
    protected TileSet _door;
    protected TileSet _decor; // walls and other decorations, always visible once discovered

    protected TileSet _discovered; // tiles the player has seen
    protected TileSet _inFov;      // current fov of player

    protected List<Item> _items;
    protected List<Enemy> _enemies;

    // -----------------------------------------------------------------------
    public Level(Player p, string map, Game game)
    {
        if (game == null || p == null || map == null)
            throw new ArgumentNullException("game, player, or map cannot be null");

        _player = p;
        _player.Pos = new Vector2(4, 12); // random, or at stairs
        _map = map;
        _game = game;
        _items = new List<Item>();
        _enemies = new List<Enemy>();

        initMapTileSets(map);
        updateDiscovered();
        registerCommandsWithScene();
        spreadGold();
        spreadEnemies();
    }

    // -----------------------------------------------------------------------
    // Spreading
    // -----------------------------------------------------------------------

    private void spreadGold()
    {
        var rng = new Random();
        var hm = rng.Next(10, 20);

        for (int i = 0; i < hm; i++)
        {
            var pos = _floor.ElementAt(rng.Next(_floor.Count));
            _items.Add(new Gold(pos, rng.Next(100, 200)));
        }
    }

    private void spreadEnemies()
    {
        var rng = new Random();
        var tiles = _floor.ToList();

        // spawn orcs
        for (int i = 0; i < 4; i++)
        {
            var pos = tiles[rng.Next(tiles.Count)];
            if (_walkables.Contains(pos))
            {
                _enemies.Add(new Orc(pos));
                _walkables.Remove(pos);
            }
        }

        // spawn trolls
        for (int i = 0; i < 2; i++)
        {
            var pos = tiles[rng.Next(tiles.Count)];
            if (_walkables.Contains(pos))
            {
                _enemies.Add(new Troll(pos));
                _walkables.Remove(pos);
            }
        }
    }

    // -----------------------------------------------------------------------
    // Update
    // -----------------------------------------------------------------------

    public override void Update()
    {
        updateDiscovered();

        // --- item pickup ---
        var item = _items.Find(i => i.Pos == _player!.Pos);
        if (item is not null && item is Gold gold)
            _player!.AddGold(gold.Amount);

        _player!.Update();

        // --- enemy turns ---
        foreach (var enemy in _enemies)
        {
            if (enemy.Alive)
            {
                enemy.Update();
                enemy.Move(_walkables, _player.Pos);
            }
        }

        // free tiles of dead enemies and remove them
        foreach (var dead in _enemies.Where(e => !e.Alive))
            _walkables.Add(dead.Pos);

        _enemies.RemoveAll(e => !e.Alive);

        // --- player death ---
        if (!_player.Alive)
            _levelActive = false;
    }

    // -----------------------------------------------------------------------
    // Draw
    // -----------------------------------------------------------------------

    public override void Draw(IRenderWindow? disp)
    {
        var tilesToDraw = new TileSet(_decor);
        tilesToDraw.IntersectWith(_discovered);
        tilesToDraw.UnionWith(_inFov);

        disp.fDraw(tilesToDraw, _map, ConsoleColor.Gray);

        drawItems(disp);

        var rng = new Random();
        if (_player.Turn % 5 == 0)
            _player._color = (ConsoleColor)rng.Next(10, 16);

        _player!.Draw(disp);

        drawEnemies(disp);

        disp.Draw(_player.HUD, new Vector2(0, 24), ConsoleColor.Green);
    }

    // -----------------------------------------------------------------------
    // Commands
    // -----------------------------------------------------------------------

    public override void DoCommand(Command command)
    {
        if (command.Name == "up") MovePlayer(Vector2.N);
        else if (command.Name == "down") MovePlayer(Vector2.S);
        else if (command.Name == "left") MovePlayer(Vector2.W);
        else if (command.Name == "right") MovePlayer(Vector2.E);
        else if (command.Name == "quit") _levelActive = false;
    }

    private void registerCommandsWithScene()
    {
        RegisterCommand(ConsoleKey.UpArrow, "up");
        RegisterCommand(ConsoleKey.W, "up");
        RegisterCommand(ConsoleKey.K, "up");

        RegisterCommand(ConsoleKey.DownArrow, "down");
        RegisterCommand(ConsoleKey.S, "down");
        RegisterCommand(ConsoleKey.J, "down");

        RegisterCommand(ConsoleKey.LeftArrow, "left");
        RegisterCommand(ConsoleKey.A, "left");
        RegisterCommand(ConsoleKey.H, "left");

        RegisterCommand(ConsoleKey.RightArrow, "right");
        RegisterCommand(ConsoleKey.D, "right");
        RegisterCommand(ConsoleKey.L, "right");

        RegisterCommand(ConsoleKey.Q, "quit");
    }

    // -----------------------------------------------------------------------
    // Movement + bump combat
    // -----------------------------------------------------------------------

    public void MovePlayer(Vector2 delta)
    {
        var newPos = _player!.Pos + delta;

        // --- bump combat: player walks into an enemy ---
        var target = _enemies.Find(e => e.Alive && e.Pos == newPos);
        if (target != null)
        {
            target.ReceiveAttackFromPlayer(_player);

            if (target.Alive)
                _player.TakeDamage(target.AttackPlayer(_player));

            return;  // bump = attack, not a step
        }

        // --- normal movement ---
        if (_walkables.Contains(newPos))
        {
            var oldPos = _player!.Pos;
            _player!.Pos = newPos;
            _walkables.Remove(newPos); // new tile is now occupied
            _walkables.Add(oldPos);    // old tile is now free
        }
    }

    public void QuitLevel()
    {
        _levelActive = false;
    }

    // -----------------------------------------------------------------------
    // Private draw helpers
    // -----------------------------------------------------------------------

    private void drawItems(IRenderWindow disp)
    {
        foreach (var item in _items)
        {
            if (_discovered.Contains(item.Pos))
                disp.Draw(item.Glyph, item.Pos, ConsoleColor.Yellow);
        }
    }

    private void drawEnemies(IRenderWindow disp)
    {
        foreach (var enemy in _enemies)
        {
            if (enemy.Alive && _inFov.Contains(enemy.Pos))
                enemy.Draw(disp);
        }
    }

    // -----------------------------------------------------------------------
    // Map initialisation
    // -----------------------------------------------------------------------

    private void initMapTileSets(string map)
    {
        // ------ rules for map ------
        // . - floor, walkable and transparent.
        // + - door, walkable and transparent.
        // # - tunnel, walkable and transparent.
        // ' ' - solid stone, not walkable, not transparent.
        // '|' - wall, not walkable, not transparent, but discoverable.
        //  others are treated the same as wall.
        // tunnel, wall, and doorways are decor — once discovered they stay visible.

        _floor = new TileSet();
        _tunnel = new TileSet();
        _door = new TileSet();
        _decor = new TileSet();

        foreach (var (c, p) in Vector2.Parse(map))
        {
            if (c == '.') _floor.Add(p);
            else if (c == '+') _door.Add(p);
            else if (c == '#') _tunnel.Add(p);
            else if (c != ' ') _decor.Add(p);
        }

        _walkables = _floor.Union(_tunnel).Union(_door).ToHashSet();
    }

    // -----------------------------------------------------------------------
    // FOV
    // -----------------------------------------------------------------------

    protected void updateDiscovered()
    {
        _inFov = fovCalc(_player!.Pos, _senseRadius);

        if (_discovered is null)
            _discovered = new TileSet();

        _discovered.UnionWith(_inFov);
    }

    protected TileSet fovCalc(Vector2 pos, int sens)
        => Vector2.getAllTiles().Where(t => (pos - t).RookLength < sens).ToHashSet();
}