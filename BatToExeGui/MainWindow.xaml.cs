using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Ookii.Dialogs.Wpf;
using System.IO;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace BatToExeGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void sourceFolderBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            var browser = new VistaFolderBrowserDialog();
            if ( browser.ShowDialog() == true)
            {
                sourceTextBox.Text = browser.SelectedPath;
            }
        }

        private void destinationFolderBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            var browser = new VistaFolderBrowserDialog();
            if (browser.ShowDialog() == true)
            {
                DestinationFolderTextBox.Text = browser.SelectedPath;
            }
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(sourceTextBox.Text) && Directory.Exists(DestinationFolderTextBox.Text) && File.Exists(RocketLauncherLocationTextBox.Text))
            {
                var sourceFolder = sourceTextBox.Text;
                var destinationFolder = DestinationFolderTextBox.Text;
                var rocketLauncherLocation = RocketLauncherLocationTextBox.Text;
                var systemName = systemNameTextBox.Text;

                var gamesList = Directory.GetFiles(sourceFolder);

                foreach (var game in gamesList)
                {
                    var gameName = game.Remove(0, sourceFolder.Length);
                    gameName = gameName.Remove(gameName.Length - 4);
                    if (gameName[0] == '\\')
                    {
                        gameName = gameName.Remove(0, 1);
                    }
                    
                    var arguments = string.Format("\"{0}\" -s {1} -r \"{2}\" -f RocketLauncherUI -p RocketLauncherUI", rocketLauncherLocation, systemName, gameName);

                    var finalFile = System.IO.Path.Combine(destinationFolder, gameName) + ".bat";
                    StreamWriter streamWriter = new StreamWriter(finalFile);
                    streamWriter.WriteLine(arguments);
                    streamWriter.Close();
                }

                var batList = Directory.GetFiles(destinationFolder, "*.bat");

                foreach (var bat in batList)
                {
                    var gameName = bat.Remove(0, destinationFolder.Length);
                    gameName = gameName.Remove(gameName.Length - 4) + ".exe";
                    var finalLocation = string.Format("{0}{1}", destinationFolder, gameName);
                    CompileBatToExe(bat, finalLocation);
                }
                MessageBox.Show("Finished Compilation");
            }
        }

        public static void CompileBatToExe(string batchFile, string outputExe)
        {
            CSharpCodeProvider compiler = new CSharpCodeProvider();
            CompilerParameters comParms = new CompilerParameters();

            comParms.GenerateExecutable = true;
            comParms.GenerateInMemory = false;
            comParms.IncludeDebugInformation = false;

            comParms.MainClass = "GenericConsole.Program";
            comParms.CompilerOptions = "/optimize";
            comParms.OutputAssembly = outputExe;
            comParms.TreatWarningsAsErrors = false;

            comParms.ReferencedAssemblies.AddRange(new string[] { "mscorlib.dll", "System.dll", "System.Data.dll", "System.Xml.dll" });

            //Update template source code to reflect batch file contents
            string source = CreateSourceCode(batchFile);
            //Perform actual compilation
            CompilerResults comRes = compiler.CompileAssemblyFromSource(comParms, source);
        }

        private static string CreateSourceCode(string batchFile)
        {
            StringBuilder sourceCode = new StringBuilder(GetGenericSource());
            string batchContent = File.ReadAllText(batchFile);
            //Command Line instructions often contain double quotes -> " <- which prematurely terminates string assignment
            //resulting in compilation errors. As a work around, replace all double quotes with $&$ before compiling. 
            //Before executing command line script apply the same logic in reverse, replace all instances of $&$ with "
            batchContent = batchContent.Replace("\"", "$&$");

            sourceCode.Replace("batchFileContents = \"\";", "batchFileContents = " + "@" + "\"" + batchContent + "\";");
            sourceCode.Replace("redirectStandardOutput = true", "redirectStandardOutput = false");


            return sourceCode.ToString();
        }

        /// <summary>
        /// Retrieving source code template, pay special attention!
        /// </summary>
        /// <returns></returns>
        private static string GetGenericSource()
        {
            //The source code template used in run-time compilation forms part of this Visual Studio Solution: GenericConsole.cs
            //GenericConsole.cs has been added as an embedded resource to this project, with Persistence configured as:
            // "Linked at compile time" -- The result being any changes made to the template file aka GenericConsole.cs will 
            //always update the embedded resource

            return BatToExeGui.Properties.Resources.GenericConsole;
        }

        private void rocketLauncherExeBrowserButton_Click(object sender, RoutedEventArgs e)
        {
            var browser = new VistaOpenFileDialog();
            if (browser.ShowDialog() == true)
            {
                RocketLauncherLocationTextBox.Text = browser.FileName;
            }

        }
    }
}
