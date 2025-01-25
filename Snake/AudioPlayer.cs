using System;
using System.IO;
using System.Reflection;
using System.Windows.Media;

namespace Snake
{
    public class AudioPlayer
    {
        private MediaPlayer _mediaPlayer;

        public AudioPlayer()
        {
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.MediaEnded += MediaPlayer_MediaEnded; // To handle media end event
        }

        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            // Reset the media player when the audio finishes
            _mediaPlayer.Stop();
            string tempFilePath = _mediaPlayer.Source.AbsolutePath;
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

        public void PlayAudio(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream($"Snake.Audio.{resourceName}");

            if (resourceStream == null)
            {
                throw new InvalidOperationException($"Audio resource {resourceName} not found!");
            }

            // Save the resource to a temporary file
            string tempFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".mp3");
            using (var fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write))
            {
                resourceStream.CopyTo(fileStream);
            }

            // Play the audio from the temporary file
            _mediaPlayer.Open(new Uri(tempFilePath));
            _mediaPlayer.Play();
        }

        public void StopAudio()
        {
            _mediaPlayer.Stop();
            string tempFilePath = _mediaPlayer.Source.AbsolutePath;
            if (File.Exists(tempFilePath))
            {
                File.Delete(tempFilePath);
            }
        }

        public void Pause()
        {
            _mediaPlayer.Pause();
        }

        public void Resume()
        {
            _mediaPlayer.Play();
        }
    }
}
