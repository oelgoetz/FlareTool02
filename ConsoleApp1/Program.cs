using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using FlareTool01;
using tools;
using txtFiles;
using BuildProjectBuilder;
using System.Threading;
using System.Data.SqlClient;
//using DBSQLServerUtils;
//using DBUtils;

namespace ConsoleApp1
{
    class Program
    {
        private static bool LOUD = false; //false; //true;
        private static bool TEST = false; //false; //true;

        private static bool MOD_OUTPUT = false; //false; //true;
        private static bool MOD_SOURCES  = true; //false; //true;

        //private static bool DATABASE = false; //false; //true;
        private static bool CLEAR = true; //false; //true;
        private static bool WRITE = true; //false; //true;
        private static bool BUILD_WEBHELP = false; //true; //false; //true;
        private static bool BUILD_PDFBATCHTARGET = false; //false; //true;
        private static bool CLOSEWHENREADY = true;

        //private static string SettingsFileName = @"C:\docu\docusrc\Settings.xml";
        //private static string csvSettingsFile = @"C:\docu\docusrc\CasModuleNames.csv";
        //private static string ImportProperty = "pdfFileName";

        private const int READCONFIG = 0;
		private const int WRITECONFIG = 1;

        private static string BRANCH;
        private static string LANG;
        private static string FILEROOT;
        private static string projectPath;
        private static string BuildType;
        private static string TargetProjectPath; //GetNodesGLHelp            
        private static string targetName;

        //public Dictionary<string, string> ExternalTopics = new Dictionary<string, string>();

        public static FlareProject MainProject;
        private static XmlDocument batchTargetDocument;
        static void Main(string[] args)
        {
            if(args.Length < 4) //&& (args.Length != 4))
            {
                Console.WriteLine("You have to pass parameters as arguments.");
                Console.WriteLine();
                Console.WriteLine("Params 1 to 4 are obligatory:");                
                Console.WriteLine(" 1. Name of the Build project (Currently it must be 'GLHelp' or 'V4Help')");
                Console.WriteLine(" 2. File path where all the Flare project merging will take place (Local Path of the git branch)");
                Console.WriteLine(" 3. Language ('00', '01', '02', '03' or '17' - it must be a child folder of the previous path.");
                Console.WriteLine("    '17' is currently only available for GLHelp)");
                Console.WriteLine(" 4. The name of the \"main\" project");
                Console.WriteLine("    (i.e. the project that delivers all common resources for the build project - currently it is always 'Main')");
                //Console.WriteLine(" Project Target");
                Console.WriteLine("All other parameters are optional.");
                Console.WriteLine("Optional parameters always start with a'-'");
                Console.WriteLine(" '-wait' leaves the console open after the program is finished");
                Console.WriteLine(" '-copy' causes the Flare compiler to all required files into the build project that is specified in argument(1).");
                Console.WriteLine("    If a folder with the build project name already exists in the path specified by(2) and(3),");
                Console.WriteLine("    it will be deleted completely before the copy process starts");
                Console.WriteLine(" '-build' causes the Flare compiler to build the Online Help target of the build project open");
                Console.WriteLine("    after the program is finished. If you want to use this, 'madbuild.exe' must be included in your PATH variable.");
                Console.WriteLine();
                Console.ReadLine();
                return;
            }
            else 
            {
                //docu 01 Main GLTopNavi GLHelp
                BRANCH = args[1].Substring(args[1].IndexOf('\\') + 1);
                FILEROOT = args[1].Substring(0, args[1].IndexOf('\\') + 1);
                LANG = args[2];
                projectPath = FILEROOT + BRANCH + @"\" + LANG + @"\" + args[3];
                BuildType = args[0];
                if (BuildType == "GLHelp") targetName = "GLTopNavi.fltar";
                if (BuildType == "V4Help") targetName = "Main-TdmNext.fltar";
                TargetProjectPath = FILEROOT + BRANCH + @"\" + LANG + @"\" + BuildType; //GetNodesGLHelp            
                //targetName = args[4] + ".fltar";
                if(args.Length > 4) 
                {
                    //targetName = args[4] + ".fltar";
                    List<string> parameter2 = new List<string>();

                    int i = 4;
                    for (int arg = 4; arg < args.Length; arg++)
                        parameter2.Add(args[arg].Trim().ToLower());
                    if(parameter2.Contains("-wait"))
                            CLOSEWHENREADY = false;
                        else
                            CLOSEWHENREADY = true;
                    if (parameter2.Contains("-copy"))
                    {
                        CLEAR = true;
                        WRITE = true;
                        BUILD_WEBHELP = false;
                    }
                    else
                    {
                        CLEAR = false;
                        WRITE = false;
                        BUILD_WEBHELP = false;
                    }
                    if (parameter2.Contains("-build")) 
                    {
                        CLEAR = true; 
                        WRITE = true;
                        BUILD_WEBHELP = true;
                    }
                    else 
                    {
                        BUILD_WEBHELP = false;
                    }                    
                }
            }
            
            if (TEST)
            {
                //CLEAR = false;
                //WRITE = false;
                //MOD_SOURCES = false;
                CLEAR = true;
                WRITE = true;
                MOD_SOURCES = true;
                BUILD_WEBHELP = false;
                BUILD_PDFBATCHTARGET = false;
                //DATABASE = false;
            }

            //System.Data.SqlClient.SqlConnection conn = DBUtils.GetDBConnection();
            //DataBaseFunctions.SetConnection(conn);

            //if (DATABASE) 
            //{


            //    try
            //    {
            //        Console.WriteLine("Opening database ...");
            //        conn.Open();
            //        Console.WriteLine("Connection successful!");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine("Error: " + ex.Message);
            //    }

            //}

            // --------------------------------------------------------------------	
            //Test für Französisch:
            //LANG = "03";

            //Test für GitBranch:
            //BRANCH = "docu"; 

            //Test für ProjectPath:
            //projectPath = @"C:\" + BRANCH + @"\docusrc\" + LANG + @"\Main";

            switch (BuildType) 
            {
                case "GLHelp":
                    TargetProjectPath = FILEROOT + BRANCH + @"\" + LANG + @"\GLHelp";
                    targetName = @"GLTopNavi.fltar";
                    break;
                case "V4Help":
                    TargetProjectPath = FILEROOT + BRANCH + @"\" + LANG + @"\V4Help";
                    targetName = @"Main-TdmNext.fltar";
                    break;
                       default:
                    Console.WriteLine("Build Type not valid. Press ENTER to quit.");
                    Console.ReadLine();
                    return;
                    break;
            }

            if (!Directory.Exists(projectPath))
            {
                Console.WriteLine("Project Path " + projectPath + " not found. Cannot continue.");
                Console.ReadLine();
                return;
            }

            //Test für GL:
            //TargetProjectPath = @"C:\" + BRANCH + @"\docusrc\" + LANG + @"\GLHelp"; //GetNodesGLHelp            
            //targetName = @"GLTopNavi.fltar";
            //BuildType = "GLHelp";

            //Test für V4:
            //TargetProjectPath = @"C:\" + BRANCH + @"\docusrc\" + LANG + @"\V4Help";
            //targetName = "Main-TdmNext.fltar";
            //BuildType = "V4Help";

            //targetName = "CAM - TopNavi.fltar"; //Tut noch nicht. Das TOC wird nicht kopiert.
            //string targetName = @"Main-TdmNext.fltar";
            Console.WriteLine("  ------- ");
            Console.WriteLine("  Generating build project: " + TargetProjectPath);
            Console.WriteLine("  Target name: " + targetName);
            Console.WriteLine("  BuildType: " + BuildType);
            
            string LogFileName = "";
            if (WRITE)
            {
                LogFileName = TargetProjectPath + '\\' + "Actions.xml";
                LogFileName = FILEROOT + BRANCH + @"\" + LANG + @"\Actions" + BuildType + ".xml";
                    Console.WriteLine("  LogFile: " + LogFileName);
            }

            //string SettingsFileName = @"C:\" + BRANCH + @"\docusrc\" + LANG + @"\GetNodesGLSettings" + LANG + ".xml";
            string SettingsFileName = FILEROOT + BRANCH + @"\targets.xml";
            string ErrorsFileName = FILEROOT + BRANCH + "\\" + LANG + @"\Errors" + BuildType + ".xml";

            string modFile = FILEROOT + BRANCH + @"\StyleChanges.xml";

            //Console.WriteLine("  SettingsFileName: " + SettingsFileName);
            //Console.WriteLine("  ModFileName: " + modFile);
            //Console.WriteLine("  ------- ");
            //Console.ReadLine();

            //string targetName = @"CAM-TopNavi.fltar";
            //string TargetProjectPath = @"C:\docu\docusrc\00\CAMGL";
            //string SettingsFileName = @"C:\docu\docusrc\00\CAMGLSettings00.xml";

            //string targetName = @"Main-TDMNext.fltar";
            //string TargetProjectPath = @"C:\docu\docusrc\00\V4Help";
            //string SettingsFileName = @"C:\docu\docusrc\00\V4Settings00.xml";

            XmlDocument docErrors = tools1.createXmlFile(ErrorsFileName, "Errors");
            XmlAttribute p = docErrors.CreateAttribute("Path");
            p.Value = FILEROOT + BRANCH + "\\" + LANG;
            docErrors.DocumentElement.Attributes.Append(p);

            XmlDocument docSettings;
            XmlNode root;
            if (!File.Exists(SettingsFileName))
                docSettings = tools1.createXmlFile(SettingsFileName, "targets");
            else 
            {
                docSettings = new XmlDocument();
                tools1.LoadXmlFile(docSettings, SettingsFileName);
            }

            XmlNode BuildProjectNode = docSettings.SelectSingleNode("//" + BuildType);
            if(BuildProjectNode == null) 
            {
                BuildProjectNode = docSettings.CreateElement(BuildType, null);
                docSettings.DocumentElement.AppendChild(BuildProjectNode);
            }

            XmlNodeList oldNodes = docSettings.SelectNodes("//" + BuildType + "/ " + tools1.isoLanguage(LANG));
            foreach (XmlNode oldNode in oldNodes) oldNode.ParentNode.RemoveChild(oldNode);
            
            if (!BuildProjectNode.HasChildNodes)
                root = BuildProjectNode.AppendChild(docSettings.CreateElement(tools1.isoLanguage(LANG))); 
            else
            {
                int l = tools1.tdmLanguagePosition(LANG);
                if ((l >= 0) && (BuildProjectNode.ChildNodes.Count > l))
                {
                    root = BuildProjectNode.InsertAfter(docSettings.CreateElement(tools1.isoLanguage(LANG)), BuildProjectNode.ChildNodes[l - 1]);
                }
                else root = BuildProjectNode.AppendChild(docSettings.CreateElement(tools1.isoLanguage(LANG)));
            }            

            MainProject = new FlareProject(projectPath, targetName, 0, BuildType, /*BRANCH, LANG,*/ LOUD, root, 2, docErrors.DocumentElement); // WRITECONFIG); //level = 0 ==> root; root als parameter benötigt?
            tools1.sortSubNodes(docSettings);

            //docSettings.Save(SettingsFileName);
			//tools1.BeautifyXml(SettingsFileName, SettingsFileName);

            //docErrors.Save(ErrorsFileName);
            //tools1.BeautifyXml(ErrorsFileName,ErrorsFileName);
            //---
            //SettingsFileName = @"C:\docu\docusrc\00\V4HelpSettings00.xml";
            //XmlDocument docV4 = tools1.createXmlFile(SettingsFileName,"settings");
            //root = docV4.DocumentElement;
            //FlareProject MainV4Project = new FlareProject(projectPath,@"Main-TDMNext.fltar", 0, LOUD, root, WRITECONFIG);
            //docV4.Save(SettingsFileName);
            //tools1.BeautifyXml(SettingsFileName, SettingsFileName);

            //FlareProject[] projects = { MainProject,MainV4Project };
            //FlareProject[] projects = { MainProject/*, MainV4Project*/};

            //createNewSettingFile(projects, projectPath, SettingsFileName);
            //ImportSettingFromCsv(SettingsFileName, csvSettingsFile, ImportProperty);

            string rootPath = projectPath.Substring(0, projectPath.LastIndexOf('\\'));
            rootPath = rootPath.Substring(0, rootPath.LastIndexOf('\\'));

			//applySettingsFromFile(@"C:\docu\docusrc\CasSettingsReviewed.xml", rootPath);
			//CollectVariables(projects);
			//CollectStyles(projects);

			if (CLEAR)
            {
                if (Directory.Exists(TargetProjectPath))
                {
                    Console.WriteLine("Deleting target directory ---");
                    try
                    {
                        Directory.Delete(TargetProjectPath, true);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + ". The target directory could not be deleted.");
                        Console.WriteLine("Press a key, close all programs that might block the directory and try again.");
                        Console.ReadKey();
                        return;
                    }
                    //Console.WriteLine("Finished.");
                }
            }
            else Console.WriteLine("CLEAR = false ---");
            ///DEBUG
            //Console.WriteLine("..");
            //Console.ReadLine();
            //Alias-File für CSH-IDs vorbereiten
            if (WRITE)
            {
                Console.WriteLine("Copying ---");
                //Console.WriteLine("Build Type: " + BuildType);
                
                XmlDocument logfile = tools1.createXmlFile(LogFileName, "Actions");
                XmlDocument alias = new XmlDocument();
                XmlDocument synonyms = new XmlDocument();
                
                //Console.ReadLine();
                //Alias-File (".flali") anlegen, header-File erzeugen
                string aliasFilePath = TargetProjectPath + @"\Project\Advanced\CSH\MainAlias.flali";
                tools1.createXmlListFile(alias, aliasFilePath, "CatapultAliasFile");
                //Synonym-File (".mcsyns") für Suchergebnis-Optimierung zusammenfassen
                string synonymFilePath = TargetProjectPath + @"\Project\Advanced\Synonyms.mcsyns";
                if (!File.Exists(synonymFilePath))
                {
                    tools1.createXmlListFile(synonyms, synonymFilePath, "MadCapSynonyms");

                    XmlNode newNode = synonyms.CreateElement("Directional");
                    synonyms.DocumentElement.AppendChild(newNode);
                    newNode = synonyms.CreateElement("Groups");
                    synonyms.DocumentElement.AppendChild(newNode);
                    synonyms.Save(synonymFilePath);
                }
                else
                {
                    tools1.LoadXmlFile(synonyms, synonymFilePath);
                }

                string batchTarget = MainProject.batchtargetFile;
                
                if (MainProject.batchtargetFile != "")
                {
                    batchTargetDocument = new XmlDocument();
                    tools1.LoadXmlFile(batchTargetDocument, batchTarget);
                    Console.WriteLine("Found batch target file " + batchTarget);
                }
                else 
                {
                    Console.WriteLine("No batch target file found. " + batchTarget);
                }
                //DEBUG: batchTargetDocument = null;
                //SqlConnection conn = DBUtils.GetDBConnection();
                MainProject.Copy(TargetProjectPath, logfile, LogFileName, alias, synonyms, /*conn,*/ batchTargetDocument);
                //MainProject.CheckExternalFiles(true);
                //else MainProject.Copy(TargetProjectPath, logfile, LogFileName, alias, synonyms, conn);
                //Prüfen, ob externe Topics bereits kopiert wurden:
                //displayProjectInfo(MainProject,0);
                //Überzählige Help-IDs aus dem Alias-File entfernen
                //TEST: vorher
                //alias.Save(aliasFilePath + "1");
                fetchHelpKeys(alias, TargetProjectPath);
                //TEST: nachher
                //alias.Save(aliasFilePath + "2");
                Console.WriteLine("-------------------------------------");
                Console.WriteLine("Saved alias file: " + aliasFilePath);
                alias.Save(aliasFilePath);
                CreateHeaderFile(alias, aliasFilePath);

                synonyms.Save(synonymFilePath);
                Console.WriteLine("Saved synonym file: " + synonymFilePath);
                //foreach (string file in Directory.GetFiles(TargetProjectPath)) if (file.EndsWith(".flprj")) File.Move(file, TargetProjectPath + "\\GLTopNavi.flprj");
                //<File FileName="tdmlogo.png" ID="\Content\Resources\Images\general\tdmlogo.png" FileType="Image" Lookalike7="LGMGL" Lookalike6="TMSGL" Lookalike5="TPSGL" Lookalike4="OLGGL" Lookalike3="CAD3D" Lookalike2="CAD2D" Lookalike1="TDMGL" Lookalike14="CLGR" Lookalike13="TDMConventions" Lookalike12="tdmNews2019" Lookalike11="tdmCompact" Lookalike10="TDTGL" Lookalike9="MPCGL" Lookalike8="SFMGL"/>
                
                //XmlNodeList nodes = logfile.SelectNodes("//File[@FileType = 'Image']");
                //foreach(XmlNode img in nodes) 
                //{
                //    string ProjectPath = img.ParentNode.Attributes["Path"].Value;
                //    string token = img.ParentNode.Attributes["Token"].Value;
                //    foreach (XmlAttribute att in img.Attributes) 
                //    {                        
                //        if (att.Name.StartsWith("Lookalike"))
                //        {
                //            Console.WriteLine(ProjectPath + ": " + token);
                //            token = att.Value;                            
                //        }
                //        //Console.ReadLine();
                //    }
                //    Console.WriteLine(token);
                //    Console.ReadLine();
                //}
            }

            if (MOD_SOURCES)
            {
                modFile = FILEROOT + BRANCH + @"\SourceChanges.xml";
                SourceModder mod = new SourceModder(TargetProjectPath, BuildType, LOUD, modFile, MainProject);                
            }
            
            //modFile = @"C:\" + BRANCH + @"\docusrc\OutputChanges.xml";
                
            string batchTargetName = "";                

            switch (BuildType)
            {
                case "GLHelp":
                    batchTargetName = "GLPDFBatchTarget.flbat";
                    break;
                case "V4Help":
                    batchTargetName = "V4PDFBatchTarget.flbat";
                    break;
                default:
                    Console.WriteLine("Build Type unknown.");
                    break;
            }
                       
            if (BUILD_PDFBATCHTARGET)
            {
                Builder pdfBuilder;
                if (batchTargetName != "")
                {
                    string projectFile = TargetProjectPath + "\\" + "GLTopNavi.flprj";
                    string batchFile = TargetProjectPath + @"\Project\Targets\" + batchTargetName;
                    if (!File.Exists(batchFile)) 
                        Console.WriteLine("Batch target " + batchTargetName + " not found. Could not start build process.");
                    else 
                    {
                        string arguments = " -project " + projectFile + " -batch " + Path.GetFileNameWithoutExtension(batchTargetName); // + " -log true";
                        pdfBuilder = new Builder(arguments);
                        Console.WriteLine("-------------------------------------");
                    }
                }                
            }

            string pdfLinkTopic = TargetProjectPath + @"\Content\Main\" + "mainPDFs.htm";
            //string pdfInterfaceLinkTopic = TargetProjectPath + @"\Content\Main\" + "mainInterfaceOverview.htm";

            //fetchPdfOverviews(BuildType, BRANCH, LANG, pdfLinkTopic, TargetProjectPath);

            if (BUILD_WEBHELP)
			{
                //Console.ReadKey();
                //Builder build = new Builder(@"C:\Program Files\MadCap Software\MadCap Flare 15\Flare.app\madbuild.exe", newProjectFileName, TargetFileName);
                modFile = FILEROOT + BRANCH + @"\OutputChanges.xml";                

                Builder build = new Builder(TargetProjectPath + "\\" + "GLTopNavi.flprj", targetName);
                
				Console.WriteLine("-------------------------------------");
                
            }

			if(MOD_OUTPUT)
			{
				OutputModder mod = new OutputModder(TargetProjectPath, targetName, modFile, MainProject);
			}
            Console.WriteLine("Done with " + TargetProjectPath + ".");
            if(!CLOSEWHENREADY) 
                Console.ReadKey();
		}

        private static void createPdfDestinations() 
        {
            //In jedem pdf target soll stehen:

            //< Destinations >
            //    < Destination
            //      Link = "/Project/Destinations/PDF.fldes"
            //      Publish = "true"
            //      IsResource = "false" />
            //</ Destinations >

            //Inhalt von PDF.fldes:

            //<? xml version = "1.0" encoding = "utf-8" ?>
            //    < CatapultDestination   
            //      Version = "1"
            //      Comment = "Destination automatically created by FlareTool01"
            //      Host = ""
            //      User = "MyAccountId"
            //      Password = ""
            //      Directory = "C:\docuR2022\docusrc\00\GLHelp\Content\Main"
            //      Port = ""
            //      ViewUrl = ""
            //      Type = "file"
            //      RemoveStale = "false" >
            //</ CatapultDestination >       
            
            //Und im Batch target steht eine Referenz auf das PDF target und Build und Publish = true:
            //< Target
            //  Icon = "System.Drawing.Bitmap"
            //  Name = "MainGL-PDF"
            //  Type = "PDF"
            //  Build = "true"
            //  Publish = "true" />

            //Dann müssen noch die Links auf der pdf-Seite des Hilfesystems angepasst werden.
        }

        private static void fetchPdfOverviews(string BuildType, string BRANCH, string LANG, string pdfLinkTopic, string TargetProjectPath) 
        {
            List<string> standardPdfs = new List<string>();
            List<string> interfacePdfs = new List<string>();

            XmlDocument batchTargetDocument = new XmlDocument();
            XmlDocument pdfTopic = new XmlDocument();
            XmlNodeList targets = null;

            string batchtargetFile = "";
            int errorlevel = 0;
            string pdfSourceDirectory = "";

            switch (BuildType) 
            {
                case "GLHelp":
                    pdfSourceDirectory = FILEROOT + BRANCH + @"\" + LANG + @"\GLHelp\Content\Main";
                    batchtargetFile = TargetProjectPath + @"\Project\Targets\GLPDFBatchTarget.flbat"; 
                    break;
                case "V4Help":
                    pdfSourceDirectory = FILEROOT + BRANCH + @"\hlp\TDM\" + LANG + @"\PDF";
                    batchtargetFile = TargetProjectPath + @"\Project\Targets\V4PDFBatchTarget.flbat"; 
                    break;
                default: 
                    errorlevel++; 
                    break;
            }

            if (!Directory.Exists(pdfSourceDirectory))
            {
                Console.WriteLine(pdfSourceDirectory + " not found.");
                errorlevel++;
            }
            else
            {
                if (File.Exists(pdfLinkTopic)) 
                    tools1.LoadXmlFile(pdfTopic, pdfLinkTopic);
                else
                {
                    Console.WriteLine(pdfLinkTopic + " not found.");
                    errorlevel++;
                }
            }
            if (errorlevel > 0) return;

            //Vorhandene pdfs im Content Ordner sammeln und sortieren
            //TODO: Warum nicht beim Kopieren der pdf-Targets gleich nach dem Dateinamen suchen?
            foreach (string file in Directory.GetFiles(pdfSourceDirectory))
            {
                if (file.EndsWith(".pdf"))
                {
                    string pdfFileName = Path.GetFileName(file);
                    if (!pdfFileName.StartsWith("i"))
                        standardPdfs.Add(Path.GetFileName(file));
                    else
                        interfacePdfs.Add(Path.GetFileName(file));
                }
            }
            
            if (standardPdfs.Count + interfacePdfs.Count > 0)
            {
                standardPdfs.Sort();
                interfacePdfs.Sort(); 
            }

            //List<string> tagets = 
            //int subProjects = MainProject.countChildProjects(MainProject, true);

            XmlNode body = pdfTopic.SelectSingleNode("//body");
            //alle Links zu pdfs löschen
            XmlNode linkedImage = pdfTopic.SelectSingleNode("//body/p/a/img");
            while (linkedImage != null)
            {
                XmlNode pa = linkedImage.ParentNode.ParentNode;
                body.RemoveChild(pa);
                linkedImage = pdfTopic.SelectSingleNode("//body/p/a/img");
            }
            //alle header2 löschen
            XmlNode header2 = pdfTopic.SelectSingleNode("//body/h2");
            while (header2 != null)
            {
                XmlNode pa = header2;
                body.RemoveChild(pa);
                header2 = pdfTopic.SelectSingleNode("//body/h2");
            }

            if (standardPdfs.Count > 0)
            {
                XmlNode standardHeader = body.AppendChild(pdfTopic.CreateElement("h2"));
                switch (LANG)
                {
                    case "00": standardHeader.InnerText = "Handbücher zu Standard Modulen"; break;
                    case "01": standardHeader.InnerText = "Manuals for Standard Modules"; break;
                    case "02": standardHeader.InnerText = "Manuels des modules standard"; break;
                    case "03": standardHeader.InnerText = "Manuali per moduli standard"; break;
                    case "17": standardHeader.InnerText = "默认模块手册"; break;
                    default: standardHeader.InnerText = "Manuals for Standard Modules"; break;
                }
            }

            if (File.Exists(batchtargetFile))
            {
                tools1.LoadXmlFile(batchTargetDocument, batchtargetFile);
                targets = batchTargetDocument.SelectNodes("//Target");
                foreach (XmlNode target in targets)
                {
                    XmlAttribute Build = target.Attributes["Build"]; if (Build == null) continue;
                    XmlAttribute Publish = target.Attributes["Publish"]; if (Publish == null) continue;
                    XmlAttribute Type = target.Attributes["Type"]; if (Type == null) continue;
                    if ((Build.Value == "true") && (Publish.Value == "true") && (Type.Value == "PDF"))
                    {
                        string pdfTargetFile = TargetProjectPath + @"\Project\Targets\" + target.Attributes["Name"].Value + ".fltar";
                        if (File.Exists(pdfTargetFile))
                        {
                            XmlDocument pdftarget = new XmlDocument();
                            tools1.LoadXmlFile(pdftarget, pdfTargetFile);
                            XmlNode OutputFile = pdftarget.SelectSingleNode("//CatapultTarget/@OutputFile");
                            string pdfManualFileName = OutputFile.Value + ".pdf";
                            if (standardPdfs.Contains(pdfManualFileName))
                            {
                                XmlNode ModuleInitials = pdftarget.SelectSingleNode("//CatapultTarget/Variables/Variable[@Name='MyVariables/ModuleInitials']");
                                XmlNode ModuleName = pdftarget.SelectSingleNode("//CatapultTarget/Variables/Variable[@Name='MyVariables/ModuleName']");
                                XmlText n = null;
                                if (ModuleInitials.InnerText == "")
                                {
                                    n = body.OwnerDocument.CreateTextNode(" " + ModuleName.InnerText);
                                    if (ModuleName.OwnerDocument.BaseURI.Contains("tdmNews"))
                                    {
                                        XmlNode VersionNumber = pdftarget.SelectSingleNode("//CatapultTarget/Variables/Variable[@Name='MyVariables/TDMVersionNumber']");
                                        string versionNumber = VersionNumber.InnerText;
                                        n.InnerText += " " + versionNumber;
                                    }
                                }
                                else
                                    n = body.OwnerDocument.CreateTextNode(" " + ModuleInitials.InnerText + " - " + ModuleName.InnerText);
                                addLinkToPdfManual(body, pdfManualFileName, n);
                                Console.WriteLine("Linked pdf manual " + pdfManualFileName + ".");
                            }
                            else 
                            {
                                Console.WriteLine("Could not link pdf manual " + pdfManualFileName + ". File not found.");
                                //Console.ReadLine();
                            }
                        }
                    }
                }

                if (interfacePdfs.Count > 0)
                {
                    XmlNode interfaceHeader = body.AppendChild(pdfTopic.CreateElement("h2"));
                    switch (LANG)
                    {
                        case "00": interfaceHeader.InnerText = "Handbücher zu Schnittstellen"; break;
                        case "01": interfaceHeader.InnerText = "Manuals for Interfaces"; break;
                        case "02": interfaceHeader.InnerText = "Manuels des interfaces"; break;
                        case "03": interfaceHeader.InnerText = "Manuali per interfacce"; break;
                        case "17": interfaceHeader.InnerText = "界面手册"; break;
                        default: interfaceHeader.InnerText = "Manuals for Interfaces"; break;
                    }
                }

                pdfTopic.Save(pdfLinkTopic);
            }
        }
        
        private static void addLinkToPdfManual(XmlNode body, string pdfManualFileName, XmlText n) 
        {
            XmlElement p = body.OwnerDocument.CreateElement("p");
            XmlElement a = body.OwnerDocument.CreateElement("a");
            XmlAttribute href = body.OwnerDocument.CreateAttribute("href");
            href.Value = pdfManualFileName;
            a.Attributes.Append(href);
            XmlAttribute targ = body.OwnerDocument.CreateAttribute("target");
            targ.Value = "_blank";
            XmlAttribute a_class = body.OwnerDocument.CreateAttribute("class");
            a_class.Value = "translatedLink";

            a.Attributes.Append(targ);
            a.Attributes.Append(a_class);
            p.AppendChild(a);
            XmlElement image = body.OwnerDocument.CreateElement("img");
            XmlAttribute src = body.OwnerDocument.CreateAttribute("src");
            src.Value = "../Resources/Images/general/iconPdf.png";
            image.Attributes.Append(src);
            XmlAttribute clas = body.OwnerDocument.CreateAttribute("class");
            clas.Value = "href";

            

            a.AppendChild(image);            
            a.AppendChild(n);
            image.Attributes.Append(clas);
            body.AppendChild(p);
        }

        private static List<string> findPdfTargets(string path) 
        {
            List<string> pdfTargets = new List<string>();
            foreach (string file in Directory.GetFiles(path + "\\Project\\Targets")) 
            {
                if (file.EndsWith("-PDF.fltar")) pdfTargets.Add(file);
            }
            return pdfTargets;
        }

        private static void CollectStyles(FlareProject[] projects)
		{
			Dictionary<string,int> globalStylesByReference = new Dictionary<string,int>();

			foreach(FlareProject p in projects)
			{
				foreach(KeyValuePair<string,int> entry in p.StyleReferences)
				{
					if(!globalStylesByReference.ContainsKey(entry.Key)) globalStylesByReference.Add(entry.Key,entry.Value);
					else globalStylesByReference[entry.Key] = globalStylesByReference[entry.Key] + entry.Value;
					foreach(FlareProject sp in p.SubProjects)
					{
						foreach(KeyValuePair<string,int> subEntry in sp.StyleReferences)
						{
							if(!globalStylesByReference.ContainsKey(subEntry.Key)) globalStylesByReference.Add(subEntry.Key,subEntry.Value);
							else globalStylesByReference[subEntry.Key] = globalStylesByReference[subEntry.Key] + subEntry.Value;
						}
					}
				}

			}

			Console.WriteLine("");
			Console.WriteLine("Style Usage:");
			int l = 0;
			foreach(string style in globalStylesByReference.Keys) if(style.Length > l) l = style.Length;

			//// https://docs.microsoft.com/de-de/dotnet/csharp/language-reference/keywords/orderby-clause
			///Create the query.
			//IEnumerable<Student> sortedStudents =
			//	from student in students
			//	orderby student.Last ascending, student.First ascending
			//	select student;

			//globalStylesByReference.OrderBy()
			foreach(KeyValuePair<string,int> entry in globalStylesByReference)
			{
				Console.Write(entry.Key);
				for(int n = entry.Key.Length; n < l; n++) Console.Write(" ");
				Console.Write(";");
				Console.WriteLine(entry.Value.ToString());
			}
		}

		private static void CollectVariables(FlareProject[] projects)
		{
			List<string> globalVariables = new List<string>();
			List<string> globalTargetVariables = new List<string>();
			Dictionary<string,int> globalVariablesByReference = new Dictionary<string,int>();
			foreach(FlareProject p in projects)
			{
				foreach(string v in p.Variables)
					if(!globalVariables.Contains(v)) globalVariables.Add(v);
				foreach(FlareProject sp in p.SubProjects)
					foreach(string v in sp.Variables)
						if(!globalVariables.Contains(v)) globalVariables.Add(v);
				foreach(string v in p.TargetVariables)
					if(!globalTargetVariables.Contains(v)) globalTargetVariables.Add(v);
				foreach(FlareProject sp in p.SubProjects)
					foreach(string v in sp.TargetVariables)
						if(!globalTargetVariables.Contains(v)) globalTargetVariables.Add(v);
			}

			foreach(FlareProject p in projects)
			{
				foreach(KeyValuePair<string,int> entry in p.VariableReferences)
				{
					if(!globalVariablesByReference.ContainsKey(entry.Key)) globalVariablesByReference.Add(entry.Key,entry.Value);
					else globalVariablesByReference[entry.Key] = globalVariablesByReference[entry.Key] + entry.Value;
					foreach(FlareProject sp in p.SubProjects)
					{
						foreach(KeyValuePair<string,int> subEntry in sp.VariableReferences)
						{
							if(!globalVariablesByReference.ContainsKey(subEntry.Key)) globalVariablesByReference.Add(subEntry.Key,subEntry.Value);
							else globalVariablesByReference[subEntry.Key] = globalVariablesByReference[subEntry.Key] + subEntry.Value;
						}
					}
				}

			}

			Console.WriteLine("");
			Console.WriteLine("Variables:");
			foreach(string v in globalVariables) Console.WriteLine(v);

			Console.WriteLine("");
			Console.WriteLine("TargetVariables:");
			foreach(string v in globalTargetVariables) Console.WriteLine(v);

			Console.WriteLine("");
			Console.WriteLine("Variable Usage:");
			foreach(KeyValuePair<string,int> entry in globalVariablesByReference)
				Console.WriteLine(entry.Key + ": " + entry.Value.ToString());

		}

		private static void applySettingsFromFile(string filename, string rootPath)
        {
            XmlDocument Settings = new XmlDocument();
            tools1.LoadXmlFile(Settings, filename);
            string[] subDirs = { "00", "01", "02", "03" };
            
            foreach(string subDir in subDirs)
            {
                string lang = "";
                switch (subDir)
                {
                    case "00": lang = "de-DE"; break;
                    case "01": lang = "en-US"; break;
                    case "02": lang = "fr"; break;
                    case "03": lang = "it"; break;
                    case "17": lang = "zh-CN"; break;
                    default: break;
                }
                XmlNodeList pdfFileNames = Settings.SelectNodes("//project/pdfFileName/" + lang);
                foreach (XmlElement pF in pdfFileNames)
                {
                    string Token = pF.ParentNode.ParentNode.Attributes["token"].Value;
                    string pdfFileNameFile = rootPath + "\\" + subDir + "\\" + Token + @"\Project\Targets\" + Token + "-PDF.fltar";
                    string pdfOutputFileName = pF.InnerText;

                    if (File.Exists(pdfFileNameFile))
                    {
                        XmlDocument target = new XmlDocument();
                        tools1.LoadXmlFile(target, pdfFileNameFile);
                        if (target.DocumentElement.Attributes["OutputFile"].Value != pdfOutputFileName)
                        {
                            Console.WriteLine(Token + " " + lang + ": " + "Changing PDF Output File Name: ");
                            Console.WriteLine(target.DocumentElement.Attributes["OutputFile"].Value);
                            target.DocumentElement.Attributes["OutputFile"].Value = pdfOutputFileName;
                            Console.WriteLine(target.DocumentElement.Attributes["OutputFile"].Value);
                            //Console.WriteLine("");
                        }
                        else
                        {
                            if (LOUD) Console.WriteLine(lang + " " + Token + ": " + target.DocumentElement.Attributes["OutputFile"].Value);
                        }
                        target.Save(pdfFileNameFile);
                        tools1.BeautifyXml(pdfFileNameFile, pdfFileNameFile);                        
                    }
                }

            }            
        }
                
        private static void createNewSettingFile(FlareProject[] mainProjects, string projectPath, string filename)
        {
            XmlDocument Settings = tools1.createXmlFile(filename, "settings");

            foreach(FlareProject mP in mainProjects)
            {
                addProjectSettings(mP, Settings, projectPath);
                foreach (FlareProject p in mP.SubProjects)
                {
                    addProjectSettings(p, Settings, projectPath);
                }

            }
            Settings.Save(filename);
        }

		private static void addProjectSetting(XmlDocument Settings, string projectToken, string newNodeName, string value)
		{
			XmlNode ProjectNode = Settings.SelectSingleNode("//project[@token = '" + projectToken + "']");
			if(ProjectNode == null)
			{
				XmlNode newNode = Settings.CreateElement(newNodeName);
				newNode.InnerText = value;
				ProjectNode.AppendChild(newNode);
			}
		}

		private static void addProjectSettings(FlareProject p, XmlDocument Settings, string projectPath)
        {
            XmlNode n = null;
            XmlNode pdfFileName = null;
            XmlNode name = null;
            if (Settings.SelectSingleNode("//project[@token = '" + p.Token + "']") == null)
            {
                n = tools1.addChildNode(Settings, Settings.DocumentElement, "project", "");
                name = tools1.addAttribute(Settings, n, "token", p.Token);
                if (Settings.SelectSingleNode("//project[@token = '" + p.Token + "']//pdfFileName") == null)
                {
                    XmlNode newNode = Settings.CreateElement("pdfFileName");
                    n.AppendChild(newNode);
                }
            }
            
            string rootPath = projectPath.Substring(0, projectPath.LastIndexOf('\\'));
            rootPath = rootPath.Substring(0, rootPath.LastIndexOf('\\'));
            string[] subDirs = { "00", "01", "02", "03" };
            string lang = "";
            foreach (string subDir in subDirs)
            {
                switch (subDir)
                {
                    case "00": lang = "de-DE"; break;
                    case "01": lang = "en-US"; break;
                    case "02": lang = "fr"; break;
                    case "03": lang = "it"; break;
                    default: break;
                }

                string pdfFileNameFile = rootPath + "\\" + subDir + "\\" + p.Token + @"\Project\Targets\" + p.Token + "-PDF.fltar";
                if (File.Exists(pdfFileNameFile))
                {
                    XmlDocument target = new XmlDocument();
                    tools1.LoadXmlFile(target, pdfFileNameFile);
                    string f = target.DocumentElement.Attributes["OutputFile"].Value.ToString().Replace('/', '\\');

                    n = Settings.SelectSingleNode("//project[@token = '" + p.Token + "']/pdfFileName");
                    if(Settings.SelectSingleNode("//project[@token = '" + p.Token + "']/pdfFileName/" + lang) == null)
                    {
                        XmlNode pdfFileNameNode = Settings.CreateElement(lang);
                        pdfFileNameNode.InnerText = f;
                        n.AppendChild(pdfFileNameNode);
                        Console.WriteLine(f);
                    }
                }
            }
        }
        
        static void ImportSettingFromCsv(string SettingsXmlFileName, string CsvFile, string ImportProperty)
        {
            if(!File.Exists(SettingsXmlFileName) || !File.Exists(CsvFile))
            {
                if (!File.Exists(SettingsXmlFileName)) Console.WriteLine(SettingsXmlFileName + " not found.");
                if (!File.Exists(CsvFile)) Console.WriteLine(CsvFile + " not found.");
                return;
            }
            bool changed = false;
            TextDatei t = new TextDatei();
            string s = t.ReadFile(CsvFile).Replace("\r\n", "\n");
            string[] lines = s.Split('\n');

            XmlDocument settings = new XmlDocument();
            tools1.LoadXmlFile(settings, SettingsXmlFileName);

            foreach (string line in lines)
            {
                string[] values = line.Split(',');
                string token = values[0];
                XmlNode project = settings.SelectSingleNode("//project[@token='" + token + "']");
                if (project != null)
                {
                    string lang = "";
                    for (int i = 1; i < values.Length; i++)
                    {
                        string v = values[i];
                        v = v.Replace("/", " ");
                        v = v.Replace("\\", " ");
                        switch (i)
                        {
                            case 1:
                                v = v.Replace(" inkl. Serverlizenz", "");
                                if (token == "OLG") v = v.Replace(" Basis TDM V4", "");
                                lang = "de-DE";
                                break;
                            case 2:
                                v = v.Replace(" incl. server license", "");
                                if (token == "OLG") v = v.Replace(" TDM V4 based", "");
                                lang = "en-US";
                                break;
                            case 3:
                                v = v.Replace(" incl. licence de serveur", "");
                                if (token == "OLG") v = v.Replace(" basé sur TDM V4", "");
                                lang = "fr";
                                break;
                            case 4:
                                v = v.Replace("incl. licenza server", "");
                                if (token == "OLG") v = v.Replace(" base TDM V4", "");
                                lang = "it";
                                break;
                            default:
                                break;
                        }
                        XmlNode val = settings.SelectSingleNode("//project[@token='" + token + "']/ImportProperty/" + lang);
                        v = token + " - " + v;
                        if (val != null)
                        {
                            if (val.InnerText != v)
                            {
                                Console.WriteLine(lang + ": " + val.InnerText);
                                string sep = ""; while (sep.Length < lang.Length) sep += " ";
                                Console.WriteLine(sep + "  " + v);
                                val.InnerText = v;
                                changed = true;
                            }
                        }
                        else
                        {
                            XmlElement name = settings.CreateElement(lang);
                            name.InnerText = v;
                            XmlNode pdf = settings.SelectSingleNode("//project[@token='" + token + "']/ImportProperty");
                            pdf.AppendChild(name);
                        }
                    }
                }
            }
            if (changed)
            {
                settings.Save(SettingsXmlFileName);
                Console.WriteLine("Saved Settings in: " + SettingsXmlFileName);
            }
            Console.WriteLine("Merged " + SettingsXmlFileName + " with " + CsvFile);
            Console.ReadKey();
        }

        /// <summary>
        /// Entfernt bookmark von der Help ID, wenn Topic da ist, aber das Bookmark darin fehlt.
        /// wird nicht mehr gebraucht, da fetchHelpKeys entsprechend erweitert wurde.
        /// </summary>
        /// <param name="alias"></param>
        /// <param name="TargetProjectPath"></param>
        //private static void checkHelpKeyBookmarks(XmlDocument alias, string TargetProjectPath) 
        //{
        //    XmlNodeList HelpKeys = alias.SelectNodes("//Map");
        //    int changes = 0;
        //    int misses = 0;
        //    foreach (XmlNode HelpKey in HelpKeys) 
        //    {
        //        if (HelpKey.Attributes["Link"].Value.Contains("#")) 
        //        {
        //            string [] temp = HelpKey.Attributes["Link"].Value.Trim().Split('#');
        //            string bookmark = temp[1];
        //            string file = TargetProjectPath + temp[0];
        //            if (File.Exists(file)) 
        //            {
        //                XmlDocument f = new XmlDocument();
        //                tools1.LoadXmlFile(f, file);

        //                if (f.SelectSingleNode("//a[@name='" + bookmark + "']") == null) 
        //                {
        //                    HelpKey.Attributes["Link"].Value = temp[0];
        //                    changes++;
        //                }
        //            }
        //            else
        //                misses++;
        //        }
        //    }
        //    if(changes > 0) 
        //    {
        //        Console.WriteLine("removed " + changes.ToString() + " bookmarks.");
        //    }
        //    if (misses > 0)
        //    {
        //        Console.WriteLine(misses.ToString() + " Help keys won't hit a target topic.");
        //    }
        //}

        private static void fetchHelpKeys (XmlDocument alias, string TargetProjectPath)
        {
            //TODO: Darf nicht (nur) nach nicht vorhandenen Source-Topics schauen, sonern auch (UND) danach, 
            // ob das Topic vom Target direkt - oder indirekt - benötigt wird (conditions::in_includes)
            int i = 0;
            List<string> checkKeys = new List<string>();
            List<string> multipleKeys = new List<string>();
            //mehrfache Help keys finden:
            while (i < alias.DocumentElement.ChildNodes.Count)
            {
                XmlNode Map = alias.DocumentElement.ChildNodes[i];

                string key = "";
                if (Map.Attributes["Name"] != null)
                {
                    key = Map.Attributes["Name"].Value;
                    if (!checkKeys.Contains(key))
                    {
                        checkKeys.Add(key);
                    }
                    else
                    {
                        if (!multipleKeys.Contains(key)) multipleKeys.Add(key);
                    }
                }
                //
                if (Map.Attributes["Link"] != null)
                {
                    string linkedFile = Map.Attributes["Link"].Value.Replace("/", "\\");
                    string bookmark = "";
                    if (linkedFile.Contains('#')) 
                    {
                        bookmark = linkedFile.Substring(linkedFile.IndexOf('#') + 1);
                        linkedFile = linkedFile.Substring(0, linkedFile.IndexOf('#'));
                    }                    
                    if (File.Exists(TargetProjectPath + linkedFile)) 
                    {
                        if (bookmark != "")
                        {
                            XmlDocument targetTopic = new XmlDocument();
                            targetTopic.Load(TargetProjectPath + linkedFile);
                            XmlNode anchor = targetTopic.SelectSingleNode("//a[@name='" + bookmark + "']");
                            if (anchor == null)
                            {
                                Map.Attributes["Link"].Value = linkedFile.Replace("\\", "/");
                                Console.WriteLine("  Removed bookmark " + bookmark + " from "+ key + " Target Topic: " + linkedFile);
                            }
                        }
                        i++; 
                    }                        
                    else
                    {
                        Map.ParentNode.RemoveChild(Map);
                        if (bookmark != "")
                            Console.WriteLine("  Removed help key " + key + "@" + bookmark + ", Target Topic Missing: " + linkedFile);
                        else
                            Console.WriteLine("  Removed help key " + key + ", Target Topic Missing: " + linkedFile);
                    }                        
                }
                else
                {
                    Map.ParentNode.RemoveChild(Map);
                    i++;
                }
            }
            if (multipleKeys.Count > 0)
            {
                Console.WriteLine("Found multiple usage of Help keys ");
                foreach (string key in multipleKeys)
                {
                    XmlNodeList multiples = alias.SelectNodes("//Map[@Name = '" + key + "']");
                    foreach (XmlNode map in multiples)
                    {
                        Console.WriteLine("  " + map.Attributes["Name"].Value + " --> " + map.Attributes["Link"].Value);
                    }
                }
            }
        }

        private static void displayProjectInfo(FlareProject p,int lvl)
		{
			//Console.WriteLine("Project: " + p.Token);
			Console.WriteLine("level: " + lvl);
			Console.WriteLine("LinkedTopics: " + p.LinkedTopics.Count);
			Console.WriteLine("TOCLinkedTopics: " + p.TOCLinkedTopics.Count);
			Console.WriteLine("Styles: " + p.Styles.Count);
			Console.WriteLine("PageLayouts: " + p.PageLayouts.Count);
			Console.WriteLine("MasterPages: " + p.MasterPages.Count);
			Console.WriteLine("Skins: " + p.Skins.Count);
			Console.WriteLine("TOCs: " + p.TOCs.Count);
			Console.WriteLine("Targets: " + p.Targets.Count);
			Console.WriteLine("Images: " + p.Images.Count);
			Console.WriteLine("Pdfs: " + p.Pdfs.Count);
			Console.WriteLine(" ----");
			foreach(FlareProject sp in p.SubProjects) displayProjectInfo(sp,lvl + 1);
		}
        
        private static void CreateHeaderFile(XmlDocument alias, string aliasFile)
        {
            string headerFileName = aliasFile.Replace(".flali", ".h");
            XmlNodeList Ids = alias.SelectNodes("//Map");
            int i = 1000;
            string s = "";
            foreach (XmlNode Id in Ids)
            {
                s += "#define " + Id.Attributes["Name"].Value + " " + i.ToString() + "\r\n";
                i++;
            }
            TextDatei header = new TextDatei();
            header.WriteFile(headerFileName, s);
            Console.WriteLine("Saved header file: " + headerFileName);
            //#define lgmBook 3000
        }		
    }
}
