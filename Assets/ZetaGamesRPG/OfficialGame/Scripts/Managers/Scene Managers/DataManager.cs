using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance;
        public List<BaseCharacter> characterTypes;
        public List<EquippableItem> hairTypes;
        public List<EquippableItem> armorTypes;
        public List<EquippableItem> bootTypes;
        public List<EquippableItem> gloveTypes;
        public List<EquippableItem> hatTypes;
        public List<EquippableItem> maskTypes;

        private void Awake() {
            Instance = this;
        }
    }
}
