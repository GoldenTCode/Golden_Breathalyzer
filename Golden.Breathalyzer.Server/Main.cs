using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX.Core;
using CitizenFX.Core.Native;

namespace Golden.Breathalyzer.Server
{
    public class Main : BaseScript
    {
        public static Dictionary<string, Dictionary<int, string>> PlayerBACs = new Dictionary<string, Dictionary<int, string>>();

        public Main()
        {
            PlayerBACs.Add("players", new Dictionary<int, string>());
        }

        #region Player Connected
        [EventHandler("Golden:Breathalyzer:Server:SetUpPlayer")]
        private void SetupPlayer([FromSource] Player player, int playerId)
        {
            PlayerBACs["players"].Add(playerId, "0.00");
        }
        #endregion

        #region Update and Get BAC
        [EventHandler("Golden:Breathalyzer:Server:SendUpdatedPlayerBAC")]
        private void EditPedBAC([FromSource] Player player, int playerId, string bac)
        {
            PlayerBACs["players"][playerId] = bac;

            player.TriggerEvent("Golden:Breathalyzer:Client:Notification", $"You have set your players bac - {bac}!");
        }

        [EventHandler("Golden:Breathalyzer:Server:GetPlayerBACForEnforcer")]
        private async void GetPlayerBACEnforcer([FromSource] Player player, int enforcer, int breathalyzedPlayer)
        {
            if (breathalyzedPlayer == -1 || breathalyzedPlayer.ToString() == "-1") return;

            if (breathalyzedPlayer != enforcer)
            {
                string bac = PlayerBACs["players"][breathalyzedPlayer];

                player.TriggerEvent("Golden:Breathalyzer:Client:Notification", $"You have started breathalyzing {Players[breathalyzedPlayer].Name}!");
                Players[breathalyzedPlayer].TriggerEvent("Golden:Breathalyzer:Client:Notification", $"You have been breathalyzed by {Players[enforcer].Name}!");
                Players[breathalyzedPlayer].TriggerEvent("Golden:Breathalyzer:Client:BreathalyzeSubjectAnim");
                await Delay(3000);
                player.TriggerEvent("Golden:Breathalyzer:Client:SendResultsToClient", bac);
            }
        }

        [EventHandler("Golden:Breathalyzer:Client:GetPlayerBACForEdit")]
        private void GetPlayerBACForEdit([FromSource] Player player, int playerId)
        {
            string bac = PlayerBACs["players"][playerId];

            player.TriggerEvent("Golden:Breathalyzer:Server:SendPlayerBACToEdit", bac);
        }
        #endregion

        #region Player Dropped
        [EventHandler("playerDropped")]
        private async void OnPlayerDropped([FromSource] Player player, string reason)
        {
            await Delay(0);
            player.TriggerEvent("Golden:Breathalyzer:Client:OnPlayerDropped");
        }

        [EventHandler("Golden:Breathalyzer:Server:OnPlayerDropped")]
        private async void OnPlayerDroppedServer(int playerId)
        {
            await Delay(0);
            PlayerBACs["players"].Remove(playerId);
        }
        #endregion
    }
}
