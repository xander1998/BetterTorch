using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using CitizenFX.Core.UI;

namespace BetterTorch.Client
{
    public class Main : BaseScript
    {
        // Animation Dictionary
        private readonly Dictionary<string, dynamic> animations = new Dictionary<string, dynamic>()
        {
            ["equip"] = new { dict = "amb@incar@male@patrol@torch@base", name = "base" },
            ["store"] = new { dict = "amb@incar@male@patrol@torch@exit", name = "exit" }
        };

        // Flashlight Equipped
        private bool isEquipped = false;

        // Synced Clients
        private List<Player> SyncedClients = new List<Player>();

        // Constructor
        public Main()
        {
            Tick += TrackWeapons;
            EventHandlers.Add("BetterTorch:AddSyncedClient", new Action<string>(Event_AddSyncedClient));
            EventHandlers.Add("BetterTorch:RemoveSyncedClient", new Action<string>(Event_RemoveSyncedClient));

            // Register Decors
            API.DecorRegister("FlashlightOn", 2);
        }

        // EventHandler - Add Synced Client
        private void Event_AddSyncedClient(string _id)
        {
            Player newClient = new PlayerList()[Convert.ToInt32(_id)];
            SyncedClients.Add(newClient);
            if (SyncedClients.Count == 1)
            {
                Tick += DrawSyncedLights;
            }

            // Pos Sync
            Prop weaponObject = newClient.Character.Weapons.CurrentWeaponObject;
            weaponObject.Detach();
            weaponObject.AttachTo(newClient.Character.Bones[Bone.SKEL_R_Hand], new Vector3(0.125f, 0.07f, -0.03f), new Vector3(95f, -25f, 0f));
        }

        // EventHandler - Remove Synced Client
        private void Event_RemoveSyncedClient(string _id)
        {
            Player newClient = new PlayerList()[Convert.ToInt32(_id)];
            SyncedClients.Remove(newClient);

            if (SyncedClients.Count == 0)
            {
                Tick -= DrawSyncedLights;
            }
        }

        // Wathching Weapon Changes
        private async Task TrackWeapons()
        {
            if (Game.Player.Character.Weapons.Current == WeaponHash.Flashlight && !isEquipped)
            {
                EquipFlashlight();
            }
            if (Game.Player.Character.Weapons.Current != WeaponHash.Flashlight && isEquipped)
            {
                StoreFlashlight();
            }
            await Delay(500);
        }

        // Equipping Flashlight
        private async void EquipFlashlight()
        {
            Tick += FlashlightHandler;
            await Game.Player.Character.Task.PlayAnimation(animations["equip"].dict, animations["equip"].name, 8f, -8f, -1, AnimationFlags.AllowRotation | AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly, -8f);

            // Set Equipped
            isEquipped = true;

            await Delay(250);
            TriggerServerEvent("BetterTorch:PassSyncedClient");
        }

        // Holstering Flashlight
        private async void StoreFlashlight()
        {
            await Game.Player.Character.Task.PlayAnimation(animations["store"].dict, animations["store"].name, 3f, -3f, -1, AnimationFlags.AllowRotation | AnimationFlags.StayInEndFrame | AnimationFlags.UpperBodyOnly, -3f);
            Game.Player.Character.Task.ClearAll();

            // Set UnEquipped
            Tick -= FlashlightHandler;
            isEquipped = false;
            TriggerServerEvent("BetterTorch:PassUnsyncedClient");
        }

        // Flashlight Controls
        private async Task FlashlightHandler()
        {
            // Disable Aim (Disables Default Light
            Game.DisableControlThisFrame(0, Control.Aim);
            if (Game.IsControlJustPressed(0, Control.Aim))
            {
                bool flashlightOn = API.DecorGetBool(Game.Player.Character.Handle, "FlashlightOn");
                API.DecorSetBool(Game.Player.Character.Handle, "FlashlightOn", !flashlightOn);
            }
            await Task.FromResult(0);
        }

        private async Task DrawSyncedLights()
        {
            foreach (Player client in SyncedClients)
            {
                bool flashlightOn = API.DecorGetBool(client.Character.Handle, "FlashlightOn");
                if (flashlightOn)
                {
                    Prop flashlight = client.Character.Weapons.CurrentWeaponObject;
                    if (flashlight != null)
                    {
                        Vector3 LightPosition = flashlight.GetOffsetPosition(new Vector3(0.2f, 0f, 0f));
                        Vector3 ForwardPosition = client.Character.ForwardVector;
                        World.DrawSpotLight(LightPosition, ForwardPosition, System.Drawing.Color.FromArgb(255, 255, 255), 100f, 5f, 25f, 15f, 50f);
                    }
                }
            }
            await Task.FromResult(0);
        }

    }
}
