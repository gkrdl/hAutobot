/*
 * FileHandler.cs is part of the opensource VoliBot AutoQueuer project.
 * Credits to: shalzuth, Maufeat, imsosharp
 * Find assemblies for this AutoQueuer on LeagueSharp's official forum at:
 * http://www.joduska.me/
 * You are allowed to copy, edit and distribute this project,
 * as long as you don't touch this notice and you release your project with source.
 */

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace hAutobot
{
    public static class FileHandlers
    {
        static string accountsTxtLocation = AppDomain.CurrentDomain.BaseDirectory + @"config\\accounts.txt";

        public static void Account(string controlType, string Region, string Username, string Password, string QueueType, string Champion, string Spell1, string Spell2, string MaxLevel)
        {
            
            var content = Region + "|" + Username + "|" + Password + "|" + QueueType + "|" + Champion + "|" + Spell1 + "|" + Spell2 + "|" + MaxLevel;
            try
            {
                

                if (controlType.Equals("ADD"))
                    File.AppendAllText(accountsTxtLocation, content + Environment.NewLine);
                else
                {
                    string accs = File.ReadAllText(accountsTxtLocation);
                    string[] acc = accs.Split('\n');

                    foreach (string item in acc)
                    {
                        if (item.StartsWith(content))
                        {
                            accs = accs.Replace(item, "");
                        }
                    }

                    accs = accs.Replace("\n\n", "\n");
                    
                    File.WriteAllText(accountsTxtLocation, accs);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message.ToString());
            }
        }

    }
}
