using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Composition;

namespace UWPSpeechToText
{
    public class STT
    {
        public async Task<string> Listen()
        {
            MediaCapture MediaCapture = new MediaCapture();
            await MediaCapture.InitializeAsync();

            // MediaCapture.RecordLimitationExceeded += MediaCapture_RecordLimitationExceeded; *shrug*

            var localFolder = ApplicationData.Current.TemporaryFolder;
            var recordingName = $"{Guid.NewGuid()}.mp3";
            StorageFile file = await localFolder.CreateFileAsync(recordingName, CreationCollisionOption.ReplaceExisting);
            var _mediaRecording = await MediaCapture.PrepareLowLagRecordToStorageFileAsync(
                MediaEncodingProfile.CreateMp3(AudioEncodingQuality.High), file);
            await _mediaRecording.StartAsync();

            await Task.Run(async () =>
            {
                await Task.Delay(TimeSpan.FromSeconds(10));
                await _mediaRecording.StopAsync();
            });

            await _mediaRecording.FinishAsync();

            return "done";
        }
    }
}
