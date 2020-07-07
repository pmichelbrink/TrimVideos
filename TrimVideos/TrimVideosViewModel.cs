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

namespace TrimVideos
{
    public class TrimVideosViewModel : INotifyPropertyChanged
    {
        public string SourceFolderPath { get; set; }
        public string OutputFolderPath { get; set; }
        public double TrimBeginningSeconds { get; set; }
        public double TrimEndSeconds { get; set; }
        public string LastTimeToTrim { get; set; }
        public string StatusText { get; set; } = "Idle";
        public bool IsProcessing { get; set; }
        private CancellationTokenSource cts;

        public ObservableCollection<string> CompletedVideos { get; set; } = new ObservableCollection<string>();

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
        }
        private void StartProcessing()
        {
            Task.Run(() =>
            {
                try
                {
                    cts = new CancellationTokenSource();

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        IsProcessing = true;
                        RaisePropertyChanged(nameof(IsProcessing));
                        StatusText = "Processing";
                        RaisePropertyChanged(nameof(StatusText));
                        CompletedVideos.Clear();
                    });

                    foreach (string file in Directory.GetFiles(SourceFolderPath, "*.*", SearchOption.AllDirectories))
                    {
                        if (cts.Token.IsCancellationRequested)
                            break;

                        var inputFile = new MediaFile { Filename = file };
                        string outputFilePath = Path.Combine(OutputFolderPath, Path.GetFileName(file));

                        if (File.Exists(outputFilePath))
                            File.Delete(outputFilePath);

                        var outputFile = new MediaFile { Filename = outputFilePath };

                        using (var engine = new Engine())
                        {
                            engine.GetMetadata(inputFile);

                            var options = new ConversionOptions();

                            options.CutMedia(TimeSpan.FromSeconds(TrimBeginningSeconds), TimeSpan.FromSeconds(inputFile.Metadata.Duration.TotalSeconds - (TrimBeginningSeconds + TrimEndSeconds)));
                            DateTime start = DateTime.Now;

                            engine.Convert(inputFile, outputFile, options);

                            App.Current.Dispatcher.Invoke(() =>
                            {
                                LastTimeToTrim = DateTime.Now.Subtract(start).ToString().Substring(0, 8);
                                RaisePropertyChanged(nameof(LastTimeToTrim));

                                CompletedVideos.Add(file);
                                RaisePropertyChanged(nameof(CompletedVideos));
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Canceling kills the ffmpeg process which causes the MediaToolkit to thrown an exception
                    if (!cts.Token.IsCancellationRequested)
                    {
                        MessageBox.Show("Trim Videos", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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
