using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace BuildProjectBuilder
{
    class Builder
    {
        // using System.Diagnostics;
        Process Proc = new Process();

		public Builder(string project, string target)
		{
			int errors = 0;
			if(!File.Exists(project))
			{
				Console.WriteLine(project + " not found. No build.");
				errors++;
			}
			string targetFile = project.Substring(0,project.LastIndexOf('\\') + 1) + @"Project\Targets\" + target;
			if(!File.Exists(targetFile))
			{
				Console.WriteLine(target + " not found. No build.");
				errors++;
			}
			if(errors > 0) return;
			//"C:\Program Files\MadCap Software\MadCap Flare 14\Flare.app"
			//madbuild - project c: \Users\tdm085\Documents\docu\Build\00\Build.flprj - target Main - TdmNext - log true
			Proc.StartInfo.FileName = "madbuild.exe";
			//string args = " -project " + project + " -target\"" + Path.GetFileNameWithoutExtension(target) + "\""; // + " -log true";
			string args = " -project " + project + " -target " + Path.GetFileNameWithoutExtension(target); // + " -log true";
			Proc.StartInfo.Arguments = args;
			// hier kann z.B. eine Textdatei mit übergeben werden
			// P.StartInfo.Arguments = "Test.txt";
			try
			{
				Proc.Start();
			}
			catch (Exception ex)
			{
                Console.WriteLine("Could not Build " + target);
                Console.WriteLine(ex.Message);
                Console.WriteLine("Please check if Flare is in your PATH variable and try again.");
			}
		}

        //public Builder(string args)
        //{
        //    int errors = 0;            
        //    Proc.StartInfo.FileName = "madbuild.exe";
        //    Proc.StartInfo.Arguments = args;
        //    // hier kann z.B. eine Textdatei mit übergeben werden
        //    // P.StartInfo.Arguments = "Test.txt";
        //    try
        //    {
        //        Proc.Start();
        //    }
        //    catch
        //    {
        //        Console.WriteLine("Build Failed.");
        //        Console.WriteLine("arguments: " + args);
        //        Console.WriteLine("Please check arguments and if Flare is in your PATH variable. Then try again.");
        //    }
        //}

        public Builder(string app, string project, string target)
        {
            int errors = 0;
            if (!File.Exists(app))
            {
				if(File.Exists(app.Replace("15","14"))) app = app.Replace("15","14"); 
				else 
				{
					Console.WriteLine(app + " not found. No build.");
					errors++;
				}
			}
            if (!File.Exists(project))
			{
				Console.WriteLine(project + " not found. No build.");
                errors++;
            }
            string targetFile = project.Substring(0,project.LastIndexOf('\\') + 1) + @"Project\Targets\" + target;
            if (!File.Exists(targetFile))
            {
                Console.WriteLine(target + " not found. No build.");
                errors++;
            }
            if (errors > 0) return;
			//"C:\Program Files\MadCap Software\MadCap Flare 14\Flare.app"
			//madbuild - project c: \Users\tdm085\Documents\docu\Build\00\Build.flprj - target Main - TdmNext - log true
			Proc.StartInfo.FileName = "madbuild.exe";
            //string args = " -project " + project + " -target\"" + Path.GetFileNameWithoutExtension(target) + "\""; // + " -log true";
            string args = " -project " + project + " -target " + Path.GetFileNameWithoutExtension(target); // + " -log true";
            Proc.StartInfo.Arguments = args;
            // hier kann z.B. eine Textdatei mit übergeben werden
            // P.StartInfo.Arguments = "Test.txt";
            try
            {
                Proc.Start();
            }
            catch
            {
                Console.WriteLine("Could not Build " + target + ". Please check if Flare is in your PATH variable and try again.");
            }            
        }

        public Builder(string arguments)
        {
            Proc.StartInfo.FileName = "madbuild.exe";
            Proc.StartInfo.Arguments = arguments;
            try
            {
                Proc.Start();
            }
            catch
            {
                Console.WriteLine("Build Failed.");
                Console.WriteLine("arguments: " + arguments);
                Console.WriteLine("Please check arguments and if Flare is in your PATH variable. Then try again.");
            }
        }
		/*
		string app = @"C:\Program Files\MadCap Software\MadCap Flare 14\Flare.app\madbuild.exe";
		string project = @"C:\docu\docusrc\00\BCI\BCI.flprj";
		string target = "BCI-PDF";

		if(!File.Exists(project))
		{
			Console.WriteLine(project + " not found. No build.");
			Console.ReadKey();
			return;
		}
		if(!File.Exists(app))
		{
			Console.WriteLine(app + " not found. No build.");
			Console.ReadKey();
			return;
		}

		Process Proc = new Process();
		//"C:\Program Files\MadCap Software\MadCap Flare 13\Flare.app"
		//madbuild - project c: \Users\tdm085\Documents\docu\Build\00\Build.flprj - target Main - TdmNext - log true
		Proc.StartInfo.FileName = app;
		Proc.StartInfo.Arguments = " -project " + project + " -target" + target;
		// hier kann z.B. eine Textdatei mit übergeben werden
		// P.StartInfo.Arguments = "Test.txt";
		Proc.Start();
		Console.ReadKey();
		*/
	}
}
