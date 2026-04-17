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
    protected int _senseRadius = 400;

    // --- Tile Sets -----
    protected TileSet _walkables;
    protected TileSet _floor;
    protected TileSet _tunnel;
    protected TileSet _door;
    protected TileSet _decor;

    protected TileSet _discovered;
    protected TileSet _inFov;

    protected List<Item> _items;
    protected List<Enemy> _enemies;

    // _exitPos holds the grid position of the '>' character we placed in the map string.
    // initMapTileSets reads the map and fills this in automatically.
    private Vector2 _exitPos;

    // _won becomes true when the player steps on the exit with the Key.
    // We use it to show the win message and then close the game on the next keypress.
    private bool _won = false;

    // -----------------------------------------------------------------------

    public Level(Player p, string map, Game game)
    {
        if (game == null || p == null || map == null)
            throw new ArgumentNullException("game, player, or map cannot be null");

        _player = p;
        _player.Pos = new Vector2(4, 12);
        _map = map;
        _game = game;
        _items = new List<Item>();
        _enemies = new List<Enemy>();

        initMapTileSets(map);
        updateDiscovered();
        registerCommandsWithScene();
        spreadGold();
        spawnKey();       // place one Key item somewhere on the floor
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

    // Picks a random floor tile for the Key, making sure it doesn't land on the player.
    private void spawnKey()
    {
        var rng = new Random();
        Vector2 pos;

        do { pos = _floor.ElementAt(rng.Next(_floor.Count)); }
        while (pos == _player!.Pos);

        _items.Add(new Key(pos));
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

        if (item is Gold gold)
        {
            _player!.AddGold(gold.Amount);
            _items.Remove(gold);
        }

        // If the player walked onto the Key, pick it up and remove it from the map.
        if (item is Key key && _player is Rogue rogue)
        {
            rogue.HasKey = true;
            _items.Remove(key);
        }

        // If the player is standing on the exit tile AND has the Key, they win.
        // We only set _won here. _levelActive is set in DoCommand on the next keypress
        // so the win message gets one full frame to display before the game closes.
        if (_player!.Pos == _exitPos && _player is Rogue rogueAtExit && rogueAtExit.HasKey)
        {
            _won = true;
        }

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

        // Draw the exit as a magenta '>' only after the player has discovered that tile.
        if (_discovered.Contains(_exitPos))
            disp.Draw('>', _exitPos, ConsoleColor.Magenta);

        drawEnemies(disp);

        // Append [KEY] to the HUD when the player is carrying it.
        var keyStatus = (_player is Rogue r && r.HasKey) ? " [KEY]" : "";
        disp.Draw(_player.HUD + keyStatus, new Vector2(0, 24), ConsoleColor.Green);

        // If the player won, clear the console and draw a win overlay.
        if (_won)
        {
            Console.Clear();
            disp.Draw("*** YOU ESCAPED THE DUNGEON! CONGRATULATIONS! ***", new Vector2(14, 11), ConsoleColor.Yellow);
            disp.Draw("            Press any key to exit.              ", new Vector2(14, 12), ConsoleColor.Yellow);
            QuitLevel();
        }
    }

    // -----------------------------------------------------------------------
    // Commands
    // -----------------------------------------------------------------------

    public override void DoCommand(Command command)
    {
        // If the player has won, any keypress closes the game.
        // We don't process movement so the win screen stays visible.
        if (_won)
        {
            _levelActive = false;
            return;
        }

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
            _walkables.Remove(newPos);
            _walkables.Add(oldPos);
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
            else if (c == '>')
            {
                // '>' is the exit tile. Treated as floor so the player can walk onto it.
                // Position saved so Update and Draw can reference it.
                _floor.Add(p);
                _exitPos = p;
            }
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