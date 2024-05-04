using ConsoleApp1;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using tools;
using txtFiles;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Security.Permissions;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using static System.Windows.Forms.LinkLabel;
using System.Security.Policy;
using System.Collections;

namespace FlareTool01
{
    class FlareProject
	{
		private const int READCONFIG = 0;
		private const int WRITECONFIG = 1;
        private const int WRITEFILECONFIG = 2;

        //private string[] AmeWithExtensions =
        //{
        //    "iCATIA5",
        //    "iCCW",
        //    "iCEDG",
        //    "iCGIBBS",
        //    "iCMGBS15",
        //    "iCMVMDE",
        //    "iCTOP7",
        //    "iMALC",
        //    "iMEST iCEST iCESP",
        //    "iMCREO iCCREO",
        //    "iMDLM6 iCDLM6",
        //    "iMEST2 iCEST2",
        //    "iMEUR iCEUR",
        //    "iMG2C iCG2C",
        //    "iMHYP iCHYP",
        //    "iMIMS iCIMS",
        //    "iMLRTM iCLRTM",
        //    "iMMCM iCMCM",
        //    "iMNCS iCNCS",
        //    "iMNX iCNX",
        //    "iMPML iCPML",
        //    "iMSCA iCSCA",
        //    "iMST iCST",
        //    "iMVCT iCVCT",
        //    "iMTBS iCTBS",
        //    "iMVTS iCVTS",
        //    "iMXPT iCXPT",
        //    "iMFCM iCFCM",
        //    "iMG2C iCG2C"
        //};

        bool Loud;

        string TARGETSUBPATH = @"\Project\Targets\";
		string TOCSUBPATH = @"\Project\TOCs\";
		string TOPICSUBPATH = @"\Content\";
		string VARIABLESSUBPATH = @"\Project\VariableSets\";
        string STYLESSUBPATH = @"\Content\Resources\Stylesheets\";
        string TABLESTYLESSUBPATH = @"\Content\Resources\TableStyles\";
        string RESOURCESSUBPATH = @"\Content\Resources\";
        string MANUALSUBPATH = @"\Content\Resources\Manual\";
        string MASTERPAGESUBPATH = @"\Content\Resources\MasterPages\";
        string IMAGESUBPATH = @"\Content\Resources\Images\";
        string IMAGESUBPATHGENERAL = @"\Content\Resources\Images\general\";
        string IMAGESUBPATHSPECIFIC = @"\Content\Resources\Images\language-specific\";
        string PAGELAYOUTSUBPATH = @"\Content\Resources\PageLayouts\";
		string CONDITIONSUBPATH = @"\Project\ConditionTagSets\";
		string SKINSUBPATH = @"\Project\Skins\";
		string SCRIPTSUBPATH = @"\Content\Resources\Scripts\";
        string DESTINATIONSSUBPATH = @"\Project\Destinations\";

		public List<string> ProjectFiles = new List<string>();
		public List<string> TOCs = new List<string>();
		public List<string> LinkedTopics = new List<string>();
		public List<string> TOCLinkedTopics = new List<string>();
        public List<string> externalTopics = new List<string>();
        public List<string> ManualPages = new List<string>();
        public List<string> Pdfs = new List<string>();
		public List<string> Targets = new List<string>();
		public List<string> Skins = new List<string>();
		public List<string> Scripts = new List<string>();
		public List<string> Styles = new List<string>();
        public List<string> ConditionTagSets = new List<string>();
		public List<string> Images = new List<string>();
		public List<string> Variables = new List<string>();

		public List<string> TargetVariables = new List<string>();
		public List<string> PageLayouts = new List<string>(); //(tocs, targets, flprj)
		public List<string> MasterPages = new List<string>(); //(stylesheets, targets, topics)

		public List<styleClass> cssClasses = new List<styleClass>();

		public List<FlareProject> SubProjects = new List<FlareProject>();
		public List<string> SubProjectFiles = new List<string>();

        public Dictionary<string, int> VariableReferences = new Dictionary<string, int>();
        public Dictionary<string, int> StyleReferences = new Dictionary<string, int>();

        private List<string> includes = new List<string>();
		private List<string> excludes = new List<string>();

        private List<string> cache = new List<string>();
        //List<FlareProject> Projects;		

        public string ProjectPath = "";
        public string ProjectParentPath = "";
        public string batchtargetFile = "";

        FileInfo fi = null;
        DateTime lastWriteTime = DateTime.MinValue;

        public string Token = "";
        //private string Branch;
        //private string Language;
        public int Level;
		private int RW;
        private int varReferences;

        private string sep;
        private string BuildType;
        
        //SqlConnection conn;
        //bool DataBase = false;
        XmlDocument Logfile;
        XmlNode LogNode;

		XmlDocument Doc;
		XmlNode settingsNode;
        XmlNode errorsNode;
        //XmlDocument SettingFile;
        //string SettingsFilePath = @"C:\docuR2020\BuildTargets.xml";

        XmlDocument VarFile = new XmlDocument();        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">Path to the Flare project of the build</param>
        /// <param name="target">Filename of the primary target</param>
        /// <param name="level">starts with 0</param>
        /// <param name="buildType">GLHelp, V4Help</param>
        /// <param name="branch">Repository branch - redundant?</param>
        /// <param name="lang">Language code (00,01,02 ...)- redundant?</param>
        /// <param name="loud">true for excessive messaging</param>
        /// <param name="doc">some protocol file</param>
        /// <param name="rw">READ/WRITE lvl</param>
        public FlareProject(string path, string target, int level, string buildType, /*string branch, string lang,*/ bool loud, XmlNode doc = null, int rw = READCONFIG, XmlNode errors = null)
		{
			int count = 0;
            //Branch = branch;
            //Language = lang;
            ProjectPath = path;
            ProjectParentPath = ProjectPath.Substring(0,ProjectPath.LastIndexOf('\\'));
            Token = path.Substring(ProjectPath.LastIndexOf('\\') + 1);
            //if (Token.Contains("NCM")) 
            //{
            //    Console.WriteLine(Token);
            //    Console.ReadLine();
            //}
            errorsNode = errors;
            Level = level;
            Loud = loud;
            BuildType = buildType;
            sep = ""; for (int j = 0; j < Level; j++) sep += "  ";
            RW = rw;
            //if((RW > 0) && (SettingFile == null))
            //{
            //    SettingFile.Load(SettingsFilePath);
            //} 
            if(Level == 0) 
                searchBatchTarget();            
			if(doc != null) 
                Doc = doc.OwnerDocument;
			if((Doc != null) && (RW >= READCONFIG))
			{
				settingsNode = Doc.CreateElement("project");
				doc.AppendChild(settingsNode);
                if (Token == "tdmConventions")
                    Token = "TDMConventions";
                XmlAttribute token = Doc.CreateAttribute("token");
				token.Value = Token;
				settingsNode.Attributes.Append(token);				
				
				if(RW == WRITECONFIG) 
                {
                    XmlAttribute projectPath = Doc.CreateAttribute("path");
                    projectPath.Value = ProjectPath;
                    settingsNode.Attributes.Append(projectPath);
                }
            }
			//if (Loud)             
            Console.WriteLine(sep + "-> Project: " + Token);
			includes.Clear();
			excludes.Clear();
            //if(!Directory.Exists(ProjectPath + VARIABLESSUBPATH))
            //{
            //    Console.WriteLine("Path is missing: " + ProjectPath + VARIABLESSUBPATH);
            //    Console.ReadLine();
            //}

            tools1.loadXmlDocumentProtected(VarFile, ProjectPath + VARIABLESSUBPATH + "MyVariables.flvar", "Flare Project.Constrtuctor");

			foreach(string file in Directory.GetFiles(path))
				if (file.EndsWith(".flprj")) ProjectFiles.Add(System.IO.Path.GetFileName(file));
			Console.WriteLine(sep + "Target: " + target);
            if ((Doc != null) && (RW >= READCONFIG))
            {
                XmlNode targetNode;
                targetNode = Doc.CreateElement("target");
                if (RW == WRITECONFIG) 
                {
                    targetNode.Attributes.Append(Doc.CreateAttribute("FileName"));
                    targetNode.Attributes["FileName"].Value = target;
                }
                if (RW == WRITEFILECONFIG)
                {
                    targetNode.InnerText = target;
                }
                settingsNode.AppendChild(targetNode);
            }

            //Hier wird geschaut, was alles für den Build benötigt wird
            //TODO: An dieser Stelle wird auch beim V4-Build die Datei tmsglGlobalSearch.htm angezogen. 
            //Aus i-welchen Gründen findet sie den Weg in den V4-Online-Help Output, obwohl sie dort nicht verlinkt ist.

            //alt:
            count += checkUsedResources(target);
            count += checkUsedResources(searchPdfTarget(Token));
            if (Token.ToLower() == "main")
                count += checkUsedResources(searchPdfTarget("CAM"));
            count += checkUsedResources(searchPdfGLTarget());

            //neu: (falsch?)
            //count += checkUsedResources(searchPdfTarget(Token));
            ////if (Token.ToLower() == "main")
            ////    count += checkUsedResources(searchPdfTarget("CAM"));
            //count += checkUsedResources(searchPdfGLTarget());
            //count += checkUsedResources(target);

            checkVariableFiles();
            if(Directory.Exists(ProjectPath + CONDITIONSUBPATH)) 
            {
                foreach (string file in Directory.GetFiles(ProjectPath + CONDITIONSUBPATH))
                {
                    string localFileName = CONDITIONSUBPATH + System.IO.Path.GetFileName(file);
                    if (localFileName.EndsWith(".flcts") && !ConditionTagSets.Contains(localFileName))
                        ConditionTagSets.Add(localFileName);
                }
            }
            else 
            {
                if (ConditionTagSets.Count < 1)
                    Console.WriteLine(sep + Token + ": No condition files found.");
            }
            unifyLinkedTopics();
			int i = 0;

            while (i < LinkedTopics.Count)
			{                    
                parseTopic(LinkedTopics[i]);
                ParseForStyleReferences(LinkedTopics[i]);
				//Console.ReadLine();
				i++;
			}
			foreach(string file in TOCLinkedTopics)
			{
                ParseForStyleReferences(file);
				//Console.ReadLine();
			}
		}

		public DateTime GetLastWriteTime() 
        {
            return lastWriteTime;
        }

        private void unifyLinkedTopics()
        {
            LinkedTopics.Sort();
            int i = 0;
            while (i < LinkedTopics.Count)
            {
                string s = LinkedTopics[i];
                if (TOCLinkedTopics.Contains(s))
                    LinkedTopics.RemoveAt(i);
                else 
                    i++;
            }           
        }

		private int checkUsedResources(string target)
		{            
            if (target == "") return 0;
            //TODO: Exclude-Liste beim Aufruf erlauben
            if (target.Contains("Main-PDF.fltar")) 
            {
                Console.WriteLine("Excluding: " + target);
                //Console.ReadLine();
                return 0;
            }
            int count = 0;
			bool isOnlineTarget = false;
			count += checkTarget(target, ref isOnlineTarget);
			int i = 0;
			while (i < TOCs.Count)
			{
                //Console.WriteLine(sep + ProjectPath + TOCs[i]); //DEBUG ONLY
                parseToc(ProjectPath + TOCs[i], isOnlineTarget);
				i++;
			}            
           
            //foreach (string s in includes)
            //    Console.Write(s + "\",\"");
            //Console.WriteLine();
            //foreach (string s in excludes)
            //    Console.Write(s + "\",\"");
            //Console.ReadLine();
            return count;
        }

		private int parseToc(string TocFile, bool mergeProjects)
		{
			if(!File.Exists(TocFile))
			{
				Console.WriteLine(sep + Token + ": " + TocFile + " not found.");
				return 0;
			}
            //Console.WriteLine("DEBUG:" + sep + Token + ": " + TocFile);
            XmlDocument toc = new XmlDocument();
			toc.PreserveWhitespace = true;
	        tools1.LoadXmlFile(toc, TocFile);
			XmlNode root = toc.DocumentElement;           
			GetNodes(root, 0, mergeProjects);
			return 0;
		}

		private void GetNodes(XmlNode node, int level, bool mergeProjects)
		{            
            bool save = false;
            if (node.NodeType== XmlNodeType.Element)
			{                
                if (isUsed(node))
				{
					if(node.Name == "TocEntry")
					{
						if(node.Attributes["Link"] != null)
						{                            
                            string link = node.Attributes["Link"].Value.Replace('/','\\');                            
                            string bookmark = "";
							if(link.Contains('#'))
							{
								bookmark = link.Substring(link.IndexOf('#') + 1);
								link = link.Substring(0,link.IndexOf('#'));
								//Console.WriteLine(sep + level.ToString() + ": " + link + "  " + bookmark);
							}
							string fileExt = link.Substring(link.LastIndexOf("."));
							switch(fileExt)
							{
								case ".htm":                                    
                                    if(link.StartsWith(TOPICSUBPATH))
									{
										if(link.StartsWith(MANUALSUBPATH))
										{
											if(!ManualPages.Contains(link))
											{
												if(localFileExists(link))
												{
													ManualPages.Add(link);
													parseTopic(link);
												}
											}
										}
										else
										{
                                            if (!TOCLinkedTopics.Any(s => s.Equals(link, StringComparison.OrdinalIgnoreCase)))
                                            {
												if(localFileExists(link))
												{
                                                    TOCLinkedTopics.Add(link);
													parseTopic(link);
												}
                                                else 
                                                {
                                                    //TODO: Dies ist leider ein Fehler, der den Compiler anhält --> Überlegen, wie damit umgegangen werden soll!
                                                    //TODO: Behandlung gehört evtl. in die Copy-Routine verschoben!
                                                    Console.WriteLine(sep + ' ' + link + " not found - condition=\"Default.NotYetTranslated\" applied.");
                                                    
                                                    XmlAttribute conditions;
                                                    if (node.Attributes["conditions"] != null)
                                                        conditions = node.Attributes["conditions"];
                                                    else
                                                        conditions = node.OwnerDocument.CreateAttribute("conditions");
                                                    conditions.Value = "Default.NotYetTranslated";
                                                    node.Attributes.Append(conditions);
                                                    save = true;
                                                }
                                            }
										}
									}
									else
									{
                                        if (link.StartsWith("..")) 
                                        {
                                            //Root Path für alle
                                            //string externalPath = ProjectPath.Substring(0, ProjectPath.IndexOf(Token) - 1);
                                            //Nur externer Subpfad:
                                            string externalPath = "";
                                            //extern gelinkte Main Projekt - Topics sehen so aus: 
                                            if (link.StartsWith(@"..\..\..\"))
                                            {
                                                externalPath += "\\Main" + link.Substring(8);
                                            }
                                            else
                                            //alle anderen sehen so aus:
                                            //../../TPS/Content/tpsDataPre.htm
                                            {
                                                string[] temp = link.Split('\\');
                                                int i = 0;
                                                while ((i < temp.Length) && ((temp[i] == "..") || (temp[i] == "Subsystems")))
                                                    i++;                                                
                                                while (i < temp.Length)
                                                {
                                                    externalPath += "\\" + temp[i];
                                                    i++;
                                                }
                                            }
                                            if (!externalTopics.Contains(externalPath)) 
                                            {
                                                externalTopics.Add(externalPath);
                                                Console.WriteLine(sep + Token + " TOC links external file: " + externalPath);
                                            }
                                        }
                                        //Console.ReadKey();
                                    }
                                    break;
								case ".fltoc":
									if(!TOCs.Contains(link))
									{
										if(localFileExists(link))
										{
                                            //Console.WriteLine("parsing sub toc: " + ProjectPath + link);
                                            TOCs.Add(link);
											parseToc(ProjectPath + link, mergeProjects);
										}
									}
									break;
								case ".flprj":                              
                                    if (mergeProjects)
									{                                        
                                        string subProjectFile = link;                                        
                                        if (BuildType == "GLHelp" && !bookmark.ToLower().Contains("-topnavi"))
                                        {
                                            string f = node.BaseURI.Substring("file:///".Length).Replace("/", "\\").Trim();
                                            Console.WriteLine(sep + f + ": linking wrong target: " + link + "#" + bookmark);
                                            
                                            string externalToken = subProjectFile;
                                            if (externalToken.StartsWith("..\\"))
                                            {
                                                while (externalToken.StartsWith("..\\"))
                                                    externalToken = externalToken.Substring(3);
                                            }
                                            externalToken = externalToken.Substring(0,externalToken.IndexOf("\\"));
                                            string replacement = ProjectParentPath + "\\" + externalToken + "\\Project\\Targets\\" + externalToken + "-TopNavi.fltar";  //CLGR-TopNavi.fltar
                                            if (File.Exists(replacement))
                                            {
                                                Console.WriteLine(replacement + "found.");

                                            }
                                            //Console.ReadLine();
                                        }
                                        if (BuildType == "V4Help" && !bookmark.ToLower().Contains("-tdmnext"))
                                        {
                                            string f = node.BaseURI.Substring("file:///".Length).Replace("/", "\\").Trim();
                                            Console.WriteLine(sep + f + ": linking wrong target: " + link + "#" + bookmark);
                                            //.ReadLine();
                                        }
                                        while (subProjectFile.StartsWith("..\\")) subProjectFile = subProjectFile.Substring(3);
										subProjectFile = tools1.headPath(ProjectPath,1) + '\\' + subProjectFile;
										if(!SubProjectFiles.Contains(subProjectFile))
										{
											if(!File.Exists(subProjectFile))
											{
												Console.WriteLine(sep + Token + ": Missing linked subproject file: " + link);
											}
											else
											{
												SubProjectFiles.Add(subProjectFile);
												string subProjectPath = tools1.headPath(subProjectFile,1);
												FlareProject newSubProject = new FlareProject(subProjectPath, bookmark + ".fltar", Level + 1, BuildType, /*Branch, Language,*/ Loud, settingsNode, RW, errorsNode);
												SubProjects.Add(newSubProject);
											}
										}
									}
									//if (link.EndsWith("BCI.flprj")) System.Windows.Forms.MessageBox.Show(link + "\n" + bookmark);
									break;
							}//switch
							if(node.Attributes["PageLayout"] != null)
							{
								string pageLayout = node.Attributes["PageLayout"].Value.ToString().Replace('/','\\');
								if(!PageLayouts.Contains(pageLayout) && localFileExists(pageLayout))
								{
									PageLayouts.Add(pageLayout);
									checkPageLayout(pageLayout);
								}
							}
							if(node.Attributes["AbsoluteLink"] != null)
							{
								//Console.WriteLine("!!! Removing Absolute Link: " + node.Attributes["AbsoluteLink"].Value);
								node.Attributes.Remove(node.Attributes["AbsoluteLink"]);
								save = true;
							}
						}
					}
				}
				if(node.HasChildNodes) //TODO: Springt auch hierhin, wenn der isUsed(node)==false !!
				{
					foreach(XmlNode child in node.ChildNodes)
					{
						GetNodes(child,level + 1, mergeProjects);
					}
				}
                if (save) 
                {
                    string filename = node.OwnerDocument.BaseURI.Replace("/", "\\");
                    if (filename.StartsWith("file:"))
                        filename = filename.Substring(5);
                    while(filename.StartsWith("\\"))
                        filename = filename.Substring(1);
                    node.OwnerDocument.Save(filename);
                    tools1.BeautifyXml(filename, filename);
                }
                
			}
		}

        public void searchBatchTarget() 
        {
            string batchTargetId = "";
            switch (BuildType)
            {
                case "GLHelp":
                    batchTargetId = TARGETSUBPATH + "GLPDFBatchTarget.flbat";
                    if (File.Exists(ProjectPath + TARGETSUBPATH + "GLPDFBatchTarget.flbat"))
                        batchtargetFile = ProjectPath + batchTargetId;
                    break;
                case "V4Help":
                    batchTargetId = TARGETSUBPATH + "V4PDFBatchTarget.flbat";
                    if (File.Exists(ProjectPath + TARGETSUBPATH + "V4PDFBatchTarget.flbat"))
                        batchtargetFile = ProjectPath + batchTargetId;
                    break;
                default: return;
            }
            //Stört nicht, ist aber evtl. auch unnötig:
            if (File.Exists(batchtargetFile) && (Level == 0) && batchTargetId != "") 
                Targets.Add(batchTargetId);
        }

        public string searchPdfTarget(string Token)
		{
			string target = Token + "-PDF.fltar";
			if(File.Exists(ProjectPath + TARGETSUBPATH + target))
			{
				Console.WriteLine(sep + "Target: " + target);
                if ((Doc != null) && (RW >= READCONFIG))
                {
                    XmlNode targetNode;
                    targetNode = Doc.CreateElement("target");
                    if (RW == WRITECONFIG)
                    {
                        targetNode.Attributes.Append(Doc.CreateAttribute("FileName"));
                        targetNode.Attributes["FileName"].Value = target;
                    }
                    if (RW == WRITEFILECONFIG)
                    {
                        targetNode.InnerText = target;
                    }
                    settingsNode.AppendChild(targetNode);
                }
                return target;
			}
			else return "";
		}

		public string searchPdfGLTarget()
		{
			string target = Token + "GL-PDF.fltar";
			if(File.Exists(ProjectPath + TARGETSUBPATH + target))
			{
				Console.WriteLine(sep + "Target: " + target);

                //XmlDocument pdfTarget = new XmlDocument();
                //pdfTarget.Load(ProjectPath + TARGETSUBPATH + target);
                //string OutputFile = "";
                //if (pdfTarget.DocumentElement.Attributes["OutputFile"] != null)
                //{
                //    OutputFile = pdfTarget.DocumentElement.Attributes["OutputFile"].Value;
                //    OutputFile = ProjectPath.Replace(Token,"Main") + "\\Content\\" + OutputFile + ".pdf";
                //    if (File.Exists(OutputFile))
                //    {
                //        Console.WriteLine(Token + " " +  OutputFile);    
                //    }
                //}

                if ((Doc != null) && (RW >= READCONFIG))
                {
                    XmlNode targetNode;
                    targetNode = Doc.CreateElement("target");
                    if (RW == WRITECONFIG)
                    {
                        targetNode.Attributes.Append(Doc.CreateAttribute("FileName"));
                        targetNode.Attributes["FileName"].Value = target;
                    }
                    if (RW == WRITEFILECONFIG)
                    {
                        targetNode.InnerText = target;
                    }
                    settingsNode.AppendChild(targetNode);
                }
                return target;
			}
			else return "";
		}

		public string searchDatasheetTarget()
		{
			string target = Token + "-Datasheet.fltar";
			if(File.Exists(ProjectPath + TARGETSUBPATH + target))
			{
				Console.WriteLine(sep + "Target: " + target);
                if ((Doc != null) && (RW >= READCONFIG))
                {
                    XmlNode targetNode;
                    targetNode = Doc.CreateElement("target"); if (RW == WRITECONFIG)
                    {
                        targetNode.Attributes.Append(Doc.CreateAttribute("FileName"));
                        targetNode.Attributes["FileName"].Value = target;
                    }
                    if (RW == WRITEFILECONFIG)
                    {
                        targetNode.InnerText = target;
                    }
                    settingsNode.AppendChild(targetNode);
                }
                return target;
			}
			else return "";
		}
		
		public bool isUsedSub(XmlNode node)
		{
            //So sehen die Definitionen in den targets aus:
            //ConditionTagExpression="include[Default.ScreenOnly] exclude[Default.javascript or Default.NotYetTranslated or Default.OutOfDate or Default.PrintOnly or Default.ReviewOnly or Default.Under_Construction] "
            //ConditionTagExpression="include[Default.PrintOnly or Default.ReviewOnly] exclude[Default.javascript or Default.NotYetTranslated or Default.OutOfDate or Default.ScreenOnly or Default.Under_Construction] "
            //ConditionTagExpression="include[Default.PrintOnly] exclude[Default.javascript or Default.NotYetTranslated or Default.OutOfDate or Default.ReviewOnly or Default.ScreenOnly or Default.Under_Construction] "
            //So werden sie genutzt:
            //<div MadCap:conditions="Default.Under_Construction,Default.OutOfDate">
            //< MadCap:conditionalText MadCap:conditions = "Default.ScreenOnly" > Shopcontrol - </ MadCap:conditionalText > Grafische Auswertung der Historie </ h1 >

            if (node.Attributes == null) return true;

            if (node.Attributes["conditions"] == null)
            {
                if (node.Attributes["MadCap:conditions"] == null) return true;
                if (node.Attributes["MadCap:conditions"].Value == "") return true;
                foreach (string condition in includes) if (node.Attributes["MadCap:conditions"].Value.Contains(condition)) return true;
                return false;
            }
            if (node.Attributes["conditions"].Value == "") return true;
			foreach(string condition in includes) if(node.Attributes["conditions"].Value.Contains(condition)) return true;
			return false;
		}

		public bool isUsed(XmlNode node)
		{
            while (node.ParentNode != null)
			{
				if(isUsedSub(node)) 
					node = node.ParentNode;
				else
					return false;
			}			
			return true;
		}

		private bool localFileExists(string link)
		{
			if(File.Exists(ProjectPath + link)) return true;
			//DEBUG: Console.WriteLine(sep + Token + ": Missing linked local file: " + Token + link);
            return false;
		}

		private void parseTopic(string Topic)
		{            
            //if (Topic.EndsWith("PDFs.htm")) Console.WriteLine(Topic);
            if (File.Exists(this.ProjectPath + Topic))
			{
				XmlDocument topic = new XmlDocument();
				topic.PreserveWhitespace = true;
                try 
                {
                    tools1.LoadXmlFile(topic, this.ProjectPath + Topic);
                    findImageLinks(topic);
                    findHrefs(topic);
                    findSkins(topic);
                    findStyles(topic);
                    findMasterPages(topic);
                    ParseForVariableReferences(topic);
                    findLocalPdfReferences(topic);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(this.ProjectPath + Topic);
                    Console.ReadLine();
                }				
			}
		}

		private void findLocalPdfReferences(XmlDocument Topic)
		{
			XmlNodeList hrefNodes = Topic.DocumentElement.SelectNodes("//a");
			foreach(XmlNode a in hrefNodes)
			{
				if(a.Attributes["href"] != null)
				{
					string href = a.Attributes["href"].Value;
					if(!href.StartsWith("http") && href.EndsWith(".pdf"))
					{
						if(!Pdfs.Contains(TOPICSUBPATH + href) && localFileExists(TOPICSUBPATH + href))
							Pdfs.Add(TOPICSUBPATH + href);
					}
				}
			}
		}

		private void findImageLinks(XmlDocument Topic)
		{			
            XmlNodeList ImageNodes = Topic.DocumentElement.SelectNodes("//img");
			foreach(XmlNode img in ImageNodes)
			{
                //if (img.Attributes["src"].Value.EndsWith("cad2dglFunctionMenu.png"))
                //{
                //    Console.WriteLine("hier. 1");
                //    Console.ReadKey();
                //}
                //if (Topic.BaseURI.EndsWith("Copyright.htm"))
                //{
                //    Console.WriteLine("hier. 1");
                //    Console.ReadKey();
                //}
                string ImageFile = img.Attributes["src"].Value.Replace('/', '\\');
                if (isUsed(img))
				{
					//string ImageFile = img.Attributes["src"].Value.Replace('/','\\');
                    if (ImageFile.StartsWith("..\\..\\")) //cad3dglOverview.png
                    //if (ImageFile.EndsWith("tdmglBannerUsageToolItem.png"))
                    {
                        fetchBadImageLink(img);                        
                    }
                    ImageFile = TOPICSUBPATH + ImageFile;                    
                    if (ImageFile.Contains(".."))
					{
						//TODO: Dies ist leider ein Fehler, der den Compiler anhält --> Überlegen, wie damit umgegangen werden soll!
                        Console.WriteLine(sep + Token + ": Link to external image! " + Topic.BaseURI);
						continue;
					}
                    //if (ImageFile.EndsWith("cad2dGlStartViaBasicData.png"))
                    //{
                    //    Console.WriteLine(sep + Topic);
                    //    Console.ReadKey();
                    //}
                    if (!Images.Contains(ImageFile))
                    {                        
                        if(localFileExists(ImageFile))
                            Images.Add(ImageFile);
                        else 
                        {
                            if (errorsNode != null)
                            {
                                XmlNode missingFile = errorsNode.SelectSingleNode("//missingFile[text()='" + Token + ImageFile + "']");
                                if (missingFile == null)
                                {
                                    missingFile = errorsNode.OwnerDocument.CreateNode(XmlNodeType.Element, "missingFile", null);
                                    missingFile.InnerText = Token + ImageFile;
                                    errorsNode.AppendChild(missingFile);
                                }
                            }
                        }
                    }
				}
			}
		}

        private void fetchBadImageLink(XmlNode imageNode) 
        {
            //TODO: Sonderbehandlung wenn  
            //this.Token = tdmnews. Dann sollte die Kopierrichtung umgekehrt sein.
            string requiredFile = imageNode.Attributes["src"].Value.Replace('/', '\\');
            Console.WriteLine(sep + "found bad link to image: ");
            Console.WriteLine(sep + requiredFile);
            Console.WriteLine(sep + "in " + imageNode.OwnerDocument.BaseURI);
            //Console.ReadLine();
            //1. Fehlende Datei umkopieren:
            while (requiredFile.StartsWith("\\") || requiredFile.StartsWith("."))
                requiredFile = requiredFile.Substring(1);
            string sourceToken = requiredFile.Substring(0, requiredFile.IndexOf('\\'));
            string targetFile = this.ProjectPath + requiredFile.Substring(requiredFile.IndexOf('\\'));
            requiredFile = this.ProjectPath.Replace(this.Token, sourceToken) + requiredFile.Substring(sourceToken.Length);
            
            if (File.Exists(requiredFile)) 
            {
                if (!File.Exists(targetFile)) 
                {
                    if (!Directory.Exists(Path.GetDirectoryName(targetFile)))
                        tools1.createPath((Path.GetDirectoryName(targetFile)));
                    File.Copy(requiredFile, targetFile);
                    if (File.Exists(requiredFile + ".props")) 
                    {
                        File.Copy(requiredFile + ".props", targetFile + ".props", true);
                    }                    
                }
                //2. Link in Topic anpassen:
                string link = imageNode.Attributes["src"].Value;
                while (!link.StartsWith("Resources"))
                    link = link.Substring(1);
                imageNode.Attributes["src"].Value = link;
                string filename = imageNode.OwnerDocument.BaseURI.Substring(8).Replace('/', '\\');
                imageNode.OwnerDocument.Save(filename);
                //tools1.BeautifyXml(filename, filename);
            }
            else 
            {
                Console.WriteLine("File is also missing in the external help project. Link could not be fixed.");
            }                
            /*found bad link to image:
            *..\..\tdmNews\Content\Resources\Images\language-specific\tdmglBannerUsageToolItem.png
            *in file:///C:/docu/docusrc/00/tdmCompact/Content/tdmCompactToolAssembly02.htm
            */
        }

        private void findHrefs(XmlDocument Topic)
		{
            //if (Topic.BaseURI.Contains("Home.htm")) 
            //{
            //    Console.WriteLine(Topic.BaseURI);
            //    Console.ReadLine();
            //}
                

			XmlNodeList Links = Topic.DocumentElement.SelectNodes("//a");
			int count = -1;
			foreach(XmlNode link in Links)
			{
				count++;
				//Console.Write("Link " + count);
				//href vorhanden?
				if(link.Attributes["href"] == null) continue;
				//href lokal?
				string linkfile = link.Attributes["href"].Value.ToString().Replace('/','\\');

                if (linkfile.StartsWith(".."))
                {
                    //TODO: Man könnte alle externen Links global sammeln und am Ende nochmal prüfen, ob welche noch nicht kopiert wurden. 
                    //string externalPath = "";
                    ////extern gelinkte Main Projekt - Topics sehen so aus: 
                    //if (linkfile.StartsWith(@"..\..\..\"))
                    //{
                    //    externalPath += "\\Main" + linkfile.Substring(8);
                    //}
                    //else
                    ////alle anderen sehen so aus:
                    ////../../TPS/Content/tpsDataPre.htm
                    //{
                    //    string[] temp = linkfile.Split('\\');
                    //    int i = 0;
                    //    while ((i < temp.Length) && ((temp[i] == "..") || (temp[i] == "Subsystems")))
                    //        i++;
                    //    while (i < temp.Length)
                    //    {
                    //        externalPath += "\\" + temp[i];
                    //        i++;
                    //    }
                    //}
                    //if (!externalTopics.Contains(externalPath))
                    //{
                    //    externalTopics.Add(externalPath);
                    //    Console.WriteLine(sep + Token + " Topic links external file: " + externalPath);
                    //}
                    continue;
                }
				if(linkfile.StartsWith("http")) continue;
				if(linkfile.StartsWith("#")) continue;

				if(!linkfile.EndsWith("htm")) continue;
				if(linkfile.Contains("#")) linkfile = linkfile.Substring(0,linkfile.IndexOf('#'));

				if(isUsed(link))
				{
					//Console.Write(" is in use: " + linkfile);
					string topic = (TOPICSUBPATH + linkfile).Trim();
                    if (!LinkedTopics.Any(s => s.Equals(topic, StringComparison.OrdinalIgnoreCase)))
                    {
                        if (localFileExists(topic)) LinkedTopics.Add(topic);
                    }
                    //if (!LinkedTopics.Contains(topic))
                    //    if (localFileExists(topic))
                    //        LinkedTopics.Add(topic);
                }
				else
				{
					//Console.ReadLine();
					//Console.Write(" is NOT in use: " + linkfile);
				}
				//Console.WriteLine();
			}
		}

		private void findSkins(XmlDocument Topic)
		{
			XmlNamespaceManager manager = new XmlNamespaceManager(Topic.NameTable);
			manager.AddNamespace("MadCap","http://www.madcapsoftware.com/Schemas/MadCap.xsd");
			XmlNodeList Nodes = Topic.SelectNodes("//MadCap:searchBarProxy",manager);
			foreach(XmlNode node in Nodes)
			{
				string SkinFile = node.Attributes["data-mc-skin"].Value.Replace('/','\\');
				if(!Skins.Contains(SkinFile) && localFileExists(SkinFile)) Skins.Add(SkinFile);
			}
		}

		private void findMasterPages(XmlDocument Topic)
		{
            //<html xmlns:MadCap="http://www.madcapsoftware.com/Schemas/MadCap.xsd" class="HomePage" style="mc-master-page: url('Resources\MasterPages\HomePage.flmsp');">
			XmlNamespaceManager manager = new XmlNamespaceManager(Topic.NameTable);
			manager.AddNamespace("MadCap","http://www.madcapsoftware.com/Schemas/MadCap.xsd");
			XmlNode node = Topic.SelectSingleNode("//html", manager);
            if (node == null) return;
            
            XmlNode style = Topic.SelectSingleNode("//link[@rel ='style']");
            if (style != null) 
            {
                string MasterPage = node.Attributes["style"].Value.Replace('/', '\\');
                if (MasterPage.Contains('\''))
                {
                    MasterPage = MasterPage.Substring(MasterPage.IndexOf('\'') + 1);
                    MasterPage = MasterPage.Substring(0, MasterPage.IndexOf('\''));
                }
                MasterPage = MASTERPAGESUBPATH + System.IO.Path.GetFileName(MasterPage);
                if (!MasterPages.Contains(MasterPage))
                {
                    if (localFileExists(MasterPage))
                    {
                        MasterPages.Add(MasterPage);
                        checkMasterPage(MasterPage);
                    }
                }
            }


            XmlAttribute htmlStyle = node.Attributes["style"];
            if (htmlStyle != null)
            {
                string MasterPage = htmlStyle.Value.Replace('/', '\\');
                if (MasterPage.Contains('\''))
                {
                    MasterPage = MasterPage.Substring(MasterPage.IndexOf('\'') + 1);
                    MasterPage = MasterPage.Substring(0, MasterPage.IndexOf('\''));
                }
                MasterPage = MASTERPAGESUBPATH + System.IO.Path.GetFileName(MasterPage);
                if (!MasterPages.Contains(MasterPage))
                {
                    if (localFileExists(MasterPage))
                    {
                        MasterPages.Add(MasterPage);
                        checkMasterPage(MasterPage);
                    }
                }
            }
        }
		
		private void findStyles(XmlDocument Topic)
		{
			//<link href="Resources/Stylesheets/HomePage.css" rel="stylesheet" type="text/css" />
			//XmlNamespaceManager manager = new XmlNamespaceManager(Topic.NameTable);
			//manager.AddNamespace("MadCap","http://www.madcapsoftware.com/Schemas/MadCap.xsd");
			XmlNodeList Nodes = Topic.SelectNodes("//link");
			foreach(XmlNode node in Nodes)
			{
                if (node.Attributes["href"] == null) return;
                string style = node.Attributes["href"].Value.Replace('/','\\');
                if (style.Contains("\\TableStyles\\")) style = TABLESTYLESSUBPATH + System.IO.Path.GetFileName(style);
                else style = STYLESSUBPATH + System.IO.Path.GetFileName(style);
                if (!Styles.Contains(style) && localFileExists(style))								
                {
                    Styles.Add(style);
                    //Console.WriteLine(">STYLE: " + Token + " " + style);
                    checkStylesheet(style);
                }
			}
		}

		private void findScripts(XmlDocument doc)
		{
			//<script type="text/javascript" src="../Scripts/footer-padding.js"></script>
			//XmlNamespaceManager manager = new XmlNamespaceManager(Topic.NameTable);
			//manager.AddNamespace("MadCap","http://www.madcapsoftware.com/Schemas/MadCap.xsd");
			XmlNodeList Nodes = doc.SelectNodes("//script");
			foreach(XmlNode node in Nodes)
			{
				if(node.Attributes["src"] == null) continue;
				string script = node.Attributes["src"].Value.Replace('/','\\');
				script = SCRIPTSUBPATH + System.IO.Path.GetFileName(script);				
				if(!Scripts.Contains(script) && localFileExists(script))
				{
					Scripts.Add(script);
				}
			}
		}

		private void checkVariableFiles()
		{
            if (!Directory.Exists(ProjectPath + VARIABLESSUBPATH)) 
                return;
            foreach(string file in Directory.GetFiles(ProjectPath + VARIABLESSUBPATH) )
			{
				int count = 0;
				if(file.EndsWith(".flvar"))
				{
					XmlDocument doc = new XmlDocument();
                    tools1.LoadXmlFile(doc, file);
                    if((Doc != null) && (RW == WRITECONFIG)) 
                    {
                        XmlNode varFile = Doc.CreateElement("VariableFile");
                        varFile.Attributes.Append(Doc.CreateAttribute("filename"));
                        varFile.Attributes["filename"].Value = System.IO.Path.GetFileName(file);
                        //varFile.InnerText = System.IO.Path.GetFileName(file);
                        XmlNodeList vars = doc.SelectNodes("//Variable");
                        foreach (XmlNode var in vars)
                        {
                            XmlElement newVar = Doc.CreateElement("Variable");
                            newVar.InnerText = var.InnerText;
                            foreach (XmlAttribute att in var.Attributes)
                            {
                                string Name = att.Name;
                                string Value = att.Value;
                                if (Value != "")
                                {
                                    newVar.Attributes.Append(Doc.CreateAttribute(Name));
                                    newVar.Attributes[Name].Value = Value;
                                }
                            }
                            varFile.AppendChild(newVar);
                        }
                        settingsNode.AppendChild(varFile);
                    }
					count++;
				}
				if(count > 1) Console.WriteLine(sep + Token + ": " + count.ToString() + " Variable Files found.");
			}
		}

		private void checkPageLayout(string pageLayout)
		{
			//<Frame BackgroundImage="../Images/general/TDMFrontImg.png"
			//< xhtml:img xhtml:src = "../Images/general/tdmlogo.png" />
			if(File.Exists(ProjectPath + pageLayout))
			{
				XmlDocument layout = new XmlDocument();
				XmlNamespaceManager manager = new XmlNamespaceManager(layout.NameTable);
				manager.AddNamespace("MadCap","http://www.madcapsoftware.com/Schemas/MadCap.xsd");
				manager.AddNamespace("xhtml","http://www.w3.org/1999/xhtml");

                tools1.LoadXmlFile(layout, ProjectPath + pageLayout);
				XmlNodeList links = layout.DocumentElement.SelectNodes("//Frame");
				foreach(XmlNode link in links)
				{
					if(link.Attributes["BackgroundImage"] == null) continue;
					string val = link.Attributes["BackgroundImage"].Value.Replace('/','\\'); ;
					if(val == "none") continue;
					if(val.StartsWith("..\\"))
					{
						if(val.Contains("Images"))
						{
							if(val.Contains("general")) val = IMAGESUBPATHGENERAL + System.IO.Path.GetFileName(val);
							else
							if(val.Contains("language-specific")) val = IMAGESUBPATHSPECIFIC + System.IO.Path.GetFileName(val);
							else val = IMAGESUBPATH + System.IO.Path.GetFileName(val);
                            //if (val.EndsWith("cad2dGlStartViaBasicData.png"))
                            //{
                            //    Console.WriteLine(sep + val);
                            //    Console.ReadKey();
                            //}
                            if (!Images.Contains(val) && localFileExists(val))
							{
								Images.Add(val);
								//Console.WriteLine(Token + /*" " + System.IO.Path.GetFileName(MasterPage) + */" img[src]:" + val);
							}
						}
					}
					else
						Console.WriteLine(">" + val);
				}
				links = layout.DocumentElement.SelectNodes("//xhtml:img",manager);
				foreach(XmlNode link in links)
				{
					if(link.Attributes["xhtml:src"] == null) continue;
					string val = link.Attributes["xhtml:src"].Value.Replace('/','\\'); ;
					if(val == "none") continue;
					if(val.StartsWith("..\\"))
					{
						if(val.Contains("Images"))
						{
                            //if (val.EndsWith("cad2dGlStartViaBasicData.png"))
                            //{
                            //    Console.WriteLine(sep + val);
                            //    Console.ReadKey();
                            //}
                            if (val.Contains("general")) val = IMAGESUBPATHGENERAL + System.IO.Path.GetFileName(val);
							else
							if(val.Contains("language-specific")) val = IMAGESUBPATHSPECIFIC + System.IO.Path.GetFileName(val);
							else val = IMAGESUBPATH + System.IO.Path.GetFileName(val);
							if(!Images.Contains(val) && localFileExists(val))
							{
								Images.Add(val);
								//Console.WriteLine(Token + /*" " + System.IO.Path.GetFileName(MasterPage) + */" img[src]:" + val);
							}
						}
					}
					else
						Console.WriteLine(">" + val);
				}
				links = layout.DocumentElement.SelectNodes("//MadCap:variable",manager);
				foreach(XmlNode link in links)
				{
					if(link.Attributes["xhtml:name"] == null) continue;
					else
					{
						string VariableName = link.Attributes["xhtml:name"].Value.Replace('/','.');
						if(!Variables.Contains(VariableName))
							Variables.Add(VariableName);
						if(!VariableReferences.ContainsKey(VariableName)) VariableReferences.Add(VariableName,1);
							else VariableReferences[VariableName] = VariableReferences[VariableName] + 1;
					}
				}
			} 
		}
		
		private void checkStylesheet(string stylesheet)
        {
            TextDatei file = new TextDatei();
            string s = file.ReadFile(ProjectPath + stylesheet);
            while (s.Contains("url('"))
            {
                s = s.Substring(s.IndexOf("url('"));
                string url = s.Substring(5, s.IndexOf("');") - 5).Replace('/', '\\');
                if (url.StartsWith("..\\"))
                {
                    url = RESOURCESSUBPATH + url.Substring(3);
                    if (url.StartsWith(RESOURCESSUBPATH))
                    {
                        if (url.StartsWith(IMAGESUBPATH))
                        {
                            if (!Images.Contains(url) && localFileExists(url)) Images.Add(url);
                        }
                        else 
                        if (url.StartsWith(MASTERPAGESUBPATH)) 
                        {
                            if (!MasterPages.Contains(url) && localFileExists(url))
                            {
                                MasterPages.Add(url);
                                checkMasterPage(url);
                            }
                        }
                        else 
                        {
                            Console.WriteLine(ProjectPath + stylesheet + ": Linked file not found: (" + Path.GetFileName(url) + ").");
                            //Console.ReadLine();
                        }                        
                    }
                }
                else
                { 
                    Console.WriteLine(ProjectPath + stylesheet + ": Bad stylesheet link: (" + Path.GetFileName(url) + ").");
                    //Console.ReadLine();
                }
            s = s.Substring(s.IndexOf("');") + 1);
            }
            //IMAGESUBPATH
            //IMAGESUBPATHGENERAL
            //IMAGESUBPATHSPECIFIC
        }

        private void checkMasterPage(string MasterPage)
        {
            if(File.Exists(ProjectPath + MasterPage))
            {
                XmlDocument masterPage = new XmlDocument();
                tools1.LoadXmlFile(masterPage, ProjectPath + MasterPage);
                XmlNodeList links = masterPage.DocumentElement.SelectNodes("//a");
                foreach(XmlNode link in links)
                {
                    if (link.Attributes["href"] != null)
                    {
                        string val = link.Attributes["href"].Value.Replace('/','\\');
                        if (val.StartsWith("..\\"))
                        {
                            while (val.StartsWith("..\\"))
                                val = val.Substring(3);
                            val = TOPICSUBPATH + System.IO.Path.GetFileName(val);
                            if (!LinkedTopics.Contains(val) && localFileExists(val))
                            {
                                LinkedTopics.Add(val);
                            }
                        }
                        else
                        {
                            if((val=="#")||(val.StartsWith("javascript"))||(val.StartsWith("http")) || (val.StartsWith("mailto")))
                            {
                            }
                            else
                            Console.WriteLine(Token + " " + System.IO.Path.GetFileName(MasterPage) + " a[href]:" + val);
                        }
                    }
                }
                links = masterPage.DocumentElement.SelectNodes("//link");
                foreach (XmlNode link in links)
                {
                    if (link.Attributes["href"] != null)
                    {
                        string val = link.Attributes["href"].Value.Replace('/', '\\');
                        if (val.StartsWith("..\\"))
                        {
                            if (val.Contains("Images"))
                            {
                                if (val.Contains("general")) val = IMAGESUBPATHGENERAL + System.IO.Path.GetFileName(val);
                                else
                                if (val.Contains("language-specific")) val = IMAGESUBPATHSPECIFIC + System.IO.Path.GetFileName(val);
                                else val = IMAGESUBPATH + System.IO.Path.GetFileName(val);
                                if (!Images.Contains(val) && localFileExists(val))
                                {
                                    Images.Add(val);
                                }
                            }
                            else
                            {
                                if (val.Contains("Stylesheets"))
                                {
                                    val = STYLESSUBPATH + System.IO.Path.GetFileName(val);
                                    if (!Styles.Contains(val) && localFileExists(val)) Styles.Add(val);
                                }
                                else
                                {
                                    Console.WriteLine(Token + " " + System.IO.Path.GetFileName(MasterPage) + " link[href]:" + val);
                                    continue;
                                }
                            }
                        }
                        else
                            Console.WriteLine(Token + " " + System.IO.Path.GetFileName(MasterPage) + " link[href]:" + val);
                    }                    
                }
                links = masterPage.DocumentElement.SelectNodes("//img");
                foreach (XmlNode link in links)
                {
                    if (link.Attributes["src"] != null)
                    {
                        string val = link.Attributes["src"].Value.Replace('/', '\\');
                        if (val.StartsWith("..\\"))
                        {
                            if (val.Contains("Images"))
                            {
                                if (val.Contains("general")) val = IMAGESUBPATHGENERAL + System.IO.Path.GetFileName(val);
                                else
                                if (val.Contains("language-specific")) val = IMAGESUBPATHSPECIFIC + System.IO.Path.GetFileName(val);
                                else val = IMAGESUBPATH + System.IO.Path.GetFileName(val);
                                if (!Images.Contains(val) && localFileExists(val))
                                {
                                    Images.Add(val);
                                    //Console.WriteLine(Token + /*" " + System.IO.Path.GetFileName(MasterPage) + */" img[src]:" + val);
                                }
                            }
                        }
                    }
                }
				findScripts(masterPage);
			}
            //Main GLOtherTopics.flmsp link[href]:..\Images\general\favicon.ico
            //Main GLOtherTopics.flmsp img[src]:..\Images\general\IconTop.png
            //OLGGL GLOtherTopics.flmsp link[href]:..\Images\favicon.ico
            //OLGGL GLOtherTopics.flmsp img[src]:..\Images\IconTop.png
            //RG3DGL GLOtherTopics.flmsp link[href]:..\Images\favicon.ico
            //RG3DGL GLOtherTopics.flmsp img[src]:..\Images\IconTop.png
            //PDMGL GLOtherTopics.flmsp link[href]:..\Images\favicon.ico
            //PDMGL GLOtherTopics.flmsp img[src]:..\Images\IconTop.png
            //tdmCompact GLOtherTopics.flmsp link[href]:..\Images\favicon.ico
            //tdmCompact GLOtherTopics.flmsp img[src]:..\Images\IconTop.png
            //TDMConventions GLOtherTopics.flmsp link[href]:..\Images\favicon.ico
            //TDMConventions GLOtherTopics.flmsp img[src]:..\Images\IconTop.png
            //Main HomePage.flmsp a[href]:..\..\Main\mainPDFs.htm
            //Main HomePage.flmsp a[href]:..\..\Main\_KnowHow.htm
            //Main HomePage.flmsp link[href]:..\Stylesheets\HomePage.css
            //Main HomePage.flmsp link[href]:..\Images\general\favicon.ico
            //Main HomePage.flmsp img[src]:..\Images\general\Logo_small.png
            //Main HomePage.flmsp img[src]:..\Images\general\IconIndustryArena.png
            //Main HomePage.flmsp img[src]:..\Images\general\IconTwitter.png
            //Main HomePage.flmsp img[src]:..\Images\general\IconLinkedIn.png
            //Main HomePage.flmsp img[src]:..\Images\general\IconYouTube.png
        }

        public int checkTarget(string targetFile, ref bool isOnlineTarget)
		{
			//Console.WriteLine("DEBUG: " + Token + " " + targetFile);
			//Console.ReadLine();
			XmlNode targetNode;
			targetNode = Doc.CreateElement("target");
			int count = 0;
			if(localFileExists(TARGETSUBPATH + targetFile))
			{
				if(!Targets.Contains(TARGETSUBPATH + targetFile))
				{
					Targets.Add(TARGETSUBPATH + targetFile);
                    if (Loud) Console.WriteLine(sep + "Adding Target: " + targetFile);
					count++;
				}
                XmlDocument target = new XmlDocument();
                try 
                {
                    tools1.LoadXmlFile(target, ProjectPath + TARGETSUBPATH + targetFile);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ProjectPath + TARGETSUBPATH + targetFile);
                    Console.ReadLine();
                }
                XmlAttribute att = target.DocumentElement.Attributes["Type"];
				if(att != null)
				{
					if(att.Value.Contains("WebHelp")) 
					{
						isOnlineTarget = true;
						//Console.WriteLine("DEBUG: " + Token + " " + targetFile + " is online target.");
					}                        
                    else
					{
						isOnlineTarget = false;
						//Console.WriteLine("DEBUG: " + Token + " " + targetFile + " is print target.");
					}
				}
				if((Doc != null) && (RW == WRITECONFIG))
				{					
					targetNode.Attributes.Append(Doc.CreateAttribute("FileName"));
					targetNode.Attributes["FileName"].Value = targetFile;
					targetNode.Attributes.Append(Doc.CreateAttribute("Type"));
					targetNode.Attributes["Type"].Value = att.Value;
					settingsNode.AppendChild(targetNode);
				}
				att = target.DocumentElement.Attributes["MasterToc"];
				if(att != null)
				{
					string toc = att.Value.ToString().Replace('/','\\');
					if(!TOCs.Contains(toc) && localFileExists(toc))
					{
						TOCs.Add(toc);
						count++;
					}
					if((Doc != null) && (RW == WRITECONFIG))
					{
						targetNode.Attributes.Append(Doc.CreateAttribute("MasterToc"));
						targetNode.Attributes["MasterToc"].Value = toc;
					}
				}
				att = target.DocumentElement.Attributes["OutputFile"];
				if(att != null)
				{
					if((Doc != null) && (RW == WRITECONFIG))
					{
						targetNode.Attributes.Append(Doc.CreateAttribute("OutputFile"));
						targetNode.Attributes["OutputFile"].Value = att.Value;
					}
				}
				att = target.DocumentElement.Attributes["ConditionTagExpression"];
				if(att != null)
				{
					string conditions = att.Value.ToString().Trim();
					while(conditions.Contains("  ")) conditions = conditions.Replace("  "," ");

					XmlNode cond = Doc.CreateElement("conditions"); 
					parseConditionTag(conditions, cond);
                    if ((Doc != null) && (RW == WRITECONFIG))
                    {
                        targetNode.AppendChild(cond);
                    }
				}
				att = target.DocumentElement.Attributes["DefaultUrl"];
				if(att != null)
				{
					string defaultTopic = att.Value.ToString().Replace('/','\\');
					if(!LinkedTopics.Contains(defaultTopic) && localFileExists(defaultTopic)) 
					{
						LinkedTopics.Add(defaultTopic);
						count++;
					}
					if((Doc != null) && (RW == WRITECONFIG))
					{
						targetNode.Attributes.Append(Doc.CreateAttribute("DefaultUrl"));
						targetNode.Attributes["DefaultUrl"].Value = defaultTopic;
					}
					//TODO: Links auswerten
				}
				att = target.DocumentElement.Attributes["MasterPageLayout"];
				if(att != null)
				{
					string pageLayout = att.Value.ToString().Replace('/','\\');
					if(!PageLayouts.Contains(pageLayout) && localFileExists(pageLayout)) 
					{
						PageLayouts.Add(pageLayout);
							checkPageLayout(pageLayout);
							count++;
					}
					if((Doc != null) && (RW == WRITECONFIG))
					{
						targetNode.Attributes.Append(Doc.CreateAttribute("MasterPageLayout"));
						targetNode.Attributes["MasterPageLayout"].Value = pageLayout;
					}
				}
				att = target.DocumentElement.Attributes["Skin"];
				if(att != null)
				{
					string skin = att.Value.ToString().Replace('/','\\');
					if(!Skins.Contains(skin) && localFileExists(skin)) 
					{
						Skins.Add(skin);
						count++;
					}
					if((Doc != null) && (RW == WRITECONFIG))
					{
						targetNode.Attributes.Append(Doc.CreateAttribute("Skin"));
						targetNode.Attributes["Skin"].Value = skin;
					}
				}
				att = target.DocumentElement.Attributes["TopicToolbarSkin"];
				if(att != null)
				{
					string skin = att.Value.ToString().Replace('/','\\');
					if(!Skins.Contains(skin) && localFileExists(skin)) 
					{
						Skins.Add(skin);
						count++;
					}
					if((Doc != null) && (RW == WRITECONFIG))
					{
						targetNode.Attributes.Append(Doc.CreateAttribute("TopicToolbarSkin"));
						targetNode.Attributes["TopicToolbarSkin"].Value = skin;
					}
				}
				att = target.DocumentElement.Attributes["MasterStylesheet"];
				if(att != null)
				{
					string style = att.Value.ToString().Replace('/','\\');
					if(style.Contains("\\TableStyles\\")) 
						style = TABLESTYLESSUBPATH + System.IO.Path.GetFileName(style);
					else 
						style = STYLESSUBPATH + System.IO.Path.GetFileName(style);
					if(!Styles.Contains(style) && localFileExists(style)) 
					{
                        Styles.Add(style);                        
                        checkStylesheet(style);
                        count++;
					}
					if((Doc != null) && (RW == WRITECONFIG))
					{
						targetNode.Attributes.Append(Doc.CreateAttribute("MasterStylesheet"));
						targetNode.Attributes["MasterStylesheet"].Value = style;
					}
				}
				att = target.DocumentElement.Attributes["MasterPage"];
				if(att != null)
				{
					string masterPage = att.Value.ToString().Replace('/','\\');
                    masterPage = MASTERPAGESUBPATH + System.IO.Path.GetFileName(masterPage);
                    if (!MasterPages.Contains(masterPage) && localFileExists(masterPage)) 
					{
						MasterPages.Add(masterPage);
                        checkMasterPage(masterPage);
                        count++;
					}
					if((Doc != null) && (RW == WRITECONFIG))
					{
						targetNode.Attributes.Append(Doc.CreateAttribute("MasterPage"));
						targetNode.Attributes["MasterPage"].Value = masterPage;
					}
				}
				else
				{
					//Console.WriteLine("Target " + target.BaseURI + " without MasterPage");
				}
				XmlNodeList vars = target.SelectNodes("//Variable");
				//<Variable
				//	Name = "MyVariables/Month&amp;Year"
				//	Type = "DateTime">MMMM yyyy</Variable>
				if(vars.Count > 0)
				{
					XmlNode Variables = Doc.CreateElement("Variables");
                    if ((Doc != null) && (RW == WRITECONFIG))
                    {
                        targetNode.AppendChild(Variables);
                    }
                    foreach (XmlNode var in vars)
					{
						string name = var.Attributes["Name"].Value;
						if(!TargetVariables.Contains(name)) TargetVariables.Add(name);
						if((Doc != null) && (RW == WRITECONFIG))
						{
							XmlNode Variable = Doc.CreateElement("Variable");
							Variable.Attributes.Append(Doc.CreateAttribute("Name"));
							Variable.Attributes["Name"].Value = name;
							if(var.Attributes["Type"] != null)
							{
								Variable.Attributes.Append(Doc.CreateAttribute("Type"));
								Variable.Attributes["Type"].Value = var.Attributes["Type"].Value;
							}
							Variable.InnerText = var.InnerText;
							Variables.AppendChild(Variable);
						}
					}
				}
			}
			return count;
		}

        //private string copyExternalTopicNew(string sourceFile, bool SubFolder)
        //{
        //    string[] temp = sourceFile.Trim('\\').Split('\\');
        //    string externalToken = temp[0];
        //    string targetTopicFile = TOPICSUBPATH + externalToken + "\\" + Path.GetFileName(sourceFile);

        //    string source = ProjectParentPath + sourceFile;
        //    string copy = ProjectParentPath + '\\'  + BuildType + targetTopicFile;
        //    List<string> images = new List<string>();

        //    XmlNode externalProjectNode = Logfile.SelectSingleNode("//Project[@Token='" + externalToken + "']");
        //    if (externalProjectNode == null)
        //    {
        //        externalProjectNode = Logfile.CreateElement("Project");
        //        XmlAttribute projectToken = Logfile.CreateAttribute("Token");
        //        projectToken.Value = this.Token;
        //        XmlAttribute projectPath = Logfile.CreateAttribute("Path");
        //        projectPath.Value = this.ProjectPath;
        //        projectPath.Value = this.ProjectPath;
        //        externalProjectNode.Attributes.Append(projectToken);
        //        externalProjectNode.Attributes.Append(projectPath);
        //        Logfile.DocumentElement.AppendChild(externalProjectNode);
        //    }

        //    //logCopyFile(XmlNode parent, string file, string filetype, string externalToken = null)
            
        //    XmlNode newFile = Logfile.CreateElement("File");
        //    XmlAttribute extRef = Logfile.CreateAttribute("extRef");
        //    extRef.Value = this.Token;            
        //    externalProjectNode.Attributes.Append(extRef);
        //    //LogNode = logCopyProject();

        //    XmlDocument Topic = new XmlDocument();
        //    Topic.PreserveWhitespace = true;
        //    tools1.LoadXmlFile(Topic, source);

        //    if (SubFolder) fetchImageLinks(Topic, images);


        //    return (copy);
        //}

        private string copyExternalTopic(string externalProjectPath, string sourceFile, string TargetPath, string targetFile, bool SubFolder)
        {
            string externalToken = sourceFile.Substring(1);
            externalToken = externalToken.Substring(0, externalToken.IndexOf('\\'));
            
            string source = externalProjectPath + sourceFile;            
            string copy = TargetPath + targetFile;

            //if (copy.Contains("tdmglItemAddSupport.htm"))
            //{
            //    Console.WriteLine("tdmglItemAddSupport.htm");
            //    Console.ReadLine();
            //}

            List<string> images = new List<string>();
            //if (!Directory.Exists(Path.GetDirectoryName(copy))) tools1.createPath(Path.GetDirectoryName(copy));
            //Copy and fetch Topic
            //File.Copy(source, copy);
            //if (SubFolder)
            //{
            //    copy = TargetPath + sourceFile.Replace(TOPICSUBPATH, TOPICSUBPATH + Token + "\\");
            //}

            XmlDocument Topic = new XmlDocument();
            Topic.PreserveWhitespace = true;
            tools1.LoadXmlFile(Topic, source);

            if (SubFolder) fetchImageLinks(Topic, images);
            foreach(string image in images) 
            {
                copyExternalImage(externalToken, image, TargetPath, true);
            }

            //if (sourceFile.EndsWith("howtoGLCustomizeWorkArea.htm")) Console.ReadLine();
            fetchHyperlinks(Topic, SubFolder);

            //TODO:	VAR
            //if (Token.Contains("NCM")) 
            //{
            //    Console.WriteLine(Token);
            //    Console.ReadLine();
            //}
            varReferences += replaceVariableReferences(Topic, VarFile);
            //fetchVarReferences(Topic, Token);

            //ruft intern fetchHyperlink auf. TODO: prüfen, wie es beim PrintBuild kommt!
            fetchRelatedTopics(Topic, SubFolder);

            fetchStylesheetLinks(Topic);

            //if (sourceFile.ToLower() != LinkedTopics[0].ToLower())
            if (!source.Contains("Home.htm")) fetchHeader(Topic);
            
            Topic.Save(copy);
            //DataBaseFunctions.InsertData(source);
            //DataBaseFunctions.UpdateFileChecked(source);            

            logCopyFile(LogNode, sourceFile.Substring(externalToken.Length + 2), "Topic", externalToken);

            return (copy);
            //logXmlFileCopy(source,"Topic");			
        }

        //(externalProjectPath, image, TargetPath, true)
        private void copyExternalImage(string externalToken, string sourceFile, string TargetPath, bool SubFolder)
        {            
            string source = ProjectParentPath + "\\" + externalToken + TOPICSUBPATH + sourceFile;
            if (!File.Exists(source))
            { 
                Console.WriteLine(sep + "external image not found. " + source); 
                return; 
            }
            string copy = TargetPath + TOPICSUBPATH + sourceFile;
            //if (!Directory.Exists(Path.GetDirectoryName(copy))) tools1.createPath(Path.GetDirectoryName(copy));            
            if (!File.Exists(copy))
            {
                if (!Directory.Exists(Path.GetDirectoryName(copy)))
                    tools1.createPath(Path.GetDirectoryName(copy));
                //Copy Image and - if necessary - .props File too
                File.Copy(source, copy);                
                logCopyFile(LogNode, TOPICSUBPATH + sourceFile, "Image", externalToken);
                //logXmlFileCopy(source,"Image");
            }
            else
            {
                //Console.WriteLine(copy + " exists already.");
                LogDuplicateFile(sourceFile);
            }
            if (File.Exists(source + ".props"))
            {
                if (!File.Exists(copy + ".props"))
                {
                    File.Copy(source + ".props", copy + ".props");
                    logCopyFile(LogNode, TOPICSUBPATH + sourceFile + ".props", "ImageProperties", externalToken);
                    //logXmlFileCopy(source,"ImageProperties");
                }
                else
                {
                    //Console.WriteLine(copy + ".props" + " exists already.");
                    LogDuplicateFile(sourceFile + ".props");
                }
            }
        }

        public List<string> getLinkedFilesCount()
		{
			List<string> result = new List<string>();
			result.Add("LinkedTopics " + LinkedTopics.Count.ToString());
			result.Add("TOCLinkedTopics " + TOCLinkedTopics.Count.ToString());
			result.Add("Styles " + Styles.Count.ToString());
			result.Add("PageLayouts " + PageLayouts.Count.ToString());
			result.Add("MasterPages " + MasterPages.Count.ToString());
			result.Add("Skins " + Skins.Count.ToString());
			result.Add("TOCs " + TOCs.Count.ToString());
			result.Add("Targets " + Targets.Count.ToString());
			result.Add("Images " + Images.Count.ToString());
			result.Add("Pdfs " + Pdfs.Count.ToString());
			return result;
		}

        public List<string> getLinkedFiles()
		{
			List<string> result = new List<string>();
			foreach(string s in LinkedTopics) result.Add(ProjectPath + s);
			foreach(string s in TOCLinkedTopics) result.Add(ProjectPath + s);
			foreach(string s in Styles) result.Add(ProjectPath + s);
			foreach(string s in PageLayouts) result.Add(ProjectPath + s);
			foreach(string s in MasterPages) result.Add(ProjectPath + s);
			foreach(string s in Skins) result.Add(ProjectPath + s);
			foreach(string s in TOCs) result.Add(ProjectPath + s);
			foreach(string s in Targets) result.Add(ProjectPath + s);
			foreach(string s in Images) result.Add(ProjectPath + s);
			foreach(string s in Pdfs) result.Add(ProjectPath + s);
			return result;
		}

		public List<string> getIncludes()
		{
			List<string> result = new List<string>();
			foreach(string s in includes) result.Add(ProjectPath + s);
			return result;
		}

		public List<string> getExcludes()
		{
			List<string> result = new List<string>();
			foreach(string s in excludes) result.Add(ProjectPath + s);
			return result;
		}

		public void parseConditionTag(string cS, XmlNode cond = null)
		{
			char[] separators = { '[',']',' ' };
			int i0 = 0;
			//bool including = false;
			int status = 0;
			string s = "";
			string separator = "";
			string expression = "";
			string argument = "";
			while(i0 < cS.Length)
			{
				switch(status)
				{
					case 0:
						int i1 = i0;
						//do
						while(i1 < cS.Length)
						{
							if(separators.Contains(cS[i1]))
							{
							}
							if((cS[i1] != '[') && (cS[i1] != ']') && (cS[i1] != ' '))
							{
								s += cS[i1];
								i1++;
							}
							else
							{
								status = 1;
								separator += cS[i1];
								i0 = i1;
								break;
							}
						}
						break;
					case 1:
						argument = s.Trim();
						if(argument != "include" && argument != "exclude" && argument != "or")
						{
							if(expression == "include")
							{
								XmlElement inc = cond.OwnerDocument.CreateElement("include");
								inc.InnerText = argument;
								cond.AppendChild(inc);
								if(!includes.Contains(argument))
								{
									includes.Add(argument);
									if(excludes.Contains(argument)) 
                                        excludes.Remove(argument);
								}
							}
							if(expression == "exclude")
							{
								XmlElement exc = cond.OwnerDocument.CreateElement("exclude");
								exc.InnerText = argument;
								cond.AppendChild(exc);
								if(!excludes.Contains(argument) && !includes.Contains(argument)) 
                                    excludes.Add(argument);
							}
						}
						else
						{ 
						}
						//if(argument == "exclude") including = false;
						s = cS[i0].ToString();
						if(s != "")
						{
							//Console.WriteLine(argument);
							if((argument == "include") || (argument == "exclude")) 
							expression = argument;
						}
						if(separator != "")
						{
							//Console.WriteLine(separator);
						}
						separator = "";
						//Console.ReadKey();
						status = 0;
						break;
					default: break;
				}
				i0++;
			}
			//Console.WriteLine(argument);
			//Console.WriteLine(separator);
		}

        //------------------------------------------------------------------------------------------------------------------

        public void Copy(string TargetPath, XmlDocument logfile, string logfileName, XmlDocument alias, XmlDocument synonyms, /*SqlConnection Conn,*/ XmlDocument batchfile = null)
		{
            if (!Directory.Exists(TargetPath)) 
                tools1.createPath(TargetPath);            
            this.Logfile = logfile;            
            //XmlNode logRoot = logfile.SelectSingleNode("Actions");
			LogNode = logCopyProject();
			//if(Loud)
                Console.WriteLine(sep + "-> Copying " + Token + ":");
            if (Level == 0)
            {
                copyProjectFile(ProjectFiles[0], TargetPath);      
                string sourceFile = ProjectPath + "\\" + ProjectFiles[0];
                //Bamboo ruft später GLTopNavi.flprj bzw. V4Help.flprj auf, das sollte noch generalisiert werden (auf GLHelp)
                string targetFile = TargetPath + "\\GLTopNavi.flprj";
                File.Copy(sourceFile, targetFile); 
                switch (BuildType)
                {
                    case "GLHelp":
                        targetFile = TargetPath + "\\GLHelp.flprj";
                        File.Copy(sourceFile, targetFile);
                        break;
                    case "V4Help":
                        targetFile = TargetPath + "\\V4Help.flprj";
                        File.Copy(sourceFile, targetFile);
                        break;
                    default:
                        Console.WriteLine("Build Type " + BuildType + " unknown. " + ProjectFiles[0] + " will be the only project file in the Build project.");
                        break;
                }
            }
            
            if (!Directory.Exists(TargetPath + TARGETSUBPATH))
                tools1.createPath(TargetPath + TARGETSUBPATH);

            bool PdfTargetYetCopied = false;
            foreach (string target in Targets) 
			{
                switch (BuildType) 
                {
                    case "GLHelp":
                        //TODO: Prüfen, ob die (TDMGL-)TopNavi.fltars korrekt kopiert und geloggt werden 
                        //Befund: In Main ist TargetCount=4, in TDMGL bereits nur 1 ...
                        if (target.ToLower().EndsWith("topnavi.fltar") && (Level == 0)) copyTarget(target, TargetPath, true); //&& (Level == 0)
                        if (target.ToLower().EndsWith("glpdfbatchtarget.flbat") && (Level == 0)) copyTarget(target, TargetPath, true);
                        if (target.ToLower().EndsWith("pdf.fltar"))
                        {
                            if (!PdfTargetYetCopied) 
                            {
                                //Wenn ein Projekt eingebunden ist, das nicht spezifisch GL ist:
                                //Wenn es ein "GL-PDF"-target gibt, nimm es, ansonsten nimm das ohne GL
                                if (!Token.EndsWith("GL"))
                                {
                                    string GLspecificPDFtarget = TARGETSUBPATH + Token + "GL-PDF.fltar";
                                    if (localFileExists(GLspecificPDFtarget))
                                    {
                                        copyTarget(GLspecificPDFtarget, TargetPath, true);
                                        if(batchfile != null) 
                                        {
                                            checkPdfInBatchtargetFile(GLspecificPDFtarget, batchfile);
                                        }
                                        PdfTargetYetCopied = true;
                                    }
                                }
                                if (!PdfTargetYetCopied) 
                                {
                                    copyTarget(target, TargetPath, true);
                                    if (batchfile != null)
                                    {
                                        checkPdfInBatchtargetFile(target, batchfile);
                                    }
                                    PdfTargetYetCopied = true;
                                }
                            }
                        }
                        break;
                    case "V4Help":                       
                        //TODO: Prüfen, ob die TDM-Next.fltars korrekt kopiert und geloggt werden 
                        //Check: Problem war, dass die flprj-Files fehlten
                        if (target.EndsWith("TdmNext.fltar") && (Level == 0))
                            copyTarget(target, TargetPath, true); 
                        //TODO: Was geschieht mit dem BatchTaregt bei V4?
                        if (target.EndsWith("V4PDFBatchTarget.flbat") && (Level == 0)) copyTarget(target, TargetPath, true);
                        if (target.EndsWith("PDF.fltar") && !target.EndsWith("GL-PDF.fltar")) copyTarget(target, TargetPath, true);
                        break;
                    default: break;
                }                
            }                
            if (!Directory.Exists(TargetPath + TOCSUBPATH + Token))
                tools1.createPath(TargetPath + TOCSUBPATH + Token);
            foreach (string toc in TOCs)
                copyTOC(toc, TargetPath, true);
            if (!Directory.Exists(TargetPath + TOPICSUBPATH + Token))
                tools1.createPath(TargetPath + TOPICSUBPATH + Token);
            foreach (string topic in TOCLinkedTopics)
            {                
                string newTopicFile = copyTopic(topic, TargetPath, true);  
                //Console.WriteLine(newTopicFile + " TL");
                //Console.ReadLine();
            }
            foreach (string topic in LinkedTopics)
            {
                if (!TOCLinkedTopics.Contains(topic)) 
                {
                    string newTopicFile = copyTopic(topic, TargetPath, true);
                    //Console.WriteLine(newTopicFile + "  L");
                }
                //Console.ReadLine();
            }
            if (externalTopics.Count > 0) 
            {
                Console.WriteLine(sep + "Copying external files:");
                
                foreach (string externalTopicFile in externalTopics)
                {
                    if (File.Exists(ProjectParentPath + "\\" + externalTopicFile))
                    {                        
                        string externalToken = externalTopicFile.Substring(1);
                        externalToken = externalToken.Substring(0, externalToken.IndexOf('\\'));
                        //if (SubFolder)
                        string targetProjectPath = ProjectParentPath + "\\" + BuildType;
                        string targetTopicFile = TOPICSUBPATH + externalToken + "\\" + Path.GetFileName(externalTopicFile);
                        //string externalProjectPath = ProjectParentPath + "\\" + externalToken;

                        if (!Directory.Exists(ProjectParentPath + "\\" + BuildType + TOPICSUBPATH + externalToken))
                            tools1.createPath(ProjectParentPath + "\\" + BuildType + TOPICSUBPATH + externalToken);
                        if (!File.Exists(targetTopicFile))
                            //copyExternalTopicNew(externalTopicFile, true);
                            //TODO: Externe Topics kopieren und besser loggen
                            //TODO: Führt bei durch Actions.xml gemergten Projekten (XPathNavigationSample) massenhaft zu "external image not found."
                            //Aber nur bei BuildType: GLHelp
                            copyExternalTopic(ProjectParentPath, externalTopicFile, targetProjectPath, targetTopicFile, true);
                    }
                    else
                        Console.WriteLine(sep + ProjectParentPath + "\\" + externalTopicFile);
                }
            }

            foreach (string pdf in Pdfs) 
            {
                copyPdf(pdf, TargetPath, true);
            }

            if (!Directory.Exists(TargetPath + IMAGESUBPATHGENERAL))
                tools1.createPath(TargetPath + IMAGESUBPATHGENERAL);
            if (!Directory.Exists(TargetPath + IMAGESUBPATHSPECIFIC))
                tools1.createPath(TargetPath + IMAGESUBPATHSPECIFIC);
            foreach (string image in Images)
            { 
                copyImage(image, TargetPath);
            }                
            if (!Directory.Exists(TargetPath + VARIABLESSUBPATH))
                tools1.createPath(TargetPath + VARIABLESSUBPATH);
            //foreach (string var in Variables)
            if (Level == 0) copyVarFile(VARIABLESSUBPATH + "MyVariables.flvar", TargetPath);
            else 
            {                
                Console.WriteLine(sep + "Replaced " + varReferences.ToString() + " Variable references.");
                //if (varReferences > 0) addVariableFile2BuildMergeActions();
            }
            if (!Directory.Exists(TargetPath + MANUALSUBPATH))
                tools1.createPath(TargetPath + MANUALSUBPATH);
            foreach (string manualPage in ManualPages)
                copyManualFile(manualPage, TargetPath);
            if (!Directory.Exists(TargetPath + SKINSUBPATH))
                tools1.createPath(TargetPath + SKINSUBPATH);
            foreach (string skin in Skins) copySkin(skin, TargetPath);
            if (!Directory.Exists(TargetPath + PAGELAYOUTSUBPATH))
                tools1.createPath(TargetPath + PAGELAYOUTSUBPATH);
            foreach (string pageLayout in PageLayouts)
                copyPageLayoutFile(pageLayout, TargetPath);
            if (!Directory.Exists(TargetPath + MASTERPAGESUBPATH))
                tools1.createPath(TargetPath + MASTERPAGESUBPATH);
            foreach (string masterPage in MasterPages)
                copyMasterPage(masterPage, TargetPath);
            if (!Directory.Exists(TargetPath + STYLESSUBPATH))
                tools1.createPath(TargetPath + STYLESSUBPATH);
            if (!Directory.Exists(TargetPath + TABLESTYLESSUBPATH))
                tools1.createPath(TargetPath + TABLESTYLESSUBPATH);
            foreach (string stylesheet in Styles)
                copyStylesheet(stylesheet, TargetPath);
			if(!Directory.Exists(TargetPath + SCRIPTSUBPATH))
				tools1.createPath(TargetPath + SCRIPTSUBPATH);
			foreach(string script in Scripts)
				copyScript(script, TargetPath);
			if(!Directory.Exists(TargetPath + CONDITIONSUBPATH))
                tools1.createPath(TargetPath + CONDITIONSUBPATH);
			foreach (string condition in ConditionTagSets)
                copyConditionTagSet(condition, TargetPath);
            //Help IDs:
            XmlNodeList newIds = CollectCshIDs(true);
            XmlNode IdRoot = alias.SelectSingleNode("//CatapultAliasFile");
            if (newIds != null)
            {
                foreach (XmlNode newId in newIds)
                {
                    if (newId.Attributes.Count == 0) continue;
                    XmlNode tempNode = alias.ImportNode(newId, true);
                    switch (BuildType) 
                    {
                        case ("GLHelp"): 
                            if (tempNode.Attributes["Name"].Value.StartsWith("GL")) 
                                IdRoot.AppendChild(tempNode); 
                            break;
                        case ("V4Help"): 
                            if (!tempNode.Attributes["Name"].Value.StartsWith("GL")) IdRoot.AppendChild(tempNode); 
                            break;
                        default: IdRoot.AppendChild(tempNode); 
                            break;
                    }                    
                }
            }

            //TODO: Collect Synonyms:
            //XmlNode newNode = synonyms.CreateElement("Directional");
            //synonyms.DocumentElement.AppendChild(newNode);
            //newNode = synonyms.CreateElement("Groups");
            //synonyms.DocumentElement.AppendChild(newNode);
            
            //prepare for PDF Builds:
            //1. duplicate internal hrefs
            //Das Gerät schaltet sich ein und zeigt das<a href= "icsiLogOut.htm#Das" > Anmeldemenü </ a > an.
            //Das Gerät schaltet sich ein und zeigt das < a href = "icsiLogOut.htm#Das" MadCap: conditions = "Default.ScreenOnly" > Anmeldemenü </ a >< MadCap:conditionalText MadCap:conditions = "Default.PrintOnly" > Anmeldemenü < MadCap:xref href = "icsiLogOut.htm#AuswahlKostenstelle" > (S.1)</ MadCap:xref ></ MadCap:conditionalText > an.
            //2. Deactivate (unbind) external hrefs into "--> Module Name"

            if(Directory.Exists(ProjectPath + @"\Project\Advanced")) 
            {
                foreach (string file in Directory.GetFiles(ProjectPath + @"\Project\Advanced"))
                {
                    if (file.EndsWith(".mcsyns"))
                    {
                        XmlDocument newSynonymFile = new XmlDocument();
                        tools1.LoadXmlFile(newSynonymFile, file);

                        XmlNodeList newDirectionalSynonyms = newSynonymFile.SelectNodes("//MadCapSynonyms/Directional/DirectionalSynonym");
                        if (newDirectionalSynonyms != null)
                        {
                            if (newDirectionalSynonyms.Count > 0)
                            {
                                XmlNode directionalRoot = synonyms.SelectSingleNode("//MadCapSynonyms/Directional");
                                foreach (XmlNode newDirectionalSynonym in newDirectionalSynonyms)
                                {
                                    XmlNode tempNode = synonyms.ImportNode(newDirectionalSynonym, true);
                                    directionalRoot.AppendChild(tempNode);
                                }
                            }
                            //synonyms.Save(@"C:\docu\docusrc\00\GLHelp\Project\Advanced\Synonyms.mcsyns");
                        }

                        XmlNodeList newGroupedSynonyms = newSynonymFile.SelectNodes("//MadCapSynonyms/Groups/SynonymGroup");
                        if (newGroupedSynonyms != null)
                        {
                            if (newGroupedSynonyms.Count > 0)
                            {
                                XmlNode groupedRoot = synonyms.SelectSingleNode("//MadCapSynonyms/Groups");
                                foreach (XmlNode newGroupedSynonym in newGroupedSynonyms)
                                {
                                    XmlNode tempNode = synonyms.ImportNode(newGroupedSynonym, true);
                                    groupedRoot.AppendChild(tempNode);
                                }
                            }
                        }
                        //synonyms.Save(@"C:\docu\docusrc\00\GLHelp\Project\Advanced\Synonyms.mcsyns");
                    }
                }
            }
            //Subsystems
            foreach (FlareProject p in SubProjects)
            {
                //if (Token.ToLower() == "icbfgl")
                //    Console.ReadLine();
                p.Copy(TargetPath, logfile, logfileName, alias, synonyms, /*conn,*/ batchfile);
            }

            string destination = ""; 
            switch (BuildType)
            {                
                case "GLHelp":
                    destination = DESTINATIONSSUBPATH + "GLBuild.fldes";                    
                    break;
                case "V4Help":
                    destination = DESTINATIONSSUBPATH + "V4Build.fldes";
                    break;
                default: break;
            }
            if(Level == 0) 
                copyDestination(destination, TargetPath);
            logfile.Save(logfileName);
			tools1.BeautifyXml(logfileName,logfileName);
			//if(Loud) Console.WriteLine(sep + "<- Finished copying " + Token);
		}
        
        private void checkPdfInBatchtargetFile(string targetName, XmlDocument bt) 
        {
            //checking BATCHTARGETENTRY C:\docuR2022\docusrc\00\Main, Main, \Project\Targets\MainGL-PDF.fltar
            //Console.WriteLine(sep + "checking BATCHTARGETENTRY " + Path + ", " + Token + ", " + targetName);
            string tName = targetName.Substring(targetName.LastIndexOf('\\') + 1);
            tName = tName.Substring(0, tName.LastIndexOf('.'));
            string xpath = "//Targets/Target[@Name='" + tName + "']";
            XmlNode targetEntry = null;
            try
            {
                targetEntry = bt.SelectSingleNode(xpath);
                if (targetEntry != null)
                {
                    Console.WriteLine(sep + "found BATCHTARGETENTRY " + ProjectPath + ", " + tName);
                }
                else
                {
                    Console.WriteLine(sep + "!!! did not find BATCHTARGETENTRY " + ProjectPath + ", " + tName);
                }
            }
            catch
            {
                Console.WriteLine(sep + "!!! search for BATCHTARGETENTRY failed: " + xpath);
            }
        }

        private void copyProjectFile(string sourceFile, string TargetPath)
        {
            string source = ProjectPath + '\\' + sourceFile;
            string copy = TargetPath + '\\' + sourceFile;
            //if (!Directory.Exists(Path.GetDirectoryName(copy))) tools1.createPath(Path.GetDirectoryName(copy));

            if (!File.Exists(copy))
            {
                File.Copy(source, copy);
                logCopyFile(LogNode, sourceFile, "ProjectFile");
                //logXmlFileCopy(source,"ProjectFile");
            }
            else
            {
                LogXmlDuplicate(source);
            }
        }

        private string copyTopic(string sourceFile, string TargetPath, bool SubFolder)
        {
            string source = ProjectPath + sourceFile;
            string copy = TargetPath + sourceFile;
            //if (!Directory.Exists(Path.GetDirectoryName(copy))) tools1.createPath(Path.GetDirectoryName(copy));
            //Copy and fetch Topic
            //File.Copy(source, copy);
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            if (SubFolder)
            {
                copy = TargetPath + sourceFile.Replace(TOPICSUBPATH, TOPICSUBPATH + Token + "\\");
            }

            XmlDocument Topic = new XmlDocument();
            Topic.PreserveWhitespace = true;
            tools1.LoadXmlFile(Topic, source);

            if (SubFolder)
                fetchImageLinks(Topic);

            //if (sourceFile.EndsWith("howtoGLCustomizeWorkArea.htm")) Console.ReadLine();
            fetchHyperlinks(Topic, SubFolder);
            

            //TODO:	VAR
            //if (Token.Contains("NCM"))
            //{
            //    Console.WriteLine(Token);
            //    Console.ReadLine();
            //}
            varReferences += replaceVariableReferences(Topic, VarFile);
            //fetchVarReferences(Topic, Token);

            //ruft intern fetchHyperlink auf. TODO: prüfen, wie es beim PrintBuild kommt!
            fetchRelatedTopics(Topic, SubFolder);

            fetchStylesheetLinks(Topic);

            //if (sourceFile.ToLower() != LinkedTopics[0].ToLower())
            if (!source.Contains("Home.htm"))
            {
                fetchHeader(Topic);
                //TODO: Bei der Navigation verschwinden header hinter der Navigationsleiste.
                //<a name="top"/> einfügen hilft aber nichts.

                //checkTopAnchor(Topic);
            }
            if (!Directory.Exists(Path.GetDirectoryName(copy)))
                tools1.createPath(Path.GetDirectoryName(copy));
            Topic.Save(copy);
            //DataBaseFunctions.InsertData(source);
            //DataBaseFunctions.UpdateFileChecked(source);            

            logCopyFile(LogNode, sourceFile, "Topic");

            return (copy);
            //logXmlFileCopy(source,"Topic");			
        }

        private void copyTarget(string sourceFile, string TargetPath, bool SubFolder)
        {            
            string source = ProjectPath + sourceFile;
            string copy = TargetPath + sourceFile;
            //if (!Directory.Exists(Path.GetDirectoryName(copy))) tools1.createPath(Path.GetDirectoryName(copy));
            //Copy and fetch target (if it is not a batch target)
            if (!sourceFile.EndsWith(".flbat"))
            {
                XmlDocument Target = new XmlDocument();
                Target.PreserveWhitespace = true;
                tools1.LoadXmlFile(Target, source);
                fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;

                if (SubFolder)
                {
                    //MasterToc="/Project/TOCs/INDEX.fltoc"
                    string MasterToc = Target.DocumentElement.Attributes["MasterToc"].Value;
                    MasterToc = MasterToc.Replace(TOCSUBPATH.Replace('\\', '/'), TOCSUBPATH.Replace('\\', '/') + Token + '/');
                    Target.DocumentElement.Attributes["MasterToc"].Value = MasterToc;

                    if (!sourceFile.EndsWith("-PDF.fltar"))
                    {
                        if (Target.DocumentElement.Attributes["DefaultUrl"] != null)
                        {
                            string DefaultUrl = Target.DocumentElement.Attributes["DefaultUrl"].Value;
                            DefaultUrl = DefaultUrl.Replace(TOPICSUBPATH.Replace('\\', '/'), TOPICSUBPATH.Replace('\\', '/') + Token + '/');
                            Target.DocumentElement.Attributes["DefaultUrl"].Value = DefaultUrl;
                            //DefaultUrl = "/Content/IndexOverview.htm"
                        }
                    }
                }
                if (!Directory.Exists(Path.GetDirectoryName(copy)))
                    tools1.createPath(Path.GetDirectoryName(copy));
                Target.Save(copy);
                tools1.BeautifyXml(copy, copy);
                logCopyFile(LogNode, sourceFile, "Target");
            }
            else
            {
                File.Copy(source, copy);
                logCopyFile(LogNode, sourceFile, "BatchTarget");
            }
            if (Loud) Console.WriteLine(sep + "Copying Master Target " + System.IO.Path.GetFileNameWithoutExtension(copy));            
            //logXmlFileCopy(source,"ProjectFile");
        }

        private void copyTOC(string sourceFile, string TargetPath, bool SubFolder)
        {
            string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            string copy = TargetPath + sourceFile;
            if (SubFolder)
				copy = TargetPath + sourceFile.Replace(TOCSUBPATH, TOCSUBPATH + Token + "\\");

            //Copy and fetch TOc            
            XmlDocument TOC = new XmlDocument();
            TOC.PreserveWhitespace = true;
            tools1.LoadXmlFile(TOC, source);
            XmlNode root = TOC.DocumentElement;
            fetchTocEntries(root);
            if (!Directory.Exists(Path.GetDirectoryName(copy)))
                tools1.createPath(Path.GetDirectoryName(copy));
            TOC.Save(copy);
            tools1.BeautifyXml(copy, copy);

            if (Loud) Console.WriteLine(sep + "Copying TOC " + System.IO.Path.GetFileNameWithoutExtension(copy));
            logCopyFile(LogNode, sourceFile, "TOC");
        }

        private void fetchTocEntries(XmlNode node)
        {
            if (node.Attributes["AbsoluteLink"] != null)
            {
                //Console.WriteLine("!!! Removing Absolute Link: " + node.Attributes["AbsoluteLink"].Value);
                node.Attributes.Remove(node.Attributes["AbsoluteLink"]);
            }
            foreach (XmlNode child in node.ChildNodes)
            {
                if (child.Attributes == null) continue;
                if (child.Attributes["AbsoluteLink"] != null) child.Attributes.Remove(child.Attributes["AbsoluteLink"]);
                if ((child.Name == "TocEntry") && isUsed(child))
                {
                    if (child.Attributes["Link"] != null)
                    {
                        //if ((Level == 0) && (child.ParentNode.Name == "CatapultToc"))
                        if ((Level == 0) && (node.Name == "CatapultToc"))
                        {
                            //TODO: Schreibweise der Titel im Root aus TOC ins Topic übernehmen
                            string newTitle = child.Attributes["Title"].Value;                            
                            //Console.WriteLine(newTitle);
                            //    newTitle = newTitle.Substring(newTitle.IndexOf(" - ") + 3);
                            //    newTitle = newTitle + " (" + this.Token;
                            //    if (this.Token != "CLGR" && !this.Token.EndsWith("GL"))
                            //        newTitle = newTitle + "GL)";
                            //    else
                            //        newTitle = newTitle + ")";

                            //    if (Loud) Console.WriteLine(sep + "---> " + child.Attributes["Title"].Value + "---> " + newTitle);
                            //    child.Attributes["Title"].Value = newTitle;
                            //    //Console.ReadKey();
                        }                        
                        string link = child.Attributes["Link"].Value.Replace('\\', '/');
                        //Titel über Referenzen ersetzen ...
                        //Title="[%=System.LinkedTitle%]"
                        if (child.Attributes["Title"] != null)
                        {
                            string title = child.Attributes["Title"].Value;
                            //if (title == "[%=System.LinkedTitle%]")
                            //{
                            //    //Console.WriteLine("Found linked Title in TOC: " + node.BaseURI);
                            //    string TopicFile = (tools1.headPath(ProjectPath, 1) + "\\" + this.Token + "\\" + link).Replace('/', '\\');                                
                            //    if (File.Exists(TopicFile)) 
                            //    {
                            //        string newTitle = getTitlefromTopicFile(TopicFile, 0);
                            //    }
                            //    else 
                            //    {
                            //        Console.WriteLine(TopicFile + " not found.");
                            //    }
                            //    //Console.ReadLine();                                
                            //}
                        }
                        string bookmark = "";
                        if (link.Contains('#'))
                        {
                            bookmark = link.Substring(link.IndexOf('#') + 1);
                            link = link.Substring(0, link.IndexOf('#'));
                        }
                        //if (bookmark.Contains("tdmCompact-GLTopNavi"))
                        //{
                        //    Console.WriteLine(sep + Token + " " + bookmark);
                        //}
                        string ext = link.Substring(link.LastIndexOf("."));
                        switch (ext)
                        {
                            case ".htm":
                                /*
                                private const string DashPattern = @"[\u2012\u2013\u2014\u2015]";
                                private static Regex _dashRegex = new Regex(DashPattern);
                                public static string RemoveLongDashes(string s)
                                {
                                   return _dashRegex.Replace(s, "-");
                                }
                                */
                                //if (child.Attributes["Title"].Value.StartsWith(this.Token + " - "))
                                //&#160;–&#160;
                                string newTitle = child.Attributes["Title"].Value.Trim();
                                if (newTitle == "[%=System.LinkedHeader%]")
                                {                                    
                                    //Console.WriteLine(newTitle);
                                    //if(Token == "TDM") Console.ReadLine();
                                    if (localFileExists(link))
                                    {
                                        newTitle = getTopicHeaderText(link); //if (File.Exists(ProjectPath + link))
                                    }
                                    //Console.WriteLine(newTitle);
                                    //Console.ReadLine();
                                }
                                
                                if (newTitle.StartsWith(this.Token + " ") || newTitle.StartsWith(this.Token + "&#160;"))
                                {
                                    while (newTitle.Contains("&#160;")) newTitle = newTitle.Replace("&#160;", " ");
                                    if(newTitle.Contains(" - "))
                                    {
                                        newTitle = newTitle.Substring(newTitle.IndexOf(" - ") + 3);
                                        //Sonderbehandlung: AME-Schnittstellen und Ausbaustufen                                    
                                        if (this.Token.StartsWith("i"))
                                        {
                                        }
                                        else
                                        {
                                            newTitle = newTitle + " (" + this.Token;
                                            if (this.Token != "CLGR" && !this.Token.EndsWith("GL") && (BuildType == "GLHelp"))
                                                newTitle = newTitle + "GL)";
                                            else
                                                newTitle = newTitle + ")";
                                        }
                                    }
                                    else 
                                    { 
                                    }
                                    
                                    if (child.Attributes["Title"].Value != newTitle) 
                                    {
                                        Console.WriteLine(sep + "---> Renaming TOC entry title: " + child.Attributes["Title"].Value + "---> " + newTitle);
                                        child.Attributes["Title"].Value = newTitle;                                        
                                    }
                                    //if (this.Token.StartsWith("i")) Console.ReadLine();
                                    //Console.ReadKey();
                                }
                                if (link.StartsWith("Content") || link.StartsWith("/Content"))
                                {
                                    ///Content/cad2dWorkingWithCAD2D.htm"
                                    ///Content/Resources/Manual/Deckblatt.htm
                                    string p = System.IO.Path.GetDirectoryName(link);

                                    if (!p.EndsWith("\\Manual"))
										link = link.Replace("/Content/", "/Content/" + Token + "/");
                                }
                                else
                                {
                                    //Console.WriteLine(link);
                                    //Console.ReadKey();
                                    //../../../Content/mainglStartPanel.htm
                                    if (link.StartsWith("../../../"))
                                    {
                                        while (link.StartsWith("../"))
                                            link = link.Substring(3);
                                        string[] temp = link.Split('/');
                                        link = "/" + temp[0] + "/" + "main" + "/" + temp[1];
                                    }
                                    else
                                    {
                                        //../../tdmConventions/Content/cad2dLayerConcept.htm
                                        if (link.StartsWith("../../"))
                                        {
                                            //Console.WriteLine(link);
                                            //Console.ReadLine();
                                            while (link.StartsWith("../"))
                                                link = link.Substring(3);
                                            string[] temp = link.Split('/');
                                            link = "/" + temp[1] + "/" + temp[0] + "/" + temp[2];// link.Replace("/Content/","/");
                                        }
                                        else 
                                        {
                                            if (link.StartsWith("../Subsystems/")) 
                                            {
                                                //../Subsystems/tdmCompact/Content/howToPrint.htm
                                                link = link.Substring(("../Subsystems/").Length);
                                                string[] temp = link.Split('/');
                                                link = "/" + temp[1] + "/" + temp[0] + "/" + temp[2];// link.Replace("/Content/","/");
                                                //Console.WriteLine(link);
                                                //Console.ReadLine();
                                            }
                                        }
                                    }
                                }
                                if (bookmark.Length > 0)
                                    link = link + '#' + bookmark;
                                child.Attributes["Link"].Value = link;
                                break;
                            case ".fltoc":
                                link = link.Replace(TOCSUBPATH.Replace('\\', '/'), TOCSUBPATH.Replace('\\', '/') + Token + "/");
                                child.Attributes["Link"].Value = link;
                                break;
                            case ".flprj":
                                //if (link.Contains("tdmNews2020"))
                                //{
                                //    Console.WriteLine("tdmNews2020");
                                //    Console.ReadLine();
                                //}
                                string subProjectFile = link;
                                while (subProjectFile.StartsWith("../")) subProjectFile = subProjectFile.Substring(3);
                                subProjectFile = '/' + subProjectFile.Substring(0, subProjectFile.IndexOf('.'));
                                string subToken = tools1.tailPath(subProjectFile, 1);
                                string SourcePath = tools1.headPath(ProjectPath, 1);
                                string SubTargetFile = SourcePath + "\\" + subToken + TARGETSUBPATH + bookmark + ".fltar";
                                //if (!File.Exists(SubTargetFile))
                                //{
                                //    Console.WriteLine(sep + "Target file not found: " + SubTargetFile);
                                //    Console.ReadKey();
                                //}
                                string TOC = Path.GetFileName(getMasterTocFromTarget(SubTargetFile));
                                link = (TOCSUBPATH + subToken + "\\" + TOC).Replace("\\", "/");
                                child.Attributes["Link"].Value = link;
                                break;
                        }
                    }
                    fetchTocEntries(child);
                }
            }
        }

        private string getTopicHeaderText(string Topic) 
        {
            XmlDocument topic = new XmlDocument();
            tools1.LoadXmlFile(topic, ProjectPath + Topic);
            XmlNode headerNode = topic.SelectSingleNode("//h1");
            if (headerNode != null) return headerNode.InnerText;
            else return "NOT FOUND!";
        } 
        
        private string getTitlefromTopicFile(string Topic, int n) 
        {
            //n=0: title tag
            //n>0: header (... ?? check if bookmark is available if not create a new one ...)
            string result = "";            
            return result;
        }

        private string getMasterTocFromTarget(string targetFile)
        {
            if (!File.Exists(targetFile))
            {
                Console.WriteLine(sep + "Target file not found: " + targetFile);
                return("");
            }
            //MasterToc = " / Project/TOCs/INDEX.fltoc"
            XmlDocument target = new XmlDocument();
            target.PreserveWhitespace = true;
            tools1.LoadXmlFile(target, targetFile);
            return target.DocumentElement.Attributes["MasterToc"].Value.ToString().Replace('/', '\\');
        }
        
        private void makeTopicMediumSpecific(string topicFile) 
        {
            XmlDocument topic = new XmlDocument();
            tools1.LoadXmlFile(topic, topicFile);
            XmlNodeList anchors = topic.SelectNodes("//a");
            string link = "";
            foreach (XmlNode anchor in anchors)
            {
                if (anchor.Attributes["href"] != null) 
                {
                    if (isUsed(anchor)) 
                    {
                        link = anchor.Attributes["href"].Value;
                        if (link.StartsWith("#")) continue;
                        //Console.WriteLine("Used hyperlink: " + link + " " + anchor.InnerText);
                        if (link.StartsWith("../"))
                        {
                            //Console.WriteLine("Preparing external hyperlink for print and screen media: " + link + " " + anchor.InnerText);
                        }
                        else
                        {
                            if (link.StartsWith("http")) continue;
                            if (link.StartsWith("Resources/Images"))
                            {
                                Console.WriteLine("Linked Image: " + link + " " + anchor.InnerText);
                                continue;
                            }
                            //Console.WriteLine("Preparing internal hyperlink for print and screen media: " + link + " " + anchor.InnerText);
                        }
                    }
                    else 
                    {
                        //Console.WriteLine("Unused hyperlink: " + link + " " + anchor.InnerText);
                    }
                }
            }
        }
        
        private void fetchImageLinks(XmlDocument Topic, List<string> images = null)
        {
            //DEBUG:
            //icmgbs15HelpImportTurnInt.htm
            //OK: icmgbs15ExampleT04-03.png
            //nicht OK: iCMGIBBS15exOrHQ2.png

            //if(ImageFile.EndsWith("icmgbs15ExampleT04-03.png")) Console.ReadKey();

            XmlNodeList ImageNodes = Topic.DocumentElement.SelectNodes("//img");
            foreach (XmlNode img in ImageNodes)
            {
                //TODO: ImageLinks, die aus dem Projekt herauszeigen beseitigen (und melden)
                //Die führen zum Compilerabbruch
                //z.B.: <img src="../../TDMGL/Content/Resources/Images/general/glIconUpload.png" />
                //Die kommen immer wieder zustande durch Kopieren von Sources zwischen zwei Flare-Instanzen
                
                //DEBUG: "IsUsed" Abfrage umgehen, um Flare-Compilermeldungen zu minimieren!
                //if (isUsed(img))
                {
                    string imageFile = img.Attributes["src"].Value;
                    if (images != null)
                    {
                        if (!images.Contains(imageFile.Replace('/','\\')))
                            images.Add(imageFile.Replace('/', '\\'));
                    }
                    img.Attributes["src"].Value = "../" + imageFile;
                    //Console.WriteLine(img.Attributes["src"].Value);
                }
            }
        }

		private void fetchScriptLinks(XmlDocument doc, bool subfolder)
		{
			//    Console.ReadKey();
			XmlNodeList scriptNodes = doc.DocumentElement.SelectNodes("//script");

			foreach(XmlNode script in scriptNodes)
			{
				if(script.Attributes["src"] != null)
				{
					string src = script.Attributes["src"].Value;
					if(src.StartsWith("/"))
					{
						Console.WriteLine("Bad link to script" + src + " in " + doc.BaseURI);
						break;
					}
					else
					{
						src = "../" + src;
						script.Attributes["src"].Value = src;
					}
				}
			}
		}

        private void addVariableFile2BuildMergeActions()
        {
            logCopyFile(LogNode, VARIABLESSUBPATH + "MyVariables.flvar", "VariableFile");
        }

        private void fetchHyperlinks(XmlDocument Topic, bool subfolder)
        {
            //    Console.ReadKey();
            XmlNodeList hrefNodes = Topic.DocumentElement.SelectNodes("//a");

            foreach (XmlNode a in hrefNodes)
            {
                if (a.Attributes["href"] != null)
                {
                    string href = a.Attributes["href"].Value;
                    if (href.StartsWith("/"))
                    {
                        Console.WriteLine("Bad link " + href + " in " + Topic.BaseURI);
                        break;
                    }
                    else
                    {
                        if (href.StartsWith(".."))
                        {
                            //links in andere Module:
                            int i = fetchHyperLink(a, "href", subfolder);
                            if (i != 0)
                            {
                                Console.WriteLine("Bad href Attribute in HyperLink (errorlvl: " + i.ToString() + "):");
                                Console.WriteLine(Topic.BaseURI);
                                Console.WriteLine(href);
                                //Console.ReadLine();
                            }
                        }
                        else
                        {
                            //Links auf Bookmarks innerhalb der Seite: Nichts tun
                            if (href.StartsWith("#")) continue;
                            //Webliks: Nichts tun
                            if (href.StartsWith("http")) continue;
                            //Projekt-Interne Links: Link nicht verändern, aber checken, ob die Zieldatei schon vorhanden ist.                            
                        }
                    }
                }
            }
        }

        private void fetchRelatedTopics(XmlDocument doc, bool subfolder)
        {
            ////MadCap:relatedTopic src = "../../tdm/Content/tdmTool.htm" />
            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
            manager.AddNamespace("MadCap", "http://www.madcapsoftware.com/Schemas/MadCap.xsd");
            XmlNodeList RelTopics = doc.SelectNodes("//MadCap:relatedTopic", manager);
            foreach (XmlNode RelTopic in RelTopics)
            {
                string src = RelTopic.Attributes["src"].Value;
                //if (src.StartsWith("..")) RelTopic.Attributes["src"].Value = fetchHyperLink(doc, src, subfolder, "main");
                if (src.StartsWith(".."))
                {
                    if (fetchHyperLink(RelTopic, "src", subfolder) != 0)
                    {
                        Console.WriteLine("Bad src Attribute in Related Topic Link " + doc.BaseURI + ":");
                        Console.WriteLine(src);
                        Console.ReadKey();
                    }
                }
            }
        }

        private int fetchHyperLink(XmlNode a, string attribute, bool subfolder)
        {
            if (a.Attributes[attribute] == null) 
                return -1;
            string val = a.Attributes[attribute].Value;
            string newVal = "";
            string[] temp = val.Split('/');
            //Subsystem --> Main
            if (val.StartsWith("../../../Content"))
            {
                if (subfolder)
                {
                    //../../../Content/mainglOverview.htm
                    //../tmsgl/mainglOverview.htm                    
                    newVal = "../main/" + temp[4];
                    a.Attributes[attribute].Value = newVal;
                }
            }
            else
            {
                //Subsystem --> Subsystem
                if (val.StartsWith("../../"))
                {
                    if (subfolder)
                    {
						if(temp.Length >= 5)
						{
							//../../tmsgl/Content/tmsglUserManagementApp.htm
							//../tmsgl/tmsglUserManagementApp.htm
							newVal = "../" + temp[2] + "/" + temp[4];
							a.Attributes[attribute].Value = newVal;
						}
						else Console.WriteLine(Token + ": Could not fetch Link between sub-projects: " + val);
                    }
                }
                else
                {
                    //Main --> Subsystem
                    if (val.StartsWith("../Subsystems"))
                    {
                        if (subfolder)
                        {
							//../Subsystems/sfmgl/Content/sfmglOverview.htm
							//../sfmgl/sfmglOverview.htm
							if(temp.Length >= 5)
							{
								newVal = "../" + temp[2] + "/" + temp[4];
								a.Attributes[attribute].Value = newVal;
							}
							else Console.WriteLine(Token + ": Could not fetch Link from sub-project to Main: " + val);
						}
                    }
                    else
                    {
                        return -2;
                    }
                }
            }
            //if (a.BaseURI.EndsWith("_KnowHow.htm"))
            //{
            //    Console.WriteLine(a.BaseURI + ":");
            //    Console.WriteLine("  " + val);
            //    Console.WriteLine("  " + newVal);
            //    Console.ReadKey();
            //}
            return 0;
        }

        private void fetchHeader(XmlDocument doc)
        {
            //XmlNodeList styles = doc.SelectNodes("//link[@rel = 'stylesheet']");
            //XmlNodeList headerLinks = doc.SelectNodes("//head//link");
            XmlNode header = doc.SelectSingleNode("//head");
            if (header == null) 
            {
                Console.WriteLine("Missing header in:");
                Console.WriteLine(doc.BaseURI);
                return;
            }
            int i = 0;
            while (i < header.ChildNodes.Count)
            {
                XmlNode node = header.ChildNodes[i];
                if (node.Name == "link")
                {
                    if (node.Attributes["rel"] != null)
                    {
                        if (node.Attributes["rel"].Value == "stylesheet")
                        {
                            header.RemoveChild(node);
                            continue;
                        }
                    }
                }
                i++;
            }
            //< link href = "../Resources/Stylesheets/StylesGL.css" rel = "stylesheet" />
            XmlNode glStylesheet = doc.CreateElement("link");
            XmlAttribute href = doc.CreateAttribute("href");

            string defaultStylesheet = "";
            if(BuildType == "GLHelp") defaultStylesheet = "../Resources/Stylesheets/StylesGL.css";
            if(BuildType == "V4Help") defaultStylesheet = "../Resources/Stylesheets/Styles.css";
            href.Value = defaultStylesheet;

            XmlAttribute rel = doc.CreateAttribute("rel");
            rel.Value = "stylesheet";
            glStylesheet.Attributes.Append(href);
            glStylesheet.Attributes.Append(rel);
            header.AppendChild(glStylesheet);
            //while ((i < headerLinks.Count) && (headerLinks.Count > 0))
            //{
            //    XmlNode node = headerLinks[i];
            //    if ((node.Attributes["rel"]!=null))
            //    {
            //        if (node.Attributes["rel"].Value == "stylesheet")
            //        {
            //            node.ParentNode.RemoveChild(node);
            //            continue;
            //        }
            //    }
            //    i++;
            //}
        }

        private void checkTopAnchor(XmlDocument doc)
        {
            XmlNodeList anchor = doc.SelectNodes("//a[@name='top']");
            if (anchor.Count != 1) 
            {
                if (anchor.Count == 0)
                {
                    XmlAttribute name = doc.CreateAttribute("name");
                    name.Value = "top";
                    XmlElement a = doc.CreateElement("a");
                    a.Attributes.Append(name);
                    XmlNodeList headers = doc.SelectNodes("/html/body/*[self::h1 or self::h2 or self::h3 or self::h4 or self::h5 or self::h6]");
                    if (headers.Count == 0)
                    {
                        Console.WriteLine(sep + "No header found in " + doc.BaseURI);
                        return;
                    }
                    else
                    {
                        headers[0].PrependChild(a);
                        Console.WriteLine(sep + "Added 'top' to " + doc.BaseURI);
                        return;
                    }
                }
                if (anchor.Count > 1)
                {
                    Console.WriteLine(sep + "found a[@name='top'] " + anchor.Count.ToString() + " times in " + doc.BaseURI);
                    return;
                    //Console.ReadLine();
                }
            }

            //int topicChanges = 0;
            //int n = 0;
            //List<string> bookmarks = new List<string>();
            //
            //for (int i = 0; i < headers.Count; i++)
            //{
            //    XmlNode a = headers[i].ChildNodes
            //    if (headers[i].HasChildNodes) 
            //    { 
            //    }
            //    else 
            //    { 
            //    }
            //}
        }

        private void fetchStylesheetLinks(XmlDocument doc)
        {

            //< link href = "Resources/TableStyles/grid_01_gray.css" rel="stylesheet" MadCap: stylesheetType = "table" />
            //< link href = "Resources/Stylesheets/Styles.css" rel = "stylesheet" type = "text/css" />

            XmlNodeList styles = doc.SelectNodes("//link[@rel = 'stylesheet']");
            for (int i = 0; i < styles.Count; i++)
            {
                XmlNode style = styles[i];
                if (style.Attributes["href"] == null) continue;
                style.Attributes["href"].Value = "../" + style.Attributes["href"].Value;
            }
            //< table class="TableStyle-grid_01_gray" style="mc-table-style: url('Resources/TableStyles/grid_01_gray.css');margin-left: 0;margin-right: auto;" cellspacing="0">
            styles = doc.SelectNodes("//table");
            foreach (XmlNode style in styles)
            {
                if (style.Attributes["style"] == null) continue;
                string val = style.Attributes["style"].Value;
                if (val.Contains("mc-table-style: url('Resources"))
                {
                    val = val.Replace("mc-table-style: url('Resources", "mc-table-style: url('../Resources");
                }
                style.Attributes["style"].Value = val;
            }
            // < html xmlns: MadCap = "http://www.madcapsoftware.com/Schemas/MadCap.xsd" class="HomePage" style="mc-master-page: url('Resources\MasterPages\HomePage.flmsp');">
            XmlNode html = doc.SelectSingleNode("//html");
            if ((html != null) && (html.Attributes["style"] != null))
            {
                string val = html.Attributes["style"].Value;
                if (val.Contains("mc-master-page: url('Resources"))
                {
                    if (val.Contains('\\')) val = val.Replace("mc-master-page: url('Resources", "mc-master-page: url('..\\Resources");
                    else val = val.Replace("mc-master-page: url('Resources", "mc-master-page: url('../Resources");
                }
                html.Attributes["style"].Value = val;
            }
        }

		private int replaceVariableReferences(XmlDocument doc, XmlDocument vars)
		{
            //if (doc.BaseURI.Contains("icatia5ClassParam")) 
            //{
            //    Console.WriteLine(doc.BaseURI);
            //    Console.ReadLine();
            //}
            //TODO: Apply to TOCS too, xpath: //TocEntry[@ Title] 
            //Title="[%=MyVariables.ModuleName%]"
            int counter = 0;
            string v = vars.BaseURI.Replace("/", "\\");
            string f = doc.BaseURI.Replace("/", "\\");
            if (f.Contains(MANUALSUBPATH)) 
                return counter;
            //if (f.Contains("iccwOverview.htm"))
            //{
            //    Console.WriteLine("iccwOverview.htm");
            //    Console.ReadLine();
            //}
            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
			manager.AddNamespace("MadCap","http://www.madcapsoftware.com/Schemas/MadCap.xsd");
			XmlNodeList varReferences = doc.SelectNodes("//MadCap:variable", manager);
			for(int i = 0; i < varReferences.Count; i++)
			{
				XmlNode varReference = varReferences[i];
                if (!isUsed(varReference))
                    continue;
				string varName = "";
				if(varReference.Attributes["name"] != null)
				{
                    ///TODO: Es gibt immer noch Problem beim Variablen ersetzen!!!
                    //if (Token == "iCTOP7")
                    //{
                    //    Console.WriteLine("hier");
                    //    Console.ReadLine();
                    //}
                    try
					{
						varName = varReference.Attributes["name"].Value;
						varName = varName.Substring("MyVariables.".Length);
						//Console.WriteLine(var.ParentNode.InnerXml);
						string xp = "//Variable[@Name='" + varName + "']";
                        XmlNode VariableDefinition = vars.SelectSingleNode(xp);
                        if(VariableDefinition == null) 
                        {
                            Console.WriteLine(sep + "Warning: Could not replace variables " + xp + " in " + f);
                            Console.WriteLine(sep + "Warning: The variable definition was not found in " + v);
                            //Console.ReadLine();
                        }
                        else 
                        {
                            string variableValue = VariableDefinition.InnerText;
                            XmlNode newTextNode = null;                           
                            if (varReference.PreviousSibling != null)
                            {
                                if (varReference.PreviousSibling.NodeType == XmlNodeType.Whitespace)
                                {
                                    if(varReference.NextSibling != null) varReference.NextSibling.InnerText = variableValue + varReference.NextSibling.InnerText;
                                    varReference.ParentNode.RemoveChild(varReference);
                                    //Console.WriteLine("Here we are!");
                                    //Console.WriteLine(doc.BaseURI);
                                    //Console.ReadLine();
                                }
                                else
                                {
                                    newTextNode = varReference.PreviousSibling;
                                    newTextNode.InnerText += variableValue;
                                    varReference.ParentNode.RemoveChild(varReference);
                                }
                            }
                            else
                            {
                                if (varReference.NextSibling != null)
                                {
                                    newTextNode = varReference.NextSibling;
                                    newTextNode.InnerText = variableValue + newTextNode.InnerText;
                                    varReference.ParentNode.RemoveChild(varReference);
                                }
                                else
                                {
                                    newTextNode = varReference.ParentNode;
                                    newTextNode.InnerText = variableValue;
                                    newTextNode.RemoveChild(varReference);
                                }
                            }

                        }
                        //if(x != null)
                        //{
                        //    x.InnerText += value;							
                        //}
                        //else
                        //{
                        //	x = var.NextSibling;
                        //	if(x != null)
                        //	{
                        //		x.InnerText = value + x.InnerText;
                        //	}
                        //	else
                        //	{
                        //		x = x.ParentNode;
                        //		x.InnerText = value;
                        //	}
                        //}
                        //var.ParentNode.RemoveChild(var);
                        counter++;
						//Console.WriteLine(x.ParentNode.InnerXml);
						//Console.ReadLine();
						//Console.WriteLine(sep + Token + ": No previous sibling found: " + var.Attributes["name"].Value + " in " + doc.BaseURI);
					}
					catch (Exception ex)
					{
                        //if(!VariableReferences.ContainsKey(var.Attributes["name"].Value))
                        //file:\\\C:\docuR2020\03\iMEUR\Content\imeurOverview.htm
                        Console.WriteLine(sep + Token + " " + doc.BaseURI.Replace("/","\\").Substring(7) + " " + ex.Message + "  " + varName); // ": Variable not found: " + var.Attributes["name"].Value + " in " + doc.BaseURI);
						//Console.ReadLine();
					}
				}
                else 
                {
                    Console.WriteLine("hier.");
                }
			}
            return counter;
		}

        private void fetchVarReferences(XmlDocument doc, string token)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(doc.NameTable);
            manager.AddNamespace("MadCap", "http://www.madcapsoftware.com/Schemas/MadCap.xsd");
            XmlNodeList VarNodes = doc.SelectNodes("//MadCap:variable", manager);
            foreach (XmlNode var in VarNodes)
            {
                string name = "";
                if (var.Attributes["name"] != null)
                {
                    //name = "MyVariables.Fremdsystem"
                    name = var.Attributes["name"].Value;
                    name = name.Replace("MyVariables", token);
                    var.Attributes["name"].Value = name;
                }
                else if (var.Attributes["xhtml:name"] != null)
                {
                    name = var.Attributes["xhtml:name"].Value;
                    name = name.Replace("MyVariables", token);
                    var.Attributes["xhtml:name"].Value = name;
                }
                else
                    continue;
                
            }

        }        

        private void copyImage(string sourceFile, string TargetPath)
        {
            string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            string copy = TargetPath + sourceFile;
            if (!Directory.Exists(Path.GetDirectoryName(copy))) tools1.createPath(Path.GetDirectoryName(copy));            
            if (!File.Exists(copy))
            {
                //Copy Image and - if necessary - .props File too
                File.Copy(source, copy);
                logCopyFile(LogNode, sourceFile, "Image");
                //logXmlFileCopy(source,"Image");
            }
            else
            {
                //Console.WriteLine(copy + " exists already.");
                LogDuplicateFile(sourceFile);
            }
            if (File.Exists(source + ".props"))
            {
                if (!File.Exists(copy + ".props"))
                {
                    File.Copy(source + ".props", copy + ".props");
                    logCopyFile(LogNode, sourceFile + ".props", "ImageProperties");
                    //logXmlFileCopy(source,"ImageProperties");
                }
                else
                {
                    //Console.WriteLine(copy + ".props" + " exists already.");
                    LogDuplicateFile(sourceFile + ".props");
                }
            }
        }

        private void copyPdf(string sourceFile, string TargetPath, bool SubFolder)
        {
            string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            string copy = TargetPath + sourceFile;
            if (SubFolder)
            {
                copy = TargetPath + sourceFile.Replace(TOPICSUBPATH, TOPICSUBPATH + Token + "\\");
            }
            //if (!Directory.Exists(Path.GetDirectoryName(copy))) tools1.createPath(Path.GetDirectoryName(copy));

            if (!File.Exists(copy))
            {
                if (File.Exists(source))
                {
                    File.Copy(source, copy);
                    logCopyFile(LogNode, sourceFile, "Pdf");
                }
                else
                {
                    Console.WriteLine("Missing pdf File: " + source);
                }
                //logXmlFileCopy(source,"Pdf");
            }
        }

		private void copyScript(string sourceFile, string TargetPath)
		{
			string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            string copy = TargetPath + sourceFile;
			if(!File.Exists(copy))
			{
				if(File.Exists(source))
				{
					File.Copy(source,copy);
					logCopyFile(LogNode,sourceFile,"Script");
				}
				else
				{
					Console.WriteLine("Missing Script File: " + source);
				}
				//logXmlFileCopy(source,"Script");
			}
		}

		private void copyStylesheet(string sourceFile, string TargetPath)
        {
            string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            string copy = TargetPath + sourceFile;
            if (!File.Exists(copy))
            {
                if (File.Exists(source))
                {
                    File.Copy(source, copy);
                    logCopyFile(LogNode, sourceFile, "Stylesheet");
                }
                else
                {
                    Console.WriteLine("Missing Stylesheet File: " + source);
                }
                //logXmlFileCopy(source,"Stylesheet");
            }
        }

        private void copyDestination(string sourceFile, string TargetPath)
        {
            string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            string copy = TargetPath + sourceFile;
            if (!File.Exists(copy))
            {
                if (File.Exists(source))
                {
                    if (!Directory.Exists(TargetPath + DESTINATIONSSUBPATH))
                        tools1.createPath(TargetPath + DESTINATIONSSUBPATH);
                    File.Copy(source, copy);
                    logCopyFile(LogNode, sourceFile, "Destination");
                }
                else
                {
                    Console.WriteLine("Missing Destination File: " + source);
                }
                //logXmlFileCopy(source,"Stylesheet");
            }
        }

        private void copyConditionTagSet(string sourceFile, string TargetPath)
        {
            string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            string copy = TargetPath + sourceFile;
            if (!File.Exists(copy))
            {
                if (File.Exists(source))
                {
                    File.Copy(source, copy);
                    logCopyFile(LogNode, sourceFile, "ConditionTagSet");
                }
                else
                {
                    Console.WriteLine("Missing ConditionTagSet: " + source);
                }
                //logXmlFileCopy(source,"Stylesheet");
            }
        }

        private void copySkin(string sourceFile, string TargetPath)
        {
            string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            string copy = TargetPath + sourceFile;
            if (!File.Exists(copy))
            {
                if (File.Exists(source))
                {
                    File.Copy(source, copy);
                    logCopyFile(LogNode, sourceFile, "Skin");
                }
                else
                {
                    Console.WriteLine("Missing Skin File: " + source);
                }
                //logXmlFileCopy(source,"Skin");
            }
        }

        private void copyManualFile(string sourceFile, string TargetPath)
        {
            string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            string copy = TargetPath + sourceFile;
            if (!File.Exists(copy))
            {
                if (File.Exists(source))
                {
                    File.Copy(source, copy);
                    logCopyFile(LogNode, sourceFile, "ManualFile");
                }
                else
                {
                    Console.WriteLine("Missing Manual File: " + source);
                }
                //logXmlFileCopy(source,"ManualFile");
            }
        }

        private void copyPageLayoutFile(string sourceFile, string TargetPath)
        {
            string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            string copy = TargetPath + sourceFile;
            if (!File.Exists(copy))
            {
                if (File.Exists(source))
                {
                    XmlDocument layoutFile = new XmlDocument();
                    tools1.LoadXmlFile(layoutFile, source);
                    //fetchVarReferences(layoutFile, Token);
                    if (!Directory.Exists(Path.GetDirectoryName(copy)))
                        tools1.createPath(Path.GetDirectoryName(copy));
                    layoutFile.Save(copy);
                    //File.Copy(source, copy);
                    logCopyFile(LogNode, sourceFile, "PageLayout");
                }
                else
                {
                    Console.WriteLine("Missing PageLayout File: " + source);
                }
                //logXmlFileCopy(source,"PageLayout");
            }
        }

        private void copyMasterPage(string sourceFile, string TargetPath)
        {
            string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            string copy = TargetPath + sourceFile;
            if (!File.Exists(copy))
            {
                if (File.Exists(source))
                {
                    XmlDocument masterPage = new XmlDocument();
                    tools1.LoadXmlFile(masterPage, source);
					//fetchVarReferences(masterPage, Token);
					fetchScriptLinks(masterPage,true);
                    if (!Directory.Exists(Path.GetDirectoryName(copy)))
                        tools1.createPath(Path.GetDirectoryName(copy));
                    masterPage.Save(copy);
                    //File.Copy(source, copy);                    
                    logCopyFile(LogNode, sourceFile, "MasterPage");
                }
                else
                {
                    Console.WriteLine("Missing MasterPage: " + source);
                }
                //logXmlFileCopy(source,"MasterPage");
            }
        }

        private void copyVarFile(string sourceFile, string TargetPath)
        {
            string source = ProjectPath + sourceFile;
            fi = new FileInfo(source); if (fi.LastWriteTime > lastWriteTime) lastWriteTime = fi.LastWriteTime;
            //string copy = TargetPath + sourceFile;
            //string copy = TargetPath + VARIABLESSUBPATH + Token + ".flvar";
            string copy = TargetPath + VARIABLESSUBPATH + "MyVariables.flvar";
			//if (!Directory.Exists(Path.GetDirectoryName(copy))) tools1.createPath(Path.GetDirectoryName(copy));

			if (!File.Exists(copy))
            {
                if (File.Exists(source))
                {
                    File.Copy(source, copy);
                    logCopyFile(LogNode, sourceFile, "VariableFile");
                    //logXmlFileCopy(source,"Variables");
                }
                else LogXmlDuplicate(source);
            }
        }

        public XmlNodeList CollectCshIDs(bool Subfolder)
        {
            XmlNodeList newIds = null;
            List<string> IDs = new List<string>();
            string[] possibleCshDirs = { @"\Project\Advanced\CSH", @"\Project\Advanced" };
            int count = 0;
            bool found = false;
            foreach (string possibleCshDir in possibleCshDirs)
            {
                if (Directory.Exists(ProjectPath + possibleCshDir))
                {
                    foreach (string file in Directory.GetFiles(ProjectPath + possibleCshDir))
                    {
                        if (file.EndsWith(".flali"))
                        {
                            //if(!tailPath(file,2).ToLower().Contains(token.ToLower()))
                            {
                                //if (loud) Console.WriteLine(tailPath(file, 2));
                                found = true;
                                XmlDocument newAlias = new XmlDocument();

                                tools1.LoadXmlFile(newAlias, file);
                                newIds = newAlias.SelectNodes("//Map");
                                foreach (XmlNode Id in newIds)
                                {
                                    if (Id.Attributes["Name"] == null) continue;
                                    string name = Id.Attributes["Name"].Value;
                                    switch (BuildType)
                                    {
                                        case ("GLHelp"):
                                            if (!name.StartsWith("GL")) continue;
                                            break;
                                        case ("V4Help"):
                                            if (name.StartsWith("GL")) continue;
                                            break;
                                        default:
                                            break;
                                    }
                                    
                                    if (Id.Attributes["Link"] == null) continue;
                                    string link = Id.Attributes["Link"].Value;
                                    if (!IDs.Contains(name))
                                    {
                                        if (Subfolder)
                                        {
                                            link = link.Replace("/Content/", "/Content/" + Token + "/");
                                            Id.Attributes["Link"].Value = link;
                                        }
                                        //{
                                        //	//Link = "/Content/mainSearchGraphical.htm"
                                        //	string link = Id.Attributes["Link"].Value;
                                        //	link = link.Replace("/Content/","/Content/" + projectToken + "/");
                                        //	Id.Attributes["Link"].Value = link;
                                        //}										
                                        IDs.Add(name);
                                        count++;
                                    }
                                }
                                //break;
                            }
                        }
                    }
                }
            }
            if (!found) Console.WriteLine(sep + "No CSH keys found.");
            else Console.WriteLine(sep + "Found " + newIds.Count.ToString()+ " CSH keys.");
            return newIds;
        }

		private void logXmlFileCopy(string file, string filetype)
		{
			XmlNode newNode = Logfile.CreateElement("File");
			XmlAttribute FileName = Logfile.CreateAttribute("FileName");
			FileName.Value = System.IO.Path.GetFileName(file).ToLower();
			XmlAttribute FileType = Logfile.CreateAttribute("FileType");
			FileType.Value = filetype;
			XmlAttribute FilePath = Logfile.CreateAttribute("SourcePath");
			FilePath.Value = System.IO.Path.GetDirectoryName(file);
			newNode.Attributes.Append(FileName);
			newNode.Attributes.Append(FileType);
			newNode.Attributes.Append(FilePath);
			Logfile.DocumentElement.AppendChild(newNode);
		}

		private XmlNode logCopyProject()
		{
            XmlNode newNode = Logfile.SelectSingleNode("//Project[@Token='" + this.Token + "']");
            if (newNode == null)
            {
                newNode = Logfile.CreateElement("Project");
                XmlAttribute projectToken = Logfile.CreateAttribute("Token");
                projectToken.Value = this.Token;
                if (projectToken.Value == "tdmConventions")
                    projectToken.Value = "TDMConventions";
                XmlAttribute projectPath = Logfile.CreateAttribute("Path");
                projectPath.Value = this.ProjectPath;
                newNode.Attributes.Append(projectToken);
                newNode.Attributes.Append(projectPath);
            }

            Logfile.DocumentElement.AppendChild(newNode);

            //if (DataBase)
            //    DataBaseFunctions.DBCallProjectsUpdate(this.Token, null, BuildType);

            return newNode;
		}

		//------------------------------------------------------------------------------------------------------------------

		public override bool Equals(object obj)
		{
			if(obj is FlareProject other)
			{
				return this.Token == other.Token;
			}
			return false;
		}

        private void logCopyFile(XmlNode parent, string id, string filetype, string externalToken = null)
        {
            XmlNode newNode = Logfile.CreateElement("File");
            XmlAttribute FileName = Logfile.CreateAttribute("FileName");
            FileName.Value = System.IO.Path.GetFileName(id);
            XmlAttribute ID = Logfile.CreateAttribute("ID");
            //ID.Value = file.ToLower();
            if (!id.StartsWith("\\"))
                id = "\\" + id;
            ID.Value = id;
            XmlAttribute FileType = Logfile.CreateAttribute("FileType");
            FileType.Value = filetype;

            newNode.Attributes.Append(FileType);
            newNode.Attributes.Append(ID);
            newNode.Attributes.Append(FileName);

            if (externalToken != null) 
            {
                if (externalToken == "tdmConventions") 
                    externalToken = "TDMConventions";
                if (externalToken == "tms")
                    externalToken = "TMS";
                XmlAttribute ExternalProject = Logfile.CreateAttribute("ExternalProject");
                //ExternalProject.Value = Token;
                ExternalProject.Value = externalToken;
                newNode.Attributes.Append(ExternalProject);

                string xpath = "//Project[@Token='" + externalToken + "']";
                XmlNode externalProjectNode = parent.SelectSingleNode(xpath);
                if (externalProjectNode == null)
                {
                    externalProjectNode = Logfile.CreateElement("Project");
                    XmlAttribute Token = Logfile.CreateAttribute("Token");
                    Token.Value = externalToken;
                    XmlAttribute Path = Logfile.CreateAttribute("Path");
                    Path.Value = ProjectParentPath + "\\" + externalToken;

                    externalProjectNode.Attributes.Append(Token);
                    externalProjectNode.Attributes.Append(Path);

                    parent.ParentNode.AppendChild(externalProjectNode);
                    externalProjectNode.AppendChild(newNode);
                }                
                else
                    parent.AppendChild(newNode);

            }
            else
                parent.AppendChild(newNode);


            //if (externalToken != null)
            //{
            //    /*
            //    <File
            //    FileType="Image"
            //    ID="Resources\Images\general\glPrintExampleThumbnailView.png"
            //    FileName="glPrintExampleThumbnailView.png"
            //    ExternalProject="tdmCompact" />
            //    */
            //    XmlAttribute ExternalProject = Logfile.CreateAttribute("ExternalProject");
            //    ExternalProject.Value = externalToken;
            //    newNode.Attributes.Append(ExternalProject);
            //}
            //string fullFilePath = (ProjectPath + '\\' + file).Replace(@"\\", @"\");
            //if (!File.Exists(fullFilePath))
            //{
            //    Console.WriteLine(sep + "DB-Logger says: \"File not found\": " + fullFilePath);
            //    Console.ReadLine();
            //}
            //else
            //{
            //    if (DataBase) 
            //        DataBaseFunctions.DBCallFilesCheckedUpdate(fullFilePath, Branch, Language, Token, file, BuildType);
            //}
        }

        private void LogDuplicateFile(string source)
        {
            //< File FileName = "KeyFeatures.png" SourcePath = "C:\docu\docusrc\00\Main\Content\Resources\Images" />

            string FileName = System.IO.Path.GetFileName(source);
            //string a = "//File[@ID='" + source.ToLower() + "']";
            string a = "//File[@ID='" + source + "']";
            XmlNode node = Logfile.SelectSingleNode(a);
            int i = 1;
            bool found = false;
            string sourcePath = System.IO.Path.GetDirectoryName(source);
            if (node != null)
            {
                if (this.Token != node.ParentNode.Attributes["Token"].Value)
                {
                    foreach (XmlAttribute att in node.Attributes)
                    {
                        if (att.Name.StartsWith("Lookalike"))
                        {
                            i++;
                            if (att.Value == this.Token) found = true;
                        }
                    }
                    if (!found)
                    {
                        string suffix = i.ToString();
                        XmlAttribute NewFilePath = Logfile.CreateAttribute("Lookalike" + suffix);
                        NewFilePath.Value = this.Token;
                        node.Attributes.Append(NewFilePath);
                    }
                }
            }
            //else
            //{
            //	Console.WriteLine("Ambiguity with double Files: " + FileName);
            //	Console.ReadKey();
            //}

        }
 
		private void LogXmlDuplicate(string source)
        {
            //< File FileName = "KeyFeatures.png" SourcePath = "C:\docu\docusrc\00\Main\Content\Resources\Images" />
            //string FileName = System.IO.Path.GetFileName(source).ToLower();
            string FileName = System.IO.Path.GetFileName(source); //.ToLower();
            string a = "//File[@FileName='" + FileName + "']";
            XmlNode node = Logfile.SelectSingleNode(a);
            int i = 0;
            bool found = false;
            string sourcePath = System.IO.Path.GetDirectoryName(source);
            if (node != null)
            {
                foreach (XmlAttribute att in node.Attributes)
                {
                    if (att.Name.StartsWith("SourcePath"))
                    {
                        i++;
                        if (att.Value == sourcePath) found = true;
                    }
                }
                if (!found)
                {
                    string suffix = i.ToString();
                    XmlAttribute NewFilePath = Logfile.CreateAttribute("SourcePath" + suffix);
                    NewFilePath.Value = sourcePath;
                    node.Attributes.Append(NewFilePath);
                }
            }
            //else
            //{
            //	Console.WriteLine("Ambiguity with double Files: " + FileName);
            //	Console.ReadKey();
            //}

        }

        private void ParseForVariableReferences(XmlDocument Topic)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(Topic.NameTable);
            manager.AddNamespace("MadCap", "http://www.madcapsoftware.com/Schemas/MadCap.xsd");
            XmlNodeList VarNodes = Topic.SelectNodes("//MadCap:variable", manager);
            foreach (XmlNode var in VarNodes)
            {
                string VariableName = var.Attributes["name"].Value;
                if (!Variables.Contains(VariableName)) Variables.Add(VariableName);
                if (!VariableReferences.ContainsKey(VariableName)) VariableReferences.Add(VariableName, 1);
                else VariableReferences[VariableName] = VariableReferences[VariableName] + 1;
            }
        }

        public void ParseForStyleReferences(string filename)
		{
			if(!File.Exists(this.ProjectPath + filename)) return;
			XmlTextReader reader = new XmlTextReader(this.ProjectPath + filename);
			string currentSelektor = "";
			string currentClass = "";
			while(reader.Read())
			{				
				switch(reader.NodeType)
				{
					case XmlNodeType.Element: // The node is an element.     
						currentSelektor = reader.Name;
						//Console.Write("<" + reader.Name);
						//Console.WriteLine(">");
						//if(reader.AttributeCount > 0) 
						currentClass = WriteReaderAttributes(reader);
						//Console.Indent();
						string styleName = currentSelektor + currentClass;
                        if (!StyleReferences.ContainsKey(styleName))
                        {
                            //Console.WriteLine(styleName);
                            StyleReferences.Add(styleName, 1);
                        }
                        else
                            StyleReferences[styleName] = StyleReferences[styleName] + 1;

						break;
					case XmlNodeType.Text: //Display the text in each element.     
						//Console.WriteLine(reader.Value);
						break;
					case XmlNodeType.EndElement: //Display the end of the element.     

						//Console.Unindent();
						//Console.Write("</" + reader.Name);
						//Console.WriteLine(">");
						break;
					case XmlNodeType.Attribute:
						//Console.Write("</" + reader.Name);
						//Console.WriteLine(">");
						break;
				}
			}
			//Console.ReadLine();     
		}

		private string WriteReaderAttributes(XmlTextReader reader)
		{
			int attributeCount = reader.AttributeCount;
			for(int i = 0; i < attributeCount; i++)
			{
				reader.MoveToAttribute(i);
				if(reader.Name == "class")
				{
					return "." + reader.Value.ToString();
					break;
				}
				//Console.Write("  Attribute ");
				//Console.Write("</" + reader.Name + " + " + reader.Value.ToString());
				//Console.WriteLine(">");
			}
			return "";
		}
	}
}
