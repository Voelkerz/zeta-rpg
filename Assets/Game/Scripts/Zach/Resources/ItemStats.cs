using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    abstract class ItemStats {

        public enum ItemTypes {
            /*******************
             * HARVESTING TOOLS
             *******************/
            Axe,
            Pickaxe,
            Hands
        }

        public static int toolDamage = 5;
    }
}

