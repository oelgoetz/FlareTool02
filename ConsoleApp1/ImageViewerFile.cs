using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using tools;

namespace ConsoleApp1
{
    class ImageViewerFile
    {
        XmlNode _body;
        public ImageViewerFile(string filename)         
        {
            

            XmlDocument d = tools1.createHtmlFile(filename);
            _body = d.SelectSingleNode("//body");
        }
    }
}
