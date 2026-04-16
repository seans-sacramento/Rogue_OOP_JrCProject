namespace RlGameNS;

public class Rogue : Player
{
    public int Gold { get; set; }

    //Daniel Guerrero
    // Tracks whether the player is carrying the Key item.
    // Level checks this when the player steps on the '>' exit tile.
    public bool HasKey { get; set; } = false;

    public override void Update()
    {
        base.Update();
    }
}
