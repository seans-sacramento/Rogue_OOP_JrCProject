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
    protected List<Trap> _traps;
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
        SpreadPotion();
        SpreadWeapon();
        SpreadArmor();
        SpreadSpikes();
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
        
    private void SpreadPotion()
    {
        var rng = new Random();
        var hm = rng.Next(5, 10);

        for (int i = 0; i < hm; i++)
        {
            var pos = _floor.ElementAt(rng.Next(_floor.Count));
            _items.Add(new Potion(pos));
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
    public override void Update()
    {
        updateDiscovered();

        var item = _items.Find(i => i.Pos == _player!.Pos);
        var trap = _traps.Find(t => t.Pos == _player!.Pos);

        if (item is not null && item is Gold gold)
        {
            _player!.AddGold(gold.Amount);
        }
        if (trap is not null && trap is Spike)
        {
            _player!._hp -= Spike.Damage;
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
        //if (_player.Turn % 5 == 0)
        //    _player._color = (ConsoleColor)rng.Next(10, 16);
        _player!.Draw(disp);
        // disp.Draw(_player!.Glyph, _player!.Pos, ConsoleColor.Cyan);

        // stop character randomization
        // if _player.Health - 10 _player._color = ConsoleColor.Orange
        // if _player.Slowed = true blue
        // if _player.Rage = red

        drawEnemies(disp);
        disp.Draw(_player.HUD, new Vector2(0, 24), ConsoleColor.Green);
    }

    public override void DoCommand(Command command)
    {
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
                //A switch statement will go here when more items are added
                if (item is Potion)
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
                //A switch statement will go here when more traps are added
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
        //name, nquantity
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
            else if (c != ' ') _decor.Add(p);
        }

        _walkables = _floor.Union(_tunnel).Union(_door).ToHashSet();

        //      for (int row = 0; row < lines.Length; ++row) {
        //         for (int col = 0; col < lines[row].Length; ++col) {
        //            char tile = lines[row][col];
        //
        //            if (tile == '.' || tile == '+' || tile == '#') {
        //               _walkables.Add(new Vector2(col, row));
        //               _decor.Add(new Vector2(col, row));
        //            } else if (tile != ' ') {
        //               _decor.Add(new Vector2(col, row));
        //            }
        //         }
        //      }
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