using System;
using CitizenFX.Core;

namespace BetterTorch.Server
{
    public class Main : BaseScript
    {

        public Main()
        {
            EventHandlers.Add("BetterTorch:PassSyncedClient", new Action<Player>(PassSyncedClient));
            EventHandlers.Add("BetterTorch:PassUnsyncedClient", new Action<Player>(PassUnsyncedClient));
        }

        private void PassSyncedClient([FromSource] Player _player)
        {
            TriggerClientEvent("BetterTorch:AddSyncedClient", _player.Handle);
        }

        private void PassUnsyncedClient([FromSource] Player _player)
        {
            TriggerClientEvent("BetterTorch:RemoveSyncedClient", _player.Handle);
        }

    }
}
