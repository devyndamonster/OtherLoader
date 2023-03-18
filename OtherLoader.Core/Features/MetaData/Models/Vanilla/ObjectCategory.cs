using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OtherLoader.Core.Features.MetaData.Models.Vanilla
{
    public enum ObjectCategory
    {
        Uncategorized = 0,
        Firearm = 1,
        Magazine = 2,
        Clip = 3,
        Cartridge = 4,
        Attachment = 5,
        SpeedLoader = 6,
        Thrown = 7,
        MeleeWeapon = 10,
        Explosive = 20,
        Powerup = 25,
        Target = 30,
        Prop = 0x1F,
        Furniture = 0x20,
        Tool = 40,
        Toy = 41,
        Firework = 42,
        Ornament = 43,
        Loot = 50,
        VFX = 51,
        SosigClothing = 60
    }
}
