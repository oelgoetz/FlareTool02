using System.Xml;
using System.IO;
using System;
using txtFiles;
using System.Collections.Generic;
using System.Xml.Linq;
using tools;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;
using FlareTool01;
using System.Windows.Forms;

namespace ConsoleApp1
{
    internal class OutputModder
    {
        string TARGETSUBPATH = @"\Project\Targets\";
        
        public OutputModder(string Path, string targetFile, string modFile, FlareProject main_project = null)
        {
            if (!Directory.Exists(Path))
            {
                Console.WriteLine(Path + " not found. Quit.");
                return;
            }            
            string Localusername = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            if (Localusername.Contains("\\"))
                Localusername = Localusername.Substring(Localusername.IndexOf('\\') + 1).Replace(" ", "_");
            string OutputPath = Path + "\\Output\\" + Localusername + "\\" + System.IO.Path.GetFileNameWithoutExtension(targetFile);

            XmlDocument target = new XmlDocument();
            tools1.LoadXmlFile(target, Path + TARGETSUBPATH + targetFile);
            XmlAttribute att = target.DocumentElement.Attributes["Type"];
            if (att != null)
                if (att.Value != "WebHelp2") return;
            Console.WriteLine("Modding Output ...");
            
            XmlDocument mods = new XmlDocument();
            tools1.LoadXmlFile(mods, modFile);
            XmlNodeList stylemods = mods.SelectNodes("//Stylesheet");
            foreach (XmlNode stylesheet in stylemods)
            {
                TextDatei stylesheetFile = new TextDatei();
                string s = stylesheetFile.ReadFile(OutputPath + "\\" + stylesheet.Attributes["Name"].Value);
                Console.WriteLine(stylesheet.Attributes["Name"].Value);// (s);
                s = s.Replace("\r\n", "\n");
                foreach (XmlNode child in stylesheet.ChildNodes)
                {
                    if (child.Name == "Style")
                    {
                        int i0 = s.IndexOf(child.Attributes["Name"].Value);
                        int i1 = i0 + child.Attributes["Name"].Value.Length + 2;
                        string t = "";
                        while (i1 < s.Length)
                        {
                            if (s[i1] == '}') break;
                            t += s[i1];
                            i1++;
                        }
                        Console.WriteLine(child.Attributes["Name"].Value + ":");
                        //Console.WriteLine(t);
                        Console.ReadLine();
                        string[] temp0 = t.Trim().Split('\n');
                        foreach (string attribute in temp0)
                        {
                            string[] temp1 = attribute.Trim().Split(':');
                            if (temp1.Length > 2) Console.WriteLine(attribute);

                            if (temp1[0].Trim() == child.Attributes["Name"].Value.Trim()) Console.WriteLine(" -->" + child.Attributes["Name"].Value.Trim());
                        }
                        Console.ReadLine();
                    }
                }
            }
        }

    }

    internal class SourceModder
    {
        private List<string> includes = new List<string>();
        private List<string> excludes = new List<string>();
        private List<string> dependencies = new List<string>();

        private bool _loud = false;

        private string projectPath;
        private string buildType;

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

        public SourceModder(string path, string BuildType, bool loud, string modFile = null, FlareProject mainProject = null)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine("SourcesModder: " + path + " not found. Quit.");
                return;
            }

            _loud = loud;
            projectPath = path;
            buildType = BuildType;

            Console.WriteLine("-------------------------------------");
            Console.WriteLine("Modding Sources ...");

            if(buildType == "GLHelp") 
            {
                includes.Add("GLOnly");
                excludes.Add("V4Only");
            }

            if (buildType == "V4Help")
            {
                includes.Add("V4Only");
                excludes.Add("GLOnly");
            }

            includes.Add("ScreenOnly");
            includes.Add("PrintOnly");

            excludes.Add("Under_Construction");
            excludes.Add("NotYetTranslated");
            excludes.Add("OutOfDate");
            
            //internal, external, dead Link counter:
            int[] totalChanges = { 0,0,0 };

            foreach (string subfolder in Directory.GetDirectories(path + TOPICSUBPATH))
            {
                foreach (string file in Directory.GetFiles(subfolder))
                {
                    if (file.EndsWith(".htm"))
                    {
                        makeWhitespacesLikeLinq(file);
                        string file2 = file; //file.Replace(".htm", "2.htm")
                        int[] result = modifyHyperlinks(file, file2);
                        if (file != file2) makeWhitespacesLikeLinq(file2);
                        if (_loud) 
                        {
                            if (result[0] + result[1] + result[2] > 0)
                                Console.WriteLine(Path.GetFileName(file) + ", int: " + result[0].ToString() + " ext: " + result[1].ToString() + " del: " + result[2].ToString());
                        }
                        totalChanges[0] += result[0];
                        totalChanges[1] += result[1];
                        totalChanges[2] += result[2];
                    }
                }
            }

            //Jetzt noch die pdfs verlinken:
            //@"C:\docuR2023\00\GLHelp\Content\Main\MainPDFs.htm"

            DateTime lastWriteTime = mainProject.GetLastWriteTime();
            Console.WriteLine("  main: " + lastWriteTime.ToString());

            foreach (FlareProject sP in mainProject.SubProjects) 
            {
                lastWriteTime = sP.GetLastWriteTime();
                Console.WriteLine("  " + sP.Token + ": " + lastWriteTime.ToString());
            }

            List<string> pdfManuals = new List<string>();
            List<string> pdfmanuals = new List<string>();            

            List<string> GeneralPDFs = new List<string>();
            List<string> StandardPDFs = new List<string>();
            List<string> InterfacePDFs = new List<string>();
            List<string> DataPDFs = new List<string>();

            string pdfFile = projectPath + @"\Content\Main\MainPDFs.htm";
            if (File.Exists(pdfFile)) 
            {
                Console.WriteLine("Fetching pdf links in " + pdfFile);
                XmlDocument pdfTopic = new XmlDocument();
                tools1.LoadXmlFile(pdfTopic, pdfFile);
               
                foreach(string targetFile in Directory.GetFiles(projectPath + TARGETSUBPATH)) 
                {
                    if (targetFile.ToLower().EndsWith("-pdf.fltar")) 
                    {
                        string pdfFileName = getPDFNamefromTarget(targetFile);
                        if (File.Exists(pdfFileName))
                        {
                            string pdfFileCopy = pdfFileName.Replace("Main\\Content", buildType + "\\Content\\Main");
                            if (!File.Exists(pdfFileCopy))
                            {
                                File.Copy(pdfFileName, pdfFileCopy);
                            }
                        }
                    }
                }

                foreach (string pdfManual in Directory.GetFiles(projectPath + TOPICSUBPATH + "Main"))
                {
                    if (pdfManual.EndsWith(".pdf")) 
                    {
                        pdfManuals.Add(Path.GetFileName(pdfManual));
                        pdfmanuals.Add(Path.GetFileName(pdfManual).ToLower());
                    }                    
                }

                string homepage = projectPath + @"\Content\Main\Home.htm";
                int savehp = 0;
                if (File.Exists(homepage))
                {
                    Console.WriteLine("Fetching pdf links in " + homepage);
                    XmlDocument HomeTopic = new XmlDocument();
                    tools1.LoadXmlFile(HomeTopic, homepage);

                    XmlNodeList hpLinks = HomeTopic.SelectNodes("//div[@class='table-images center HomePDFSection']");

                    //TDMGL
                    //LGMGL
                    //T3D
                    //TPSGL
                    //TMSGL
                    //SFMGL
                    //XmlNodeList hrefs = HomeTopic.SelectNodes("//a");
                    //foreach(XmlNode href in hrefs) 
                    //{
                    //    if (href.Attributes["href"] != null) 
                    //    {
                    //        string f = href.Attributes["href"].Value.Trim();
                    //        if (f.ToLower().EndsWith(".pdf"))
                    //        {
                    //            string tok = href.Attributes["href"].Value;
                    //            tok = tok.Substring(0, tok.IndexOf(" - "));
                    //            string newName = "";
                    //            foreach (string s in pdfManuals)
                    //            {
                    //                if (s.StartsWith(tok))
                    //                {
                    //                    href.Attributes["href"].Value = s;
                    //                    savehp ++;
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //    }
                    //}
                    //if (hpLinks != null)
                    //{
                    //    foreach(XmlNode hpLink in hpLinks) 
                    //    {
                    //        XmlNodeList refs = hpLink.SelectNodes("/div/a");
                    //        foreach (XmlNode _ref in refs)
                    //        {
                    //            string tok = _ref.Attributes["href"].Value;
                    //            tok = tok.Substring(0, tok.IndexOf(" - "));
                    //            string newName = "";
                    //            foreach (string s in pdfManuals)
                    //            {
                    //                if (s.StartsWith(tok))
                    //                {
                    //                    _ref.Attributes["href"].Value = s;
                    //                    savehp = true;
                    //                    break;
                    //                }
                    //            }
                    //        }
                    //    }

                    //    //while (hpLinks.HasChildNodes) hpLinks.RemoveChild(hpLinks.ChildNodes[0]);

                    //    //if (GeneralPDFs.Count > 0)
                    //    //{
                    //    //    addPdfLinks(GeneralPDFs, hpLinks);
                    //    //}
                    //    //else
                    //    //{
                    //    //    hpLinks.ParentNode.RemoveChild(hpLinks.PreviousSibling);//h2 löschen
                    //    //}
                    //}
                }                

                foreach (string targetFile in Directory.GetFiles(projectPath + TARGETSUBPATH)) 
                {
                    if (targetFile.ToLower().EndsWith("-pdf.fltar")) 
                    {
                        string token = Path.GetFileName(targetFile); 
                        token = token.Substring(0, token.Length - 10);           //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!TODO: -10 ist gefährlich!             
                        string pdfFilename = getpdfFileName(targetFile) + ".pdf";
                        pdfFilename = pdfFilename.Trim();
                        if (token.StartsWith("i"))
                            InterfacePDFs.Add(pdfFilename);
                        else
                        if( token.ToLower().StartsWith("main") || 
                            token.ToLower().StartsWith("tdmnews") || 
                            token.ToLower().StartsWith("tdmcompact") || 
                            token.ToLower().StartsWith("tdm2d3d") || 
                            token.ToLower().StartsWith("tdmconv"))
                            GeneralPDFs.Add(pdfFilename);
                        else
                            StandardPDFs.Add(pdfFilename);

                        if (File.Exists(projectPath + TOPICSUBPATH + "Main\\" + pdfFilename)) 
                        {
                            int i = pdfmanuals.IndexOf(pdfFilename.ToLower());
                            pdfmanuals.Remove(pdfFilename.ToLower());
                            pdfManuals.RemoveAt(i);
                            //Console.WriteLine("found: " + pdfFilename);
                        }
                        else 
                        {
                            Console.WriteLine("Pdf manual missing: " + pdfFilename);
                            searchPdf(pdfFilename);
                        }                                               
                    }
                }

                foreach(string pdfManual in pdfManuals)
                {                                            
                    Console.WriteLine("Pdf manual not found in main and subprojects: " + pdfManual);
                    //iMST - CAD CAM-Schnittstelle TDM - INDEX Virtual Machine (AME).pdf
                    searchPdf(pdfManual);
                    DataPDFs.Add(pdfManual);
                }

                GeneralPDFs.Sort();
                StandardPDFs.Sort();
                InterfacePDFs.Sort();
                DataPDFs.Sort();

                XmlNode genM = pdfTopic.SelectSingleNode("//div[@ID='General']");
                if (genM != null)
                {
                    while (genM.HasChildNodes) genM.RemoveChild(genM.ChildNodes[0]);

                    if(GeneralPDFs.Count > 0)
                    {
                        addPdfLinks(GeneralPDFs, genM);                    
                    }
                    else 
                    {
                        genM.ParentNode.RemoveChild(genM.PreviousSibling);//h2 löschen
                    }
                }

                XmlNode StandardM = pdfTopic.SelectSingleNode("//div[@ID='StandardModules']");
                if(StandardM != null) 
                {
                    while (StandardM.HasChildNodes) StandardM.RemoveChild(StandardM.ChildNodes[0]);

                    if (StandardPDFs.Count > 0)
                    {
                        addPdfLinks(StandardPDFs, StandardM);
                    }
                    else
                    {
                        StandardM.ParentNode.RemoveChild(StandardM.PreviousSibling);//h2 löschen
                    }
                }

                XmlNode InterfaceM = pdfTopic.SelectSingleNode("//div[@ID='Interfaces']");
                if (InterfaceM != null)
                {
                    while (InterfaceM.HasChildNodes) InterfaceM.RemoveChild(InterfaceM.ChildNodes[0]);

                    if (InterfacePDFs.Count > 0)
                    {
                        addPdfLinks(InterfacePDFs, InterfaceM);
                    }
                    else
                    {
                        InterfaceM.ParentNode.RemoveChild(InterfaceM.PreviousSibling);//h2 löschen
                    }
                }

                XmlNode DataM = pdfTopic.SelectSingleNode("//div[@ID='Data']");
                if (DataM != null)
                {
                    while (DataM.HasChildNodes) DataM.RemoveChild(DataM.ChildNodes[0]);
                    if (DataPDFs.Count > 0)
                    {
                        addPdfLinks(DataPDFs, DataM);
                    }
                    else
                    {
                        DataM.ParentNode.RemoveChild(DataM.PreviousSibling);//h2 löschen
                    }
                }

                pdfTopic.Save(pdfFile);
                tools1.BeautifyXml(pdfFile, pdfFile);
            }
            else 
            {
                Console.WriteLine(pdfFile + " not found. No pdf links were generated.");
            }            
            //Console.ReadLine();

            Console.WriteLine("Changed " + totalChanges[0].ToString() + " internal links. " + totalChanges[1].ToString() + " external links. ");
            Console.WriteLine("Removed " + totalChanges[2].ToString() + " dead links. ");

            Console.WriteLine("--- Sourcemodding finished.");
        }

        private string getPDFNamefromTarget(string target)
        {
            XmlDocument pdfTarget = new XmlDocument();
            pdfTarget.Load(target);
            string OutputFile = "";
            if (pdfTarget.DocumentElement.Attributes["OutputFile"] != null)
            {
                OutputFile = pdfTarget.DocumentElement.Attributes["OutputFile"].Value;
                OutputFile = projectPath.Replace(buildType, "Main") + "\\Content\\" + OutputFile + ".pdf";
                //if (File.Exists(OutputFile))
                //{
                //    Console.WriteLine(Token + " " + OutputFile);
                //}
            }
            return OutputFile;
        }

        private void searchPdf(string pdfFile) 
        {
            string [] temp = projectPath.Split('\\');
            string language = temp[2];
            string pdfDirectory = temp[0] + '\\' + temp[1] + "\\hlp\\TDM\\" + language + "\\PDF"; //projectPath.Substring(projectPath, proje)
            if(Directory.Exists(pdfDirectory))
            {
                bool found = false;
                foreach (string file in Directory.GetFiles(pdfDirectory)) //Directory.GetFiles(directory)) 
                {
                    string f1 = pdfFile.ToLower().Trim();
                    string f2 = Path.GetFileName(file).ToLower();
                    if ( f2 == f1)
                    {
                        Console.WriteLine("Found: " + file);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Console.WriteLine("Not found: " + pdfFile);
                }
            }               
            //}
        }

        private void addPdfLinks(List<string> pdfs, XmlNode g) 
        {
            if (g == null) return;
            XmlDocument pdfTopic = g.OwnerDocument;
            foreach (string pdf in pdfs)
            {
                //<p>
                //<a href="CAD2DGL - TDM 2D-Grafik Editor Global Line.pdf" target ="_blank" >
                //<img src="Resources/Images/general/iconPdf.png" class="href" />CAD2DGL - TDM 2D-Grafik Editor Global Line</a>
                //</p>
                XmlElement newP = pdfTopic.CreateElement("p");
                XmlElement newI = pdfTopic.CreateElement("img");
                XmlAttribute newS = pdfTopic.CreateAttribute("src"); newS.Value = "../Resources/Images/general/iconPdf.png";
                XmlAttribute newC = pdfTopic.CreateAttribute("class"); newC.Value = "href";
                newI.Attributes.Append(newS);
                newI.Attributes.Append(newC);
                XmlElement newA = pdfTopic.CreateElement("a"); 
                XmlText newB = pdfTopic.CreateTextNode(pdf.Replace(".pdf", ""));
                newA.AppendChild(newI);
                newA.AppendChild(newB);
                XmlAttribute newH = pdfTopic.CreateAttribute("href"); newH.Value = pdf;
                XmlAttribute newT = pdfTopic.CreateAttribute("target"); newT.Value = "_blank";
                newA.Attributes.Append(newH);
                newA.Attributes.Append(newT);
                newP.AppendChild(newA);

                g.AppendChild(newP);
            }
        }

        private string getpdfFileName(string target) 
        { 
            XmlDocument pdfTopic = new XmlDocument();
            tools1.LoadXmlFile(pdfTopic, target);
            XmlNode catapultTarget = pdfTopic.SelectSingleNode("//CatapultTarget");
            if (catapultTarget != null) 
            {
                if (catapultTarget.Attributes["OutputFile"] != null)
                    return catapultTarget.Attributes["OutputFile"].Value;
            }
            return "";
        }

        private void switchToPrintBuild() 
        {
            if (includes.Contains("ScreenOnly")) includes.Remove("ScreenOnly");
            if (!excludes.Contains("ScreenOnly")) excludes.Add("ScreenOnly");

            if (excludes.Contains("PrintOnly")) excludes.Remove("PrintOnly");
            if (!includes.Contains("PrintOnly")) includes.Add("PrintOnly");
        }

        private void switchToScreenBuild()
        {
            if (includes.Contains("PrintOnly")) includes.Remove("PrintOnly");
            if (!excludes.Contains("PrintOnly")) excludes.Add("PrintOnly");

            if (excludes.Contains("ScreenOnly")) excludes.Remove("ScreenOnly");
            if (!includes.Contains("ScreenOnly")) includes.Add("ScreenOnly");
        }

        private int[] modifyHyperlinks(string topic1, string topic2 = null)
        {
            int[] result = { 0, 0, 0 };
            const string EXTPROJECT_HREF_PREFIX = "../";
            XmlDocument topic = new XmlDocument();
            tools1.LoadXmlFile(topic, topic1);
            XmlNamespaceManager manager = new XmlNamespaceManager(topic.NameTable);
            manager.AddNamespace("MadCap", "http://www.madcapsoftware.com/Schemas/MadCap.xsd");
            XmlNodeList links = topic.SelectNodes("//a", manager);
            //foreach (XmlNode link in links)
            for (int i = 0; i < links.Count; i++)
            {
                XmlNode link = links[i];
                if (isUsed(link))
                {
                    XmlAttribute href = link.Attributes["href"];
                    if (href != null)
                    {
                        if (link.InnerText == "")
                        {
                            foreach(XmlNode child in link.ChildNodes) 
                            {
                                if(child.Name == "img") 
                                { 
                                    if(child.Attributes["src"] == null) 
                                    {
                                        Console.WriteLine("\"src\" attribute missing for linked image in: " + link.BaseURI);
                                    }
                                }
                                else 
                                {
                                    Console.WriteLine("No rule for modding link in: " + link.BaseURI);
                                }
                            }                            
                            //Console.ReadLine();
                        }                        
                        string target = href.Value;
                        //Links auf Bookmarks innerhalb der Seite: Nichts tun
                        if (target.StartsWith("#")) continue;
                        //Webliks: Nichts tun
                        if (target.StartsWith("http")) continue;
                        //TODO: Links überprüfen, ggf. Stillegen und result[2]++;
                        string bookmark = "";
                        if (target.Contains("#"))
                        {
                            bookmark = target.Substring(target.IndexOf('#') + 1);
                            target = target.Substring(0, target.IndexOf('#'));
                        }
                        //prüfen, ob die Zieldatei vorhanden ist:
                        string sourcePath = Path.GetDirectoryName(topic1);
                        string targetcheck = target;
                        //Ungültige Links generell stillegen:
                        while (targetcheck.StartsWith(EXTPROJECT_HREF_PREFIX))
                        {
                            targetcheck = targetcheck.Substring(EXTPROJECT_HREF_PREFIX.Length);
                            sourcePath = sourcePath.Substring(0, sourcePath.LastIndexOf('\\'));
                        }
                        string targetFile = sourcePath + '\\' + targetcheck.Replace('/', '\\');
                        if (!File.Exists(targetFile))
                        {
                            unbindTextNode(link);
                            result[2]++;
                            continue;
                        }
                        //Jetzt bleibt nicht mehr viel übrig ...
                        //Gültige Links in andere Projekte nur für Print Output stillegen:
                        if (target.StartsWith(EXTPROJECT_HREF_PREFIX))
                        {
                            //Textinhalt für Print Output duplizieren
                            duplicateTextNode(link, "Default.PrintOnly");
                            //Gültigen Link in anderes Projekt für Screen Output markieren:
                            XmlAttribute conditions = link.Attributes["MadCap:conditions"];
                            if (conditions == null)
                            {
                                conditions = topic.CreateAttribute("MadCap", "conditions", "http://www.madcapsoftware.com/Schemas/MadCap.xsd");
                                link.Attributes.Append(conditions);
                            }
                            conditions.Value = "Default.ScreenOnly";
                            result[1]++;
                            continue;
                        }
                    }
                }
            }
            if (result[0] + result[1] + result[2] > 0)
            {
                if (topic2 == null)
                    topic.Save(topic1);
                else
                    topic.Save(topic2);
            }
            return result;
        }

        private static void duplicateTextNode(XmlNode link, string condition)
        {
            string txt = link.InnerText;
            XmlElement span = link.OwnerDocument.CreateElement("span");
            XmlAttribute cond = link.OwnerDocument.CreateAttribute("MadCap", "conditions", "http://www.madcapsoftware.com/Schemas/MadCap.xsd");
            cond.Value = condition;
            span.Attributes.Append(cond);
            span.InnerText = txt;
            link.ParentNode.InsertAfter(span, link);
        }

        private static void unbindTextNode(XmlNode node)
        {
            string txt = node.InnerText;
            try
            {
                //Console.WriteLine(var.ParentNode.InnerXml);
                XmlNode x = node.PreviousSibling;
                if (x != null)
                {
                    x.InnerText = x.InnerText + txt;
                    x.ParentNode.RemoveChild(node);
                }
                else
                {
                    x = node.NextSibling;
                    if (x != null)
                    {
                        x.InnerText = txt + x.InnerText;
                        x.ParentNode.RemoveChild(node);
                    }
                    else
                    {
                        x = node.ParentNode;
                        x.InnerText = txt;
                        //x.RemoveChild(node);
                    }
                }                
            }
            catch
            {
            }
        }

        private void makeWhitespacesLikeLinq(string topic)
        {
            XDocument xml = XDocument.Load(topic);
            xml.Save(topic);
        }

        private bool isUsed(XmlNode node)
        {
            while (node.ParentNode != null)
            {
                if (isUsedSub(node))
                    node = node.ParentNode;
                else
                    return false;
            }
            return true;
        }

        private bool isUsedSub(XmlNode node)
        {
            if (node.Attributes == null) return true;

            if (node.Attributes["conditions"] == null)
            {
                if (node.Attributes["MadCap:conditions"] == null) return true;
                if (node.Attributes["MadCap:conditions"].Value == "") return true;
                foreach (string condition in includes) if (node.Attributes["MadCap:conditions"].Value.Contains(condition)) return true;
                return false;
            }
            if (node.Attributes["conditions"].Value == "") return true;
            foreach (string condition in includes) if (node.Attributes["conditions"].Value.Contains(condition)) return true;
            return false;
        }

        private void checkMasterTOC(string tocFile) 
        {
            dependencies.Clear();
            XmlDocument TOC = new XmlDocument();
            tools1.LoadXmlFile(TOC, tocFile);
            GetNodes(TOC.DocumentElement);
        }

        private void GetNodes(XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Element)
            {
                //if (node.Attributes["Link"] != null && node.Attributes["Link"].Value.Contains("../../TMSGL/Content/tmsglCostCenterApp.htm"))
                //{
                //    Console.WriteLine(node.OwnerDocument.BaseURI);
                //    Console.ReadLine();
                //}

                if (isUsed(node))
                {
                    if (node.Name == "TocEntry")
                    {
                        if (node.Attributes["Link"] != null)
                        {
                            string link = node.Attributes["Link"].Value.Replace('/', '\\');
                            
                            string bookmark = "";
                            if (link.Contains("#"))
                            {
                                bookmark = link.Substring(link.IndexOf('#') + 1);
                                link = link.Substring(0, link.IndexOf('#'));
                                //Console.WriteLine(sep + level.ToString() + ": " + link + "  " + bookmark);
                            }
                            string file = projectPath + link;
                            if (File.Exists(file))
                            {
                                if (!dependencies.Contains(file)) dependencies.Add(file);
                                //parseTopic(file);
                            }
                            else 
                            {
                                Console.WriteLine("Missing linked TocEntry: " + file);
                            }
                            string fileExt = link.Substring(link.LastIndexOf("."));
                            switch (fileExt)
                            {
                                case ".htm":
                                    if (File.Exists(file))
                                    {
                                        if (!dependencies.Contains(file)) dependencies.Add(file);
                                        //parseTopic(file);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Missing linked TocEntry: " + file);
                                    }
                                    break;
                                case ".fltoc":
                                    if (File.Exists(file))
                                    {
                                        if (!dependencies.Contains(file)) dependencies.Add(file);
                                        XmlDocument subTOC = new XmlDocument();
                                        tools1.LoadXmlFile(subTOC, file);
                                        GetNodes(subTOC.DocumentElement);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Missing linked TocEntry: " + file);
                                    }
                                    break;
                                case ".flprj":
                                    Console.WriteLine("Error: linked flprj in " + file + " !");
                                    Console.ReadLine();
                                    break;
                            }//switch
                            if (node.Attributes["PageLayout"] != null)
                            {
                                string pageLayout = node.Attributes["PageLayout"].Value.ToString().Replace('/', '\\');
                                pageLayout = projectPath + pageLayout;
                                if (File.Exists(pageLayout))
                                {
                                    if (!dependencies.Contains(pageLayout)) dependencies.Add(pageLayout);
                                    //parseTopic(file);
                                }
                                else
                                {
                                    Console.WriteLine("Missing linked PageLayout: " + pageLayout);
                                }
                            }
                        }
                    }
                }
                if (node.HasChildNodes)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        GetNodes(child);
                    }
                }
            }
        }

    }
}
