using NAudio.Wave;
using System;
using System.IO;

namespace UIGameClientTourist.GameLogic
{
    public class MusicService
    {
        private static MusicService _instance;
        private readonly WaveOutEvent _waveOut;
        private readonly AudioFileReader _audioFileReader;

        private MusicService()
        {
            _waveOut = new WaveOutEvent();
            string projectDirectory = Directory.GetParent(Environment.CurrentDirectory).Parent.FullName;
            string filePath = Path.Combine(projectDirectory, "GameResources", "Music", "utomp3.com-One-Piece-OST-Overtaken-EPIC-VERSION-Drums-of-Liberation_320kbps.wav");
            _audioFileReader = new AudioFileReader(filePath);
            _waveOut.Init(_audioFileReader);
        }

        public static MusicService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MusicService();
                }
                return _instance;
            }
        }

        public void PlayMusic()
        {
            if (_waveOut.PlaybackState == PlaybackState.Stopped)
            {
                _audioFileReader.Position = 0;
                _waveOut.Play();
            }
            else if (_waveOut.PlaybackState == PlaybackState.Paused)
            {
                _waveOut.Play();
            }
        }

        public void PauseMusic()
        {
            if (_waveOut.PlaybackState == PlaybackState.Playing)
            {
                _waveOut.Pause();
            }
        }

        public void StopMusic()
        {
            if (_waveOut.PlaybackState != PlaybackState.Stopped)
            {
                _waveOut.Stop();
                _audioFileReader.Position = 0;
            }
        }

        public void SetVolume(double volume)
        {
            if (volume < 0)
                volume = 0;
            else if (volume > 1)
                volume = 1;
            _waveOut.Volume = (float)volume;
        }
    }
}
