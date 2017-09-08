using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using IWshRuntimeLibrary;

namespace SkinText {

    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application {

        private static void FirstRun(string appdatapath, string curFileName) {
            try {
                //MessageBox.Show("DEBUG: creating folder");
                Directory.CreateDirectory(appdatapath);
                //MessageBox.Show("DEBUG: copying exe file");
                System.IO.File.Copy(curFileName, appdatapath + "\\" + "SkinText.exe", true);
                //MessageBox.Show("DEBUG: creating default skin folder");
                Directory.CreateDirectory(appdatapath + @"\Default");
                //MessageBox.Show("DEBUG: registering in registry");
                FileAssociaton(appdatapath + "\\" + "SkinText.exe");
                //MessageBox.Show("DEBUG: creating desktop shortcut");
                CreateShortcut(appdatapath);
                if (!System.IO.File.Exists(appdatapath + "\\" + "config.ini"))
                {
                    CustomMethods.CurrentSkin = @"\Default";
                    //MessageBox.Show("DEBUG: setting default skin");
                    CustomMethods.SaveCurrentSkin();
                    //skininfo.ini Area
                    //MessageBox.Show("DEBUG: creating default skin config");
                    CustomMethods.CreateModifySkin("Default", "Default", "FrostHive", "2.0.0", "DefaultIcon", "SkinText Default Skin");
                    //skininfo.ini Area
                }
            }
            catch (Exception ex) {
                //MessageBox.Show("Error creating Initial Config", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
#endif
                throw;
            }
        }



        /*
         start
         check for parameters
         get appdata path
         check if runing from appdata
         if yes run acording to parameters
         if not then check if appdata exists
            {
                create-copy-update-register
            }
            if error run as admin the above function, close current

            open appdata exe
            close, delete current exe
             */

        private static void FileAssociaton(string path) {

            IllusoryStudios.Wpf.LostControls.Win32.FileAssociationHelper.RegisterHandlerForPath(nameof(SkinText), "SkinText File", path);
            IllusoryStudios.Wpf.LostControls.Win32.FileAssociationHelper.RegisterAssociation(".xamlp", nameof(SkinText));
            IllusoryStudios.Wpf.LostControls.Win32.FileAssociationHelper.RegisterAssociation(".xaml", nameof(SkinText));
            IllusoryStudios.Wpf.LostControls.Win32.FileAssociationHelper.RegisterAssociation(".sktskin", nameof(SkinText));

            /*IllusoryStudios.Wpf.LostControls.Win32.StartupSettings startupSettings2 = new IllusoryStudios.Wpf.LostControls.Win32.StartupSettings(nameof(SkinText)) {
                CurrentPath = path,
                DesiredPath = path,
                StartsWithSystem = false
            };*/
        }
        private static void CreateShortcut(string appdatapath) {
            string link = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            + Path.DirectorySeparatorChar + "SkinText.lnk";
            WshShell shell = new WshShell();
            IWshShortcut shortcut = shell.CreateShortcut(link) as IWshShortcut;
            //shortcut.Description = "SkinText \"smart\" Shortcut";
            shortcut.Hotkey = "Ctrl+Shift+N";
            shortcut.TargetPath = appdatapath+@"\SkinText.exe";
            shortcut.WorkingDirectory = appdatapath;
            //shortcut...
            shortcut.Save();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private void Application_Startup(object sender, StartupEventArgs e) {

            string curFileNamePath = Process.GetCurrentProcess().MainModule.FileName;
            string curFileName = Path.GetFileName(curFileNamePath);

            string appdatapath = CustomMethods.GAppPath;
            CustomMethods.AppDataPath = appdatapath;
            string curPath = Path.GetDirectoryName(curFileNamePath);

            #if DEBUG
            //MessageBox.Show("DEBUG:\r\n AppDatapath: " +appdatapath+"\r\n curPath: " + curPath);
            #endif


            CheckCmdParameters(e, appdatapath, curFileName);

            //check if runing from appdata or if portable
            if (curPath == appdatapath) {
                #if DEBUG
                //MessageBox.Show("DEBUG: runing from appdata");
                #endif
                //if yes run acording to parameters (Open Files)
            }
            else {
                if (!Directory.Exists(appdatapath)) {
                    MessageBox.Show("Welcome to the SkinText Family! \r\nFor the first run we need to do a quick config", "Welcome!", MessageBoxButton.OK, MessageBoxImage.Information);
                    try {
                        FirstRun(appdatapath, curFileName);
                        MessageBox.Show("Config complete!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                        RunAppdata(appdatapath);
                        DeleteSelf();
                    }
                    catch (Exception ex) {
                        MessageBox.Show("Error creating Initial Config, requesting admin rights", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        #if DEBUG
                        MessageBox.Show("DEBUG: "+ex.ToString());
                        //throw;
                        #endif
                        FirstRunAdmin(curFileName);
                    }
                    CloseThis();
                }
                //update
                UpdateFile(curFileNamePath, curFileName, appdatapath);
                //if update not neccesary it will not auto-close and continue to use current exe isntead of appdata exe
            }

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void UpdateFile(string curFileNamePath, string curFileName, string appdatapath) {
            if (!System.IO.File.Exists(appdatapath + "\\" + "SkinText.exe")) {
                try {
                    //File.Copy(curFileName, appdatapath + "\\" + "SkinText.exe", true);
                    //FileAssociaton(appdatapath + "\\" + "SkinText.exe");
                    FirstRun(appdatapath, curFileName);
                    MessageBox.Show("Detected a missing file and created it", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                    RunAppdata(appdatapath);
                    DeleteSelf();
                }
                catch (Exception ex) {
#if DEBUG
                    MessageBox.Show("DEBUG: "+ex.ToString());
                    //throw;
#endif
                    FirstRunAdmin(curFileName);
                }
                CloseThis();
            }
            else {
                try {
                    //check version for update
                    FileVersionInfo oldFileVersionInfo = FileVersionInfo.GetVersionInfo(appdatapath + "\\" + "SkinText.exe");
                    FileVersionInfo currentFileVersionInfo = FileVersionInfo.GetVersionInfo(curFileNamePath);
                    bool update = false;
                    if (currentFileVersionInfo.FileMajorPart > oldFileVersionInfo.FileMajorPart) {
                        update = true;
                    }
                    else {
                        if (currentFileVersionInfo.FileMajorPart == oldFileVersionInfo.FileMajorPart) {
                            if (currentFileVersionInfo.FileMinorPart > oldFileVersionInfo.FileMinorPart) {
                                update = true;
                            }
                            else {
                                if (currentFileVersionInfo.FileMinorPart == oldFileVersionInfo.FileMinorPart) {
                                    if (currentFileVersionInfo.FileBuildPart > oldFileVersionInfo.FileBuildPart) {
                                        update = true;
                                    }/* Since FileBuildPart seems to be random, it's beter to not check it
                                    else {
                                        if (currentFileVersionInfo.FileBuildPart == oldFileVersionInfo.FileBuildPart) {
                                            if (currentFileVersionInfo.FilePrivatePart > oldFileVersionInfo.FilePrivatePart) {
                                                update = true;
                                            }
                                        }
                                    }*/
                                }
                            }
                        }
                    }
                    if (update) {
                        //File.Copy(curFileName, appdatapath + "\\" + "SkinText.exe", true);
                        FirstRun(appdatapath, curFileName);
                        MessageBox.Show("Thanks for updating!");
                        RunAppdata(appdatapath);
                        DeleteSelf();
                        CloseThis();
                    }

                }
                catch (Exception ex) {
#if DEBUG
                    MessageBox.Show("DEBUG: "+ex.ToString());
                    //throw;
#endif
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void CheckCmdParameters(StartupEventArgs e, string appdatapath, string curFileName) {
            // Application is running
            // Process command line args
            bool firstRun = false;
            bool Update = false;
            for (int i = 0; i != e.Args.Length; ++i) {
                firstRun |= (e.Args[i] == "-FirstRun");
                Update   |= (e.Args[i] == "-Update");
            }
            // Create main application window, starting minimized if specified
            if (firstRun) {
                try {
                    FirstRun(appdatapath, curFileName);
                    MessageBox.Show("Config complete!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
                    RunAppdata(appdatapath);
                    DeleteSelf();
                CloseThis();
                }
                catch (Exception ex) {
                    //if admin and still crashes
                    MessageBox.Show("Detected a missing file and could not fix it, file associations will not work", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    #if DEBUG
                    MessageBox.Show("DEBUG: "+ex.ToString());
                    //throw;
                    #endif
                }
            }
            else {
                if (e.Args.Length>0) {
                    if (System.IO.File.Exists(e.Args[0])) {
                        CustomMethods.FilepathAssociated = e.Args[0];
                        /*if (Path.GetExtension(e.Args[0]).ToUpperInvariant() == ".SKTSKIN") {
                            //open skin package
                            CustomMethods.FilepathAssociated = e.Args[0];
                        }
                        else {
                            //if double clicked an assosiated file
                            CustomMethods.FilepathAssociated = e.Args[0];
                        }*/
                    }
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void RunAppdata(string appdatapath) {
            try {
                using (Process process = new Process()) {
                    process.StartInfo.FileName = appdatapath + "\\" + "SkinText.exe";
                    process.Start();
                    //process.WaitForExit();
                }
            }
            catch (Exception ex) {
                #if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
                #endif
            }
        }

        private static void CloseThis() {
            //Application.Current.Shutdown();
            Process.GetCurrentProcess().Kill();
            //Environment.Exit(0);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private static void FirstRunAdmin(string curFileName) {
            try {
                //Request admin rights
                using (Process process = new Process()) {
                    process.StartInfo.FileName = curFileName;
                    process.StartInfo.Arguments = "-FirstRun";
                    process.StartInfo.Verb = "runas";
                    process.Start();
                    //process.WaitForExit();
                    //process.Dispose();
                }
            }
            catch (Exception ex) {
                //if failed to launch as admin
                MessageBox.Show("Detected a missing file and could not fix it, file associations will not work", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
#if DEBUG
                MessageBox.Show("DEBUG: "+ex.ToString());
                //throw;
#endif
            }
        }

        private static void DeleteSelf() {
            ProcessStartInfo Info = new ProcessStartInfo {
                Arguments = "/C choice /C Y /N /D Y /T 3 & Del \"" +
                        Process.GetCurrentProcess().MainModule.FileName + "\"",
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
                FileName = "cmd.exe"
            };
            Process.Start(Info);
        }
    }
}