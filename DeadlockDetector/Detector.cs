using System;
using System.Threading;
using Terraria.IO;
using TerrariaApi.Server;

namespace DeadlockDetector
{
    public sealed class Detector : IDisposable
    {
        public Timer Timer { get; }

        public Detector(DdPlugin plugin)
        {
            _plugin = plugin;
            Timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromMinutes(2));
        }

        private void TimerCallback(object state)
        {
            var shouldRestartServer = Check();
            TShockAPI.TShock.Log.ConsoleInfo("Check status: " + shouldRestartServer);
            if (!shouldRestartServer) return;

            WorldFile.saveWorld(false);
            Environment.Exit(1);
        }

        private bool Check()
        {
            
            
            return false;
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
    }
}