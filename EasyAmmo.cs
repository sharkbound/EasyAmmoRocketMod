using Rocket.API;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;

namespace EasyAmmo
{
    public class EasyAmmo : RocketPlugin<EasyAmmoConfig>
    {
        public static EasyAmmo Instance;

        protected override void Load()
        {
            Instance = this;

            Logger.LogWarning("--------------EasyAmmo--------------");
            Logger.Log("EasyAmmo loaded!");
            Logger.Log("ClipLimitEnabled : " + Instance.Configuration.Instance.ClipLimitEnabled);
            Logger.Log("ClipLimit : " + Instance.Configuration.Instance.ClipLimit.ToString());
            Logger.Log("UconomySupportEnabled : " + Instance.Configuration.Instance.UconomySupportEnabled.ToString());
            Logger.Log("BulletCostMultiplier : " + Instance.Configuration.Instance.PerBulletCostMultiplier);
            Logger.Log(
                "ScaleCostByWeaponDamage : " + Instance.Configuration.Instance.ScaleCostByWeaponDamage.ToString());
            Logger.Log("WeaponDamageCostMultiplier : " +
                       Instance.Configuration.Instance.WeaponDamageCostMultiplier.ToString());
            Logger.LogWarning("------------------------------------");
        }

        protected override void Unload()
        {
            Logger.Log("EasyAmmo Unloaded!");
        }

        public override Rocket.API.Collections.TranslationList DefaultTranslations =>
            new Rocket.API.Collections.TranslationList
            {
                {
                    "over_clip_spawn_limit_giving",
                    "{0} is over the spawn limit, giving you {1} of \"{2}\" ID: \"{3}\" instead"
                },
                {
                    "over_clip_spawn_limit_dropping",
                    "{0} is over the spawn limit, dropping {1} of \"{2}\" ID: \"{3}\" instead"
                },
                {"balance", "Remaining balance: {0}"},
                {"cost", "Cost Per-Mag: {0}"},
                {"no_gun_equipped", "You dont have any guns equipped!"},
                {"nothing_equipped", "You dont have anything equipped!"},
                {"gun_asset_not_found", "Gun asset is not found!"},
                {"dropping_mags", "Dropping {0} of \"{1}\" ID: \"{2}\" on your location"},
                {"giving_mags", "Giving you {0} of \"{1}\" ID: \"{2}\""},
                {"removed_mags", "Removed {0} Magazines from your inventory!"},
                {"failed_to_spawn_mags", "Failed to spawn a magazine for the gun you are holding!"},
                {"not_enough_funds", "You dont have enough {0} to buy {1} of {2}, {0} need: {3}"},
                {"cloned_item", "Cloned \"{0}\" {1} times!"},
                {
                    "weapon_blacklisted",
                    "The weapon \"{0}\" is blacklisted, you cannot spawn mags for it using this command!"
                },
                {
                    "Clonei_item_blacklisted",
                    "The item \"{0}\" is blacklisted, you cannot clone this item using this command!"
                }
            };

        public static bool CheckIfBlacklisted(IRocketPlayer caller, SDG.Unturned.ItemGunAsset currentWeapon)
        {
            bool cantSpawnMag = false;
            if (!caller.HasPermission("easyammo.bypassblacklist"))
            {
                foreach (ushort id in Instance.Configuration.Instance.BannedIds)
                {
                    if (currentWeapon.id == id)
                    {
                        UnturnedChat.Say(caller, Instance.Translate("weapon_blacklisted", currentWeapon.itemName));
                        cantSpawnMag = true;
                    }
                }
            }

            return cantSpawnMag;
        }
    }
}