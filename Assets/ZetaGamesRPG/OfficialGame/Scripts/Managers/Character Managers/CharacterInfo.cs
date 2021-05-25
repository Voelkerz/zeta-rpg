using System;
using UnityEngine;

namespace ZetaGames.RPG {
    public class CharacterInfo : MonoBehaviour {

        public event EventHandler OnHairSwapped;
        public event EventHandler OnArmorEquipped;
        public event EventHandler OnBootsEquipped;
        public event EventHandler OnGlovesEquipped;
        public event EventHandler OnHatEquipped;
        public event EventHandler OnMaskEquipped;

        // RACE AND HAIR

        public BaseCharacter characterData;
        public EquippableItem hairData;

        // EQUIPPED ITEMS
        public EquippableItem armorData;
        public EquippableItem bootsData;
        public EquippableItem glovesData;
        public EquippableItem hatData;
        public EquippableItem maskData;

        public void SetHair(EquippableItem itemData) {
            hairData = itemData;
            if (OnHairSwapped != null) OnHairSwapped(this, null);
        }

        public void SetArmor(EquippableItem itemData) {
            armorData = itemData;
            if (OnArmorEquipped != null) OnArmorEquipped(this, null);
        }

        public void SetBoots(EquippableItem itemData) {
            bootsData = itemData;
            if (OnBootsEquipped != null) OnBootsEquipped(this, null);
        }

        public void SetGloves(EquippableItem itemData) {
            glovesData = itemData;
            if (OnGlovesEquipped != null) OnGlovesEquipped(this, null);
        }

        public void SetHat(EquippableItem itemData) {
            hatData = itemData;
            if (OnHatEquipped != null) OnHatEquipped(this, null);
        }

        public void SetMask(EquippableItem itemData) {
            maskData = itemData;
            if (OnMaskEquipped != null) OnMaskEquipped(this, null);
        }
    }
}
