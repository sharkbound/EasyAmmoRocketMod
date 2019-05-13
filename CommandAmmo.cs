using fr34kyn01535.Uconomy;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System.Collections.Generic;

namespace EasyAmmo
{
    class CommandAmmo : IRocketCommand
    {
        public List<string> Aliases => new List<string>();

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public void Execute(IRocketPlayer caller, string[] command)
        {
            ushort ammoAmountToSpawn = 0;
            bool enteredAmount = false;
            ItemGunAsset currentWeapon;
            ItemAsset currentEquiped;
            UnturnedPlayer uplayer = (UnturnedPlayer) caller;

            if (command.Length >= 1)
            {
                if (ushort.TryParse(command[0], out ammoAmountToSpawn))
                {
                    enteredAmount = true;
                }
            }

            currentEquiped = uplayer.Player.equipment.asset;
            if (currentEquiped == null)
            {
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("nothing_equipped"));
                return;
            }

            if (currentEquiped.type != EItemType.GUN)
            {
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("no_gun_equipped"));
                return;
            }

            //UnturnedChat.Say(caller, " your current equipped item is \" id: " + currentEquiped + " / " + "name: " + currentEquiped.name);
            //UnturnedChat.Say(caller, "item type: " + item.GetType().ToString());

            currentWeapon = (ItemGunAsset) currentEquiped;

            if (EasyAmmo.CheckIfBlacklisted(caller, currentWeapon))
            {
                return;
            }

            if (enteredAmount && caller.HasPermission("ammo.amount"))
            {
                if (EasyAmmo.Instance.Configuration.Instance.ClipLimitEnabled)
                {
                    if (EasyAmmo.Instance.Configuration.Instance.UconomySupportEnabled)
                    {
                        SpawnMagsWithLimit_Uconomy(ammoAmountToSpawn, caller, currentWeapon, uplayer, command);
                    }
                    else
                    {
                        SpawnMagsWithLimit(ammoAmountToSpawn, caller, currentWeapon, uplayer, command);
                    }
                }
                else
                {
                    if (EasyAmmo.Instance.Configuration.Instance.UconomySupportEnabled)
                    {
                        SpawnMagsWithLimit_Uconomy(ammoAmountToSpawn, caller, currentWeapon, uplayer, command);
                    }
                    else
                    {
                        SpawnMags(ammoAmountToSpawn, caller, currentWeapon, uplayer, command);
                    }
                }
            }
            else
            {
                if (EasyAmmo.Instance.Configuration.Instance.UconomySupportEnabled)
                {
                    SpawnMagsWithLimit_Uconomy(1, caller, currentWeapon, uplayer, command);
                }
                else
                {
                    SpawnMags(1, caller, currentWeapon, uplayer, command);
                }
            }
        }

        public string Help => "gives you the specified number of clips for your currently equipped weapon.";

        public string Name => "ammo";

        public List<string> Permissions => new List<string> {"ammo"};

        public string Syntax => "<amount of ammo>";

        public void SpawnMags(ushort ammoAmountToSpawn, IRocketPlayer caller, ItemGunAsset currentWeapon,
            UnturnedPlayer uplayer, string[] command)
        {
            if (uplayer.GiveItem(GetMagId(uplayer, currentWeapon, command), (byte) ammoAmountToSpawn))
            {
                UnturnedChat.Say(caller,
                    EasyAmmo.Instance.Translate("giving_mags", ammoAmountToSpawn.ToString(),
                        UnturnedItems.GetItemAssetById(GetMagId(uplayer, currentWeapon, command)).itemName,
                        GetMagId(uplayer, currentWeapon, command).ToString()));
            }
            else
            {
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("failed_to_spawn_mags"));
            }
        }

        public void SpawnMagsWithLimit(ushort ammoAmountToSpawn, IRocketPlayer caller, ItemGunAsset currentWeapon,
            UnturnedPlayer uplayer, string[] command)
        {
            if (ammoAmountToSpawn <= (ushort) EasyAmmo.Instance.Configuration.Instance.ClipLimit ||
                caller.HasPermission("easyammo.bypasslimit"))
            {
                if (uplayer.GiveItem(GetMagId(uplayer, currentWeapon, command), (byte) ammoAmountToSpawn))
                {
                    UnturnedChat.Say(caller,
                        EasyAmmo.Instance.Translate("giving_mags", ammoAmountToSpawn.ToString(),
                            UnturnedItems.GetItemAssetById(GetMagId(uplayer, currentWeapon, command)).itemName,
                            GetMagId(uplayer, currentWeapon, command).ToString()));
                }
                else
                {
                    UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("failed_to_spawn_mags"));
                }
            }
            else
            {
                ushort amountoverlimit = ammoAmountToSpawn;
                ammoAmountToSpawn = (ushort) EasyAmmo.Instance.Configuration.Instance.ClipLimit;

                if (uplayer.GiveItem(GetMagId(uplayer, currentWeapon, command), (byte) ammoAmountToSpawn))
                {
                    UnturnedChat.Say(caller,
                        EasyAmmo.Instance.Translate("over_clip_spawn_limit_giving", amountoverlimit.ToString(),
                            EasyAmmo.Instance.Configuration.Instance.ClipLimit,
                            UnturnedItems.GetItemAssetById(GetMagId(uplayer, currentWeapon, command)).itemName,
                            GetMagId(uplayer, currentWeapon, command).ToString()));
                }
                else
                {
                    UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("failed_to_spawn_mags"));
                }
            }
        }

        public void SpawnMagsWithLimit_Uconomy(ushort ammoAmountToSpawn, IRocketPlayer caller,
            ItemGunAsset currentWeapon, UnturnedPlayer Uplayer, string[] command)
        {
            int costMultiplier = EasyAmmo.Instance.Configuration.Instance.PerBulletCostMultiplier;
            ItemMagazineAsset magazine =
                (ItemMagazineAsset) UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command));

            if (ammoAmountToSpawn <= (ushort) EasyAmmo.Instance.Configuration.Instance.ClipLimit ||
                caller.HasPermission("easyammo.bypasslimit"))
            {
                if (Uconomy.Instance.Database.GetBalance(caller.Id) >=
                    GetCost(false, ammoAmountToSpawn, currentWeapon, magazine))
                {
                    if (Uplayer.GiveItem(GetMagId(Uplayer, currentWeapon, command), (byte) ammoAmountToSpawn))
                    {
                        Uconomy.Instance.Database.IncreaseBalance(caller.Id,
                            GetCost(true, ammoAmountToSpawn, currentWeapon, magazine));

                        UnturnedChat.Say(caller,
                            EasyAmmo.Instance.Translate("giving_mags", ammoAmountToSpawn.ToString(),
                                UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command)).itemName,
                                GetMagId(Uplayer, currentWeapon, command).ToString()));
                        UnturnedChat.Say(caller,
                            EasyAmmo.Instance.Translate("balance", Uconomy.Instance.Database.GetBalance(caller.Id)));
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
                        ammoAmountToSpawn, magazine.itemName,
                        GetCost(false, ammoAmountToSpawn, currentWeapon, magazine).ToString()));
                }
            }
            else
            {
                ushort amountoverlimit = ammoAmountToSpawn;
                ammoAmountToSpawn = (ushort) EasyAmmo.Instance.Configuration.Instance.ClipLimit;

                if (Uconomy.Instance.Database.GetBalance(caller.Id) >=
                    GetCost(false, ammoAmountToSpawn, currentWeapon, magazine))
                {
                    if (Uplayer.GiveItem(GetMagId(Uplayer, currentWeapon, command), (byte) ammoAmountToSpawn))
                    {
                        Uconomy.Instance.Database.IncreaseBalance(caller.Id,
                            GetCost(true, ammoAmountToSpawn, currentWeapon, magazine));

                        UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("over_clip_spawn_limit_giving",
                            amountoverlimit.ToString(),
                            EasyAmmo.Instance.Configuration.Instance.ClipLimit,
                            UnturnedItems.GetItemAssetById(GetMagId(Uplayer, currentWeapon, command)).itemName,
                            GetMagId(Uplayer, currentWeapon, command).ToString()));
                        UnturnedChat.Say(caller,
                            EasyAmmo.Instance.Translate("balance", Uconomy.Instance.Database.GetBalance(caller.Id)));
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
                        ammoAmountToSpawn, magazine.itemName,
                        GetCost(false, ammoAmountToSpawn, currentWeapon, magazine).ToString()));
                }
            }
        }

        public decimal GetCost(bool Negative, int amount, ItemGunAsset gun, ItemMagazineAsset magazine)
        {
            decimal cost;
            if (EasyAmmo.Instance.Configuration.Instance.ScaleCostByWeaponDamage)
            {
                cost = (decimal) (gun.playerDamageMultiplier.damage * amount) *
                       EasyAmmo.Instance.Configuration.Instance.WeaponDamageCostMultiplier;
            }
            else
            {
                cost = (decimal) (magazine.amount * amount) *
                       EasyAmmo.Instance.Configuration.Instance.PerBulletCostMultiplier;
            }

            if (Negative)
            {
                cost = -cost;
            }

            //Logger.Log(cost.ToString());
            return cost;
        }

        public ushort GetMagId(UnturnedPlayer player, ItemGunAsset gun, string[] command)
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
                else
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

        /*
            Unturned weapon metadata structure.
            metadata[0] = sight id byte 1
            metadata[1] = sight id byte 2
            metadata[2] = tactical id byte 1
            metadata[3] = tactical id byte 2
            
            metadata[4] = grip id byte 1
            metadata[5] = grip id byte 2
            metadata[6] = barrel id byte 1
            metadata[7] = barrel id byte 2
            metadata[8] = magazine id byte 1
            metadata[9] = magazine id byte 2
            metadata[10] = ammo
            metadata[11] = firemode
            metadata[12] = ??
            metadata[13] = sight durability
            metadata[14] = tactical durability
            metadata[15] = grip durability
            metadata[16] = barrel durability
            metadata[17] = magazine durability
        */
    }
}