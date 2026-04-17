using RogueLib;
using RogueLib.Dungeon;
using RogueLib.Utilities;

public abstract class Player : IActor, IDrawable
{
    public string Name { get; set; }
    public Vector2 Pos;
    //making gold more secure, do same with health and str
    public int Gold => _gold;
    public int Health => _hp;
    public char Glyph => '@';
    public ConsoleColor _color = ConsoleColor.Green;

    protected int _level = 0;
    protected int _hp = 12;
    protected int _str = 16;
    protected int _arm = 4;
    protected int _exp = 0;
    protected int _gold = 0;
    protected int _maxHp = 12;
    protected int _maxStr = 16;
    protected int _turn = 0;
    public int _healthPotions = 0;

    public int Turn => _turn;

    public Player()
    {
        Name = "Rogue";
        Pos = Vector2.Zero;
    }

    public string HUD =>
       $"Level:{_level}  Gold: {_gold}    Hp: {_hp}({_maxHp})" +
       $"  Str: {_str}({_maxStr})" +
       $"  Arm: {_arm}   Exp: {_exp}/{10} Turn: {_turn}";


    public virtual void Update()
    {
        _turn++;
    }

    public virtual void Draw(IRenderWindow disp)
    {
        disp.Draw(Glyph, Pos, _color);
    }
    public void AddGold(int amount)
    {
        _gold += amount;
    }
    public void RemoveGold(int amount)
    {
        _gold -= amount;
    }
    public void AddHealth(int amount)
    {
        _hp += amount;
    }
    public void RemoveHealth(int amount)
    {
        _hp -= amount;
    }
    public void AddArmor(int amount)
    {
        _arm += amount;
    }
    public void RemoveArmor(int amount)
    {
        _arm -= amount;
    }
    public void AddStr(int amount)
    {
        _str += amount;
    }
    public void RemoveStr(int amount)
    {
        _str -= amount;
    }
    public void IsDead()
    {
        //call rip from dungeon config pass player name and level
        Console.Clear();
        Console.WriteLine(DungeonConfig.RIP);
        Console.WriteLine("You dead");
        Draw(new ScreenBuff());
        Console.ReadKey();
        Environment.Exit(0);
    }
}