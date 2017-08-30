using System;
using System.Threading;
using DeadlockDetector.Detector;
using Terraria.IO;

namespace DeadlockDetector
{
    public sealed class Checker : IDisposable
    {
        public Timer Timer { get; }

        public Checker(DdPlugin plugin, IDetector detector)
        {
            _plugin = plugin;
            _detector = detector;
            Timer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(2));
        }

        private void TimerCallback(object state)
        {
            var shouldRestartServer = Check();
            TShockAPI.TShock.Log.ConsoleInfo("Check status: " + shouldRestartServer);
            if (!shouldRestartServer) return;
            
            TShockAPI.TShock.Log.ConsoleError("开始重启服务器……");
            WorldFile.saveWorld(false);
            Environment.Exit(1);
        }

        private bool Check()
        {
            return _detector.Detect();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Timer.Dispose();

            _disposed = true;
        }

        private bool _disposed;

        private readonly DdPlugin _plugin;
        
        private readonly IDetector _detector;
    }
}