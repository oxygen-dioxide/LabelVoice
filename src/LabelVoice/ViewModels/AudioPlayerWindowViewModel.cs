﻿using LabelVoice.Core.Managers;
using NAudio.Wave;
using ReactiveUI;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace LabelVoice.ViewModels
{
    public class AudioPlayerWindowViewModel : ViewModelBase
    {
        #region Private Fields

        private string? _audioFilePath = "请选择一个音频文件。";

        private double _progress;

        private string? _currentTimeString = "--:--";

        private string? _totalTimeString = "--:--";

        private string? _audioDevices = "音频设备：";

        private List<string>? _audioDevicesNameList;

        private CancellationTokenSource? _cts;

        private bool _isReady = false;

        #endregion Private Fields

        #region Properties

        public string? AudioFilePath
        {
            get => _audioFilePath;
            set => this.RaiseAndSetIfChanged(ref _audioFilePath, value);
        }

        public double Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        public string? CurrentTimeString
        {
            get => _currentTimeString;
            set => this.RaiseAndSetIfChanged(ref _currentTimeString, value);
        }

        public string? TotalTimeString
        {
            get => _totalTimeString;
            set => this.RaiseAndSetIfChanged(ref _totalTimeString, value);
        }

        public bool IsReady
        {
            get => _isReady;
            set => this.RaiseAndSetIfChanged(ref _isReady, value);
        }

        public string? AudioDevices
        {
            get => _audioDevices;
            set => this.RaiseAndSetIfChanged(ref _audioDevices, value);
        }

        public List<string>? AudioDevicesNameList
        {
            get => _audioDevicesNameList;
            set => this.RaiseAndSetIfChanged(ref _audioDevicesNameList, value);
        }

        public bool IsWin() => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        #endregion Properties

        #region Methods

        public void OpenAudioFile(string path)
        {
            AudioFilePath = path;
            PlaybackManager.Instance.Load(path);
            CurrentTimeString = "00:00";
            TotalTimeString = PlaybackManager.Instance.GetTotalTime().ToString(@"mm\:ss");
            IsReady = true;
            //var deviceNames = PlaybackManager.Instance.GetDevices().Select(d => d.name);
            //AudioDevices = "音频设备：\n" + string.Join("\n", deviceNames);
        }

        public void UpdateProgress()
        {
            CurrentTimeString = PlaybackManager.Instance.GetCurrentTime().ToString(@"mm\:ss");
            Progress = PlaybackManager.Instance.GetCurrentTime().TotalMilliseconds;
        }

        public void StartUpdateProgress()
        {
            _cts = new CancellationTokenSource();
            Task.Run(() =>
            {
                while (_cts != null && !_cts.IsCancellationRequested)
                {
                    if (PlaybackManager.Instance.GetPlaybackState() == PlaybackState.Playing)
                    {
                        UpdateProgress();
                        Thread.Sleep(100);
                    }
                    else
                    {
                        Thread.Sleep(50);
                    }
                }
            });
        }

        public void StopUpdateProgress()
        {
            if (_cts != null)
                _cts.Cancel();
        }

        #endregion Methods
    }
}