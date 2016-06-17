using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rocket.API;

namespace EasyAmmoRocketMod
{
    public class EasyAmmoConfig : IRocketPluginConfiguration
    {
       public int ClipLimit;
       public bool ClipLimitEnabled;

        public void LoadDefaults()
        {
            ClipLimitEnabled = true;
            ClipLimit = 10;
        }
    }
}
