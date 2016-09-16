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
    class CommandCloneItem : IRocketCommand
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
            ushort AmountToSpawn = (ushort)0;
            bool EnteredAmount = false;
            SDG.Unturned.ItemAsset currentEquiped;
            UnturnedPlayer Uplayer = (UnturnedPlayer)caller;

            if (command.Length == 1)
            {
                if (ushort.TryParse(command[0], out AmountToSpawn))
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

            if (checkIfBlacklisted(caller, currentEquiped))
            {
                UnturnedChat.Say(EasyAmmo.Instance.Translate("Clonei_item_blacklisted", currentEquiped.Name));
                return;
            }

            var state = Uplayer.Player.equipment.state;

           /* for (int count = 0; count <= state.Length - 1; count++)
            {
                Logger.Log("State " + count.ToString() + " : " + state[count].ToString());
                //state[count] = 17;
            }  */
            /*
            state[0] is a sight
            state[8] is a magazine
            state[10] is ammo count
             */
            
            SDG.Unturned.Item newItem = new SDG.Unturned.Item(currentEquiped.id, 100, 100, state);

            if (AmountToSpawn == 0)
            {
                AmountToSpawn = 1;
            }
            
            if (caller.HasPermission("clonei.amount"))
            {
                for (int ii = 0; ii < AmountToSpawn; ii++)
                {
                    Uplayer.GiveItem(newItem);
                } 
            }
            else
            {
                Uplayer.GiveItem(newItem);
            }

            UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("cloned_item", 
               UnturnedItems.GetItemAssetById(currentEquiped.id).itemName, AmountToSpawn.ToString()));
            return;
        }

        public string Help
        {
            get { return "Gives you a clone of your current item"; }
        }

        public string Name
        {
            get { return "clonei"; }
        }

        public List<string> Permissions
        {
            get { return new List<string> { "clonei" }; }
        }

        public string Syntax
        {
            get { return "(amount)"; }
        }

        bool checkIfBlacklisted(IRocketPlayer caller, ItemAsset item)
        {
            if (caller.HasPermission("clonei.bypassblacklist")) return false;
            return EasyAmmo.Instance.Configuration.Instance.BannedIds.Contains(item.Id);
        }
    }
}
