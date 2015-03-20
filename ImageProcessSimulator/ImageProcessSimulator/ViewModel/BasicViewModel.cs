using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private ObservableCollection<string> _imageFiles;
        private ObservableCollection<IFilter> _filters;

        private string _selectedFile;
        private string _selectedVideoFile;
        private IFilter _selectedFilter;


        private ImageBox _sourceBox;
        private ImageBox _destBox;

        private Context _context;
        private Capture _capture;

        #endregion

        #region "Properties"
        public ObservableCollection<string> ImageFiles
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

        public string SelectedFile
        {
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    OnPropertyChanged("SelectedFile");
                    ApplyFilter(_selectedFile);
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
        #endregion

        #region "Constructor"
        public BasicViewModel()
        {
            _imageFiles = new ObservableCollection<string>();
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
            dialog.Filter = "TIFF Files (*.tif)|*.tif|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg";
            dialog.Multiselect = true;

            var result = dialog.ShowDialog();

            if (result != null && result.Value)
            {
                foreach (var fileName in dialog.FileNames)
                {
                    // 파일 추가
                    _imageFiles.Add(fileName);
                }
            }
        }

        private void FileDelHandler(object parameter)
        {
            var selectedFile = parameter as string;

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
                    _imageFiles.Add(file);
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
            using (var src = new Mat(file, LoadImageType.Color))
            using (var dst = new Mat(src.Rows, src.Cols, src.Depth, src.NumberOfChannels))
            {
                _context.Apply(src, dst);
                _sourceBox.Source = src.ToImage<Bgr, Byte>();
                _destBox.Source = dst.ToImage<Bgr, Byte>();
            }
        }

        private void ApplyFilterForVideo(object sender, EventArgs args)
        {
            try
            {
                Mat src = new Mat();
                _capture.Retrieve(src);
                var dst = new Mat(src.Rows, src.Cols, src.Depth, src.NumberOfChannels);

                _context.Apply(src, dst);

                _sourceBox.Source = src.ToImage<Bgr, Byte>();
                _destBox.Source = dst.ToImage<Bgr, Byte>();

                dst.Dispose();
                src.Dispose();
                Task delay = Task.Delay(33);
                delay.Wait();
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
        }



        #endregion
    }
}
