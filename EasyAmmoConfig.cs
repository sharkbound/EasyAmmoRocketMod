using System.Collections.Generic;
using System.Xml.Serialization;
using Rocket.API;

namespace EasyAmmo
{
    public class EasyAmmoConfig : IRocketPluginConfiguration
    {
       public int ClipLimit;
       public bool UconomySupportEnabled;
       public int PerBulletCostMultiplier;
       public bool ClipLimitEnabled;
       public bool ScaleCostByWeaponDamage;
       public int WeaponDamageCostMultiplier;
       [XmlArrayItem(ElementName = "Id")]
       public List<ushort> BannedIds;

        public void LoadDefaults()
        {
            UconomySupportEnabled = false;
            PerBulletCostMultiplier = 1;
            ClipLimitEnabled = true;
            ClipLimit = 10;
            ScaleCostByWeaponDamage = true;
            WeaponDamageCostMultiplier = 2;
            BannedIds = new List<ushort> { 65535 };
        }
    }
}
