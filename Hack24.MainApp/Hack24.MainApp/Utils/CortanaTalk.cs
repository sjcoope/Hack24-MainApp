using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UWPTextToSpeech;

namespace Hack24.MainApp.Utils
{
    public class CortanaTalk
    {
        private Authentication auth;
        private string accessToken;
        private string requestUri = "https://speech.platform.bing.com/synthesize";

        public CortanaTalk()
        {
            var file = File.ReadAllText("./Assets/SpeechServiceAPI.key");
            auth = new Authentication(file);

            try
            {
                accessToken = auth.GetAccessToken();
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public async Task<bool> Talk(string text)
        {
            try
            {

                var cortana = new Synthesize(new Synthesize.InputOptions()
                {
                    RequestUri = new Uri(requestUri),
                    // Text to be spoken.
                    Text = text,
                    VoiceType = Gender.Female,
                    // Refer to the documentation for complete list of supported locales.
                    Locale = "en-US",
                    // You can also customize the output voice. Refer to the documentation to view the different
                    // voices that the TTS service can output.
                    VoiceName = "Microsoft Server Speech Text to Speech Voice (en-US, ZiraRUS)",
                    // Service can return audio in different output format. 
                    OutputFormat = AudioOutputFormat.Riff16Khz16BitMonoPcm,
                    AuthorizationToken = "Bearer " + accessToken,
                });

                cortana.OnAudioAvailable += TTSUtils.StoreAudio;
                cortana.OnError += TTSUtils.ErrorHandler;
                cortana.Speak(CancellationToken.None).Wait();
                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
