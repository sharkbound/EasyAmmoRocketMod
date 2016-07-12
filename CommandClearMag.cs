using Rocket.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Core.Logging;
using Rocket.Unturned.Items;

namespace EasyAmmoRocketMod
{
    class CommandClearMag : IRocketCommand
    {
        public List<string> Aliases
        {
            get { return new List<string> { "clearm", "cm" }; }
        }

        public AllowedCaller AllowedCaller
        {
            get { return Rocket.API.AllowedCaller.Player; }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer Uplayer = (UnturnedPlayer)caller;
            //bool enterNumber = false;
            //int enteredAmount = 0;
            int magsRemoved = 0;
            ItemAsset uItem = null;

         /*   if (command.Length == 1)
            {
                if (int.TryParse(command[0], out enteredAmount))
                {
                    enterNumber = true;
                }
            } */

            // this section of code is referenced from LeeIzaZombie's itemRestriction plugin here - https://dev.rocketmod.net/plugins/item-restrictions/
            //original code  - https://bitbucket.org/LeeIzaZombie/rocketmod_itemrestrictions/src/9646f79a3c4f051551cd209a9c86e8ee0ea0e829/RocketMod_ItemRestriction/IR_Plugin.cs?at=master&fileviewer=file-view-default

            PlayerInventory inventory = Uplayer.Player.inventory;

            for (byte page = 0; page < 8; page++)
            {
                byte amountOfItems = inventory.getItemCount(page);
                for (int index = amountOfItems - 1; index >= 0; index--)
                {
                    try
                    {
                        uItem = UnturnedItems.GetItemAssetById(inventory.getItem(page, (byte)index).item.id);
                    }
                    catch (Exception)
                    {
                        //Logger.LogError("Error trying to get item at Page: " + page + " Index: " + index);
                    }

                    if (uItem != null)
                    {
                        if (uItem.type == EItemType.MAGAZINE)
                        {
                           // Logger.Log("removing id " + uItem.Id.ToString());
                            inventory.removeItem(page, (byte)index);
                           /* Logger.LogWarning("removed: " + uItem.id.ToString() + "from: " + "Page-" + page.ToString() 
                                + " index-" + index.ToString()); */
                            magsRemoved++; 
                        } 
                    }

                    uItem = null;
                } 
            }

            UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("removed_mags", magsRemoved.ToString()));
        }

        public string Help
        {
            get { return "clears empty ammo magazines from your inventory"; }
        }

        public string Name
        {
            get { return "clearmag"; }
        }

        public List<string> Permissions
        {
            get { return new List<string> { "clearmag" }; }
        }

        public string Syntax
        {
            get { return "(ammo count)"; }
        }
    }
}
