/*
 * Hello and welcome to the VoliBot AutoQueuer Project!
 * Credits to: shalzuth, Maufeat, imsosharp
 * Find assemblies for this AutoQueuer on LeagueSharp's official forum at:
 * http://www.joduska.me/
 * You are allowed to copy, edit and distribute this project,
 * as long as you don't touch this notice and you release your project with source.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;
using System.Threading;
using System.Net;
using System.Management;
using System.Management.Instrumentation;
using System.Windows.Forms;

namespace hAutobot
{
    public class Program
    {
        
        static void Main(string[] args)
        {
            Application.ThreadException += Application_ThreadException;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.DoEvents();
            Application.Run(new FormMain());
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            try
            {
                FormMain f = new FormMain();

                if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\hAutobot_ERRORLOG"))
                    System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\hAutobot_ERRORLOG");

                string exceptionMessage = e.Exception.StackTrace.Replace("\r\n", "^");
                string[] errorMessage = exceptionMessage.Split('^');
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\A-NIMP_LOG\\ERROR_LOG\\ERROR_LOG_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt"))
                {
                    foreach (string line in errorMessage)
                    {
                        file.WriteLine(line);
                    }
                    file.WriteLine("\r\n\r\n");
                    file.WriteLine(Environment.OSVersion.VersionString);

                    int size = IntPtr.Size;
                    if (size == 4)
                        file.WriteLine("32bit");
                    else
                        file.WriteLine("64bit");

                    file.WriteLine();
                }
            }
            finally
            {
                Console.WriteLine("Thread Exception : {0}", e.Exception.Message);
            }
        }
        
   }
}
