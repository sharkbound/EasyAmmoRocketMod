using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fr34kyn01535.Uconomy;
using SDG.Unturned;

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
                        SpawnMagsWithLimit_Uconomy(ammoAmountToSpawn, caller, currentWeapon, Uplayer, command); 
                    }
                    else
                    {
                        SpawnMagsWithLimit(ammoAmountToSpawn, caller, currentWeapon, Uplayer, command); 

                    }
                }
                else
                {
                    if (EasyAmmo.Instance.Configuration.Instance.UconomySupportEnabled)
                    {
                        SpawnMagsWithLimit_Uconomy(ammoAmountToSpawn, caller, currentWeapon, Uplayer, command);
                    }
                    else
                    {
                        SpawnMags(ammoAmountToSpawn, caller, currentWeapon, Uplayer, command);
                    }
                }
            }
            else
            {
                if (EasyAmmo.Instance.Configuration.Instance.UconomySupportEnabled)
                {
                    SpawnMagsWithLimit_Uconomy((ushort)1, caller, currentWeapon, Uplayer, command);
                }
                else
                {
                    SpawnMags((ushort)1, caller, currentWeapon, Uplayer, command);
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

        public void SpawnMags(ushort ammoAmountToSpawn, IRocketPlayer caller, SDG.Unturned.ItemGunAsset currentWeapon, UnturnedPlayer Uplayer, string[] command)
        {
            if (Uplayer.GiveItem(GetMagId(Uplayer, currentWeapon, command), (byte)ammoAmountToSpawn))
            {
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("giving_mags", ammoAmountToSpawn.ToString(), UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command)).Name, GetMagId(Uplayer, currentWeapon, command).ToString()));
            }
            else
            {
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("failed_to_spawn_mags"));
            }
        }

        public void SpawnMagsWithLimit(ushort ammoAmountToSpawn, IRocketPlayer caller, SDG.Unturned.ItemGunAsset currentWeapon, UnturnedPlayer Uplayer, string[] command)
        {
            if (ammoAmountToSpawn <= (ushort)EasyAmmo.Instance.Configuration.Instance.ClipLimit || caller.HasPermission("easyammo.bypasslimit"))
            {
                if (Uplayer.GiveItem(GetMagId(Uplayer, currentWeapon, command), (byte)ammoAmountToSpawn))
                {
                    UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("giving_mags", ammoAmountToSpawn.ToString(), UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command)).Name, GetMagId(Uplayer, currentWeapon, command).ToString()));
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

                if (Uplayer.GiveItem(GetMagId(Uplayer, currentWeapon, command), (byte)ammoAmountToSpawn))
                {
                    UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("over_clip_spawn_limit_giving", amountoverlimit.ToString(), EasyAmmo.Instance.Configuration.Instance.ClipLimit, UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command)).Name, GetMagId(Uplayer, currentWeapon, command).ToString()));
                }
                else
                {
                     UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("failed_to_spawn_mags"));
                } 
            }
        }
        
        public void SpawnMagsWithLimit_Uconomy(ushort ammoAmountToSpawn, IRocketPlayer caller, SDG.Unturned.ItemGunAsset currentWeapon, UnturnedPlayer Uplayer, string[] command)
        {
            int costMultiplier = EasyAmmo.Instance.Configuration.Instance.PerBulletCostMultiplier;
            SDG.Unturned.ItemMagazineAsset magazine = (SDG.Unturned.ItemMagazineAsset)UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command));

            if (ammoAmountToSpawn <= (ushort)EasyAmmo.Instance.Configuration.Instance.ClipLimit || caller.HasPermission("easyammo.bypasslimit"))
            {
                if (Uconomy.Instance.Database.GetBalance(caller.Id) >= GetCost(false, ammoAmountToSpawn, currentWeapon, magazine))
                {
                    if (Uplayer.GiveItem(GetMagId(Uplayer, currentWeapon, command), (byte)ammoAmountToSpawn))
                    {
                        Uconomy.Instance.Database.IncreaseBalance(caller.Id, GetCost(true, ammoAmountToSpawn, currentWeapon, magazine));

                        UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("giving_mags", ammoAmountToSpawn.ToString(), UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command)).Name, GetMagId(Uplayer, currentWeapon, command).ToString()));
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
                    if (Uplayer.GiveItem(GetMagId(Uplayer, currentWeapon, command), (byte)ammoAmountToSpawn))
                    {
                        Uconomy.Instance.Database.IncreaseBalance(caller.Id, GetCost(true, ammoAmountToSpawn, currentWeapon, magazine));

                        UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("over_clip_spawn_limit_giving", amountoverlimit.ToString(),
                            EasyAmmo.Instance.Configuration.Instance.ClipLimit, UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command)).Name,
                            GetMagId(Uplayer, currentWeapon, command).ToString()));
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

            //Logger.Log(cost.ToString());
            return cost;
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
                magId = gun.magazineID;
            }

            return magId;
        }
    }
}
