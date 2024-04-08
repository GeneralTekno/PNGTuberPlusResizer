using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.ComponentModel;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Numerics;
using System.Xml.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace PNGPlusResizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            IsNotRunning = true;
            CanRunJob = false;
            _percentageValue = 100;
            OnPropertyChanged(nameof(PercentageValue));

        }


        private double _percentageValue;
        public string PercentageValue
        {
            get { return _percentageValue.ToString("N2"); }
            set { var val = Double.Parse(value);
                if (val < 0) val = 1;
                _percentageValue = val;
                OnPropertyChanged(nameof(PercentageValue));

            }
        }

        private string _filePath, _savePath, _progress;
        public string FilePath { get { return _filePath; } set { _filePath = value; OnPropertyChanged(nameof(FilePath)); } }

        public string SavePath
        {
            get { return _savePath; }
            set { _savePath = value; OnPropertyChanged(nameof(SavePath)); }
        }


        public string Progress
        {
            get { return _progress; }
            private set { _progress = value; OnPropertyChanged(nameof(Progress)); }
        }

        private bool _canRunJob, _isNotRunning;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool CanRunJob
        {
            get { return _canRunJob && IsNotRunning; }
            set { _canRunJob = value; OnPropertyChanged(nameof(CanRunJob)); }
        }

        public bool IsNotRunning
        {
            get { return _isNotRunning; }
            set { _isNotRunning = value;
                OnPropertyChanged(nameof(IsNotRunning));
                OnPropertyChanged(nameof(CanRunJob));
            }
        }

        private void RunJob_MouseUp(object sender, RoutedEventArgs e)
        {            var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();

            if (!CanRunJob) return;
            //dlg.Filter = "PNGTuber+ Save Files (*.save)|*.save";
            //dlg.Title = "Select a directory to save the new file to. save file to open.";
            if (dlg.ShowDialog() == true)
            {
                if (Directory.Exists(dlg.SelectedPath))
                {
                    var newFileName = Path.Combine(dlg.SelectedPath, Path.GetFileNameWithoutExtension(FilePath) + "_" + PercentageValue + Path.GetExtension(FilePath));
                    var worker = new BackgroundWorker();
                    bool success = false;
                    worker.DoWork += (s, e) => { 
                    
                       success = RunJob(FilePath, newFileName, _percentageValue / 100.0);
                    };
                    worker.RunWorkerCompleted += (s, e) =>
                    {
                        IsNotRunning = true;
                    };

                    IsNotRunning = false;
                    worker.RunWorkerAsync();
                }

            }
        }

        private void btnSelectFile_MouseUp(object sender, RoutedEventArgs e)
        {
            CanRunJob = false;
            var dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();
            dlg.Filter = "PNGTuber+ Save Files (*.save)|*.save";
            dlg.Title = "Select a PNGTuberPlus save file to open.";
            if (dlg.ShowDialog() == true)
            {
                if (File.Exists(dlg.FileName))
                {
                    FilePath = dlg.FileName;
                    CanRunJob = true;
                }

            }
        }

        private bool ResizeImage(byte[] imageData, string newFilename, double percentage, out byte[] newImageData)
        {
            newImageData = null;
            try
            {
                using (var ms = new MemoryStream(imageData))
                {
                    using (Image image = Image.Load(ms))
                    {
                        // Get the image current width
                        int sourceWidth = image.Width;
                    // Get the image current height
                    int sourceHeight = image.Height;

                    // Calculate width and height with new desired size
                    // New Width and Height
                    int destWidth = (int)(sourceWidth * percentage);
                    int destHeight = (int)(sourceHeight * percentage);
                    if (!Directory.Exists(Path.GetDirectoryName(newFilename)))
                        Directory.CreateDirectory(Path.GetDirectoryName(newFilename));

                        image.Mutate(x => x.Resize(destWidth, destHeight));
                        image.SaveAsPng(newFilename);
                    }
                        
                    newImageData = File.ReadAllBytes(newFilename);
                }
                return true;
            }
            catch {}
            {
                return false;
            }

        }

        private bool RunJob(string saveFilePath, string newSaveFilePath, double percentage)
        {
            var newDirectory = Path.GetDirectoryName(newSaveFilePath);

            if (!File.Exists(saveFilePath)) return false;
            if (newSaveFilePath == saveFilePath) return false;

            var fileText = File.ReadAllText(saveFilePath);

            try
            {
                JObject jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(fileText);

                //Go through nodes and change percentage here
                foreach (var node in jsonObj)
                {
                    JToken vals = node.Value;

                    Progress = string.Format("Processing Node {0}/{1}...",(Int32.Parse(node.Key)+1).ToString(), jsonObj.Count);



                    var path = vals["path"].ToObject<string>();
                    var oldOffset = Vec2FromString(vals["offset"].ToObject<string>());
                    var oldPosition = Vec2FromString(vals["pos"].ToObject<string>());
                    var oldXAmp = vals["xAmp"].ToObject<double>();
                    var oldYAmp = vals["yAmp"].ToObject<double>();
                    var imgData = vals["imageData"].ToObject<string>();

                    vals["offset"] = StrFromVector2(new Vector2(oldOffset.X * (float)percentage, oldOffset.Y * (float)percentage));
                    vals["pos"] = StrFromVector2(new Vector2(oldPosition.X * (float)percentage, oldPosition.Y * (float)percentage));
                    vals["xAmp"] = (oldXAmp * percentage).ToString();
                    vals["yAmp"] = (oldYAmp * percentage).ToString();


                    //paths have format of "user://relative directory"
                    var filename = Path.GetFileName(path);

                    var newPath = Path.Combine(Path.GetDirectoryName(newSaveFilePath), "sprites", filename);

                    {
                        Progress = string.Format("Processing Node {0}/{1}; Resizing {2}...", node.Key.ToString(), jsonObj.Count, filename);
                        var imageData = Convert.FromBase64String(imgData);

                        byte[] newData;
                        if (!ResizeImage(imageData, newPath, percentage, out newData))
                            throw new Exception();
                        vals["imageData"] = Convert.ToBase64String(newData);
                        vals["path"] = string.Format("user://{0}", Path.GetRelativePath(newDirectory, newPath).Replace("\\","/"));
                    }

                }
                Progress = string.Format("Saving file...");

                var output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj);
                File.WriteAllText(newSaveFilePath, output);
                Progress = string.Format("Export complete!");
            }
            catch 
            {

                Progress = string.Format("Failed to export file.");
                return false;
            }
            return true;
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private Vector2 Vec2FromString(string str)
        {
            var extracted = str.Replace("Vector2(", "").Replace(")", "").Split(',');
            if (extracted.Length == 2)
            {
                float x, y;
                float.TryParse(extracted[0], out x);
                float.TryParse(extracted[1], out y);
                return new Vector2(x, y);
            }
            else return new Vector2(0, 0);
        }

        private string StrFromVector2(Vector2 vec)
        {
            return string.Format("Vector2({0},{1})", vec.X, vec.Y);
        }
    }

}