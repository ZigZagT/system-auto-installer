using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace SystemAutoInstall
{
    public class ShellCommand
    {
        public String command
        {
            get;
            set;
        }
        private void init(String c)
        {
            command = c;
            data = "";
            p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "";
            p.StartInfo.UseShellExecute = false;
            //p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            //p.StartInfo.CreateNoWindow = true;

            p.OutputDataReceived += (sender, line) =>
            {
                lock (data)
                {
                    data += line.Data + Environment.NewLine;
                }
            };
        }
        public ShellCommand()
        {
            init("echo shell command ready.");
        }
        public ShellCommand(String c)
        {
            init(c);
        }
        public void Run()
        {
            p.Start();
            p.EnableRaisingEvents = true;
            //p.StandardInput.WriteLine("@echo off");
            //p.StandardInput.WriteLine(command);
            //p.StandardInput.WriteLine("start /wait msiexec");
            //p.StandardInput.WriteLine("exit");
            //p.StandardInput.AutoFlush = true;
            string data = p.StandardOutput.ReadToEnd();
            //p.BeginOutputReadLine();
            p.WaitForExit();
            p.Close();
            this.complete(data);
        }
        public static String[] Run(String Command)
        {
            String stdin = Command;
            var output = new String[2];
            output[0] = "";
            output[1] = "";

            var p = new System.Diagnostics.Process();
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.Arguments = "";
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.CreateNoWindow = true;

            p.OutputDataReceived += (sender, e) =>
            {
                output[0] += e.Data + Environment.NewLine;
            };
            p.ErrorDataReceived += (sender, e) =>
            {
                output[1] += e.Data + Environment.NewLine;
            };

            p.Start();
            p.StandardInput.WriteLine(Command + "&exit");
            p.StandardInput.AutoFlush = true;
            p.BeginOutputReadLine();
            p.BeginErrorReadLine();
            p.WaitForExit();
            p.Close();
            return output;
        }
        private System.Diagnostics.Process p;
        private volatile string data;
        public delegate void completeFunc(string data);
        public completeFunc complete;
    }
}
