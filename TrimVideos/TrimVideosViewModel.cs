using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using TrimVideos.Models;

namespace TrimVideos
{
    public class TrimVideosViewModel : INotifyPropertyChanged
    {
        public string SourceFolderPath { get; set; }
        public string OutputFolderPath { get; set; }
        public double TrimBeginningSeconds { get; set; }
        public double TrimEndSeconds { get; set; }
        public string StatusText { get; set; } = "Idle";
        public bool IsProcessing { get; set; }
        public static string[] VideoExtensions = {
            ".MKV", ".MP4", ".AVI", ".WMV", ".MOV", ".FLV"
        };

        public string VideoExtensionsString
        {
            get
            {
                return string.Join(',', VideoExtensions);
            }
            set
            {
                VideoExtensions = value.ToUpperInvariant().Split(',');
            }
        }
        static bool IsVideoFile(string path)
        {
            return -1 != Array.IndexOf(VideoExtensions, Path.GetExtension(path).ToUpperInvariant());
        }
        private CancellationTokenSource cts;

        public ObservableCollection<CompletedFiles> CompletedVideos { get; set; } = new ObservableCollection<CompletedFiles>();

        private ICommand _browseSourceCommand;
        public ICommand BrowseSourceCommand
        {
            get
            {
                return _browseSourceCommand ?? (_browseSourceCommand = new CommandHandler(() => SelectFolder(true), _canExecute));
            }
        }
        private ICommand _browseOutputCommand;
        public ICommand BrowseOutputCommand
        {
            get
            {
                return _browseOutputCommand ?? (_browseOutputCommand = new CommandHandler(() => SelectFolder(), _canExecute));
            }
        }
        private ICommand _startCommand;
        public ICommand StartCommand
        {
            get
            {
                return _startCommand ?? (_startCommand = new CommandHandler(() => StartProcessing(), !IsProcessing));
            }
        }
        private ICommand _cancelCommand;
        public ICommand CancelCommand
        {
            get
            {
                return _cancelCommand ?? (_cancelCommand = new CommandHandler(() => CancelProcessing(), true));
            }
        }
        private bool _canExecute = true;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void SelectFolder(bool isSourceFolder = false)
        {
            var ookiiDialog = new VistaFolderBrowserDialog();
            if (ookiiDialog.ShowDialog() == true)
            {
                if (isSourceFolder)
                {
                    SourceFolderPath = ookiiDialog.SelectedPath;
                    RaisePropertyChanged(nameof(SourceFolderPath));
                }
                else
                {
                    OutputFolderPath = ookiiDialog.SelectedPath;
                    RaisePropertyChanged(nameof(OutputFolderPath));
                }
            }
        }
        public TrimVideosViewModel()
        {
            cts = new CancellationTokenSource();
            RaisePropertyChanged(nameof(VideoExtensionsString));
        }
        private void StartProcessing()
        {
            CompletedVideos?.Clear();

            Task.Run(() =>
            {
                try
                {
                    cts = new CancellationTokenSource();

                    foreach (string file in Directory.GetFiles(SourceFolderPath, "*.*", SearchOption.AllDirectories))
                    {
                        if (cts.Token.IsCancellationRequested)
                            break;

                        var inputFile = new MediaFile { Filename = file };
                        string outputFilePath = Path.Combine(OutputFolderPath, Path.GetFileName(file));

                        if (!IsVideoFile(file) || File.Exists(outputFilePath))
                            continue;

                        var outputFile = new MediaFile { Filename = outputFilePath };

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            IsProcessing = true;
                            RaisePropertyChanged(nameof(IsProcessing));
                            StatusText = $"Processing {Path.GetFileName(file)}";
                            RaisePropertyChanged(nameof(StatusText));
                        });

                        using (var engine = new Engine())
                        {
                            engine.GetMetadata(inputFile);

                            var options = new ConversionOptions();

                            options.CutMedia(TimeSpan.FromSeconds(TrimBeginningSeconds), TimeSpan.FromSeconds(inputFile.Metadata.Duration.TotalSeconds - (TrimBeginningSeconds + TrimEndSeconds)));
                            DateTime start = DateTime.Now;

                            engine.Convert(inputFile, outputFile, options);

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                string timeToTrim = DateTime.Now.Subtract(start).ToString().Substring(0, 8);
                                CompletedVideos.Add(new CompletedFiles() { FilePath = file, TimeToTrim = timeToTrim });
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Canceling kills the ffmpeg process which causes the MediaToolkit to thrown an exception
                    if (!cts.Token.IsCancellationRequested)
                        MessageBox.Show(ex.Message, "Trim Videos", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsProcessing = false;
                    RaisePropertyChanged(nameof(IsProcessing));
                    StatusText = "Idle";
                    RaisePropertyChanged(nameof(StatusText));
                }
            });
        }       
        private void CancelProcessing()
        {
            cts?.Cancel();
            StatusText = "Canceling";
            RaisePropertyChanged(nameof(StatusText));

            foreach (var process in Process.GetProcesses().Where(p=>p.ProcessName.StartsWith("ffmpeg")).ToList())
                process.Kill();
        }
    }

    public class CommandHandler : ICommand
    {
        private Action _action;
        private bool _canExecute;
        public CommandHandler(Action action, bool canExecute)
        {
            _action = action;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action();
        }
    }
}
