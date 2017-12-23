using System;
using DeadlockDetector.Detector;
using Terraria;
using TerrariaApi.Server;

namespace DeadlockDetector
{
    [ApiVersion(2, 1)]
    public sealed class DdPlugin : TerrariaPlugin
    {
        public override string Name => GetType().Namespace;

        public override string Author => "MistZZT";

        public override string Description => GetType().Namespace;

        public override Version Version => GetType().Assembly.GetName().Version;

        public DdPlugin(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.GamePostInitialize.Register(this, OnPostInit);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GamePostInitialize.Deregister(this, OnPostInit);

                _checker.Dispose();
            }
            base.Dispose(disposing);
        }

        private void OnPostInit(EventArgs args)
        {
            _checker = new Checker(new ConnectionDetector());
        }

        private Checker _checker;
    }
}