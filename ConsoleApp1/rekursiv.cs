// -----------------------------------------------------
// Beispiel: ...\Kapitel 16\XmlNodeNavigation
// -----------------------------------------------------
static void Main(string[] args) {
  XmlDocument doc = new XmlDocument();
  doc.Load(@"..\..\Personen.xml");
  XmlNode root = doc.DocumentElement;
  GetNodes(root, 0);
  Console.ReadLine();
}
static void GetNodes(XmlNode node, int level) {
  switch (node.NodeType) {
    // prüfen, ob es sich um ein Element handelt
    case XmlNodeType.Element:
      Console.Write(new string(' ', level * 2));
      Console.Write("<{0}", node.Name); 
      // prüfen, ob das aktuelle Element Attribute hat
      if (node.Attributes != null) {
        foreach (XmlAttribute attr in node.Attributes)
          Console.Write(" {0}='{1}'", attr.Name, attr.Value);
      }
      Console.Write(">");
      // prüfen, ob das aktuelle Element untergeordnete
      // Elemente hat
      if (node.HasChildNodes)
        foreach (XmlNode child in node.ChildNodes) {
          if (child.NodeType != XmlNodeType.Text)
            Console.WriteLine();
          GetNodes(child, level + 1);
        }
      break; 
    // prüfen, ob es sich um auswertbare Daten handelt
    case XmlNodeType.Text:
      Console.Write(node.Value);
      break;         
  }
}