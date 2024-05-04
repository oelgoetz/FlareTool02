using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using txtFiles;
using System.Xml.Linq;
using System.Drawing;
using System.Text.RegularExpressions;

namespace tools
{    
    class tools1
    {
        public static bool isnumeric(string s)
		{
			if(!Regex.IsMatch(s,"[^0-9]"))
				return true;
			else
				return false;
		}

        public static bool addIfNew(string _STRING, List<string> _LIST)
        {
            if (!_LIST.Any(s => s.Equals(_STRING, StringComparison.OrdinalIgnoreCase)))
            {
                _LIST.Add(_STRING);
                return true;
            }
            else
                return false;
        }

        public static bool checkFileBOM(string filename)
        {
            var bytes = System.IO.File.ReadAllBytes(filename);
            if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                return true;
                //System.IO.File.WriteAllBytes(filename, bytes.Skip(3).ToArray());
            }
            return false;
        }

        public static void removeFileBOM(string filename)
        {
            var bytes = System.IO.File.ReadAllBytes(filename);
            if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
            {
                System.IO.File.WriteAllBytes(filename, bytes.Skip(3).ToArray());
            }
        }

        public static string extractCommonDirectoryRevised(XmlNodeList FileNodes)
        {
            string shortestCommonPath = Path.GetDirectoryName(FileNodes[0].Attributes["path"].Value.Replace('/', '\\'));
            foreach (XmlNode FileNode in FileNodes)
            {
                //Pfad zur Source-Datei ermitteln:
                string path = Path.GetDirectoryName(FileNode.Attributes["path"].Value.Replace('/', '\\'));
                //if (path.Length < result.Length)
                //    result = path;            
                string[] temp = path.Split('\\');
                string newCandidate = "";
                foreach (string part in temp)
                {
                    newCandidate += part;
                    if (!shortestCommonPath.StartsWith(newCandidate))
                    {
                        shortestCommonPath = newCandidate.Substring(0, newCandidate.LastIndexOf("\\"));
                        continue;
                    }
                    else
                    {
                        newCandidate += "\\";
                    }
                }
                //string newCandidate = temp[0];
                //int i = 1;
                //do
                //{
                //    newCandidate += "\\" + temp[i];
                //    i++;
                //}
                //while (i < temp.Length && shortestCommonPath.StartsWith(newCandidate));//&& !isnumeric(temp[i - 1]));
                //if (newCandidate.Length < shortestCommonPath.Length)
            }
            return shortestCommonPath;
        }

        public static XmlDocument createXmlFile(string filename, string rootElement)
        {
            XmlDocument logFile = new XmlDocument();
            XmlDeclaration xmlDeclaration = logFile.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = logFile.DocumentElement;
            logFile.InsertBefore(xmlDeclaration, root);
            XmlElement element1 = logFile.CreateElement(string.Empty, rootElement, string.Empty);
            logFile.AppendChild(element1);
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(filename))) tools1.createPath(System.IO.Path.GetDirectoryName(filename));
            logFile.Save(filename);
            return logFile;
            //if (this.logFile) logFileBuffer += ("created;" + aliasFilePath + "\n");
        }

        public static string extractCommonDirectory(XmlNodeList FileNodes)
        {
            if (FileNodes.Count <= 0) return "";
            string result = Path.GetDirectoryName(FileNodes[0].Attributes["path"].Value.Replace('/', '\\'));
            foreach (XmlNode FileNode in FileNodes)
            {
                //Pfad zur Source-Datei ermitteln:
                string path = Path.GetDirectoryName(FileNodes[0].Attributes["path"].Value.Replace('/', '\\'));
                if (path.Length < result.Length) result = path;
            }
            string[] temp = result.Split('\\');
            result = temp[0];
            int i = 1;
            do
            {
                result += "\\" + temp[i];
                i++;
            }
            while (i < temp.Length && !isnumeric(temp[i - 1]));
            return result;
        }

		public static bool isUsedSub(XmlNode node, List<string> includes, List<string> excludes)
        {
            //ConditionTagExpression="include[Default.ScreenOnly] exclude[Default.javascript or Default.NotYetTranslated or Default.OutOfDate or Default.PrintOnly or Default.ReviewOnly or Default.Under_Construction] "
            //ConditionTagExpression="include[Default.PrintOnly or Default.ReviewOnly] exclude[Default.javascript or Default.NotYetTranslated or Default.OutOfDate or Default.ScreenOnly or Default.Under_Construction] "
            //ConditionTagExpression="include[Default.PrintOnly] exclude[Default.javascript or Default.NotYetTranslated or Default.OutOfDate or Default.ReviewOnly or Default.ScreenOnly or Default.Under_Construction] "

            //<div MadCap:conditions="Default.Under_Construction,Default.OutOfDate">
            //< MadCap:conditionalText MadCap:conditions = "Default.ScreenOnly" > Shopcontrol - </ MadCap:conditionalText > Grafische Auswertung der Historie </ h1 >
            //if (node.BaseURI.Contains("Copyright.htm"))
            //if (node.BaseURI.Contains("HowTos"))
            //{
            //    Console.WriteLine("Hier.");
            //}
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

        public static bool isUsed(XmlNode node, List<string> includes, List<string> excludes) 
        {            
            

            while (node.ParentNode != null)
            {
                if (isUsedSub(node, includes, excludes))
                    node = node.ParentNode;
                else
                    return false;
            }
            return true;
        }

        public static void loadXmlDocumentProtected(XmlDocument doc, string filename, string functionName = null)
        {
            try
            {
                doc.Load(filename);
            }
            catch (Exception ex)
            {
                //Windows.Clipboard.Text = filename;
                //Clipboard.SetText(filename);
                string s = "Error ";
                if (functionName != null)
                    s += " in " + functionName;
                s += ": " + ex.Message;
                Console.WriteLine(s + " " + ex.Message + "File: " + filename);
                //log("Error: " + ex.Message + "File: " + filename);
            }
        }
		
        private static void replaceRegexFromFile(string file, string pattern, string replacement)
		{
			TextDatei txt = new TextDatei();
			string s = txt.ReadFile(file);
			Regex rgx = new Regex(pattern);
			s = rgx.Replace(pattern, replacement);
			txt.WriteFile(file,s);
		}

		private static void replaceRegexFromString(string input, string pattern, string replacement)
		{
			Regex rgx = new Regex(pattern);
			input = rgx.Replace(pattern,replacement);
		}

		private static int countRegexInFile(string file, string pattern, bool loud) 
		{ 
			Regex rgx = new Regex(pattern);
			TextDatei txt = new TextDatei();
			string s = txt.ReadFile(file);

			if(loud) 
			{
				foreach(Match match in rgx.Matches(s))
				{
					//MessageBox.Show("Found \n" + match.Value + "\n\n at position " + match.Index + "\n\n" + match.Groups[1]);
					//s = s.Replace(match.Value.ToString(),match.Groups[1].ToString());
				}
			}
			return rgx.Matches(s).Count;
		}

		private static int countRegexInString(string input, string pattern, bool loud)
		{
			Regex rgx = new Regex(pattern);

			if(loud)
			{
				foreach(Match match in rgx.Matches(input))
				{
					//MessageBox.Show("Found \n" + match.Value + "\n\n at position " + match.Index + "\n\n" + match.Groups[1]);
					//s = s.Replace(match.Value.ToString(),match.Groups[1].ToString());
				}
			}
			return rgx.Matches(input).Count;
		}

		public static void removeAnnotations(XmlDocument doc)
		{
			XmlNamespaceManager manager = new XmlNamespaceManager(new NameTable());
			manager.AddNamespace("MadCap","http://www.madcapsoftware.com/Schemas/MadCap.xsd");
			XmlNodeList annotations = doc.SelectNodes("//MadCap:annotation",manager);

			int i = 0;
			while(i < annotations.Count)
			{
				//in mixed xml the Siblings are xml text nodes. Therefore we write them into buffers:				
				string s0 = "";
				if(annotations[i].PreviousSibling != null) s0 = annotations[i].PreviousSibling.InnerText;
				string s2 = "";
				if(annotations[i].NextSibling != null) s2 = annotations[i].NextSibling.InnerText;
				//finally we buffer the content of the annotation itself
				string s1 = annotations[i].InnerText;
				//MessageBox.Show(s0 + "\n" + s1 + "\n" + s2);				
				//before we remove the annotation, we save the link to the parent node
				XmlNode parent = annotations[i].ParentNode;
				//now we can remove the annotation
				parent.RemoveChild(annotations[i]);
				//and apply the new Text to the parent element
				parent.InnerText = s0 + s1 + s2;
				i++;
			}
		}

		public static string tailPath(string path, int cut)
		{
			path = path.Replace('/','\\');
			string[] temp = path.Split('\\');
			path = temp[temp.Length - 1];
			for(int i = temp.Length; i > temp.Length - cut + 1; i--)
			{
				path = temp[i - 2] + '\\' + path;
			}
			return path;
		}

		public static string headPath(string path,int cut)
		{
			path = path.Replace('/','\\');
			for(int i = 0; i < cut; i++)
			{
				path = path.Substring(0,path.LastIndexOf('\\'));
			}
			//if(path.EndsWith(":")) path += '\\';
			return path;
		}

		public static string convertTimestampToIsoString(DateTime dt)
        {
            string timestampString = dt.Year.ToString();
            string t = dt.Month.ToString();
            while (t.Length < 2)
                t = '0' + t;
            timestampString = timestampString + t;
            t = dt.Day.ToString();
            while (t.Length < 2)
                t = '0' + t;
            timestampString = timestampString + t;
            t = dt.Hour.ToString();
            while (t.Length < 2)
                t = '0' + t;
            timestampString = timestampString + t;
            t = dt.Minute.ToString();
            while (t.Length < 2)
                t = '0' + t;
            timestampString = timestampString + t;
            t = dt.Second.ToString();
            while (t.Length < 2)
                t = '0' + t;
            timestampString = timestampString + t;
            return timestampString;
        }        

        public static void createPath(string exportfilepath) 
        {
            string[] temp = Path.GetFullPath(exportfilepath).Split('\\');
            string tempdir = "";
            for (int i = 0; i < temp.Length; i++)
            {
                tempdir = tempdir + temp[i];
                if (!Directory.Exists(tempdir))
                    Directory.CreateDirectory(tempdir);
                tempdir = tempdir + '\\';
            }
        }
        
        public static void findallfiles(List<string> res, string dir)
        {
            foreach (string file in Directory.GetFiles(dir)) res.Add(file);
            foreach (string subdir in Directory.GetDirectories(dir)) findallfiles(res, subdir);
        }

        public static string checkVariableDefinition(string file, string name, string newVal) 
        {
            string result = "";
            XmlDocument varDoc = new XmlDocument();
            varDoc.Load(file);
            string xpath = "//Variable[@Name = '" + name + "']";
            XmlNode version = varDoc.SelectSingleNode(xpath);
            if (version != null)
            {
                if (version.InnerText != newVal)
                {
                    //MessageBox.Show(version.InnerText);
                    version.InnerText = newVal;
                    varDoc.Save(file);
                    BeautifyXml(file, file);
                    result = "Variable " + name + " was updated in " + file;
                }
                else 
                {
                    result = "Variable " + name + " is already " + newVal + " in " + file;
                }
            }
            else
            {
                result = "Variable " + name + " is not defined in " + file;
            }
            return result;
        }

        public static string checkTargetVariable(string file, string name, string newVal)
        {
            string result = "";
            XmlDocument target = new XmlDocument();
            target.Load(file);
            string xpath = "//Variable[@Name = '" + name + "']";
            XmlNode version = target.SelectSingleNode(xpath);
            if(version != null) 
            {
                if(version.InnerText != newVal) 
                {
                    //MessageBox.Show(version.InnerText);
                    version.InnerText = newVal;
                    target.Save(file);
                    BeautifyXml(file, file);
                    result = "Variable " + name + " was updated in " + file;
                }
                else
                {
                    result = "Variable " + name + " is already " + newVal + " in " + file;
                }
            }
            else 
            { 
                result = "Variable " + name + " is not defined in " + file;
            }
            return result;
        }

        public static void findProjects(string dir, List<string> projects)
        {
            if (!Directory.Exists(dir)) return;
            foreach (string d in Directory.GetDirectories(dir))
            {
                bool found = false;
                string p1 = d.Substring(d.LastIndexOf('\\') + 1, d.Length - d.LastIndexOf('\\') - 1);
                if (p1 != "CVS") //CVS-Ordner ignorieren
                {
                    foreach (string f in Directory.GetFiles(d))
                    {
                        if (f.EndsWith(".flprj"))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        projects.Add(d);
                    }
                }
            }
        }
        
        public static void findallfiles(List<string> res, string dir, string ext)
        {
            foreach (string file in Directory.GetFiles(dir))
            if (file.EndsWith(ext)) res.Add(file);
            foreach (string subdir in Directory.GetDirectories(dir)) findallfiles(res, subdir, ext);
        }

        public static void CopyDir(string sourcedirectory, string targetdirectory, string exclude, bool recursive, bool force) 
        {
            if (!Directory.Exists(targetdirectory)) createPath(targetdirectory);
            foreach (string sourcefile in Directory.GetFiles(sourcedirectory)) 
            {
                string targetfile = targetdirectory + sourcefile.Substring(sourcefile.LastIndexOf('\\'));
                if(File.Exists(targetfile) && !(force)) continue;
                File.Copy(sourcefile, targetfile, true);
            }
            if (recursive)
            {
                foreach (string subdir in Directory.GetDirectories(sourcedirectory))
                {
                    if(!(subdir.EndsWith(exclude)))
                    {
                        CopyDir(subdir, targetdirectory, exclude, recursive, force);
                    }
                }
            }              
        }        

        /// <summary>
		/// Löscht ein Verzeichnis rekursiv
		/// </summary>
		/// <param name="dir"></param>
		public static void deleteDirectory(string dir)
        {
            foreach (string subdir in Directory.GetDirectories(dir))
            {
                deleteDirectory(subdir);
            }
            foreach (string file in Directory.GetFiles(dir))
            {
                File.Delete(file);
            }
        }

		public static string SortAttributes(string xml)
        {
            //http://stackoverflow.com/questions/7860755/sorting-attributes-of-xml
            var doc = XDocument.Parse(xml);
            foreach (XElement element in doc.Descendants())
            {
                var attrs = element.Attributes().ToList();
                attrs.Remove();
                attrs.Sort((a, b) => a.Name.LocalName.CompareTo(b.Name.LocalName));
                element.Add(attrs);
            }
            xml = doc.ToString();
            return xml;
        }        

        public static XmlDocument LoadXmlFile(XmlDocument doc, string filename, int msglvl = 0) 
        {
            try 
            {
                doc.Load(filename);
            }
            catch (Exception ex) 
            {
                if (msglvl == 0) 
                    Console.WriteLine(filename + ": " + ex.Message);
            }
            return doc;
        }

        public static XmlDocument createHtmlFile(string filename)
        {
            /*
            <!DOCTYPE html>
            <html>
                <head>
                <title>Title of the document</title>
                </head>

                <body>
                The content of the document......
                </body>
            </html>
            */
            XmlDocument htmlFile = new XmlDocument();
            XmlDeclaration xmlDeclaration = htmlFile.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlComment hcn = htmlFile.CreateComment("<!DOCTYPE html>");
            XmlElement root = htmlFile.DocumentElement;
            htmlFile.InsertBefore(xmlDeclaration, root);
            XmlElement element1 = htmlFile.CreateElement(string.Empty, "body", string.Empty);
            htmlFile.AppendChild(element1);
            if (!Directory.Exists(System.IO.Path.GetDirectoryName(filename))) tools1.createPath(System.IO.Path.GetDirectoryName(filename));
            htmlFile.Save(filename);
            return htmlFile;
            //if (this.logFile) logFileBuffer += ("created;" + aliasFilePath + "\n");
        }

        public static XmlNode addChildNode(XmlDocument doc, XmlNode parent, string name, string value)
        {
            XmlNode newNode = doc.CreateElement(name);
            if (value != "") newNode.InnerText = value;
            parent.AppendChild(newNode);
            return newNode;
        }

        public static XmlAttribute addAttribute(XmlDocument doc, XmlNode node, string name, string value)
        {
            XmlAttribute newAttribute = doc.CreateAttribute(name);
            newAttribute.Value = value;
            node.Attributes.Append(newAttribute);
            return newAttribute;
        }

        public static string isoLanguage(string number) 
        {
            string[] languages =    { "00"   , "01"   , "02", "03", "o4", "05", "06", "07", "08", "09", "10", "11"   , "12", "13", "14", "15", "16", "17" };
            string[] isoLanguages = { "de-DE", "en-US", "fr", "it", "pl", "sv", "hu", "cz", "nl", "es", "pt", "en-EN", "ko", "da", "fi", "nb", "ru", "zh" };
            for(int i = 0; i < languages.Length; i++) 
            {
                if (number == languages[i]) return isoLanguages[i];
            }
            return "";
        }

        public static int tdmLanguagePosition(string number)
        {
            string[] languages = { "00", "01", "02", "03", "17" };
            string[] isoLanguages = { "de-DE", "en-US", "fr", "it", "zh" };
            for (int i = 0; i < languages.Length; i++) 
                if (number == languages[i]) return i;
            return -1;
        }

        public static void sortSubNodes(XmlDocument doc) 
        {
            XmlNodeList projects = doc.SelectNodes("//project");
            foreach(XmlNode project in projects) 
            {
                //1. Alle Targets nach oben sortieren
                for (int i = 0; i < project.ChildNodes.Count; i++)
                {
                    XmlNode child = project.ChildNodes[i];
                    if (child.Name == "target") project.InsertBefore(child, project.ChildNodes[0]);
                }
                //2. Online-Targets nach oben sortieren
                for (int i = 0; i < project.ChildNodes.Count; i++) 
                {
                    XmlNode child = project.ChildNodes[i];
                    if(child.Name == "target") 
                    {
                        if(!child.InnerText.ToLower().EndsWith("pdf.fltar")) 
                            project.InsertBefore(child, project.ChildNodes[0]);
                    }
                }
            }
        }

        public static void BeautifyXml(string filename1, string filename2)
        {
            XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.XmlResolver = null;
            xmlDoc.PreserveWhitespace = false;
            xmlDoc.Load(filename1);
            string txt = IndentXml(xmlDoc, "<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            //Jetzt noch Wohlgeformtheit testen:
            XmlDocument doc = new XmlDocument();
            doc.PreserveWhitespace = true;
            try
            {
                doc.LoadXml(txt);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message + "\nFile will not be saved.", filename2);
                return;
            }
            TextDatei file = new TextDatei();

            file.WriteFile(filename2, txt);

        }

        public static void createXmlListFile(XmlDocument list, string listFile, string rootElemName)
        {
            XmlDeclaration xmlDeclaration = list.CreateXmlDeclaration("1.0", "UTF-8", null);
            XmlElement root = list.DocumentElement;
            list.InsertBefore(xmlDeclaration, root);
            XmlElement element1 = list.CreateElement(string.Empty, rootElemName, string.Empty);
            list.AppendChild(element1);
            if (!Directory.Exists(Path.GetDirectoryName(listFile))) tools1.createPath(Path.GetDirectoryName(listFile));
            list.Save(listFile);
        }

        private static string IndentXml(XmlDocument doc,string prolog)
		{
			StringBuilder sb = new StringBuilder();
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Encoding = Encoding.UTF8,
				OmitXmlDeclaration = true,
				Indent = true,
				IndentChars = "  ",
				NewLineChars = "\r\n",
				NewLineHandling = NewLineHandling.Replace,
				NewLineOnAttributes = true
			};
			using(XmlWriter writer = XmlWriter.Create(sb,settings))
			{
				doc.Save(writer);
			}

			return prolog + "\r\n" + sb.ToString();
			/*
            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", "utf-8", null);

            XmlElement root = doc.DocumentElement;
            doc.InsertBefore(xmldecl, root);
            */
		}

		/// <summary>
		/// Methode, die einen Binärvergleich von 2 Dateien macht und
		/// das Vergleichsergebnis zurückliefert.
		/// </summary>
		/// <param name="p_FileA">Voll qualifizierte Pfadangabe zur ersten Datei.</param>
		/// <param name="p_FileB">Voll qualifizierte Pfadangabe zur zweiten Datei.</param>
		/// <returns>True, wenn die Dateien binär gleich sind, andernfalls False.</returns>
		public static bool FilesEqual(string p_FileA, string p_FileB)
        {
            bool retVal = true;
            FileInfo infoA = null;
            FileInfo infoB = null;
            byte[] bufferA = new byte[128];
            byte[] bufferB = new byte[128];
            int bufferRead = 0;

            // Die Dateien überprüfen
            if (!File.Exists(p_FileA))
            {
                throw new ArgumentException(String.Format("Die Datei '{0}' konnte nicht gefunden werden", p_FileA), "p_FileA");
            }
            if (!File.Exists(p_FileB))
            {
                throw new ArgumentException(String.Format("Die Datei '{0}' konnte nicht gefunden werden", p_FileB), "p_FileB");
            }

            // Dateiinfo wegen der Dateigröße erzeugen
            infoA = new FileInfo(p_FileA);
            infoB = new FileInfo(p_FileB);

            // Wenn die Dateigröße gleich ist, dann einen Vergleich anstossen
            if (infoA.Length == infoB.Length)
            {
                // Binärvergleich
                using (BinaryReader readerA = new BinaryReader(File.OpenRead(p_FileA)))
                {
                    using (BinaryReader readerB = new BinaryReader(File.OpenRead(p_FileB)))
                    {
                        // Dateistream blockweise über Puffer einlesen
                        while ((bufferRead = readerA.Read(bufferA, 0, bufferA.Length)) > 0)
                        {
                            // Dateigrößen sind gleich, deshalb kann hier
                            // ungeprüft auch von der 2. Datei eingelesen werden
                            readerB.Read(bufferB, 0, bufferB.Length);

                            // Bytevergleich innerhalb des Puffers
                            for (int i = 0; i < Math.Min(bufferA.Length, bufferRead); i++)
                            {
                                if (bufferA[i] != bufferB[i])
                                {
                                    retVal = false;
                                    break;
                                }
                            }

                            // Wenn Vergleich bereits fehlgeschlagen, dann hier schon abbruch
                            if (!retVal)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                // Die Dateigröße ist schon unterschiedlich
                retVal = false;
            }

            return retVal;
        }

		public static bool nodeIsIncluded(XmlNode node)
		{
			while(node.ParentNode.Name != "html")
			{
				//XmlNode temp = nav;
				//int n = 0;
				//while (temp.ParentNode.ChildNodes[n] != temp) n++;
				//mypath = mypath + "-" + n.ToString();
				if(node.Attributes["MadCap:conditions"] == null)
				{
					node = node.ParentNode;
				}
				else
				{
					//if (node.Attributes["MadCap:conditions"].Value.Contains("ScreenOnly")) return false;
					if(node.Attributes["MadCap:conditions"].Value.Contains("TLSOnly")) return false;
					if(node.Attributes["MadCap:conditions"].Value.Contains("OutOfDate")) return false;
					if(node.Attributes["MadCap:conditions"].Value.Contains("Under_Construction")) return false;
					if(node.Attributes["MadCap:conditions"].Value.Contains("NotYetTranslated")) return false;
					if(node.Attributes["MadCap:conditions"].Value.Contains("javascript")) return false;
					node = node.ParentNode;
				}
			}
			return true;
		}

		// Methode, die durch die Knoten navigiert und xpath-Ausdrücke ausgibt
		public static void CollectXPathExpressions(XPathNavigator navi, List<string> hits, string prefix, string rootElement, bool unique)
        {
            //Achtung! die Funktion wird nicht nur für die Untersuchung von CatapultTargets verwendet!
            if ((rootElement == "CatapultTarget") && ((navi.Name == "Variables") /*|| (navi.Name == "Destinations")*/))
            {
                //MessageBox.Show("navi.Name");
            }
            else
            {
                if (navi.HasAttributes)
                {
                    string element = navi.Name;
                    if (prefix != "") element = prefix + "/" + element;
                    navi.MoveToFirstAttribute();
                    do
                    {
                        string expression = element + "/@" + navi.Name;
                        if (unique)
                        {
                            if (!hits.Contains(expression)) hits.Add(expression);
                        }
                        else
                        {
                            hits.Add(expression);
                        }
                        //Console.WriteLine(element + "/@" + navi.Name); // + "=" + navi.Value);
                    } while (navi.MoveToNextAttribute());
                    navi.MoveToParent();
                }
                // verschiebt den Cursor auf den ersten untergeordneten Knoten 
                if (prefix != "") prefix = prefix + "/" + navi.Name; else prefix = navi.Name;
                if (navi.MoveToFirstChild())
                {
                    // verschiebt den Cursor zum nächsten nebengeordneten Knoten
                    do
                    {
                        if (navi.NodeType == XPathNodeType.Element)
                        {
                            CollectXPathExpressions(navi, hits, prefix, rootElement, unique);
                        }
                    } while (navi.MoveToNext());
                    navi.MoveToParent();
                }
            }
        }
        
        //public static void ToClipboard(string text)
        //{
        //    System.Windows.Forms.Clipboard.SetDataObject(text, true);
        //}

        //public static void checkProjectLanguageSettings(string projectPath)
        //{
        //    string langcode = projectPath;
        //    langcode = langcode.Substring(0,langcode.LastIndexOf('\\'));
        //    langcode = langcode.Substring(langcode.LastIndexOf('\\') + 1);
        //    {
        //        string temp = "";
        //        switch (langcode)
        //        {
        //            case "00": temp = "de"; break;
        //            case "01": temp = "en-us"; break;
        //            case "02": temp = "fr-fr"; break;
        //            case "03": temp = "it"; break;
        //            case "11": temp = "en-gb"; break;
        //            case "17": temp = "zh"; break;
        //        }
        //        langcode = temp;
        //    }

        //    if (!Directory.Exists(projectPath + @"\Content"))
        //    {
        //        MessageBox.Show("No Content folder found.", projectPath);
        //        return;
        //    }
        //    else
        //    {
        //        List<string> checkedfiles = new List<string>();
        //        List<string> changedfiles = new List<string>();
        //        foreach (string file in Directory.GetFiles(projectPath))
        //        {
        //            if (file.EndsWith(".flprj"))
        //            {
        //                XmlDocument xml = new XmlDocument();
        //                xml.PreserveWhitespace = true;
        //                xml.Load(file);
        //                XmlAttribute language = xml.DocumentElement.Attributes["xml:lang"];
        //                if (language == null)
        //                {
        //                    XmlAttribute newLanguage = xml.CreateAttribute("xml:lang");
        //                    newLanguage.Value = langcode;
        //                    xml.DocumentElement.Attributes.Append(newLanguage);
        //                    xml.Save(file);
        //                    tools1.BeautifyXml(file, file);
        //                    changedfiles.Add(file);
        //                    //MessageBox.Show("No langauge defined for " + file.Substring(file.LastIndexOf('\\')+1));
        //                }
        //                else
        //                {
        //                    if (language.Value != langcode)
        //                    {
        //                        language.Value = langcode;
        //                        xml.Save(file);
        //                        tools1.BeautifyXml(file, file);
        //                        changedfiles.Add(file);
        //                    }
        //                    //MessageBox.Show("Langauge for " + file.Substring(file.LastIndexOf('\\') + 1) + " is " + language.Value);
        //                }
        //                checkedfiles.Add(file);
        //            }
        //        }
        //        foreach (string file in Directory.GetFiles(projectPath + @"\Content"))
        //        {
        //            if (file.EndsWith(".htm"))
        //            {
        //                XmlDocument xml = new XmlDocument();
        //                xml.PreserveWhitespace = true;
        //                xml.Load(file);
        //                XmlAttribute language = xml.DocumentElement.Attributes["xml:lang"];
        //                if(language == null)
        //                {
        //                    XmlAttribute newLanguage = xml.CreateAttribute("xml:lang");
        //                    newLanguage.Value = langcode;
        //                    xml.DocumentElement.Attributes.Append(newLanguage);
        //                    xml.Save(file);
        //                    changedfiles.Add(file);
        //                    //MessageBox.Show("No langauge defined for " + file.Substring(file.LastIndexOf('\\') + 1));
        //                }
        //                else
        //                {
        //                    if (language.Value != langcode)
        //                    {
        //                        language.Value = langcode;
        //                        xml.Save(file);
        //                        changedfiles.Add(file);
        //                    }                            
        //                    //MessageBox.Show("Langauge for " + file.Substring(file.LastIndexOf('\\') + 1) + " is " + language.Value);
        //                }
        //                checkedfiles.Add(file);
        //            }
        //        }
        //        foreach (string file in Directory.GetFiles(projectPath + @"\Project\targets"))
        //        {
        //            if (file.EndsWith(".fltar"))
        //            {
        //                XmlDocument xml = new XmlDocument();
        //                xml.PreserveWhitespace = true;
        //                xml.Load(file);
        //                XmlAttribute language = xml.DocumentElement.Attributes["TargetLanguage"];
        //                if (language == null)
        //                {
        //                    XmlAttribute newLanguage = xml.CreateAttribute("TargetLanguage");
        //                    newLanguage.Value = langcode;
        //                    xml.DocumentElement.Attributes.Append(newLanguage);
        //                    xml.Save(file);
        //                    tools1.BeautifyXml(file, file);
        //                    changedfiles.Add(file);
        //                    //MessageBox.Show("No langauge defined for " + file.Substring(file.LastIndexOf('\\') + 1));
        //                }
        //                else
        //                {
        //                    if (language.Value != langcode)
        //                    {
        //                        language.Value = langcode;
        //                        xml.Save(file);
        //                        tools1.BeautifyXml(file, file);
        //                        changedfiles.Add(file);
        //                    }
        //                    //MessageBox.Show("Langauge for " + file.Substring(file.LastIndexOf('\\') + 1) + " is " + language.Value);
        //                }
        //                checkedfiles.Add(file);
        //            }
        //        }
        //        MessageBox.Show("Checked " + checkedfiles.Count.ToString() + " Files.\nChanged " + changedfiles.Count.ToString() + " Files.");
        //    }            
        //}

    /// <summary>
    /// stellt eine 16-sprachige Wortliste zur Verfügung
    /// </summary>
    public class wordList
    {
        public Dictionary<string, int> WlDe = new Dictionary<string, int>();
        public Dictionary<string, int> WlEnUS = new Dictionary<string, int>();
        public Dictionary<string, int> WlFr = new Dictionary<string, int>();
        public Dictionary<string, int> WlIt = new Dictionary<string, int>();
        public Dictionary<string, int> WlPl = new Dictionary<string, int>();
        public Dictionary<string, int> WlSv = new Dictionary<string, int>();
        public Dictionary<string, int> WlHu = new Dictionary<string, int>();
        public Dictionary<string, int> WlCs = new Dictionary<string, int>();
        public Dictionary<string, int> WlNl = new Dictionary<string, int>();
        public Dictionary<string, int> WlEs = new Dictionary<string, int>();
        public Dictionary<string, int> WlPtBR = new Dictionary<string, int>();
        public Dictionary<string, int> WlEnGB = new Dictionary<string, int>();
        public Dictionary<string, int> WlDa = new Dictionary<string, int>();
        public Dictionary<string, int> WlFi = new Dictionary<string, int>();
        public Dictionary<string, int> WlKo = new Dictionary<string, int>();
        public Dictionary<string, int> WlNo = new Dictionary<string, int>();
        public Dictionary<string, int> WlRu = new Dictionary<string, int>();
        public Dictionary<string, int> WlZhHans = new Dictionary<string, int>();
        public Dictionary<string, int> WlJaJP = new Dictionary<string, int>();

        /// <summary>
        /// Gibt 1 zurück, wenn [term] in der Wortliste [lang] bereits vorhanden ist, ansonsten -1
        /// </summary>
        /// <param name="term"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public int Check(string term, string lang)
        {
            switch (lang)
            {
                case "de": if (WlDe[term] > 1) return 1; break;
                case "en-US": if (WlEnUS[term] > 1) return 1; break;
                case "fr": if (WlFr[term] > 1) return 1; break;
                case "it": if (WlIt[term] > 1) return 1; break;
                case "pl": if (WlPl[term] > 1) return 1; break;
                case "sv": if (WlSv[term] > 1) return 1; break;
                case "hu": if (WlHu[term] > 1) return 1; break;
                case "cs": if (WlCs[term] > 1) return 1; break;
                case "nl": if (WlNl[term] > 1) return 1; break;
                case "es": if (WlEs[term] > 1) return 1; break;
                case "pt-BR": if (WlPtBR[term] > 1) return 1; break;
                case "en-GB": if (WlEnGB[term] > 1) return 1; break;
                case "da": if (WlDa[term] > 1) return 1; break;
                case "fi": if (WlFi[term] > 1) return 1; break;
                case "ko": if (WlKo[term] > 1) return 1; break;
                case "no": if (WlNo[term] > 1) return 1; break;
                case "ru": if (WlRu[term] > 1) return 1; break;
                case "zh-Hans": if (WlZhHans[term] > 1) return 1; break;
                case "ja-JP": if (WlJaJP[term] > 1) return 1; break;
            }
            return -1;
        }

        /// <summary>
        /// Prüft, ob [term] in der Wortliste [lang] bereits vorhanden ist, 
        /// wenn nicht, wird [term] hinzugefügt und der Wert um 1 erhöht.
        /// </summary>
        /// <param name="term"></param>
        /// <param name="lang"></param>
        public void Count(string term, string lang)
        {
            switch (lang)
            {
                case "de": if (!WlDe.ContainsKey(term)) WlDe.Add(term, 1); else WlDe[term]++; break;
                case "en-US": if (!WlEnUS.ContainsKey(term)) WlEnUS.Add(term, 1); else WlEnUS[term]++; break;
                case "fr": if (!WlFr.ContainsKey(term)) WlFr.Add(term, 1); else WlFr[term]++; break;
                case "it": if (!WlIt.ContainsKey(term)) WlIt.Add(term, 1); else WlIt[term]++; break;
                case "pl": if (!WlPl.ContainsKey(term)) WlPl.Add(term, 1); else WlPl[term]++; break;
                case "sv": if (!WlSv.ContainsKey(term)) WlSv.Add(term, 1); else WlSv[term]++; break;
                case "hu": if (!WlHu.ContainsKey(term)) WlHu.Add(term, 1); else WlHu[term]++; break;
                case "cs": if (!WlCs.ContainsKey(term)) WlCs.Add(term, 1); else WlCs[term]++; break;
                case "nl": if (!WlNl.ContainsKey(term)) WlNl.Add(term, 1); else WlNl[term]++; break;
                case "es": if (!WlEs.ContainsKey(term)) WlEs.Add(term, 1); else WlEs[term]++; break;
                case "pt-BR": if (!WlPtBR.ContainsKey(term)) WlPtBR.Add(term, 1); else WlPtBR[term]++; break;
                case "en-GB": if (!WlEnGB.ContainsKey(term)) WlEnGB.Add(term, 1); else WlEnGB[term]++; break;
                case "da": if (!WlDa.ContainsKey(term)) WlDa.Add(term, 1); else WlDa[term]++; break;
                case "fi": if (!WlFi.ContainsKey(term)) WlFi.Add(term, 1); else WlFi[term]++; break;
                case "ko": if (!WlKo.ContainsKey(term)) WlKo.Add(term, 1); else WlKo[term]++; break;
                case "no": if (!WlNo.ContainsKey(term)) WlNo.Add(term, 1); else WlNo[term]++; break;
                case "ru": if (!WlRu.ContainsKey(term)) WlRu.Add(term, 1); else WlRu[term]++; break;
                case "zh-Hans": if (!WlZhHans.ContainsKey(term)) WlZhHans.Add(term, 1); else WlZhHans[term]++; break;
                case "ja-JP": if (!WlJaJP.ContainsKey(term)) WlJaJP.Add(term, 1); else WlJaJP[term]++; break;
            }
        }

        public void Clear()
        {
            WlDe.Clear();
            WlEnUS.Clear();
            WlFr.Clear();
            WlIt.Clear();
            WlPl.Clear();
            WlSv.Clear();
            WlHu.Clear();
            WlCs.Clear();
            WlNl.Clear();
            WlEs.Clear();
            WlPtBR.Clear();
            WlEnGB.Clear();
            WlDa.Clear();
            WlFi.Clear();
            WlKo.Clear();
            WlNo.Clear();
            WlRu.Clear();
            WlZhHans.Clear();
            WlJaJP.Clear();
        }

        //public string image2Base64(string Path)
        //{
        //    using (Image image = Image.FromFile(Path))
        //    {
        //        using (MemoryStream m = new MemoryStream())
        //        {
        //            image.Save(m, image.RawFormat);
        //            byte[] imageBytes = m.ToArray();

        //            // Convert byte[] to Base64 String
        //            string base64String = Convert.ToBase64String(imageBytes);
        //            return base64String;
        //        }
        //    }
        //}

    }

    public class SearchClass 
    {
        public string SearchTerm;
        public bool MatchCase;
        public bool WholeWord;
        public bool SearchFilter;
    }

    /// <summary>
    /// Wandelt ein Bild in einen Base64-String und zurück
    /// Quelle: https://dotnet-snippets.de/snippet/image-zu-base64-konvertieren-und-zurueck/958
    /// TODO: Bild aus File Laden: Image image = Image.FromFile(Path)
    /// TODO: Base64FormattingOptions.InsertLineBreaks
    /// </summary>
    //public class ImageToString
    //{
    //    /// <summary>
    //    /// Konvertiert ein Bild in einen Base64-String
    //    /// </summary>
    //    /// <param name="image">+
    //    /// 
    //    /// Zu konvertierendes Bild
    //    /// </param>
    //    /// <returns>
    //    /// Base64 Repräsentation des Bildes
    //    /// </returns>
    //    public static string GetStringFromImage(Image image)
    //    {
    //        if (image != null)
    //        {
    //            ImageConverter ic = new ImageConverter();
    //            byte[] buffer = (byte[])ic.ConvertTo(image, typeof(byte[]));
    //            return Convert.ToBase64String(
    //                buffer,
    //                Base64FormattingOptions.InsertLineBreaks);
    //        }
    //        else
    //            return null;
    //    }
        ////---------------------------------------------------------------------
        ///// <summary>
        ///// Konvertiert einen Base64-String zu einem Bild
        ///// </summary>
        ///// <param name="base64String">
        ///// Zu konvertierender String
        ///// </param>
        ///// <returns>
        ///// Bild das aus dem String erzeugt wird
        ///// </returns>
        //public static Image GetImageFromString(string base64String)
        //{
        //    byte[] buffer = Convert.FromBase64String(base64String);

        //    if (buffer != null)
        //    {
        //        ImageConverter ic = new ImageConverter();
        //        return ic.ConvertFrom(buffer) as Image;
        //    }
        //    else
        //        return null;
        //}
    }
}
