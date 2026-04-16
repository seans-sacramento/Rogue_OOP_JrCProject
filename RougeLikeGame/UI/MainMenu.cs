using System;
using System.Collections.Generic;
using System.Text;
using RogueLib;
using RogueLib.Dungeon;
using RogueLib.Engine;


namespace SandBox01.UI
{
    internal class MainMenu
    {
        MainMenu()
        {
            init();
            //Console.Clear(); ill test if this works as intended or not after i check if the normall rules/intro is implemented or not
        }

        private void init()
        {
            ShowIntroduction();
            ShowRules();
        }

        private void ShowIntroduction()
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

        private void ShowRules()
        {
            Console.WriteLine(@"
            ***************RogueLike Rules***************     
            *                                           *
            *                                           *
            *                                           *
            ");
        }
    }
}
