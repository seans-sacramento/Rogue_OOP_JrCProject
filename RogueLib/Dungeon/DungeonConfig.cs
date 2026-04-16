namespace RogueLib;

public static class DungeonConfig {
  public const int width  = 78;
  public const int height = 25;

  // constants for drawing
  const char block1 = '░';
  const char block2 = '▒';
  const char block3 = '▓';
  const char block4 = '█';
  const char block5 = '█';
  const char vert   = '│';
  const char hor    = '─';
  const char tlc    = '┌';
  const char trc    = '┐';
  const char blc    = '└';
  const char brc    = '┘';

  //   String representation of a possible Dungeon layout.
  //   " " - solid stone, not walkable, not transparent.
  //   "." - floor, walkable and transparent
  //   "#" - tunnel, walkable and transparent
  //   "+" - door, walkable and transparent
  //   "|", "-", and any other chars - walls, not walkable, not transparent,
  //              but discoverable. 

  public const string map1 =
      """

               ┌──────┐          ┌─────────────┐
               │......│        ##+.............│            ┌───────┐
               │......│        # │.............+##          │...>...│
               │......+######### └──────────+──┘ ###########+.......│
               │......│                     #               └───────┘
               └──+───┘                     #
           ########                 ##########
      ┌────+┐                     ┌─+───────┐              ┌──────────────────┐
      │.....│                     │.........│              │..................│
      │.....+#####################+.........│              │..................│
      │.....│                     │.........│              │..................│
      │.....│                     │.........│              │..................│
      │.....│                     │.........+##############+..................│
      └─+───┘                     └───+─────┘              └───────────────+──┘
        #                             #                                    #
        ######               ┌────────+──────────────┐                     #
             #             ##+.......................|                     #
             #             # |.......................|   ###################
             #             # |.......................|   #
             #             # |.......................+####
             #             # └───────────────────────┘
             ###############
      """;

  static string RIP =
      """

                    __________
                   /          \
                  /    REST    \
                 /      IN      \
                /     PEACE      \
               /                  \
               |  Dave Burchill   |
               |      110 Au      |
               |   killed by a    |
               |      snake       |
               |       2026       |
              *|     *  *  *      | *
      ________)/\\_//(\/(/\)/\//\/|_)_______
      """;
}