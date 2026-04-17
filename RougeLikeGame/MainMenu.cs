using System;
using System.Collections.Generic;
using System.Text;
using RogueLib;
using RogueLib.Dungeon;
using RogueLib.Engine;


namespace SandBox01.UI
{
    public static class MainMenu
    {


        public static void init()
        {
            ShowIntroduction();
            ShowRules();
            Console.Clear();
        }

        static void ShowIntroduction()
        {
            Console.WriteLine(@"
            Greetings, fellow traveller!

            ***************Welcome to RogueLikeGame***************
            *                                                    *
            *        Rogue is a dungeon exploration game         *
            *           Your objective is to escape...           *
            *   There will be many challenges along the way...   * 
            *                                                    *
            *                                                    *
            *      Press any key to continue to the rules.       *
            ******************************************************
            ");

            Console.ReadKey();
        }

        static void ShowRules()
        {

            /*
             * Walkable tiles: . 
             * Hallways: #
             * 
             * Gold: *
             * Key: !
             */

            Console.Clear();


            Console.WriteLine(@"
            ***************RogueLike Rules***************     
            *                                           *
            *   Avoid enemies                           *
            *   Collect coins                           *
            *   Find the key to escape the level        *
            *                                           *
            *****             Game Index            *****
            *   Gold: *                                 *
            *   Key: !                                  *
            *   Walkable Floor: .                       *
            *   Hallway: #                              *
            *                                           *
            *  Press any key to continue to the game.   *
            *********************************************
            ");

            Console.ReadKey();
        }
    }
}
