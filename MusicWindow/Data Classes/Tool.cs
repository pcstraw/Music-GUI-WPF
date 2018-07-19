using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Glaxion.Music;
using System.Threading.Tasks;

namespace Glaxion.Tools
{
    public class AutoClosingMessageBox
    {
        System.Threading.Timer _timeoutTimer;
        string _caption;
        AutoClosingMessageBox(string text, string caption, int timeout)
        {
            _caption = caption;
            _timeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                null, timeout, System.Threading.Timeout.Infinite);
            using (_timeoutTimer)
                MessageBox.Show(text, caption);
        }
        public static void Show(string text, string caption, int timeout)
        {
            new AutoClosingMessageBox(text, caption, timeout);
        }
        void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow("#32770", _caption); // lpClassName is #32770 for MessageBox
            if (mbWnd != IntPtr.Zero)
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            _timeoutTimer.Dispose();
        }
        const int WM_CLOSE = 0x0010;
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);
    }
    
    public class ColorScheme
    {
        public Color backColor;
        public Color foreColor;
        public object tag;

        public ColorScheme(Color ForeColor,Color BackColor,object t)
        {
            foreColor = ForeColor;
            backColor = BackColor;
            tag = t;
        }

        public ColorScheme(Color ForeColor, Color BackColor)
        {
            foreColor = ForeColor;
            backColor = BackColor;
        }
    }
    
    public class tool
    {
        static int _count; //count debug console writelines
        public static string musicEditingProgram;
        public static Form GlobalForm { get; internal set; }

        public static async void DeleteAsync(string path)
        {
            await DeleteAsyncFile(path);
        }

        public static Task DeleteAsyncFile(string path)
        {
            return Task.Run(() => { File.Delete(path); });
        }

        public static float clamp(float value, float min, float max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        public static double clamp(double value, double min, double max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        public static int clamp(int value, int min, int max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        public static long clamp(long value, long min, long max)
        {
            if (value > max)
                return max;
            if (value < min)
                return min;
            return value;
        }

        public static float RoundToSingleDecimal(float num)
        {
            //  return (float)Math.Floor(num * 10 + 0.5) / 10;
            return (float)Math.Round(num, 1, MidpointRounding.ToEven);
        }

        public static float Min(float num1, float num2)
        {
           // Tool.Show(num1, num2);
            if (num1 < num2)
                return num1;
            else
                return num2;
        }

        public static float Max(float num1, float num2)
        {
            if (num1 > num2)
                return num1;
            else
                return num2;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        static ConsoleColor cf;
        static ConsoleColor cb;
        public static void SetConsoleErrorState()
        {
            ConsoleColor cf = Console.ForegroundColor;
            ConsoleColor cb = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.White;
            Console.BackgroundColor = ConsoleColor.DarkRed;
        }
        public static void SetConsoleWarningState()
        {
            ConsoleColor cf = Console.ForegroundColor;
            ConsoleColor cb = Console.BackgroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.BackgroundColor = ConsoleColor.Yellow;
        }
        public static void ResetConsoleColor()
        {
            Console.ForegroundColor = cf;
            Console.BackgroundColor = cb;
        }
        public static void debugError(params object[] text)
        {
            SetConsoleErrorState();
            tool.debug(text);
            ResetConsoleColor();
        }
        public static void debugWarning(params object[] text)
        {
            SetConsoleWarningState();
            tool.debug(text);
            ResetConsoleColor();
        }

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        public static bool ConsoleVisible = true;

        public static void ShowConsole()
        {
            var handle = GetConsoleWindow();
            tool.ConsoleVisible = true;
            // Hide
            ShowWindow(handle, SW_SHOW);
        }

        public static void HideConsole()
        {
            var handle = GetConsoleWindow();
            tool.ConsoleVisible = false;
            // Hide
            ShowWindow(handle, SW_HIDE);
        }

        public static bool ToggleConsole()
        {
            if (tool.ConsoleVisible)
            {
                tool.HideConsole();
                return false;
            }
            else
            {
                tool.ShowConsole();
                return true;
            }
        }
        
        public static Color AddColor(Color color,int value)
        {
            int r = color.R + value;
            int g = color.G + value;
            int b = color.B + value;

            if (r > 255)
                r = 255;

            if (g > 255)
                g = 255;

            if (b > 255)
                b = 255;
            
            return Color.FromArgb(r,g,b);
        }

        
        public static void DebugText(params string[] text)
        {
            for (int i = 0; i < text.Length; i++)
                Console.Write(text[i]);
        }

        public static void Test()
        {
            Console.WriteLine("Tool-> " + _count++);
        }

        public static void Test(params object[] text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                Console.WriteLine(_count++.ToString() + ": Tool-> " + text[i].ToString());
            }
        }

        public static string AppendFileName(string path, string append)
        {
            string dir = Path.GetDirectoryName(path);
            string name = Path.GetFileNameWithoutExtension(path);
            string ext = Path.GetExtension(path);
            return string.Concat(dir, "\\", name, append, ext);
        }

        public static bool IsDirectory(string path)
        {
            bool isDir = (File.GetAttributes(path) & FileAttributes.Directory)
                                    == FileAttributes.Directory;
            if (!isDir)
                return true;
            else
                return false;
        }

        public static void SearchForFolder()
        {
            string[] drives = System.Environment.GetLogicalDrives();
            foreach (string drive in drives)
            {
                System.IO.DriveInfo dr = new System.IO.DriveInfo(drive);
                DirectoryInfo di = dr.RootDirectory;
                foreach(DirectoryInfo sub in di.GetDirectories())
                {
                    tool.debug(0, drive, " : ",sub);
                }
            }
        }
        
        public static string SearchDirectoryForFolder(string directory, string folder)
        {
            string[] dirs = Directory.GetDirectories(directory);
            foreach (string s in dirs)
            {
                string name = Path.GetFileName(s);
                if (name.ToLower() == folder.ToLower())
                {
                    //Tool.Debug(0,"folder...", name,"...found in directory...",directory);
                    tool.debug("found directory...", s,"\n");
                    return s;
                }
            }
            return null;
        }

        static public void DisplayLog(int timeout)
        {
            if (Log.Messages.Count == 0)
            {
                tool.show(timeout, "The Message Log is Empty");
                return;
            }
            foreach(string s in Log.Messages)
                tool.show(timeout, s);
        }

        public static void googleSearch(string text)
        {
            Process.Start("http://google.com/search?q=" + text);
        }

        public static string SearchDesktopFolder(string folder)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            return SearchDirectoryForFolder(path, folder);
        }

        //local to environment executable
        static string _tempFolder = @"Output\tmp\";
        public static void OpenTempFolder()
        {
            Process.Start(_tempFolder);
        }
        public static string GetTempFolder()
        {
            if (!Directory.Exists(_tempFolder))
                Directory.CreateDirectory(_tempFolder);
            return _tempFolder;
        }

        public static void OpenNodeDirectory(TreeNode node)
        {
            string s = node.Tag as string;

            if (s != null)
            {
                if (Path.HasExtension(s))
                {
                    Process.Start(Path.GetDirectoryName(s));
                    return;
                }
                else
                {
                    Process.Start(s);
                }
            }
        }
        
        public static bool PressedEnter(PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                return true;
            }
            return false;
        }

        public static bool PressedEnter(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                return true;
            }
            return false;
        }

        public static float min(float a,float b)
        {
            if (a < b)
                return a;
            return b;
        }

        public static float max(float a, float b)
        {
            if (a > b)
                return a;
            return b;
        }

        public static int min(int a, int b)
        {
            if (b < a)
                return b;
            return a;
        }

        public static int max(int a, int b)
        {
            if (b > a)
                return b;
            return a;
        }

        public static double min(double a, double b)
        {
            if (b < a)
                return b;
            return a;
        }

        public static double max(double a, double b)
        {
            if (b > a)
                return b;
            return a;
        }

        public static long min(long a, long b)
        {
            if (b < a)
                return b;
            return a;
        }

        public static long max(long a, long b)
        {
            if (b > a)
                return b;
            return a;
        }

        public static void debug(params object[] text)
        {
            StackFrame frame = new StackFrame(1);
            // Tool.Break();
            string calling = "";

            var method = frame.GetMethod();
            string type = "";
            string name = "";
            if (method != null)
            {
                type = method.DeclaringType.Name;
                calling = string.Concat(type, " : ", method.Name.ToString());
                name = method.Name;
            }
            
            Console.WriteLine(calling);
            string s = "";
            string indent = "-> ";
            for (int i = 0; i < text.Length; i++)
            {
                //s = string.Concat(indent, " ", text[i].ToString());
                s = indent;
                if (text[i] is List<object>)
                {
                    foreach (object o in text[i] as List<object>)
                    {
                        s = string.Concat(s, indent, o.ToString(), "\n");
                    }
                }
                else if (text[i] is List<string>)
                {
                    s = "";
                    foreach (object o in text[i] as List<string>)
                    {
                        s = string.Concat(indent, o, "\n");
                    }
                }
                else
                {
                    if (text[i] != null)
                        s = string.Concat(indent, " ", text[i].ToString());
                    else
                        s = string.Concat(indent, " ", "null object");
                }
                Console.WriteLine(s);
            }
            
          //  Console.WriteLine(string.Concat("Tool::Debug: ", debugIndex, " : ", s));
        }



        public static int debugIndex;

        public static String GetCallingMethod(StackFrame frame)
        {
            var method = frame.GetMethod();
            string type = "";
            string name = "";
            if (method != null)
            {
                type = method.DeclaringType.ToString();
                name = method.Name;
            }
            return string.Concat(name, " : ", method, " : ", type);

        }

        public static void debug(int FrameCount, params object[] text)
        {
           // Tool.Break();
            string s = "";
            string calling = "";
            string indent = "-";
            
            if (FrameCount != 0)
            {
                StackFrame[] stacks = new StackFrame[FrameCount];
                for (int si = FrameCount - 1; si > 0; si--)
                {
                    if (si != 1)
                    {
                        stacks[si] = new StackFrame(si);
                        calling = string.Concat(calling, "\n", indent, GetCallingMethod(stacks[si]));
                        indent += indent;
                    }
                }
            }
            
           // calling = string.Concat(GetCallingMethod(stacks[3]), "\n  ",stacks[2]);

            for (int i = 0; i < text.Length; i++)
            {
                s = string.Concat(s, " ", text[i].ToString());

                if (text[i] is List<object>)
                {
                    s = "";
                    foreach (object o in text[i] as List<object>)
                    {
                        s = string.Concat(s, o.ToString(), "\n");
                    }
                }

                if (text[i] is List<string>)
                {
                    s = "";
                    foreach (object o in text[i] as List<string>)
                    {
                        s = string.Concat(s, o, "\n");
                    }
                }
            }
            Console.WriteLine(calling);
            Console.WriteLine(string.Concat("Tool::Debug: ",debugIndex," : ",s));
            Console.WriteLine();
            debugIndex++;
        }

        public static void Break()
        {
            Debugger.Break();
        }

        public static bool IsBlankString(string text)
        {
            for (int i = 0; i < text.Length;i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    tool.show(10, text[i]);
                    return false;
                }
                    
            }
            return true;
        }

        public static bool StringCheck(string text)
        {
            //changed form the previous code.  this should
            //be the correct implementation, watchout
            //for knockones
            if (text != null && text != "")
                return true;
            return false;
            /*
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != null)
                    return true;
                else
                    return false;
            }
            return false;
            */
        }
        /*
        public static bool StringCheck(object text)
        {
            //changed form the previous code.  this should
            //be the correct implementation, watchout
            //for knockones
            if (text == null)
                return false;
            if (text is string)
            {
                string s = text as string;
                if (s == "")
                    return true;
            }
            return false;
            /*
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] != null)
                    return true;
                else
                    return false;
            }
            return false;
        }
        */
        public static bool enableBreakPoint = false;

        public static void Show(params object[] text)
        {
            if (enableBreakPoint)
                Debugger.Break();
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            var name = method.Name;
            string line = method + "\n" + type + "\n" + name + "\n>>>>>>>>>>>>>>>>>>>>>>>>\n";

            for (int i = 0; i < text.Length; i++)
            {

                if (text[i] != null)
                {
                    if (text[i] is List<string>)
                    {
                        List<string> objs = (List<string>)text[i];
                        line = string.Concat(line, "Enumerable:",objs.Count(),"\n");
                        for (int j=0; j < objs.Count;j++)
                        {
                            line = string.Concat(line, objs[j],"\n");
                        }
                    }
                    else
                    line = string.Concat(line, text[i].ToString());
                }
                else
                    line = string.Concat(line, "Value is null");
            }
            MessageBox.Show(line);
           // ViewBox.Open(line);
        }
        public static int GlobalMessageDelayTime = 1;
        public static void show(int timeOut,params object[] text)
        {
            if (enableBreakPoint)
                Debugger.Break();
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            var name = method.Name;
            string line = "";
            string call = method + "\n" + type + "\n" + name;
            string seperator = "\n>>>>>>>>>>>>>>>>>>>>>>>>\n";

            for (int i = 0; i < text.Length; i++)
            {

                if (text[i] != null)
                {
                    if (text[i] is List<string>)
                    {
                        List<string> objs = (List<string>)text[i];
                        line = string.Concat(line, "Enumerable:", objs.Count(), "\n");
                        for (int j = 0; j < objs.Count; j++)
                        {
                            line = string.Concat(line, objs[j], "\n");
                        }
                    }
                    else
                        line = string.Concat(line, text[i].ToString(),"\n");
                }
                else
                    line = string.Concat(line, "Value is null");
            }
            AutoClosingMessageBox.Show(string.Concat("\n",line,seperator,call),"Message", timeOut*1000*GlobalMessageDelayTime);
            //MessageBox.Show(line);
            // ViewBox.Open(line);
        }

        public static void show()
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            var name = method.Name;
            tool.show(1000,"Test:\n"+method + "\n" +type + "\n"+name);
        }

        public static void show(int timeOut)
        {
            StackFrame frame = new StackFrame(1);
            var method = frame.GetMethod();
            var type = method.DeclaringType;
            var name = method.Name;
            tool.Show("Test:\n" + method + "\n" + type + "\n" + name);
        }

        public static Color SelectColor()
        {
            ColorDialog cd = new ColorDialog();
            cd.ShowDialog();
            return cd.Color;
        }

        public static bool CheckValidString(string text)
        {
            if (text == "" || text == null)
                return false;
            else
                return true;
        }

        public static List<string> SelectFiles(bool FolderPicker,bool MultiSelect,string title)
        {
            CommonOpenFileDialog cd = new CommonOpenFileDialog();
            cd.IsFolderPicker = FolderPicker;
            cd.Multiselect = MultiSelect;
            cd.RestoreDirectory = true;
            cd.Title = title;
            List<string> l = new List<string>();
            if (cd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (cd.Multiselect)
                {
                    foreach (string s in cd.FileNames)
                        l.Add(s);
                    return l;
                }
                else
                {

                    l.Add(cd.FileName);
                    return l;
                }
            }
            return l;
        }

        //deprecate me
        public static List<string> OpenFiles()
        {
            CommonOpenFileDialog cd = new CommonOpenFileDialog();
            cd.IsFolderPicker = false;
            cd.Multiselect = true;
                cd.RestoreDirectory = true;
            List<string> l = new List<string>();
            if (cd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (cd.Multiselect)
                {
                    foreach (string s in cd.FileNames)
                        l.Add(s);
                    return l;
                }
                else
                {

                    l.Add(cd.FileName);
                    return l;
                }
            }
            return l;
        }
        
        //see song class for for more robust mp3 file check
        public static bool IsMP3File(string path)
        {
            if (!Path.HasExtension(path))
                return false;
            string ext = Path.GetExtension(path);
            if (ext == ".mp3")
            {
                if (File.Exists(path))
                    return true;
            }
            return false;
        }

        //dep
        public static bool AudioFileCheck(string path)
        {
            string ext = Path.GetExtension(path);
            if (ext == ".wav"
                    || ext == ".mp3"
                    || ext == ".wma"
                    || ext == ".flac"
                    || ext == ".m4a")
                return true;
            return false;
        }

        public static bool IsAudioFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return false;
            if (!Path.HasExtension(path))
                return false;
            string ext = Path.GetExtension(path);
            if (ext == ".wav"
                    || ext == ".mp3"
                    || ext == ".wma"
                    || ext == ".flac"
                    || ext == ".m4a")
                return true;
           return false;
        }

        public static bool IsTextFile(string path)
        {
            if (Path.GetExtension(path) == ".txt"
                    || Path.GetExtension(path) == ".doc"
                    || Path.GetExtension(path) == ".docx")
                return true;
            else
                return false;
        }

        public static bool IsPlaylistFile(string path)
        {
            if (Path.GetExtension(path) == ".m3u"
                    || Path.GetExtension(path) == ".wpl"
                    || Path.GetExtension(path) == ".pls")
                return true;
            else
                return false;
        }

        public static bool IsImageFile(string path)
        {
            if (Path.GetExtension(path) == ".jpg"
                    || Path.GetExtension(path) == ".png"
                    || Path.GetExtension(path) == ".bmp")
                return true;
            else
                return false;
        }

        public static bool IsVideoFile(string path)
        {
            if (Path.GetExtension(path) == ".mp4"
                    || Path.GetExtension(path) == ".mov"
                    || Path.GetExtension(path) == ".avi"
                    || Path.GetExtension(path) == ".wmv"
                    || Path.GetExtension(path) == ".flv"
                    || Path.GetExtension(path) == ".mkv"
                    || Path.GetExtension(path) == ".m4v"
                    || Path.GetExtension(path) == ".3pg"
                    || Path.GetExtension(path) == ".mkv")
                return true;
            else
                return false;
        }

        public static bool IsAlphabet(string character)
        {
            string c = character.ToLower();
            if(c == "a"| c == "b" | c == "c"| c == "d" | c == "e" | c == "f" | c == "f" | c == "g"
                | c == "h" | c == "i"| c == "j" | c == "k" | c == "l" | c == "m" | c == "p" | c == "q" 
                | c == "r" | c == "s" | c == "t" | c == "u" | c == "v" | c == "w" | c == "x" | c == "y" 
                | c == "z")
            {
                return true;
            }
            return false;
        }

        public static ListViewItem AssignAudioToListView(string path)
        {
            ListViewItem item = new ListViewItem();
            item.SubItems.Add(new ListViewItem.ListViewSubItem());
            item.SubItems[0].Text = Path.GetFileNameWithoutExtension(path);
            item.SubItems[1].Text = path;
            return item;
        }

        public static void DragCopy(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }

            string[] FileList = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            if (e.Data != null && FileList != null)
            {
                foreach (string f in FileList)
                {
                    tool.debug(string.Concat("Tool: ", f));
                }
            }
        }

        public static List<string> CommonDialog(string directory,bool folderPicker, bool multiSelect)
        {
            CommonOpenFileDialog cd = new CommonOpenFileDialog();
            cd.IsFolderPicker = folderPicker;
            cd.Multiselect = multiSelect;
            if (directory == null)
                cd.RestoreDirectory = true;
            else
                cd.InitialDirectory = directory;
            List<string> l = new List<string>();
            if (cd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (multiSelect)
                {
                    foreach (string s in cd.FileNames)
                        l.Add(s);
                    return l;
                }
                else
                {

                    l.Add(cd.FileName);
                    return l;
                }
            }
            else
            {
                l.Clear();
                return l;
            }

        }

        public static List<string> CommonDialog(bool folderPicker,bool multiSelect)
        {
            CommonOpenFileDialog cd = new CommonOpenFileDialog();
            cd.IsFolderPicker = folderPicker;
            cd.Multiselect = multiSelect;
            cd.RestoreDirectory = true;
            List<string> l = new List<string>();
            if (cd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (multiSelect)
                {

                    l = cd.FileNames as List<string>;
                    return l;
                }
                else
                {

                    l.Add(cd.FileName);
                    return l;
                }
            }
            else
            {
                l.Add(null);
                return l;
            }
            
        }

        public static List<string> LoadImageFiles(string path)
        {
            List<string> fs = new List<string>();
            if (!Directory.Exists(path))
            {
                tool.debug("invalid directory: ", path);
                return fs;
            }
            string[] files = Directory.GetFiles(path, "*.*");
           
            foreach (string f in files)
                if (tool.IsImageFile(f))
                    fs.Add(f);

            return fs;
        }

        public static List<string> LoadFiles(string path, string extention)
        {
            string[] files = Directory.GetFiles(path,"*.*",SearchOption.AllDirectories);
            List<string> fs = new List<string>();
            foreach (string f in files)
            {
                string ext = Path.GetExtension(f);

                string name = Path.GetFileNameWithoutExtension(f);

                if (ext == extention)
                {
                    fs.Add(f);
                }
            }

            return fs;
        }

        public static List<string> LoadAudioFiles(string path, SearchOption option)
        {
            if (Path.HasExtension(path))
            {
                if (!File.Exists(path))
                    return new List<string>(0);
                return new List<string>(1) { path };
            }
            if (!Directory.Exists(path))
                return new List<string>(0);

            string[] files =
                (Directory.EnumerateFiles(path, "*.*", option).Where(s => s.EndsWith(".mp3") 
            || s.EndsWith(".flac") 
            || s.EndsWith(".m4a") 
            || s.EndsWith(".wma") 
            || s.EndsWith(".ogg") 
            || s.EndsWith(".wav"))).ToArray();
            return files.ToList();
        }

        public static TreeNode SelectNode(TreeView tree)
        {
            return tree.GetNodeAt(tree.PointToClient(Cursor.Position));    
        }

        public static string GetFolderJPGFile(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Path.HasExtension(path))
                dir = path;
            if (!Directory.Exists(dir))
                return null;
            IEnumerable<string> list = Directory.EnumerateFiles(dir, "*.jpg", SearchOption.TopDirectoryOnly);
            foreach (string s in list)
            {
                if (s.Contains("Folder.jpg"))
                {
                    if (!File.Exists(s))
                        break;
                    return s;
                }
            }
            //tool.show(2, "No Folder.jpg in the folder");
            return null;
        }

        public static Color PickColor()
        {
            ColorDialog cd = new ColorDialog();
            DialogResult result = cd.ShowDialog();
            // See if user pressed ok.
            if (result == DialogResult.OK)
            {
                // Set form background to the selected color.
                return cd.Color;
            }
            return Color.Empty;
        }

        public static void SetPropertyList(List<string> list,StringCollection collection)
        {
            collection.Clear();
            foreach (string s in list)
            {
                if (!collection.Contains(s))
                {
                    collection.Add(s);
                }
            }

          //  Glaxion.Libraries.Properties.Settings.Default.Save();
        }

        public static List<string> GetAllAudioFiles(string path)
        {
            tool.show(5000, "watch this fucntion use. Checks whether the current path is audio file and returns early");
            List<string> sl = new List<string>();
            if (string.IsNullOrEmpty(path)||!File.Exists(path))
                return sl;
            if (tool.IsAudioFile(path))
                sl.Add(path);
            else
            {
                if (!Path.HasExtension(path))
                {
                    sl = tool.LoadAudioFiles(path, SearchOption.AllDirectories);
                }
            }
            return sl;
        }

        public static List<string> GetAllAudioFiles(TreeNode n)
        {
            string s = n.Tag as string;
            List<string> list = new List<string>();
            if (string.IsNullOrEmpty(s))
                return list;
            //  if()
            //    return GetAllAudioFiles(s);
            if (File.Exists(s) && Path.HasExtension(s))
            {
                list.Add(s);
            }
            if (!File.Exists(s) && n.Nodes.Count > 0)
            {
                foreach(TreeNode tn in n.Nodes)
                {
                    string p = tn.Tag as string;
                    if (string.IsNullOrEmpty(p))
                        continue;

                    List<string> ls = GetAllAudioFiles(p);
                    foreach (string text in ls)
                        list.Add(text);
                }
            }


            return list;
        }
        
        public static void OpenFileDirectory(string s)
        {

            if (s != null)
            {
                string args = string.Format("/e, /select, \"{0}\"", s);

                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = "explorer.exe";
                info.Arguments = args;
                if (Path.HasExtension(s))
                {
                    if (File.Exists(s))
                    {
                        Process.Start(info);
                        return;
                    }else
                    {
                        //path is a file not it does not exist
                        if (!File.Exists(s))
                        {
                            tool.Show("Could not find find: " + s);
                            return;
                        }
                    }

                }
                else
                {
                    //no file found

                    if (!Directory.Exists(s))
                    {
                        tool.Show("Could not find directory: " + s);
                        return;
                    }
                    
                    
                    //open if no extenstio, it is a directory
                    Process.Start(info);
                    // DirectoryInfo d = new DirectoryInfo(s);
                    return;
                }
            }
        }
        
        public static void SetVegas()
        {
            OpenFileDialog ofd = new OpenFileDialog();
            {
                ofd.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    musicEditingProgram = ofd.FileName;
                }
            }
        }

        public static void OpenVegas(List<string> list)
        {
            if (!File.Exists(musicEditingProgram))
                SetVegas();

            if (list.Count > 0 && musicEditingProgram != null)
            {
                string args = null;
                foreach (string s in list)
                {
                    if (File.Exists(s))
                    {
                        string arg = '"' + s + '"';
                        args = string.Concat(args, s, " ");
                    }
                }

                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = musicEditingProgram;
                info.Arguments = args;
                Process p = Process.Start(info);
                p.Close();
            }
        }
        

        public static void OpenVegas(string s)
        {
            if (!File.Exists(musicEditingProgram))
                SetVegas();

            if (s != null && musicEditingProgram != null)
            {
                string args = '"'+s+'"';

                ProcessStartInfo info = new ProcessStartInfo();
                info.FileName = musicEditingProgram;
                info.Arguments = args;
                if (Path.HasExtension(s))
                {
                    if (File.Exists(s))
                    {
                        Process.Start(info);
                        return;
                    }
                    else
                    {
                        //path is a file not it does not exist
                        if (!File.Exists(s))
                        {
                            tool.Show("Could not find find: " + s);
                            return;
                        }
                    }

                }
                else
                {
                    //no file found

                    if (!Directory.Exists(s))
                    {
                        tool.Show("Could not find directory: " + s);
                        return;
                    }


                    //open if no extenstio, it is a directory
                    Process.Start(info);
                    // DirectoryInfo d = new DirectoryInfo(s);
                    return;
                }
            }
        }

        public static string BrowseForDirectory(bool folderPicker, bool restoreDirectory,string title)
        {
            List<string> sl = new List<string>();
            CommonOpenFileDialog cd = new CommonOpenFileDialog();
            cd.IsFolderPicker = folderPicker;
            cd.Multiselect = true;
            cd.RestoreDirectory = restoreDirectory;
            cd.Title = title;
            if (cd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return cd.FileName;
            }
            return null;
        }

        public static List<string> BrowseForDirectories(bool folderPicker, bool restoreDirectory)
        {
            List<string> sl = new List<string>();
            CommonOpenFileDialog cd = new CommonOpenFileDialog();
            cd.IsFolderPicker = true;
            cd.Multiselect = true;
            cd.RestoreDirectory = true;
            List<string> l = new List<string>();
            if (cd.ShowDialog() == CommonFileDialogResult.Ok)
            {

                foreach (string s in cd.FileNames)
                    sl.Add(s);
                return sl;
            }
            return sl;
        }

        public static void AllowDragEffect(DragEventArgs e)
        {
            e.Effect = e.AllowedEffect;
            e.Effect = DragDropEffects.Copy;
        }

        public static ListViewItem GetItemFromPoint(ListView listView, System.Drawing.Point mousePosition)
        {
            // translate the mouse position from screen coordinates to 
            // client coordinates within the given ListView
            System.Drawing.Point localPoint = listView.PointToClient(mousePosition);
            ListViewItem lvi = listView.GetItemAt(0, localPoint.Y);
            return lvi;
        }

        public static string NodeToTagTree(TreeNode node, string name)
        {
            if (node.Text == name && tool.IsPlaylistFile(node.Tag as string))
                return node.Tag as string;

            if (node.Nodes.Count > 0)
            {
                foreach (TreeNode n in node.Nodes)
                {
                    string path = NodeToTagTree(n, name);

                    if (path != null)
                        return path;
                }
            }
            return null;
        }

        public static bool askConfirmation(string file)
        {
            DialogResult dr = MessageBox.Show("Are you sure you want to delete:\n" + file, "Confirm",MessageBoxButtons.YesNo);
            if (dr == DialogResult.Yes)
                return true;
            return false;
        }
    }
}
