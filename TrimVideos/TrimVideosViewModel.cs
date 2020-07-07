using MediaToolkit;
using MediaToolkit.Model;
using MediaToolkit.Options;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public string ButtonText { get; set; } = "Start";
        public bool IsIdle { get; set; } = true;
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
                return _startCommand ?? (_startCommand = new CommandHandler(() => StartProcessing(), _canExecute));
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
            try
            {
                Task.Run(() =>
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        IsIdle = false;
                        RaisePropertyChanged(nameof(IsIdle));
                        CompletedVideos.Clear();
                    });

                    foreach (string file in Directory.GetFiles(SourceFolderPath, "*.*", SearchOption.AllDirectories))
                    {
                        if (cts.Token.IsCancellationRequested)
                            return;

                        var inputFile = new MediaFile { Filename = file };
                        var outputFile = new MediaFile { Filename = Path.Combine(OutputFolderPath, Path.GetFileName(file)) };

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
                });
            }
            finally
            {
                IsIdle = true;
                RaisePropertyChanged(nameof(IsIdle));
            }
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
