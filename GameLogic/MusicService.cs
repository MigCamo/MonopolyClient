using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using NAudio.Wave;

namespace UIGameClientTourist.GameLogic
{
    public class MusicService
    {
        private static MusicService _instance;
        private WaveOutEvent _waveOut;
        private AudioFileReader _audioFileReader;

        private MusicService()
        {
            _waveOut = new WaveOutEvent();
            _audioFileReader = new AudioFileReader("D:\\Proyectos .NET\\UIGameClientTourist\\GameResources\\Music\\utomp3.com-One-Piece-OST-Overtaken-EPIC-VERSION-Drums-of-Liberation_320kbps.wav");
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
                // Iniciar la reproducción desde el principio
                _audioFileReader.Position = 0;
                _waveOut.Play();
            }
            else if (_waveOut.PlaybackState == PlaybackState.Paused)
            {
                // Reanudar la reproducción
                _waveOut.Play();
            }
        }

        public void PauseMusic()
        {
            if (_waveOut.PlaybackState == PlaybackState.Playing)
            {
                // Pausar la reproducción
                _waveOut.Pause();
            }
        }

        public void StopMusic()
        {
            if (_waveOut.PlaybackState != PlaybackState.Stopped)
            {
                // Detener la reproducción
                _waveOut.Stop();
                _audioFileReader.Position = 0;
            }
        }

        public void SetVolume(double volume)
        {
            // Asegúrate de que el valor de volumen esté en el rango adecuado (0 a 1)
            if (volume < 0)
                volume = 0;
            else if (volume > 1)
                volume = 1;

            // Establece el volumen en el reproductor de música
            _waveOut.Volume = (float)volume;
        }
    }
}
