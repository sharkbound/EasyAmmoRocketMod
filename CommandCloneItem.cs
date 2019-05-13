using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Items;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace EasyAmmo
{
    class CommandCloneItem : IRocketCommand
    {
        public List<string> Aliases => new List<string>();

        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public void Execute(IRocketPlayer caller, string[] command)
        {
            ushort amountToSpawn = 0;
            var uplayer = (UnturnedPlayer) caller;

            if (command.Length == 1)
            {
                ushort.TryParse(command[0], out amountToSpawn);
            }

            var currentEquiped = uplayer.Player.equipment.asset;
            if (currentEquiped == null)
            {
                UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("nothing_equipped"));
                return;
            }

            if (checkIfBlacklisted(caller, currentEquiped))
            {
                UnturnedChat.Say(caller,
                    EasyAmmo.Instance.Translate("Clonei_item_blacklisted", currentEquiped.itemName));
                return;
            }

            var state = uplayer.Player.equipment.state;

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

            var newItem = new Item(currentEquiped.id, 100, 100, state);

            if (amountToSpawn == 0)
            {
                amountToSpawn = 1;
            }

            if (caller.HasPermission("clonei.amount"))
            {
                for (int ii = 0; ii < amountToSpawn; ii++)
                {
                    uplayer.GiveItem(newItem);
                }
            }
            else
            {
                uplayer.GiveItem(newItem);
            }

            UnturnedChat.Say(caller, EasyAmmo.Instance.Translate("cloned_item",
                UnturnedItems.GetItemAssetById(currentEquiped.id).itemName, amountToSpawn.ToString()));
        }

        public string Help => "Gives you a clone of your current item";

        public string Name => "clonei";

        public List<string> Permissions => new List<string> {"clonei"};

        public string Syntax => "(amount)";

        bool checkIfBlacklisted(IRocketPlayer caller, ItemAsset item)
        {
            return !caller.HasPermission("clonei.bypassblacklist") &&
                   EasyAmmo.Instance.Configuration.Instance.BannedIds.Contains(item.id);
        }
    }
}