using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

using Base;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using ImageProcessSimulator.Filters;
using ImageProcessSimulator.View;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ImageProcessSimulator.ViewModel
{
    public class BasicContext : Context
    {

    }

    public class BasicViewModel : ViewModelBase
    {
        #region "Fields"

        private ObservableCollection<FileInfo> _imageFiles;
        private ObservableCollection<IFilter> _filters;

        private FileInfo _selectedFile;
        private string _selectedVideoFile;
        private IFilter _selectedFilter;


        private ImageBox _sourceBox;
        private ImageBox _destBox;

        private Context _context;
        private Capture _capture;

        private string _performance;
        private string _imageSize;

        private Stopwatch _stopwatch = new Stopwatch();
        #endregion

        #region "Properties"
        public ObservableCollection<FileInfo> ImageFiles
        {
            set
            {
                _imageFiles = value;
            }
            get { return _imageFiles; }
        }

        public ObservableCollection<IFilter> Filters
        {
            set { _filters = value; }
            get { return _filters; }
        }

        public FileInfo SelectedFile
        {
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    OnPropertyChanged("SelectedFile");
                    ApplyFilter(_selectedFile.FullName);
                }
            }
            get { return _selectedFile; }
        }

        public string SelectedVideoFile
        {
            set
            {
                _selectedVideoFile = value;
                CreateVideoCapture();
                OnPropertyChanged("SelectedVideoFile");
            }
        }

        public IFilter SelectedFilter
        {
            set
            {
                if (_selectedFilter != value)
                {
                    _selectedFilter = value;
                    _context.SetFilter(_selectedFilter);
                    OnPropertyChanged("SelectedFilter");
                }
            }
            get { return _selectedFilter; }
        }

        public string Performance
        {
            set
            {
                if (_performance != value)
                {
                    _performance = value;
                    OnPropertyChanged("Performance");
                }
            }
            get { return _performance; }
        }

        public string ImageSize
        {
            set
            {
                if (_imageSize != value)
                {
                    _imageSize = value;
                    OnPropertyChanged("ImageSize");
                }
            }
            get { return _imageSize; }
        }
        #endregion

        #region "Constructor"
        public BasicViewModel()
        {
            _imageFiles = new ObservableCollection<FileInfo>();
            _filters = new ObservableCollection<IFilter>();

            _context = new BasicContext();

            SetFilters();
        }

        public BasicViewModel(ImageBox sourceBox, ImageBox destBox)
            : this()
        {
            // TODO: Complete member initialization
            _sourceBox = sourceBox;
            _destBox = destBox;
        }
        #endregion

        #region "Commands"

        private RelayCommand _fileAddCommand;
        private RelayCommand _fileDelCommand;
        private RelayCommand _folderAddCommand;
        private RelayCommand _VideoFileAddCommand;

        public ICommand FileAddCommand
        {
            get { return _fileAddCommand ?? (_fileAddCommand = new RelayCommand(FileAddHandler)); }
        }

        public ICommand FileDelCommand
        {
            get { return _fileDelCommand ?? (_fileDelCommand = new RelayCommand(FileDelHandler)); }
        }


        public ICommand FolderAddCommand
        {
            get { return _folderAddCommand ?? (_folderAddCommand = new RelayCommand(FolderAddHandler)); }
        }

        public ICommand VideoFileAddCommand
        {
            get { return _VideoFileAddCommand ?? (_VideoFileAddCommand = new RelayCommand(VideoFileAddHandler)); }
        }


        #endregion

        #region "Command handler"

        private void FileAddHandler(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "JPG Files (*.jpg)|*.jpg|TIFF Files (*.tif)|*.tif|PNG Files (*.png)|*.png";
            dialog.Multiselect = true;

            var result = dialog.ShowDialog();

            if (result != null && result.Value)
            {
                foreach (var fileName in dialog.FileNames)
                {
                    // 파일 추가
                    var SafefileName = dialog.SafeFileName;
                    FileInfo info = new FileInfo(fileName);

                    _imageFiles.Add(info);
                }
            }
        }

        private void FileDelHandler(object parameter)
        {
            var selectedFile = parameter as FileInfo;

            if (selectedFile != null)
            {
                _imageFiles.Remove(selectedFile);
            }
        }

        private void FolderAddHandler(object parameter)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            var result = folderBrowserDialog.ShowDialog();

            if (result == DialogResult.OK)
            {
                var selectedPath = folderBrowserDialog.SelectedPath;
                HashSet<string> fileList = new HashSet<string>();
                SearchFiles(fileList, selectedPath);

                foreach (var file in fileList)
                {
                    FileInfo info = new FileInfo(file);
                    _imageFiles.Add(info);
                }
            }
        }
        private void VideoFileAddHandler(object parameter)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "AVI Files (*.avi)|*.avi|Mp4 Files (*.mp4)|*.mp4";
            dialog.Multiselect = false;

            var result = dialog.ShowDialog();

            if (result != null && result.Value)
            {
                SelectedVideoFile = dialog.FileName;
            }
        }
        private static bool SearchFiles(HashSet<string> filelist, string nextDir)
        {
            try
            {
                DirectoryInfo currDir = new DirectoryInfo(nextDir);

                FileInfo[] zipFiles = currDir.GetFiles();
                for (int np = 0; np < zipFiles.Count(); np++)
                {
                    if (zipFiles[np].Extension == "jpg"
                        || zipFiles[np].Extension == "tif"
                        || zipFiles[np].Extension == "png")
                    {
                        filelist.Add(zipFiles[np].FullName);
                    }
                }

                DirectoryInfo[] zipDirs = currDir.GetDirectories();
                for (int np = 0; np < zipDirs.Count(); np++)
                {
                    SearchFiles(filelist, zipDirs[np].FullName);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region "Apply filter"
        private void ApplyFilter(string file)
        {
            using (var src = new Image<Bgr,byte>(file))
            {
                int width = src.Width;
                int height = src.Height;
                var channel = src.NumberOfChannels;
                var sizeKb = width*height*channel / 1024;

                Image<Bgr, byte> dst;

                
                try
                {
                    _stopwatch.Reset();
                    _stopwatch.Start();
                    _context.Apply(src, out dst);
                    _stopwatch.Stop();

                    _destBox.Source = dst;

                    ImageSize = string.Format("ImageSize = {0}KB", sizeKb);
                    Performance = string.Format("Performance = {0}ms", _stopwatch.ElapsedMilliseconds);
                    
                }
                catch (Exception)
                {
                    _stopwatch.Stop();
                    _stopwatch.Reset();
                }
                finally
                {
                    _sourceBox.Source = src;
                    
                }
            }

        }

        private void ApplyFilterForVideo(object sender, EventArgs args)
        {
            try
            {
                Mat src = new Mat();
                _capture.Retrieve(src);
                using (Image<Bgr, Byte> srcImage = src.ToImage<Bgr, Byte>())
                {
                    Image<Bgr, Byte> dst;
                    _context.Apply(srcImage, out dst);
                    _sourceBox.Source = src;
                    _destBox.Source = dst;
                    dst.Dispose();
                }
//                 Task delay = Task.Delay(33);
//                 delay.Wait();
            }
            catch (NullReferenceException e)
            {
                MessageBox.Show(string.Format("you first Select a filter"));
                _capture.Stop();
            }
        }
        #endregion

        #region "Private methods"

        private void CreateVideoCapture()
        {
            if (_capture != null)
            {
                _capture.Stop();
                _capture.Dispose();
            }

            if (_selectedVideoFile != null)
            {
                _capture = new Capture(_selectedVideoFile);
                _capture.ImageGrabbed += ApplyFilterForVideo;
                _capture.Start();
            }
        }

        private void SetFilters()
        {
            _filters.Add(new BinaryFilter(80));
            _filters.Add(new EdgeFilter());
            _filters.Add(new HistogramFilter());
        }



        #endregion
    }
}
