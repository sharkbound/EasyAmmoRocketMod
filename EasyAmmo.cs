using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.Unturned.Player;
using Rocket.API;
using Rocket.Core.Plugins;
using Rocket.Core.Logging;

namespace EasyAmmoRocketMod
{
    public class EasyAmmo : RocketPlugin<EasyAmmoConfig>
    {
        public static EasyAmmo Instance;

        protected override void Load()
        {
            Instance = this;

            Logger.Log("EasyAmmo loaded!");
            Logger.Log("ClipLimitEnabled : " + Instance.Configuration.Instance.ClipLimitEnabled);
            if (Instance.Configuration.Instance.ClipLimitEnabled)
            {
                Logger.Log("ClipLimit : " + Instance.Configuration.Instance.ClipLimit.ToString());
            }
        }

        protected override void Unload()
        {
            Logger.Log("EasyAmmo Unloaded!");
        }

        public override Rocket.API.Collections.TranslationList DefaultTranslations
        {
            get
            {
                return new Rocket.API.Collections.TranslationList
                {
                    {"over_clip_spawn_limit", "{0} is over the spawn limit, giving you {1} of \"{2}\" instead"}
                };
            }
        }
    }
}
