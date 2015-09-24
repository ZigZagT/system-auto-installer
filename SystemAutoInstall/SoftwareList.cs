using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Xml;

namespace SystemAutoInstall
{
    public class SoftwareList
    {
        private XmlTextReader reader;
        private XmlDocument dom;
        public SoftwareItem[] list;

        public class SoftwareItem
        {
            public string Name;
            public string Description;
            public string Path;
            public string InstallArgs;
            public bool IsSelected;
            public List<String> AdditionalCmd;
            public SoftwareItem(string Name, string Description, string Path, string InstallArgs, bool IsSelected, List<String> AdditionalCmd)
            {
                this.Name = Name;
                this.Description = Description;
                this.Path = Path;
                this.InstallArgs = InstallArgs;
                this.IsSelected = IsSelected;
                this.AdditionalCmd = AdditionalCmd;
            }
        }
        public string RawData
        {
            get
            {
                var reader = new XmlTextReader("SoftwareList.xml");
                var str = "";
                while (reader.Read())
                {
                    string line;
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            line = "Start Element: \t" + reader.Name;
                            break;
                        case XmlNodeType.CDATA:
                            line = "CDATA: \t" + reader.Value;
                            break;
                        case XmlNodeType.EndElement:
                            line = "End Element: \t" + reader.Name;
                            break;
                        case XmlNodeType.Whitespace:
                            continue;
                        default:
                            line = reader.NodeType + ": \t" + reader.Name;
                            break;
                    }
                    str += line + Environment.NewLine;
                }
                return str;
            }
        }
        public string TypeData
        {
            get
            {
                var reader = new XmlTextReader("SoftwareList.xml");
                var str = "";
                while (reader.Read())
                {
                    if (reader.NodeType != XmlNodeType.Whitespace)
                    {
                        str += reader.NodeType + ": " + reader.Value + Environment.NewLine;
                    }
                }
                return str;
            }
        }
        public string Data
        {
            get
            {
                var reader = new XmlTextReader("SoftwareList.xml");
                var str = "";
                int count = 0;
                foreach (var item in list)
                {
                    str += "Software " + count + ":\n\tName: " + item.Name + "\n\tDescription: " + item.Description + "\n\tPath: " + item.Path + "\n\tInstallArgs: " + item.InstallArgs + "\n\n";
                    ++count;
                }
                return str;
            }
        }
        public SoftwareItem[] Content
        {
            get
            {
                return list;
            }
        }
        public SoftwareList() : this("SoftwareList.xml")
        {
            //reader = new XmlTextReader("SoftwareList.xml");
        }
        public SoftwareList(string XmlFilePath)
        {
            reader = new XmlTextReader(XmlFilePath);
            dom = new XmlDocument();
            dom.Load(reader);

            var softwares = dom.GetElementsByTagName("SoftwareList")[0].ChildNodes;
            this.list = new SoftwareItem[softwares.Count];

            int temp = 0; // 迭代计数
            foreach (XmlNode software in softwares)
            {
                this.list[temp] = SoftwareXml2Obj(software);
                temp++;
            }
        }
        public SoftwareItem SoftwareXml2Obj(XmlNode Software)
        {
            SoftwareItem item;
            String Name = "";
            String Description = "";
            String Path = "";
            String InstallArgs = "";
            List<String> AdditionalCmd = new List<String>();
            bool IsSelected = false;
            var properties = Software.ChildNodes;
            foreach (XmlNode info in properties)
            {
                //Name = "1234";
                switch (info.Name)
                {
                    case "Name":
                        Name = info.InnerText;
                        continue;
                    case "Description":
                        Description = info.InnerText;
                        continue;
                    case "Path":
                        Path = info.InnerText;
                        continue;
                    case "InstallArgs":
                        InstallArgs = info.InnerText;
                        continue;
                    case "IsSelected":
                        if (info.InnerText.Trim() == "True" || info.InnerText.Trim() == "true")
                            IsSelected = true;
                        continue;
                    case "AdditionalCmd":
                        AdditionalCmd.Add(info.InnerText);
                        continue;
                    default:
                        continue;
                }
            }
            item = new SoftwareItem(Name, Description, Path, InstallArgs, IsSelected, AdditionalCmd);
            return item;
        }
        public static void install(SoftwareItem software, Action Complete)
        {
            ThreadControl.Task(() =>
                {
                    var p = new System.Diagnostics.Process();
                    p.StartInfo.FileName = software.Path;
                    p.StartInfo.Arguments = software.InstallArgs;
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardInput = true;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.StartInfo.RedirectStandardError = true;
                    p.StartInfo.CreateNoWindow = false;
                    p.Start();
                    string data = p.StandardOutput.ReadToEnd();
                    //p.BeginOutputReadLine();
                    p.WaitForExit();
                    p.Close();

                    /*if (software.AdditionalCmd.Trim() != "")
                    {
                        p.StartInfo.FileName = "cmd.exe";
                        p.StartInfo.Arguments = "";
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                        p.StandardInput.WriteLine(software.AdditionalCmd);
                        p.StandardInput.WriteLine("&exit");
                        p.WaitForExit();
                        p.Close();
                    }*/
                    if (software.AdditionalCmd.Count > 0)
                    {
                        foreach (var command in software.AdditionalCmd)
                        {
                            if (command.Trim() != "")
                            {
                                ShellCommand.Run(command);
                            }
                        }
                    }
                }, Complete
                );
        }
    }
}
