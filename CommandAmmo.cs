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
using fr34kyn01535.Uconomy;

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
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("nothing_equipped"));
                return;
            }
            if (currentEquiped.ItemType != SDG.Unturned.EItemType.GUN )
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

            if (EnteredAmount && caller.HasPermission("ammo.amount"))
            {
                if (EasyAmmo.Instance.Configuration.Instance.ClipLimitEnabled)
                { 
                    if (EasyAmmo.Instance.Configuration.Instance.UconomySupportEnabled)
                    {
                        SpawnMagsWithLimit_Uconomy(ammoAmountToSpawn, caller, currentWeapon, Uplayer); 
                    }
                    else
                    {
                        SpawnMagsWithLimit(ammoAmountToSpawn, caller, currentWeapon, Uplayer); 

                    }
                }
                else
                {
                    if (EasyAmmo.Instance.Configuration.Instance.UconomySupportEnabled)
                    {
                        SpawnMagsWithLimit_Uconomy(ammoAmountToSpawn, caller, currentWeapon, Uplayer);
                    }
                    else
                    {
                        SpawnMags(ammoAmountToSpawn, caller, currentWeapon, Uplayer);
                    }
                }
            }
            else
            {
                if (EasyAmmo.Instance.Configuration.Instance.UconomySupportEnabled)
                {
                    SpawnMagsWithLimit_Uconomy((ushort)1, caller, currentWeapon, Uplayer);
                }
                else
                {
                    SpawnMags((ushort)1, caller, currentWeapon, Uplayer);
                }
            }
        }

        public string Help
        {
            get { return "gives you the specified number of clips for your currently equipped weapon."; }
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
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("giving_mags", ammoAmountToSpawn.ToString(), UnturnedItems.GetItemAssetById(currentWeapon.magazineID).Name, currentWeapon.magazineID.ToString()));
            }
            else
            {
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("failed_to_spawn_mags"));
            }
        }

        public void SpawnMagsWithLimit(ushort ammoAmountToSpawn, IRocketPlayer caller, SDG.Unturned.ItemGunAsset currentWeapon, UnturnedPlayer Uplayer)
        {
            if (ammoAmountToSpawn <= (ushort)EasyAmmo.Instance.Configuration.Instance.ClipLimit || caller.HasPermission("easyammo.bypasslimit"))
            {
                if (Uplayer.GiveItem(currentWeapon.magazineID, (byte)ammoAmountToSpawn))
                {
                        UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("giving_mags", ammoAmountToSpawn.ToString(), UnturnedItems.GetItemAssetById(currentWeapon.magazineID).Name, currentWeapon.magazineID.ToString()));
                }
                else
                {
                    UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("failed_to_spawn_mags"));
                } 
            }
            else 
            {
                ushort amountoverlimit = ammoAmountToSpawn;
                ammoAmountToSpawn = (ushort)EasyAmmo.Instance.Configuration.Instance.ClipLimit;

                if (Uplayer.GiveItem(currentWeapon.magazineID, (byte)ammoAmountToSpawn))
                {
                    UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("over_clip_spawn_limit_giving", amountoverlimit.ToString(), EasyAmmo.Instance.Configuration.Instance.ClipLimit, UnturnedItems.GetItemAssetById(currentWeapon.magazineID).Name, currentWeapon.magazineID.ToString()));
                }
                else
                {
                     UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("failed_to_spawn_mags"));
                } 
            }
        }
        
        public void SpawnMagsWithLimit_Uconomy(ushort ammoAmountToSpawn, IRocketPlayer caller, SDG.Unturned.ItemGunAsset currentWeapon, UnturnedPlayer Uplayer)
        {
            int costMultiplier = EasyAmmo.Instance.Configuration.Instance.PerBulletCostMultiplier;
            SDG.Unturned.ItemMagazineAsset magazine = (SDG.Unturned.ItemMagazineAsset)UnturnedItems.GetItemAssetById(currentWeapon.magazineID);

            if (ammoAmountToSpawn <= (ushort)EasyAmmo.Instance.Configuration.Instance.ClipLimit || caller.HasPermission("easyammo.bypasslimit"))
            {
                //if (Uconomy.Instance.Database.GetBalance(caller.Id) >= (magazine.Amount * ammoAmountToSpawn) * costMultiplier)
                if (Uconomy.Instance.Database.GetBalance(caller.Id) >= GetCost(false, ammoAmountToSpawn, currentWeapon, magazine))
                {
                    if (Uplayer.GiveItem(currentWeapon.magazineID, (byte)ammoAmountToSpawn))
                    {
                        Uconomy.Instance.Database.IncreaseBalance(caller.Id, GetCost(true, ammoAmountToSpawn, currentWeapon, magazine));

                        UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("giving_mags", ammoAmountToSpawn.ToString(), UnturnedItems.GetItemAssetById(currentWeapon.magazineID).Name, currentWeapon.magazineID.ToString()));
                        UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("balance", Uconomy.Instance.Database.GetBalance(caller.Id)));
                    }
                    else
                    {
                        UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("failed_to_spawn_mags"));
                    } 
                }
                else
                {
                    UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("not_enough_funds", 
                        Uconomy.Instance.Configuration.Instance.MoneyName,
                        ammoAmountToSpawn, magazine.itemName, GetCost(false, ammoAmountToSpawn, currentWeapon, magazine).ToString()));
                }
            }
            else
            {
                ushort amountoverlimit = ammoAmountToSpawn;
                ammoAmountToSpawn = (ushort)EasyAmmo.Instance.Configuration.Instance.ClipLimit;

                if (Uconomy.Instance.Database.GetBalance(caller.Id) >= GetCost(false, ammoAmountToSpawn, currentWeapon, magazine))
                {
                    if (Uplayer.GiveItem(currentWeapon.magazineID, (byte)ammoAmountToSpawn))
                    {
                        Uconomy.Instance.Database.IncreaseBalance(caller.Id, GetCost(true, ammoAmountToSpawn, currentWeapon, magazine));

                        UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("over_clip_spawn_limit_giving", amountoverlimit.ToString(), 
                            EasyAmmo.Instance.Configuration.Instance.ClipLimit, UnturnedItems.GetItemAssetById(currentWeapon.magazineID).Name, 
                            currentWeapon.magazineID.ToString()));
                        UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("balance", Uconomy.Instance.Database.GetBalance(caller.Id)));
                    }
                    else
                    {
                        UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("failed_to_spawn_mags"));
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("not_enough_funds",
                        Uconomy.Instance.Configuration.Instance.MoneyName,
                        ammoAmountToSpawn, magazine.itemName, GetCost(false, ammoAmountToSpawn, currentWeapon, magazine).ToString()));
                }
            }
        }

        public decimal GetCost(bool Negative, int amount, SDG.Unturned.ItemGunAsset gun, SDG.Unturned.ItemMagazineAsset magazine)
        {
            decimal cost;
            if (EasyAmmo.Instance.Configuration.Instance.ScaleCostByWeaponDamage)
            {
                cost = (decimal)(gun.playerDamageMultiplier.damage * amount) *
                    EasyAmmo.Instance.Configuration.Instance.WeaponDamageCostMultiplier;
            }
            else
            {
                cost = (decimal)(magazine.Amount * amount) * 
                    EasyAmmo.Instance.Configuration.Instance.PerBulletCostMultiplier;
            }

            if (Negative)
            {
                cost = -cost;
            }

            Logger.Log(cost.ToString());
            return cost;
        }
    }
}
