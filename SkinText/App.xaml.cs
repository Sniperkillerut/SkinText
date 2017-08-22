using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace SkinText {

    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application {

        private static void FirstRun(string path) {
            try {
                Directory.CreateDirectory(path);
                string curFileNamePath = Process.GetCurrentProcess().MainModule.FileName;
                string curFileName = Path.GetFileName(curFileNamePath);
                File.Copy(curFileName, path + "\\" + curFileName);
                MessageBox.Show("Config complete!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex) {
                MessageBox.Show("Error creating Initial Config", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#if DEBUG
                MessageBox.Show(ex.ToString());
                //throw;
#endif
                throw;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Application_Startup(object sender, StartupEventArgs e) {
            string path = CustomMethods.GAppPath;
            // Application is running
            // Process command line args
            bool firstRun = false;
            bool Update = false;
            for (int i = 0; i != e.Args.Length; ++i) {
                firstRun = (e.Args[i] == "-FirstRun");
                Update |= e.Args[i] == "-Update";
            }
            // Create main application window, starting minimized if specified
            if (firstRun) {
                FirstRun(path);
                //Application.Current.Shutdown();
                Process.GetCurrentProcess().Kill();
                //Environment.Exit(0);
            }
            if (Update) {
            }

            string curFileNamePath = Process.GetCurrentProcess().MainModule.FileName;
            string curFileName = Path.GetFileName(curFileNamePath);
            if (!Directory.Exists(path)) {
                MessageBox.Show("Welcome to the SkinText Family! \r\nFor the first run we need to do a quick config", "Welcome!", MessageBoxButton.OK, MessageBoxImage.Information);
                try {
                    Directory.CreateDirectory(path);
                    File.Copy(curFileName, path + "\\" + curFileName);
                    Directory.CreateDirectory(path + @"\Default");
                    MessageBox.Show("Config complete!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) {
#if DEBUG
                    MessageBox.Show(ex.ToString());
                    //throw;
#endif
                    try {
                        //Request admin rights
                        using (Process process = new Process()) {
                            process.StartInfo.FileName = curFileName;
                            process.StartInfo.Arguments = "-FirstRun";
                            process.StartInfo.Verb = "runas";
                            process.Start();
                            process.WaitForExit();
                        }
                    }
                    catch (Exception ex2) {
                        MessageBox.Show("Could Not complete config", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#if DEBUG
                        MessageBox.Show(ex2.ToString());
                        //throw;
#endif
                    }
                }
            }
            if (!File.Exists(path + "\\" + curFileName)) {
                try {
                    File.Copy(curFileName, path + "\\" + curFileName, true);
                    MessageBox.Show("Detected a missing file and created it", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex) {
#if DEBUG
                    MessageBox.Show(ex.ToString());
                    //throw;
#endif
                    try {
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
                    catch (Exception ex2) {
#if DEBUG
                        MessageBox.Show(ex2.ToString());
                        //throw;
#endif
                        MessageBox.Show("Detected a missing file and could not fix it, file associations will not work", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else {
                try {
                    //check version for update
                    FileVersionInfo oldFileVersionInfo = FileVersionInfo.GetVersionInfo(path + "\\" + curFileName);
                    FileVersionInfo currentFileVersionInfo = FileVersionInfo.GetVersionInfo(curFileNamePath);
                    //oldFileVersionInfo.FileMajorPart 0
                    //oldFileVersionInfo.FileMinorPart 1
                    //oldFileVersionInfo.FileBuildPart 20
                    //oldFileVersionInfo.FilePrivatePart 0
                    if (currentFileVersionInfo.FileMajorPart > oldFileVersionInfo.FileMajorPart) {
                        //MessageBox.Show("Thanks for updating!");
                        File.Copy(curFileName, path + "\\" + curFileName, true);
                    }
                    else {
                        if (currentFileVersionInfo.FileMajorPart == oldFileVersionInfo.FileMajorPart) {
                            if (currentFileVersionInfo.FileMinorPart > oldFileVersionInfo.FileMinorPart) {
                                //MessageBox.Show("Thanks for updating!");
                                File.Copy(curFileName, path + "\\" + curFileName, true);
                            }
                            else {
                                if (currentFileVersionInfo.FileMinorPart == oldFileVersionInfo.FileMinorPart) {
                                    if (currentFileVersionInfo.FileBuildPart > oldFileVersionInfo.FileBuildPart) {
                                        //MessageBox.Show("Thanks for updating!");
                                        File.Copy(curFileName, path + "\\" + curFileName, true);
                                    }
                                    else {
                                        if (currentFileVersionInfo.FileBuildPart == oldFileVersionInfo.FileBuildPart) {
                                            if (currentFileVersionInfo.FilePrivatePart > oldFileVersionInfo.FilePrivatePart) {
                                                //MessageBox.Show("Thanks for updating!");
                                                File.Copy(curFileName, path + "\\" + curFileName, true);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) {
#if DEBUG
                    MessageBox.Show(ex.ToString());
                    //throw;
#endif
                }
            }
        }
    }
}