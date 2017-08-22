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
            Timer = new Timer(TimerCallback, null, TimeSpan.FromMinutes(20), TimeSpan.FromMinutes(10));

            ServerApi.Hooks.GameUpdate.Register(_plugin, OnUpdate);
        }

        private void OnUpdate(EventArgs args)
        {
            _lastUpdateTime = DateTime.Now;
        }

        private void TimerCallback(object state)
        {
            var shouldRestartServer = Check();
            if (!shouldRestartServer) return;

            WorldFile.saveWorld(false);
            Environment.Exit(1);
        }

        private bool Check()
        {
            return (DateTime.Now - _lastUpdateTime).Seconds > 60;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            ServerApi.Hooks.GameUpdate.Deregister(_plugin, OnUpdate);

            Timer.Dispose();

            _disposed = true;
        }

        private bool _disposed;

        private DateTime _lastUpdateTime;

        private readonly DdPlugin _plugin;
    }
}