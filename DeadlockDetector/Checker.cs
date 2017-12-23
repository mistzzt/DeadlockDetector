using System;
using System.Threading;
using DeadlockDetector.Detector;
using Terraria.IO;

namespace DeadlockDetector
{
    public sealed class Checker : IDisposable
    {
        public Timer Timer { get; }

        public Checker(IDetector detector)
        {
            _detector = detector;
            Timer = new Timer(TimerCallback, null, TimeSpan.FromSeconds(5), TimeSpan.FromMinutes(5));
        }

        private void TimerCallback(object state)
        {
            var shouldRestartServer = Check();
            TShockAPI.TShock.Log.ConsoleInfo("Deadlock: " + shouldRestartServer);
            if (!shouldRestartServer) return;
            
            TShockAPI.TShock.Log.ConsoleError("Server starts terminating...");
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

        private readonly IDetector _detector;
    }
}