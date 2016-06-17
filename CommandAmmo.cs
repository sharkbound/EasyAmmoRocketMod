using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyAmmoRocketMod
{
    class CommandAmmo : IRocketCommand
    {
        public List<string> Aliases
        {
            get { return new List<string> { }; }
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

           if (command.Length == 1)
            {
                if (ushort.TryParse(command[0], out ammoAmountToSpawn))
                {
                    EnteredAmount = true;
                }
            }

            currentEquiped = Uplayer.Player.equipment.asset;
            if (currentEquiped == null)
            {
                UnturnedChat.Say(caller, "you dont have anything equipped!");
                return;
            }
            if (currentEquiped.ItemType != SDG.Unturned.EItemType.GUN )
            {
                UnturnedChat.Say(caller, "you dont have a gun equipped currently!");
                return;
            }

            //UnturnedChat.Say(caller, " your current equipped item is \" id: " + currentEquiped + " / " + "name: " + currentEquiped.name);
            //UnturnedChat.Say(caller, "item type: " + item.GetType().ToString());

            currentWeapon = (SDG.Unturned.ItemGunAsset)currentEquiped;
            if (currentWeapon == null)
            {
                UnturnedChat.Say(caller, "Gun asset is not found!!");
                return;
            }

            if (EnteredAmount)
            {
                if (EasyAmmo.Instance.Configuration.Instance.ClipLimitEnabled)
                {
                    SpawnMagsWithLimit(ammoAmountToSpawn, caller, currentWeapon, Uplayer);
                }
                else
                {
                    SpawnMags(ammoAmountToSpawn, caller, currentWeapon, Uplayer);
                }
            }
            else
            {
                SpawnMags((ushort)1, caller, currentWeapon, Uplayer);
            }
        }

        public string Help
        {
            get { return "return gives you the specified number of clips for yuor current weapon."; }
        }

        public string Name
        {
            get { return "ammo"; }
        }

        public List<string> Permissions
        {
            get { return new List<string>{"ammo"}; }
        }

        public string Syntax
        {
            get { return "<amount of ammo>"; }
        }

        public void SpawnMags(ushort ammoAmountToSpawn, IRocketPlayer caller, SDG.Unturned.ItemGunAsset currentWeapon, UnturnedPlayer Uplayer)
        {
            if (Uplayer.GiveItem(currentWeapon.magazineID, (byte)ammoAmountToSpawn))
            {
                UnturnedChat.Say(caller, "Giving you " + ammoAmountToSpawn.ToString() + " of " + UnturnedItems.GetItemAssetById(currentWeapon.magazineID).Name);
            }
            else
            {
                 UnturnedChat.Say(caller, "Failed to spawn a magazine for the gun your holding!");
            }
        }

        public void SpawnMagsWithLimit(ushort ammoAmountToSpawn, IRocketPlayer caller, SDG.Unturned.ItemGunAsset currentWeapon, UnturnedPlayer Uplayer)
        {
            if (ammoAmountToSpawn <= (ushort)EasyAmmo.Instance.Configuration.Instance.ClipLimit || caller.HasPermission("easyammo.bypasslimit"))
            {
                if (Uplayer.GiveItem(currentWeapon.magazineID, (byte)ammoAmountToSpawn))
                {
                    UnturnedChat.Say(caller, "Giving you " + ammoAmountToSpawn.ToString() + " of " + UnturnedItems.GetItemAssetById(currentWeapon.magazineID).Name);
                }
                else
                {
                    UnturnedChat.Say(caller, "Failed to spawn a magazine for the gun your holding!");
                } 
            }
            else 
            {
                ushort amountoverlimit = ammoAmountToSpawn;
                ammoAmountToSpawn = (ushort)EasyAmmo.Instance.Configuration.Instance.ClipLimit;

                if (Uplayer.GiveItem(currentWeapon.magazineID, (byte)ammoAmountToSpawn))
                {
                    UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("over_clip_spawn_limit", amountoverlimit.ToString(), EasyAmmo.Instance.Configuration.Instance.ClipLimit, UnturnedItems.GetItemAssetById(currentWeapon.magazineID).Name));
                }
                else
                {
                     UnturnedChat.Say(caller, "Failed to spawn a magazine for the gun your holding!");
                } 
            }
        }
    }
}
