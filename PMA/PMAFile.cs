using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml;

namespace PMA
{
    public class PMAFile
    {
        public static XmlDocument readFile(string path)
        {
            convertToXML(path);
            XmlDocument result = readXML(path);
            deleteXML(path);
            return result;
        }

        public static void writeFile(string path, XmlDocument doc)
        {
            writeXML(path, doc);
            convertToTXT(path);
            deleteXML(path);
        }

        public static void convertToTXT(string path)
        {
            List<String> lines = new List<String>();

            var file = File.ReadLines(path + ".xml");
            foreach (var line in file)
            {
                string str = line;
                if (str.Equals("<PMA>") || str.Equals("</PMA>")) {
                    continue;
                }

                if (str.StartsWith("\t"))
                {
                    str = str.Substring(1);
                }

                str = str.Replace("_amp_", "&");
                str = str.Replace("_semicollon_", ";");
                str = str.Replace("_sharp_", "#");
                str = str.Replace("_ul_", "&underline;");
                str = str.Replace("_dot_", "&dot;");

                if (str.Contains("</"))
                {
                    str = str.Remove(str.IndexOf("</"));
                }

                if (str.EndsWith(">"))
                {
                    str = str.Substring(0, str.Length - 1);
                } else
                {
                    str = str.Replace(">", " = ");
                }
                
                str = str.Replace("<", "");

                if (str.Contains('\t'))
                {
                    if (str.Substring(str.LastIndexOf('\t') + 1).StartsWith("_"))
                    {
                        str = str.Remove(str.LastIndexOf('\t') + 1, 1);
                    }
                } else if(str.StartsWith("_"))
                {
                    str = str.Substring(1);
                }


                str = str.Replace("_", " ");

                str = str.Replace("&underline;", "_");
                str = str.Replace("&dot;", ".");
                str = WebUtility.HtmlDecode(str);

                if (str.Replace("\t", "").Length > 0)
                {
                    lines.Add(str);
                }

                
            }

            File.WriteAllLines(path, lines, Encoding.Default);
        }

        public static void convertToXML(string path)
        {
            List<String> lines = new List<String>();

            lines.Add("<PMA>");

            Stack<string> tags = new Stack<string>();

            int lastTabCount = 0;
            int expectedNextTabCount = 0;
            bool hasChildren = false;

            var file = File.ReadLines(path,Encoding.Default);
            foreach (var line in file)
            {
                // Process line
                string str = line;



                // Remove invalid characters
                str = str.Replace("<", "&lt;");
                str = str.Replace(">", "&gt;");
                str = str.Replace("_", "_ul_");

                // Get tab count and remove tabs
                int tabCount = str.Split('\t').Length - 1;
                str = str.Replace("\t", "");

                if (tabCount < expectedNextTabCount && !hasChildren)
                {
                    lastTabCount++;
                    expectedNextTabCount++;
                }

                // Must close the previous tags
                while (lastTabCount > tabCount)
                {
                    string closeStr = "</" + tags.Pop() + ">";
                    for (int t = 0; t < lastTabCount - 1; t++)
                    {
                        closeStr = '\t' + closeStr;
                    }

                    lines.Add(closeStr);
                    lastTabCount--;
                    expectedNextTabCount--;
                }

                // Check if it's a tag value pair
                if (str.Contains("="))
                {
                    var obj = str.Split('=');
                    string tag = obj[0];
                    string value = obj[1].Trim();

                    value = WebUtility.HtmlEncode(value);

                    // Text transformations
                    tag = tabbedTagToXML(tag);

                    str = "<" + tag + ">" + value + "</" + tag + ">";

                    hasChildren = true;
                }
                else
                {
                    // Add a new tag to the stack
                    string tag = tabbedTagToXML(str);

                    if (int.TryParse(tag.Substring(0, 1), out int result) || tag.Substring(0, 1) == "-")
                    {
                        tag = "_" + tag;
                    }

                    tags.Push(tag);
                    str = "<" + tag + ">";

                    hasChildren = false;
                    expectedNextTabCount++;
                }

                // Update the last tab count
                lastTabCount = tabCount;

                // Add the tabs back to the line
                for (int t = 0; t < tabCount; t++)
                {
                    str = '\t' + str;
                }

                // Push the line
                lines.Add(str);
            }

            while (tags.Count() > 0)
            {
                string str = "</" + tags.Pop() + ">";
                for (int t = 0; t < tags.Count(); t++)
                {
                    str = '\t' + str;
                }


                lines.Add(str);
                lastTabCount--;
            }

            lines.Add("</PMA>");

            File.WriteAllLines(path + ".xml", lines);
        }

        private static string tabbedTagToXML(string tag)
        {
            tag = tag.Trim();
            tag = tag.Replace(" ", "_");
            tag = WebUtility.HtmlEncode(tag);
            tag = tag.Replace("&", "_amp_");
            tag = tag.Replace("#", "_sharp_");
            tag = tag.Replace(";", "_semicollon_");

            tag = tag.Replace(".", "_dot_");
            //tag = tag.Replace("#", "_sharp_");
            //tag = tag.Replace("&", "_amp_");
            //tag = tag.Replace("'", "_quote_");
            //tag = tag.Replace("/", "_slash_");
            //tag = RemoveSpecialCharacters(tag);
            return tag;
        }

        private static string RemoveSpecialCharacters(string str)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                if ((str[i] >= '0' && str[i] <= '9')
                    || (str[i] >= 'A' && str[i] <= 'z'
                        || str[i] == '_' || str[i] == ' ' || str[i] == '-'))
                {
                    sb.Append(str[i]);
                }
            }

            return sb.ToString();
        }

        public static XmlDocument readXML(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path + ".xml");

            return doc;
        }

        public static void writeXML(string path, XmlDocument doc)
        {
            //doc.Save(path + ".xml");

            // Create an XmlWriterSettings object with the correct options. 
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.IndentChars = ("\t");
            settings.OmitXmlDeclaration = true;

            // Create the XmlWriter object and write some content.
            XmlWriter writer = XmlWriter.Create(path + ".xml", settings);
            doc.Save(writer);

            writer.Flush();
            writer.Close();
        }

        public static void deleteXML(string path)
        {
            File.Delete(path + ".xml");
        }
    }
}
