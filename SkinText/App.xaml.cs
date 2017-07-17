using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace SkinText
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public string GAppPath
        {
            get
            {
                //TODO: create funtion library to avoid calling everywhere
                Assembly assm;
                Type at;
                Type at2;
                object[] r;
                object[] r2;
                // Get the .EXE assembly
                assm = Assembly.GetEntryAssembly();
                // Get a 'Type' of the AssemblyCompanyAttribute
                at = typeof(AssemblyCompanyAttribute);
                at2 = typeof(AssemblyTitleAttribute);
                // Get a collection of custom attributes from the .EXE assembly
                r = assm.GetCustomAttributes(at, false);
                r2 = assm.GetCustomAttributes(at2, false);
                // Get the Company Attribute
                AssemblyCompanyAttribute ct = ((AssemblyCompanyAttribute)(r[0]));
                AssemblyTitleAttribute ct2 = ((AssemblyTitleAttribute)(r2[0]));
                // Build the User App Data Path
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                //path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                path += @"\" + ct.Company;
                path += @"\" + ct2.Title;
                path += @"\Default";
                //path += @"\" + assm.GetName().Name.ToString();
                //path += @"\" + assm.GetName().Version.ToString();
                return path;
            }
        }
        private void FirstRun(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                string curFileNamePath = Process.GetCurrentProcess().MainModule.FileName;
                string curFileName = Path.GetFileName(curFileNamePath);
                File.Copy(curFileName, path + "\\" + curFileName);
                MessageBox.Show("Config complete!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("Error creating Initial Config", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {

            string path = GAppPath;

            // Application is running
            // Process command line args
            bool firstRun = false;
            bool Update = false;
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "-FirstRun")
                {
                    firstRun = true;
                }
                if (e.Args[i] == "-Update")
                {
                    Update = true;
                }
            }
            // Create main application window, starting minimized if specified
            if (firstRun)
            {
                MainWindow mainWindow = new MainWindow();
                FirstRun(path);
                mainWindow.Close();
            }
            if (Update)
            {

            }
            
            string curFileNamePath = Process.GetCurrentProcess().MainModule.FileName;
            string curFileName = Path.GetFileName(curFileNamePath);
            if (!Directory.Exists(path))
            {
                MessageBox.Show("Welcome to the SkinText Family! \r\nFor the first run we need to do a quick config", "Welcome!", MessageBoxButton.OK, MessageBoxImage.Information);
                try
                {
                    Directory.CreateDirectory(path);
                    File.Copy(curFileName, path + "\\" + curFileName);
                    MessageBox.Show("Config complete!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception)
                {
                    try
                    {
                        //Request admin rights
                        using (Process process = new Process())
                        {
                            process.StartInfo.FileName = curFileName;
                            process.StartInfo.Arguments = "-FirstRun";
                            process.StartInfo.Verb = "runas";
                            process.Start();
                            process.WaitForExit();
                            //process.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Could Not complete config", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            if (!File.Exists(path + "\\" + curFileName))
            {
                try
                {
                    File.Copy(curFileName, path + "\\" + curFileName, true);
                    MessageBox.Show("Detected a missing file and created it", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception)
                {
                    try
                    {
                        MessageBox.Show("Detected a missing file and need to create it", "!!", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                        //Request admin rights
                        using (Process process = new Process()) {
                            process.StartInfo.FileName = curFileName;
                            process.StartInfo.Arguments = "-FirstRun";
                            process.StartInfo.Verb = "runas";
                            process.Start();
                            process.WaitForExit();
                            //process.Dispose();
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Detected a missing file and could not fix it, file associations will not work", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                try
                {
                    //check version for update
                    FileVersionInfo oldFileVersionInfo = FileVersionInfo.GetVersionInfo(path + "\\" + curFileName);
                    FileVersionInfo currentFileVersionInfo = FileVersionInfo.GetVersionInfo(curFileNamePath);
                    //oldFileVersionInfo.FileMajorPart 0
                    //oldFileVersionInfo.FileMinorPart 1
                    //oldFileVersionInfo.FileBuildPart 20
                    //oldFileVersionInfo.FilePrivatePart 0
                    if (currentFileVersionInfo.FileMajorPart> oldFileVersionInfo.FileMajorPart)
                    {
                        //MessageBox.Show("Thanks for updating!");
                        File.Copy(curFileName, path + "\\" + curFileName, true);
                    }
                    else
                    {
                        if (currentFileVersionInfo.FileMajorPart == oldFileVersionInfo.FileMajorPart)
                        {
                            if (currentFileVersionInfo.FileMinorPart > oldFileVersionInfo.FileMinorPart)
                            {
                                //MessageBox.Show("Thanks for updating!");
                                File.Copy(curFileName, path + "\\" + curFileName, true);
                            }
                            else
                            {
                                if (currentFileVersionInfo.FileMinorPart == oldFileVersionInfo.FileMinorPart)
                                {
                                    if (currentFileVersionInfo.FileBuildPart > oldFileVersionInfo.FileBuildPart)
                                    {
                                        //MessageBox.Show("Thanks for updating!");
                                        File.Copy(curFileName, path + "\\" + curFileName,true);
                                    }
                                    else
                                    {
                                        if (currentFileVersionInfo.FileBuildPart == oldFileVersionInfo.FileBuildPart)
                                        {
                                            if (currentFileVersionInfo.FilePrivatePart > oldFileVersionInfo.FilePrivatePart)
                                            {
                                                //MessageBox.Show("Thanks for updating!");
                                                File.Copy(curFileName, path + "\\" + curFileName, true);
                                            }
                                            else
                                            {
                                                //current is older than the one in appdata
                                            }
                                        }
                                        else
                                        {
                                            //current is older than the one in appdata
                                        }
                                    }
                                }
                                else
                                {
                                    //current is older than the one in appdata
                                }
                            }
                        }
                        else
                        {
                            //current is older than the one in appdata
                        }
                    }

                }
                catch (Exception)
                {
                   // MessageBox.Show(ex.ToString());
                }
            }

            //read FrostHive/SkinText/default.ini
            //check what skin to use and load FrostHive/SkinText/SKIN01/skintext.ini
            //skin information saved in FrostHive/SkinText/SKIN01/skin.ini

        }
    }
}
