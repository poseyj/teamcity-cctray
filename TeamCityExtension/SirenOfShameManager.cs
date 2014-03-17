//using System;
//using SirenOfShame.Lib.Device;

//namespace TeamCityExtension
//{
//    public interface ISirenOfShameManager
//    {
//        void Building();
//        void AllBuildsGood();
//    }

//    public class SirenOfShameManager : ISirenOfShameManager
//    {
//        private readonly ISirenOfShameDevice _device;

//        public SirenOfShameManager(ISirenOfShameDevice device)
//        {
//            this._device = device;
//        }

//        public void Building()
//        {
//            var seconds = 30;
//            var ledPattern = new LedPattern();
//            ledPattern.Id = 12;

//            var duration = new TimeSpan(0, 0, 0, seconds);
//            if (_device.TryConnect())
//            {
//                _device.PlayLightPattern(ledPattern, duration);
//            }
//        }

//        public void AllBuildsGood()
//        {
//            if (_device.TryConnect())
//            {
//                _device.StopLightPattern();
//            }
//        }
//    }
//}
