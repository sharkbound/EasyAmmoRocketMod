using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EasyAmmo
{
    class CommandDropAmmo : IRocketCommand
    {
        public List<string> Aliases
        {
            get { return new List<string> { "dammo", "da", "dropa" }; }
        }

        public AllowedCaller AllowedCaller
        {
            get { return Rocket.API.AllowedCaller.Player; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            ushort ammoAmountToSpawn = (ushort)0;
            bool EnteredAmount = false;
            SDG.Unturned.ItemGunAsset currentWeapon;
            SDG.Unturned.ItemAsset currentEquiped;
            UnturnedPlayer Uplayer = (UnturnedPlayer)caller;

            if (command.Length >= 1)
            {
                if (ushort.TryParse(command[0], out ammoAmountToSpawn))
                {
                    EnteredAmount = true;
                }
            }

            currentEquiped = Uplayer.Player.equipment.asset;
            if (currentEquiped == null)
            {
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("nothing_equipped"));
                return;
            }
            if (currentEquiped.type != SDG.Unturned.EItemType.GUN)
            {
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("no_gun_equipped"));
                return;
            }

            //UnturnedChat.Say(caller, " your current equipped item is \" id: " + currentEquiped + " / " + "name: " + currentEquiped.name);
            //UnturnedChat.Say(caller, "item type: " + item.GetType().ToString());

            currentWeapon = (SDG.Unturned.ItemGunAsset)currentEquiped;
            if (currentWeapon == null)
            {
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("gun_asset_not_found"));
                return;
            }

            if (EasyAmmo.CheckIfBlacklisted(caller, currentWeapon))
            {
                return;
            }

            if (EnteredAmount && caller.HasPermission("dropammo.amount"))
            {
                if (EasyAmmo.Instance.Configuration.Instance.ClipLimitEnabled)
                {
                    DropMagsWithLimit(ammoAmountToSpawn, caller, currentWeapon, Uplayer, command);
                }
                else
                {
                    DropMags(ammoAmountToSpawn, caller, currentWeapon, Uplayer, command);
                }
            }
            else
            {
                DropMags((ushort)1, caller, currentWeapon, Uplayer, command);
            }
        }

        public string Help
        {
            get { return "drops the specified number of clips for your currently equipped weapon."; }
        }

        public string Name
        {
            get { return "dropammo"; }
        }

        public List<string> Permissions
        {
            get { return new List<string> { "dropammo" }; }
        }

        public string Syntax
        {
            get { return "(amount of ammo)"; }
        }

        public void DropMags(ushort ammoAmountToSpawn, IRocketPlayer caller, SDG.Unturned.ItemGunAsset currentWeapon, UnturnedPlayer Uplayer, string[] command)
        {
            UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("dropping_mags", ammoAmountToSpawn.ToString(), UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command)).name, GetMagId(Uplayer, currentWeapon, command).ToString()));

            for (int ii = 0; ii < (int)ammoAmountToSpawn; ii++)
            {
                ItemManager.dropItem(new Item(GetMagId(Uplayer, currentWeapon, command), true), Uplayer.Position, true, true, true);
            }
        }

        public void DropMagsWithLimit(ushort ammoAmountToSpawn, IRocketPlayer caller, SDG.Unturned.ItemGunAsset currentWeapon, UnturnedPlayer Uplayer, string[] command)
        {
            if (ammoAmountToSpawn <= (ushort)EasyAmmo.Instance.Configuration.Instance.ClipLimit || caller.HasPermission("easyammo.bypasslimit"))
            {
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("dropping_mags", ammoAmountToSpawn.ToString(), UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command)).name, GetMagId(Uplayer, currentWeapon, command).ToString()));

                for (int ii = 0; ii < (int)ammoAmountToSpawn; ii++)
                {
                    ItemManager.dropItem(new Item(GetMagId(Uplayer, currentWeapon, command), true), Uplayer.Position, true, true, true);
                }
            }
            else
            {
                UnturnedItems.GetItemAssetById(1);
                ushort amountoverlimit = ammoAmountToSpawn;
                ammoAmountToSpawn = (ushort)EasyAmmo.Instance.Configuration.Instance.ClipLimit;

                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("over_clip_spawn_limit_dropping", amountoverlimit.ToString(), EasyAmmo.Instance.Configuration.Instance.ClipLimit, UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command)).name, GetMagId(Uplayer, currentWeapon, command).ToString()));

                for (int ii = 0; ii < (int)ammoAmountToSpawn; ii++)
                {
                    ItemManager.dropItem(new Item(GetMagId(Uplayer, currentWeapon, command), true), Uplayer.Position, true, true, true);
                }
                
            }
        }

        public ushort GetMagId(UnturnedPlayer player, SDG.Unturned.ItemGunAsset gun, string[] command)
        {
            ushort magId = 0;

            if (command.Length == 2 || command.Length == 1)
            {
                if (command.Length == 1)
                {
                    if (command[0].ToLower() == "c")
                    {
                        magId = player.Player.equipment.state[8];
                    }
                }
                else if (command.Length == 2)
                {
                    if (command[1].ToLower() == "c")
                    {
                        magId = player.Player.equipment.state[8];
                    }
                }
            }

            if (magId == 0 || UnturnedItems.GetItemAssetById(magId).type != EItemType.MAGAZINE)
            {
                magId = gun.getMagazineID();
            }

            return magId;
        }
    }
}
