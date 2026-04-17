using RogueLib.Dungeon;
using RogueLib.Engine;
using RogueLib.Items;
using RogueLib.Traps;
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
    protected int _senseRadius = 400; // change this value to 400 to see the whole map

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
    protected List<Trap> _traps;

    // _exitPos holds the grid position of the '>' character we placed in the map string.
    // initMapTileSets reads the map and fills this in automatically.
    private Vector2 _exitPos;

    // _won becomes true when the player steps on the exit with the Key.
    // We use it to show the win message and then close the game on the next keypress.
    private bool _won = false;

    public Level(Player p, string map, Game game)
    {
        if (game == null || p == null || map == null)
            throw new ArgumentNullException("game, player, or map cannot be null");

        _player = p;
        _player.Pos = new Vector2(4, 12); // random, or at stairs
        _map = map;
        _game = _game;
        _items = new List<Item>();
        _traps = new List<Trap>();

        initMapTileSets(map);
        updateDiscovered();
        registerCommandsWithScene();
        spreadGold();
        spawnKey();          // place one Key item somewhere on the floor
        SpreadHealthPotion();
        SpreadStrengthPotion();
        SpreadWeapon();
        SpreadArmor();
        SpreadSpikes();
        spawnStairs();
    }

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

    private void SpreadHealthPotion()
    {
        var rng = new Random();
        var hm = rng.Next(5, 10);

        for (int i = 0; i < hm; i++)
        {
            var pos = _floor.ElementAt(rng.Next(_floor.Count));
            _items.Add(new HealthPotion(pos));
        }
    }

    private void SpreadStrengthPotion()
    {
        var rng = new Random();
        var hm = rng.Next(6, 20);

        for (int i = 0; i < hm; i++)
        {
            var pos = _floor.ElementAt(rng.Next(_floor.Count));
            _items.Add(new StrengthPotion(pos));
        }
    }

    private void SpreadWeapon()
    {
        var rng = new Random();
        var hm = rng.Next(1, 5);

        for (int i = 0; i < hm; i++)
        {
            var pos = _floor.ElementAt(rng.Next(_floor.Count));
            _items.Add(new Weapon(pos));
        }
    }

    private void SpreadArmor()
    {
        var rng = new Random();
        var hm = rng.Next(1, 5);

        for (int i = 0; i < hm; i++)
        {
            var pos = _floor.ElementAt(rng.Next(_floor.Count));
            _items.Add(new Armor(pos));
        }
    }

    private void SpreadSpikes()
    {
        var rng = new Random();
        var hm = rng.Next(1, 5);
        int smallSpikeDmg = 1;
        int medSpikeDmg = 3;
        int largeSpikeDmg = 5;

        for (int i = 0; i < hm; i++)
        {
            var pos = _floor.ElementAt(rng.Next(_floor.Count));
            _traps.Add(new Spike(pos, smallSpikeDmg));
        }
        for (int i = 0; i < hm; i++)
        {
            var pos = _floor.ElementAt(rng.Next(_floor.Count));
            _traps.Add(new Spike(pos, medSpikeDmg));
        }
        for (int i = 0; i < hm; i++)
        {
            var pos = _floor.ElementAt(rng.Next(_floor.Count));
            _traps.Add(new Spike(pos, largeSpikeDmg));
        }
    }

    private void spawnStairs()
    {
        var rng = new Random();
        var hm = rng.Next(0, 1);

        for (int i = 0; i < hm; i++)
        {
            var pos = _floor.ElementAt(rng.Next(_floor.Count));
            _items.Add(new Stairs(pos));
        }
    }

    protected void updateDiscovered()
    {
        _inFov = fovCalc(_player!.Pos, _senseRadius);

        if (_discovered is null)
            _discovered = new TileSet();

        _discovered.UnionWith(_inFov);
    }

    protected TileSet fovCalc(Vector2 pos, int sens)
       => Vector2.getAllTiles().Where(t => (pos - t).RookLength < sens).ToHashSet();

    // -----------------------------------------------------------------------
    // ⚠️ MERGE CONFLICT RISK: teammates adding NPC logic or death checks will
    // also edit this method. Our additions are at the bottom of the method
    // to reduce overlap, but watch for conflicts here when merging.
    public override void Update()
    {
        if (_player!.Health <= 0)
        {
            _player.IsDead();
        }
        updateDiscovered();

        // Check if there is any item sitting at the player's current tile.
        var item = _items.Find(i => i.Pos == _player!.Pos);
        var trap = _traps.Find(t => t.Pos == _player!.Pos);

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

        if (item is HealthPotion hp)
        {
            _player!.AddHealth(hp.HealAmount);
            _items.Remove(hp);
        }

        if (item is StrengthPotion str)
        {
            _player!.AddStr(str.StrAmount);
            _items.Remove(str);
        }

        if (trap is Spike)
        {
            _player!.RemoveHealth(Spike.Damage);
        }

        if (item is Stairs)
        {
            //change maps
        }

        // If the player is standing on the exit tile AND has the Key, they win.
        // We only set _won here. _levelActive is set in DoCommand on the next keypress
        // so the win message gets one full frame to display before the game closes.
        if (_player!.Pos == _exitPos && _player is Rogue rogueAtExit && rogueAtExit.HasKey)
        {
            _won = true;
        }

        _player!.Update();
        // foreach item update
        // foreach NPC update
        // check for player death -- on death build RIP message
    }

    public override void Draw(IRenderWindow? disp)
    {
        // using custom RenderWindow, cast to my RenderWindow
        var tilesToDraw = new TileSet(_decor);
        tilesToDraw.IntersectWith(_discovered);
        tilesToDraw.UnionWith(_inFov);

        disp.fDraw(tilesToDraw, _map, ConsoleColor.Gray);

        drawItems(disp);
        drawTraps(disp);

        var rng = new Random();
        _player!.Draw(disp);

        int lowHealthThreshold = 10;
        int highHealthThreshold = 11;

        if (_player.Health <= lowHealthThreshold)
            _player._color = ConsoleColor.DarkRed;
        if (_player.Health >= highHealthThreshold)
            _player._color = ConsoleColor.Green;

        // Draw the exit as a magenta '>' only after the player has discovered that tile.
        if (_discovered.Contains(_exitPos))
            disp.Draw('>', _exitPos, ConsoleColor.Magenta);

        drawEnemies(disp);

        // Append [KEY] to the HUD when the player is carrying it.
        // ⚠️ MERGE CONFLICT RISK: teammates may also edit this HUD line.
        var keyStatus = (_player is Rogue r && r.HasKey) ? " [KEY]" : "";
        disp.Draw(_player.HUD + keyStatus, new Vector2(0, 24), ConsoleColor.Green);

        // If the player won, clear the console and draw a win overlay in the center of the screen.
        if (_won)
        {
            Console.Clear();
            disp.Draw("*** YOU ESCAPED THE DUNGEON! CONGRATULATIONS! ***", new Vector2(14, 11), ConsoleColor.Yellow);
            disp.Draw("            Press any key to exit.              ", new Vector2(14, 12), ConsoleColor.Yellow);
        }
    }

    public override void DoCommand(Command command)
    {
        // If the player has won, any keypress closes the game.
        // We don't process movement so the win screen stays visible.
        if (_won)
        {
            _levelActive = false;
            return;
        }

        // player ctl
        if (command.Name == "up")
        {
            MovePlayer(Vector2.N);
        }
        else if (command.Name == "down")
        {
            MovePlayer(Vector2.S);
        }
        else if (command.Name == "left")
        {
            MovePlayer(Vector2.W);
        }
        else if (command.Name == "right")
        {
            MovePlayer(Vector2.E);
        } // game ctl
        else if (command.Name == "quit")
        {
            _levelActive = false;
        }
        else if (command.Name == "inventory")
        {
            //open inventory
            //hide map, display inventory  
            //considering "overlay" with map
        }
    }

    // -------------------------------------------------------------------------

    private void drawItems(IRenderWindow disp)
    {
        foreach (var item in _items)
        {
            if (_discovered.Contains(item.Pos))
            {
                if (item is StrengthPotion)
                {
                    disp.Draw(item.Glyph, item.Pos, ConsoleColor.Blue);
                }
                else if (item is HealthPotion)
                {
                    disp.Draw(item.Glyph, item.Pos, ConsoleColor.Red);
                }
                else if (item is Potion)
                {
                    disp.Draw(item.Glyph, item.Pos, ConsoleColor.Magenta);
                }
                else if (item is Weapon)
                {
                    disp.Draw(item.Glyph, item.Pos, ConsoleColor.Red);
                }
                else if (item is Armor)
                {
                    disp.Draw(item.Glyph, item.Pos, ConsoleColor.White);
                }
                else
                {
                    disp.Draw(item.Glyph, item.Pos, ConsoleColor.Yellow);
                }
            }
        }
    }

    private void drawTraps(IRenderWindow disp)
    {
        foreach (var trap in _traps)
        {
            if (_discovered.Contains(trap.Pos))
            {
                if (trap is Spike)
                {
                    disp.Draw(trap.Glyph, trap.Pos, ConsoleColor.White);
                }
                else
                {
                    disp.Draw(trap.Glyph, trap.Pos, ConsoleColor.Yellow);
                }
            }
        }
    }

    private void drawEnemies(IRenderWindow disp) { }

    private void drawInventory(IRenderWindow disp)
    {
        //if not inventory mode, close, 
        //if inventory mode
        //disp.Draw("const", new Vector2(0, 0), ConsoleColor.White);

        //for each item in inventory, print to screen and display description
        //name, quantity
        //if selected stats
    }

    private void initMapTileSets(string map)
    {
        var lines = map.Split('\n');

        // ------ rules for map ------
        // . - floor, walkable and transparent.
        // + - door, walkable and transparent // # - tunnel, walkable and transparent
        // ' ' - solid stone, not walkable, not transparent.
        // '|' - wall, not walkable, not transparent, but discoverable.'
        //  others are treated the same as wall.
        // tunnel, wall, and doorways are decor, once discovered they are visible.

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
                // '>' is the exit tile. We treat it like a floor tile so the player
                // can walk onto it. We also save its position so we can check it in
                // Update and draw it with a special color in Draw.
                _floor.Add(p);
                _exitPos = p;
            }
            else if (c != ' ') _decor.Add(p);
        }

        _walkables = _floor.Union(_tunnel).Union(_door).ToHashSet();
    }

    // ------------------------------------------------------
    // Commands 
    // ------------------------------------------------------

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

        RegisterCommand(ConsoleKey.I, "inventory");
    }

    public void MovePlayer(Vector2 delta)
    {
        var newPos = _player!.Pos + delta;

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
}