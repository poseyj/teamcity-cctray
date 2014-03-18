using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SirenOfShame.Lib.Device;
using SirenOfShame.Lib.Helpers;

namespace TeamCityExtensionTests
{
    [TestClass, Ignore]
    public class SirenOfShameIntegrationTests
    {
        [TestMethod]
        public void PlayAudio()
        {
            var seconds = 10;

            ISirenOfShameDevice sos = new SirenOfShameDevice();
            sos.TryConnect();
            var pattern = new AudioPattern();
            pattern.Id = 1;
            var duration = new TimeSpan(0, 0, 0, seconds);
            sos.PlayAudioPattern(pattern, duration);
        }

        [TestMethod]
        public void PlayLed()
        {
            var seconds = 10;

            ISirenOfShameDevice sos = new SirenOfShameDevice();
            sos.TryConnect();
            var ledPattern = new LedPattern();
            ledPattern.Id = 12;

            var duration = new TimeSpan(0, 0, 0, seconds);
            sos.PlayLightPattern(ledPattern, duration);
        }

        [TestMethod]
        public void GetAudioPatterns()
        {
            ISirenOfShameDevice sos = new SirenOfShameDevice();
            sos.TryConnect();
            var patterns = sos.AudioPatterns;
            foreach (var pattern in patterns)
            {
                Console.Out.WriteLine("pattern = {0} {1}", pattern.Id, pattern.Name);
                var b = pattern.SerializeToBytes();
                var _FileStream =
                       new FileStream("d:\\sos.wav", FileMode.Create,
                                                FileAccess.Write);
                // Writes a block of bytes to this stream using data from
                // a byte array.
                _FileStream.Write(b, 0, b.Length);

                // close file stream
                _FileStream.Close();
            }

        }

        [TestMethod]
        public void GetLedPatterns()
        {
            ISirenOfShameDevice sos = new SirenOfShameDevice();
            sos.TryConnect();
            var patterns = sos.LedPatterns;
            foreach (var ledPattern in patterns)
            {
                Console.Out.WriteLine("ledPattern = {0} {1}", ledPattern.Id, ledPattern.Name);
            }
        }

        [TestMethod]
        public void ReadDeviceInfo()
        {
            ISirenOfShameDevice sos = new SirenOfShameDevice();
            sos.TryConnect();
            if(!sos.IsConnected) Console.Out.WriteLine("not connected");
            var deviceInfo = sos.ReadDeviceInfo();
            Console.Out.WriteLine("device info = {0}", deviceInfo);
        }

        [TestMethod]
        public void IsConnected()
        {
            ISirenOfShameDevice sos = new SirenOfShameDevice();
            Assert.IsTrue(sos.TryConnect());
        }
    }
}
