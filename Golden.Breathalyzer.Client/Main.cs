using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CitizenFX;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;

namespace Golden.Breathalyzer.Client
{
    public class Main : BaseScript
    {
		public Main()
        {
			Tick += OnTick;
		}

        public bool isShowing = false;
		public bool onLoad = false;
		public bool IsKeyboardShowing = false;
		public bool IsBreathalyzerShowing = false;

        #region Commands
		[Command("breathalyze", Restricted = false)]
		private void BreathalyzeCommand()
		{
			BreathalyzePed();
		}

		[Command("setbac", Restricted = false)]
		private void SetPlayerBACCommand()
		{
			SetBAC();
		}
        #endregion

        [EventHandler("Golden:Breathalyzer:Client:BreathalyzeClosestPlayer")]
		private void BreathalyzeClosestPlayer()
        {
			BreathalyzePed();
		}

		[EventHandler("Golden:Breathalyzer:Client:SendResultsToClient")]
		private void SendResultsToClient(string playerAlcoMultiplier)
		{
			ShowBreathUI(true, playerAlcoMultiplier);
		}

		private void BreathalyzePed()
		{
			var player = GetClosestPlayer();

			if (player == 0) return;

			TriggerServerEvent("Golden:Breathalyzer:Server:GetPlayerBACForEnforcer", LocalPlayer.ServerId, player);
		}

		[EventHandler("Golden:Breathalyzer:Client:BreathalyzePlayer")]
		private void BreathalyzePedEvent()
		{
			var player = GetClosestPlayer();

			if (player == 0) return;

			TriggerServerEvent("Golden:Breathalyzer:Server:GetPlayerBAC", LocalPlayer.ServerId, player);
		}

		[EventHandler("Golden:Breathalyzer:Client:BreathalyzeSubjectAnim")]
		private async void BreathalyzeSubjectAnim()
		{
			RequestAnimDict("switch@michael@smoking");

			while (!HasAnimDictLoaded("switch@michael@smoking"))
			{
				await Delay(0);
			}

			await Game.PlayerPed.Task.PlayAnimation("switch@michael@smoking", "michael_smoking_loop", 4f, 4f, 3000, (AnimationFlags)1, 0.595f);
		}

        [EventHandler("Golden:Breathalyzer:Server:SendPlayerBACToEdit")]
		private void SendPlayerBACToEdit(string bac)
        {
			AddTextEntry("PED_BAC_TITLE", "Ped BAC (0.00):");
			DisplayOnscreenKeyboard(1, "PED_BAC_TITLE", "", bac, "", "", "", 5);
			IsKeyboardShowing = true;
		}

		[EventHandler("Golden:Breathalyzer:Client:Notification")]
		private void Notification(string message)
		{
			SetNotificationTextEntry("STRING");
			AddTextComponentString(message);
			DrawNotification(true, false);
		}

		private void SetBAC()
        {
			TriggerServerEvent("Golden:Breathalyzer:Client:GetPlayerBACForEdit", LocalPlayer.ServerId);
		}

		private void ShowBreathUI(bool toggle, string bac)
        {
			IsBreathalyzerShowing = toggle;

			SendNuiMessage(Stringify(new
            {
                action = "showBreathUI",
                showUI = toggle,
                showBAC = bac
            }));
        }

		public int GetClosestPlayer()
		{
			Vector3 playerLoc = LocalPlayer.Character.Position;

			var allPlayers = Players;

			Player nearestPlayer = (from x in allPlayers where x != Game.Player && Vector3.Distance(x.Character.Position, playerLoc) <= 2f orderby Vector3.Distance(x.Character.Position, playerLoc) select x).FirstOrDefault();

			if (nearestPlayer != null && Entity.Exists(nearestPlayer.Character))
			{
				return nearestPlayer.ServerId;
			}

			Notification("No Player Nearby!");
			return 0;
		}

		private void DisplayHelpText(string text)
        {
			BeginTextCommandDisplayHelp("STRING");
			AddTextComponentSubstringPlayerName(text);
			EndTextCommandDisplayHelp(0, false, true, -1);
		}

		private async Task OnTick()
        {
			if (!onLoad)
            {
				await Delay(100);
				onLoad = true;
				TriggerServerEvent("Golden:Breathalyzer:Server:SetUpPlayer", LocalPlayer.ServerId);
            }

			await Delay(0);

			if (IsBreathalyzerShowing)
            {
				DisplayHelpText("~w~Press ~INPUT_CELLPHONE_CANCEL~ ~w~to hide the breathalyzer");

				if (IsControlJustPressed(0, 177))
                {
					ShowBreathUI(false, "0.00");
				}
			}

			if (IsKeyboardShowing)
            {
				Game.DisableAllControlsThisFrame(0);

				if (UpdateOnscreenKeyboard() == 1 && GetOnscreenKeyboardResult() != null)
                {
					IsKeyboardShowing = false;
					TriggerServerEvent("Golden:Breathalyzer:Server:SendUpdatedPlayerBAC", LocalPlayer.ServerId, GetOnscreenKeyboardResult());
				}
            }
        }

		[EventHandler("Golden:Breathalyzer:Client:OnPlayerDropped")]
		private void OnPlayerDropped()
		{
			TriggerServerEvent("Golden:Breathalyzer:Server:OnPlayerDropped", LocalPlayer.ServerId);
		}

		#region Stringify
		public static string Stringify(object data)
		{
			bool flag = data == null;
			string result;

			if (flag)
			{
				result = null;
			}
			else
			{
				string text = null;

				try
				{
					JsonSerializerSettings settings = new JsonSerializerSettings
					{
						ReferenceLoopHandling = ReferenceLoopHandling.Ignore
					};

					text = JsonConvert.SerializeObject(data, settings);
				}
				catch (Exception ex)
				{
					text = null;
					Debug.WriteLine($"[Error] {ex}");
				}

				result = text;
			}

			return result;
		}
		#endregion
	}
}
