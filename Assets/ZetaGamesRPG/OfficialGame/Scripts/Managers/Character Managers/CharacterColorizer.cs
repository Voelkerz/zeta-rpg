using System;
using UnityEngine;

namespace ZetaGames.RPG {
    public class CharacterColorizer : MonoBehaviour {

        private CharacterInfo character;
        private SpriteRenderer hatRenderer;
        private SpriteRenderer hairRenderer;
        private SpriteRenderer maskRenderer;
        private SpriteRenderer glovesRenderer;
        private SpriteRenderer armorRenderer;
        private SpriteRenderer bootsRenderer;
        private SpriteRenderer bodyRenderer;
        private Color newColor;

        private void Awake() {
            // Cache character info
            character = GetComponent<CharacterInfo>();

            // Add listeners to character events
            character.OnHairSwapped += Character_OnHairSwapped;
            character.OnArmorEquipped += Character_OnArmorEquipped;
            character.OnBootsEquipped += Character_OnBootsEquipped;
            character.OnGlovesEquipped += Character_OnGlovesEquipped;
            character.OnHatEquipped += Character_OnHatEquipped;
            character.OnMaskEquipped += Character_OnMaskEquipped;

            // Initialize color
            newColor = new Color(1, 1, 1, 1);

            // Cache the various sprite renderers
            SpriteRenderer[] characterRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in characterRenderers) {
                switch (renderer.gameObject.name) {
                    case "Hat":
                        hatRenderer = renderer;
                        break;
                    case "Hair":
                        hairRenderer = renderer;
                        break;
                    case "Mask":
                        maskRenderer = renderer;
                        break;
                    case "Gloves":
                        glovesRenderer = renderer;
                        break;
                    case "Armor":
                        armorRenderer = renderer;
                        break;
                    case "Boots":
                        bootsRenderer = renderer;
                        break;
                    case "Body":
                        bodyRenderer = renderer;
                        break;
                    default:
                        break;
                }
            }
        }

        private void Start() {
            // Initialize colors for starting equipment
            //if (character.maskData != null) SetMaskColor(character.maskData.itemColor);
            //if (character.hatData != null) SetHatColor(character.hatData.itemColor);
            //if (character.glovesData != null) SetGlovesColor(character.glovesData.itemColor);
            //if (character.bootsData != null) SetBootsColor(character.bootsData.itemColor);
            //if (character.armorData != null) SetArmorColor(character.armorData.itemColor);
            //if (character.hairData != null) SetHairColor(character.hairData.itemColor);
        }

        public void SetHatColor(Color color) {
            hatRenderer.color = color;
        }

        public void SetHairColor(Color color) {
            hairRenderer.color = color;
        }

        public void SetMaskColor(Color color) {
            maskRenderer.color = color;
        }

        public void SetGlovesColor(Color color) {
            glovesRenderer.color = color;
        }

        public void SetArmorColor(Color color) {
            armorRenderer.color = color;
        }

        public void SetBootsColor(Color color) {
            bootsRenderer.color = color;
        }

        public void SetBodyColor(Color color) {
            bodyRenderer.color = color;
        }

        public void SetHatRGB(float red, float green, float blue, float alpha) {
            newColor.r = red / 255f;
            newColor.g = green / 255f;
            newColor.b = blue / 255f;
            newColor.a = alpha;

            SetHatColor(newColor);
        }

        public void SetHairRGB(float red, float green, float blue, float alpha) {
            newColor.r = red / 255f;
            newColor.g = green / 255f;
            newColor.b = blue / 255f;
            newColor.a = alpha;

            SetHairColor(newColor);
        }

        public void SetMaskRGB(float red, float green, float blue, float alpha) {
            newColor.r = red / 255f;
            newColor.g = green / 255f;
            newColor.b = blue / 255f;
            newColor.a = alpha;

            SetMaskColor(newColor);
        }

        public void SetGlovesRGB(float red, float green, float blue, float alpha) {
            newColor.r = red / 255f;
            newColor.g = green / 255f;
            newColor.b = blue / 255f;
            newColor.a = alpha;

            SetGlovesColor(newColor);
        }

        public void SetArmorRGB(float red, float green, float blue, float alpha) {
            newColor.r = red / 255f;
            newColor.g = green / 255f;
            newColor.b = blue / 255f;
            newColor.a = alpha;

            SetArmorColor(newColor);
        }

        public void SetBootsRGB(float red, float green, float blue, float alpha) {
            newColor.r = red / 255f;
            newColor.g = green / 255f;
            newColor.b = blue / 255f;
            newColor.a = alpha;

            SetBootsColor(newColor);
        }

        public void SetBodyRGB(float red, float green, float blue, float alpha) {
            newColor.r = red / 255f;
            newColor.g = green / 255f;
            newColor.b = blue / 255f;
            newColor.a = alpha;

            SetBodyColor(newColor);
        }

        private void Character_OnMaskEquipped(object sender, EventArgs e) {
            if (character.maskData != null) SetMaskColor(character.maskData.itemColor);
        }

        private void Character_OnHatEquipped(object sender, EventArgs e) {
            if (character.hatData != null) SetHatColor(character.hatData.itemColor);
        }

        private void Character_OnGlovesEquipped(object sender, EventArgs e) {
            if (character.glovesData != null) SetGlovesColor(character.glovesData.itemColor);
        }

        private void Character_OnBootsEquipped(object sender, EventArgs e) {
            if (character.bootsData != null) SetBootsColor(character.bootsData.itemColor);
        }

        private void Character_OnArmorEquipped(object sender, EventArgs e) {
            if (character.armorData != null) SetArmorColor(character.armorData.itemColor);
        }

        private void Character_OnHairSwapped(object sender, EventArgs e) {
            if (character.hairData != null) SetHairColor(character.hairData.itemColor);
        }
    }
}
