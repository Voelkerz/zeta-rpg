using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZetaGames.RPG {
    public class AnimationController : MonoBehaviour {

        [HideInInspector] public bool isPlaying;
        [HideInInspector] public float animMoveX;
        [HideInInspector] public float animMoveY;

        // Current Animation Spritesheet
        private Sprite[] armorSprites;
        private Sprite[] bodySprites;
        private Sprite[] bootsSprites;
        private Sprite[] glovesSprites;
        private Sprite[] hairSprites;
        private Sprite[] hatSprites;
        private Sprite[] maskSprites;
        private Sprite[] shadowSprites;

        private SpriteRenderer armorRenderer;
        private SpriteRenderer bodyRenderer;
        private SpriteRenderer bootsRenderer;
        private SpriteRenderer glovesRenderer;
        private SpriteRenderer hairRenderer;
        private SpriteRenderer hatRenderer;
        private SpriteRenderer maskRenderer;
        private SpriteRenderer shadowRenderer;

        private CharacterInfo character;
        private AnimationType currentAnimationType;
        private int animationLength;
        private float framerate;
        private int startingFrame;
        private int currentFrame;
        private float timer;
        private bool loop;
        private bool firstLoop;
        private bool loopedOnce;

        private void Awake() {
            // Cache the character info
            character = GetComponent<CharacterInfo>();

            // Add listeners to character events
            character.OnHairSwapped += Character_OnHairSwapped;
            character.OnArmorEquipped += Character_OnArmorEquipped;
            character.OnBootsEquipped += Character_OnBootsEquipped;
            character.OnGlovesEquipped += Character_OnGlovesEquipped;
            character.OnHatEquipped += Character_OnHatEquipped;
            character.OnMaskEquipped += Character_OnMaskEquipped;

            // Cache the various sprite renderers
            SpriteRenderer[] characterRenderers = GetComponentsInChildren<SpriteRenderer>();
            foreach (SpriteRenderer renderer in characterRenderers) {
                switch (renderer.gameObject.name) {
                    case "Shadow":
                        shadowRenderer = renderer;
                        break;
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
            // default animation state
            PlayAnimation(AnimationType.Idle);
        }

        private void Update() {
            if (!isPlaying) {
                return;
            }

            timer += Time.deltaTime;

            if (timer >= framerate) {
                timer -= framerate;

                if (loopedOnce) {
                    currentFrame = (currentFrame + 1) % animationLength;
                }
                
                if (bodySprites.Length / animationLength != 1) {
                    // E, SE, and S don't need to be calculated because it uses unaltered calculated frames
                    if (animMoveX < -0.3 && animMoveY < -0.3) {
                        // Moving SW
                        currentFrame += animationLength;
                    } else if (animMoveX < -0.3 && animMoveY > -0.3 && animMoveY < 0.3) {
                        // Moving W
                        currentFrame += animationLength;
                    } else if (animMoveX < -0.3 && animMoveY > 0.3) {
                        // Moving NW
                        currentFrame += animationLength * 3;
                    } else if (animMoveX < 0.3 && animMoveX > -0.3 && animMoveY > 0.3) {
                        // Moving N
                        currentFrame += animationLength * 2;
                    } else if (animMoveX > 0.3 && animMoveY > 0.3) {
                        // Moving NE
                        currentFrame += animationLength * 2;
                    }

                    if (firstLoop) {
                        startingFrame = currentFrame;
                        firstLoop = false;
                    }
                }

                if (!loop && loopedOnce && currentFrame == startingFrame) {
                    PlayAnimation(AnimationType.Idle);
                } else {
                    bodyRenderer.sprite = (bodySprites != null) ? bodySprites[currentFrame] : null;
                    shadowRenderer.sprite = (shadowSprites != null) ? shadowSprites[currentFrame] : null;
                    hairRenderer.sprite = (hairSprites != null) ? hairSprites[currentFrame] : null;
                    armorRenderer.sprite = (armorSprites != null) ? armorSprites[currentFrame] : null;
                    bootsRenderer.sprite = (bootsSprites != null) ? bootsSprites[currentFrame] : null;
                    glovesRenderer.sprite = (glovesSprites != null) ? glovesSprites[currentFrame] : null;
                    hatRenderer.sprite = (hatSprites != null) ? hatSprites[currentFrame] : null;
                    maskRenderer.sprite = (maskSprites != null) ? maskSprites[currentFrame] : null;
                }

                loopedOnce = true;
            }
        }

        public void StopPlaying() {
            isPlaying = false;
        }

        public void PlayAnimation(AnimationType animationType) {
            //Debug.Log("Playing Animation: " + animationType.ToString());

            // Set animation data based on enum type
            switch (animationType) {
                case AnimationType.Idle:
                    framerate = 1 / character.characterData.animMeta.idleFramerate;
                    animationLength = character.characterData.animMeta.idleAnimationLength;
                    loop = character.characterData.animMeta.idleLoop;

                    bodySprites = (character.characterData.bodyAnimations.idle != null) ? character.characterData.bodyAnimations.idle : null;
                    shadowSprites = (character.characterData.shadowAnimations.idle != null) ? character.characterData.shadowAnimations.idle : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.idle != null && character.hairData.animationData.idle.Length != 0) ? character.hairData.animationData.idle : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.idle != null && character.armorData.animationData.idle.Length != 0) ? character.armorData.animationData.idle : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.idle != null && character.bootsData.animationData.idle.Length != 0) ? character.bootsData.animationData.idle : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.idle != null && character.glovesData.animationData.idle.Length != 0) ? character.glovesData.animationData.idle : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.idle != null && character.hatData.animationData.idle.Length != 0) ? character.hatData.animationData.idle : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.idle != null && character.maskData.animationData.idle.Length != 0) ? character.maskData.animationData.idle : null;
                    break;
                case AnimationType.Walk:
                    framerate = 1 / character.characterData.animMeta.walkFramerate;
                    animationLength = character.characterData.animMeta.walkAnimationLength;
                    loop = character.characterData.animMeta.walkLoop;

                    bodySprites = (character.characterData.bodyAnimations.walk != null) ? character.characterData.bodyAnimations.walk : null;
                    shadowSprites = (character.characterData.shadowAnimations.walk != null) ? character.characterData.shadowAnimations.walk : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.walk != null && character.hairData.animationData.walk.Length != 0) ? character.hairData.animationData.walk : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.walk != null && character.armorData.animationData.walk.Length != 0) ? character.armorData.animationData.walk : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.walk != null && character.bootsData.animationData.walk.Length != 0) ? character.bootsData.animationData.walk : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.walk != null && character.glovesData.animationData.walk.Length != 0) ? character.glovesData.animationData.walk : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.walk != null && character.hatData.animationData.walk.Length != 0) ? character.hatData.animationData.walk : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.walk != null && character.maskData.animationData.walk.Length != 0) ? character.maskData.animationData.walk : null;
                    break;
                case AnimationType.Death:
                    framerate = 1 / character.characterData.animMeta.deathFramerate;
                    animationLength = character.characterData.animMeta.deathAnimationLength;
                    loop = character.characterData.animMeta.deathLoop;

                    bodySprites = (character.characterData.bodyAnimations.death != null) ? character.characterData.bodyAnimations.death : null;
                    shadowSprites = (character.characterData.shadowAnimations.death != null) ? character.characterData.shadowAnimations.death : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.death != null && character.hairData.animationData.death.Length != 0) ? character.hairData.animationData.death : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.death != null && character.armorData.animationData.death.Length != 0) ? character.armorData.animationData.death : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.death != null && character.bootsData.animationData.death.Length != 0) ? character.bootsData.animationData.death : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.death != null && character.glovesData.animationData.death.Length != 0) ? character.glovesData.animationData.death : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.death != null && character.hatData.animationData.death.Length != 0) ? character.hatData.animationData.death : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.death != null && character.maskData.animationData.death.Length != 0) ? character.maskData.animationData.death : null;
                    break;
                case AnimationType.Damage:
                    framerate = 1 / character.characterData.animMeta.damageFramerate;
                    animationLength = character.characterData.animMeta.damageAnimationLength;
                    loop = character.characterData.animMeta.damageLoop;

                    bodySprites = (character.characterData.bodyAnimations.damage != null) ? character.characterData.bodyAnimations.damage : null;
                    shadowSprites = (character.characterData.shadowAnimations.damage != null) ? character.characterData.shadowAnimations.damage : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.damage != null && character.hairData.animationData.damage.Length != 0) ? character.hairData.animationData.damage : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.damage != null && character.armorData.animationData.damage.Length != 0) ? character.armorData.animationData.damage : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.damage != null && character.bootsData.animationData.damage.Length != 0) ? character.bootsData.animationData.damage : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.damage != null && character.glovesData.animationData.damage.Length != 0) ? character.glovesData.animationData.damage : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.damage != null && character.hatData.animationData.damage.Length != 0) ? character.hatData.animationData.damage : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.damage != null && character.maskData.animationData.damage.Length != 0) ? character.maskData.animationData.damage : null;
                    break;
                case AnimationType.ChargeAttack:
                    framerate = 1 / character.characterData.animMeta.chargeAttackFramerate;
                    animationLength = character.characterData.animMeta.chargeAttackAnimationLength;
                    loop = character.characterData.animMeta.chargeAttackLoop;

                    bodySprites = (character.characterData.bodyAnimations.chargeAttack != null) ? character.characterData.bodyAnimations.chargeAttack : null;
                    shadowSprites = (character.characterData.shadowAnimations.chargeAttack != null) ? character.characterData.shadowAnimations.chargeAttack : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.chargeAttack != null && character.hairData.animationData.chargeAttack.Length != 0) ? character.hairData.animationData.chargeAttack : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.chargeAttack != null && character.armorData.animationData.chargeAttack.Length != 0) ? character.armorData.animationData.chargeAttack : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.chargeAttack != null && character.bootsData.animationData.chargeAttack.Length != 0) ? character.bootsData.animationData.chargeAttack : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.chargeAttack != null && character.glovesData.animationData.chargeAttack.Length != 0) ? character.glovesData.animationData.chargeAttack : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.chargeAttack != null && character.hatData.animationData.chargeAttack.Length != 0) ? character.hatData.animationData.chargeAttack : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.chargeAttack != null && character.maskData.animationData.chargeAttack.Length != 0) ? character.maskData.animationData.chargeAttack : null;
                    break;
                case AnimationType.Attack:
                    framerate = 1 / character.characterData.animMeta.attackFramerate;
                    animationLength = character.characterData.animMeta.attackAnimationLength;
                    loop = character.characterData.animMeta.attackLoop;

                    bodySprites = (character.characterData.bodyAnimations.attack != null) ? character.characterData.bodyAnimations.attack : null;
                    shadowSprites = (character.characterData.shadowAnimations.attack != null) ? character.characterData.shadowAnimations.attack : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.attack != null && character.hairData.animationData.attack.Length != 0) ? character.hairData.animationData.attack : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.attack != null && character.armorData.animationData.attack.Length != 0) ? character.armorData.animationData.attack : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.attack != null && character.bootsData.animationData.attack.Length != 0) ? character.bootsData.animationData.attack : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.attack != null && character.glovesData.animationData.attack.Length != 0) ? character.glovesData.animationData.attack : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.attack != null && character.hatData.animationData.attack.Length != 0) ? character.hatData.animationData.attack : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.attack != null && character.maskData.animationData.attack.Length != 0) ? character.maskData.animationData.attack : null;
                    break;
                case AnimationType.Logging:
                    framerate = 1 / character.characterData.animMeta.loggingFramerate;
                    animationLength = character.characterData.animMeta.loggingAnimationLength;
                    loop = character.characterData.animMeta.loggingLoop;

                    bodySprites = (character.characterData.bodyAnimations.logging != null) ? character.characterData.bodyAnimations.logging : null;
                    shadowSprites = (character.characterData.shadowAnimations.logging != null) ? character.characterData.shadowAnimations.logging : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.logging != null && character.hairData.animationData.logging.Length != 0) ? character.hairData.animationData.logging : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.logging != null && character.armorData.animationData.logging.Length != 0) ? character.armorData.animationData.logging : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.logging != null && character.bootsData.animationData.logging.Length != 0) ? character.bootsData.animationData.logging : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.logging != null && character.glovesData.animationData.logging.Length != 0) ? character.glovesData.animationData.logging : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.logging != null && character.hatData.animationData.logging.Length != 0) ? character.hatData.animationData.logging : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.logging != null && character.maskData.animationData.logging.Length != 0) ? character.maskData.animationData.logging : null;
                    break;
                case AnimationType.Mining:
                    framerate = 1 / character.characterData.animMeta.miningFramerate;
                    animationLength = character.characterData.animMeta.miningAnimationLength;
                    loop = character.characterData.animMeta.miningLoop;

                    bodySprites = (character.characterData.bodyAnimations.mining != null) ? character.characterData.bodyAnimations.mining : null;
                    shadowSprites = (character.characterData.shadowAnimations.mining != null) ? character.characterData.shadowAnimations.mining : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.mining != null && character.hairData.animationData.mining.Length != 0) ? character.hairData.animationData.mining : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.mining != null && character.armorData.animationData.mining.Length != 0) ? character.armorData.animationData.mining : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.mining != null && character.bootsData.animationData.mining.Length != 0) ? character.bootsData.animationData.mining : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.mining != null && character.glovesData.animationData.mining.Length != 0) ? character.glovesData.animationData.mining : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.mining != null && character.hatData.animationData.mining.Length != 0) ? character.hatData.animationData.mining : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.mining != null && character.maskData.animationData.mining.Length != 0) ? character.maskData.animationData.mining : null;
                    break;
                case AnimationType.Harvesting:
                    framerate = 1 / character.characterData.animMeta.harvestingFramerate;
                    animationLength = character.characterData.animMeta.harvestingAnimationLength;
                    loop = character.characterData.animMeta.harvestingLoop;

                    bodySprites = (character.characterData.bodyAnimations.harvesting != null) ? character.characterData.bodyAnimations.harvesting : null;
                    shadowSprites = (character.characterData.shadowAnimations.harvesting != null) ? character.characterData.shadowAnimations.harvesting : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.harvesting != null && character.hairData.animationData.harvesting.Length != 0) ? character.hairData.animationData.harvesting : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.harvesting != null && character.armorData.animationData.harvesting.Length != 0) ? character.armorData.animationData.harvesting : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.harvesting != null && character.bootsData.animationData.harvesting.Length != 0) ? character.bootsData.animationData.harvesting : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.harvesting != null && character.glovesData.animationData.harvesting.Length != 0) ? character.glovesData.animationData.harvesting : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.harvesting != null && character.hatData.animationData.harvesting.Length != 0) ? character.hatData.animationData.harvesting : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.harvesting != null && character.maskData.animationData.harvesting.Length != 0) ? character.maskData.animationData.harvesting : null;
                    break;
                case AnimationType.WoodWork:
                    framerate = 1 / character.characterData.animMeta.woodWorkFramerate;
                    animationLength = character.characterData.animMeta.woodWorkAnimationLength;
                    loop = character.characterData.animMeta.woodWorkLoop;

                    bodySprites = (character.characterData.bodyAnimations.woodWork != null) ? character.characterData.bodyAnimations.woodWork : null;
                    shadowSprites = (character.characterData.shadowAnimations.woodWork != null) ? character.characterData.shadowAnimations.woodWork : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.woodWork != null && character.hairData.animationData.woodWork.Length != 0) ? character.hairData.animationData.woodWork : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.woodWork != null && character.armorData.animationData.woodWork.Length != 0) ? character.armorData.animationData.woodWork : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.woodWork != null && character.bootsData.animationData.woodWork.Length != 0) ? character.bootsData.animationData.woodWork : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.woodWork != null && character.glovesData.animationData.woodWork.Length != 0) ? character.glovesData.animationData.woodWork : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.woodWork != null && character.hatData.animationData.woodWork.Length != 0) ? character.hatData.animationData.woodWork : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.woodWork != null && character.maskData.animationData.woodWork.Length != 0) ? character.maskData.animationData.woodWork : null;
                    break;
                case AnimationType.JewelryWork:
                    framerate = 1 / character.characterData.animMeta.jewelryWorkFramerate;
                    animationLength = character.characterData.animMeta.jewelryWorkAnimationLength;
                    loop = character.characterData.animMeta.jewelryWorkLoop;

                    bodySprites = (character.characterData.bodyAnimations.jewelryWork != null) ? character.characterData.bodyAnimations.jewelryWork : null;
                    shadowSprites = (character.characterData.shadowAnimations.jewelryWork != null) ? character.characterData.shadowAnimations.jewelryWork : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.jewelryWork != null && character.hairData.animationData.jewelryWork.Length != 0) ? character.hairData.animationData.jewelryWork : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.jewelryWork != null && character.armorData.animationData.jewelryWork.Length != 0) ? character.armorData.animationData.jewelryWork : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.jewelryWork != null && character.bootsData.animationData.jewelryWork.Length != 0) ? character.bootsData.animationData.jewelryWork : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.jewelryWork != null && character.glovesData.animationData.jewelryWork.Length != 0) ? character.glovesData.animationData.jewelryWork : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.jewelryWork != null && character.hatData.animationData.jewelryWork.Length != 0) ? character.hatData.animationData.jewelryWork : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.jewelryWork != null && character.maskData.animationData.jewelryWork.Length != 0) ? character.maskData.animationData.jewelryWork : null;
                    break;
                case AnimationType.BlacksmithWork:
                    framerate = 1 / character.characterData.animMeta.blacksmithWorkFramerate;
                    animationLength = character.characterData.animMeta.blacksmithWorkAnimationLength;
                    loop = character.characterData.animMeta.blacksmithWorkLoop;

                    bodySprites = (character.characterData.bodyAnimations.blacksmithWork != null) ? character.characterData.bodyAnimations.blacksmithWork : null;
                    shadowSprites = (character.characterData.shadowAnimations.blacksmithWork != null) ? character.characterData.shadowAnimations.blacksmithWork : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.blacksmithWork != null && character.hairData.animationData.blacksmithWork.Length != 0) ? character.hairData.animationData.blacksmithWork : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.blacksmithWork != null && character.armorData.animationData.blacksmithWork.Length != 0) ? character.armorData.animationData.blacksmithWork : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.blacksmithWork != null && character.bootsData.animationData.blacksmithWork.Length != 0) ? character.bootsData.animationData.blacksmithWork : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.blacksmithWork != null && character.glovesData.animationData.blacksmithWork.Length != 0) ? character.glovesData.animationData.blacksmithWork : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.blacksmithWork != null && character.hatData.animationData.blacksmithWork.Length != 0) ? character.hatData.animationData.blacksmithWork : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.blacksmithWork != null && character.maskData.animationData.blacksmithWork.Length != 0) ? character.maskData.animationData.blacksmithWork : null;
                    break;
                case AnimationType.AlchemyWork:
                    framerate = 1 / character.characterData.animMeta.alchemyWorkFramerate;
                    animationLength = character.characterData.animMeta.alchemyWorkAnimationLength;
                    loop = character.characterData.animMeta.alchemyWorkLoop;

                    bodySprites = (character.characterData.bodyAnimations.alchemyWork != null) ? character.characterData.bodyAnimations.alchemyWork : null;
                    shadowSprites = (character.characterData.shadowAnimations.alchemyWork != null) ? character.characterData.shadowAnimations.alchemyWork : null;
                    hairSprites = (character.hairData != null && character.hairData.animationData.alchemyWork != null && character.hairData.animationData.alchemyWork.Length != 0) ? character.hairData.animationData.alchemyWork : null;
                    armorSprites = (character.armorData != null && character.armorData.animationData.alchemyWork != null && character.armorData.animationData.alchemyWork.Length != 0) ? character.armorData.animationData.alchemyWork : null;
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.alchemyWork != null && character.bootsData.animationData.alchemyWork.Length != 0) ? character.bootsData.animationData.alchemyWork : null;
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.alchemyWork != null && character.glovesData.animationData.alchemyWork.Length != 0) ? character.glovesData.animationData.alchemyWork : null;
                    hatSprites = (character.hatData != null && character.hatData.animationData.alchemyWork != null && character.hatData.animationData.alchemyWork.Length != 0) ? character.hatData.animationData.alchemyWork : null;
                    maskSprites = (character.maskData != null && character.maskData.animationData.alchemyWork != null && character.maskData.animationData.alchemyWork.Length != 0) ? character.maskData.animationData.alchemyWork : null;
                    break;
                default:
                    break;
            }

            currentAnimationType = animationType;
            loopedOnce = false;
            firstLoop = true;
            isPlaying = true;
            currentFrame = 0;
            timer = framerate;
        }

        private void Character_OnArmorEquipped(object sender, System.EventArgs e) {
            // Instantly update currently used sprites for current animation sequence when changing equipped items
            switch (currentAnimationType) {
                case AnimationType.Idle:
                    armorSprites = (character.armorData != null && character.armorData.animationData.idle != null && character.armorData.animationData.idle.Length != 0) ? character.armorData.animationData.idle : null;
                    break;
                case AnimationType.Walk:
                    armorSprites = (character.armorData != null && character.armorData.animationData.walk != null && character.armorData.animationData.walk.Length != 0) ? character.armorData.animationData.walk : null;
                    break;
                case AnimationType.Death:
                    armorSprites = (character.armorData != null && character.armorData.animationData.death != null && character.armorData.animationData.death.Length != 0) ? character.armorData.animationData.death : null;
                    break;
                case AnimationType.Damage:
                    armorSprites = (character.armorData != null && character.armorData.animationData.damage != null && character.armorData.animationData.damage.Length != 0) ? character.armorData.animationData.damage : null;
                    break;
                case AnimationType.ChargeAttack:
                    armorSprites = (character.armorData != null && character.armorData.animationData.chargeAttack != null && character.armorData.animationData.chargeAttack.Length != 0) ? character.armorData.animationData.chargeAttack : null;
                    break;
                case AnimationType.Attack:
                    armorSprites = (character.armorData != null && character.armorData.animationData.attack != null && character.armorData.animationData.attack.Length != 0) ? character.armorData.animationData.attack : null;
                    break;
                case AnimationType.Logging:
                    armorSprites = (character.armorData != null && character.armorData.animationData.logging != null && character.armorData.animationData.logging.Length != 0) ? character.armorData.animationData.logging : null;
                    break;
                case AnimationType.Mining:
                    armorSprites = (character.armorData != null && character.armorData.animationData.mining != null && character.armorData.animationData.mining.Length != 0) ? character.armorData.animationData.mining : null;
                    break;
                case AnimationType.Harvesting:
                    armorSprites = (character.armorData != null && character.armorData.animationData.harvesting != null && character.armorData.animationData.harvesting.Length != 0) ? character.armorData.animationData.harvesting : null;
                    break;
                case AnimationType.WoodWork:
                    armorSprites = (character.armorData != null && character.armorData.animationData.woodWork != null && character.armorData.animationData.woodWork.Length != 0) ? character.armorData.animationData.woodWork : null;
                    break;
                case AnimationType.JewelryWork:
                    armorSprites = (character.armorData != null && character.armorData.animationData.jewelryWork != null && character.armorData.animationData.jewelryWork.Length != 0) ? character.armorData.animationData.jewelryWork : null;
                    break;
                case AnimationType.BlacksmithWork:
                    armorSprites = (character.armorData != null && character.armorData.animationData.blacksmithWork != null && character.armorData.animationData.blacksmithWork.Length != 0) ? character.armorData.animationData.blacksmithWork : null;
                    break;
                case AnimationType.AlchemyWork:
                    armorSprites = (character.armorData != null && character.armorData.animationData.alchemyWork != null && character.armorData.animationData.alchemyWork.Length != 0) ? character.armorData.animationData.alchemyWork : null;
                    break;
                default:
                    break;
            }
        }

        private void Character_OnBootsEquipped(object sender, System.EventArgs e) {
            // Instantly update currently used sprites for current animation sequence when changing equipped items
            switch (currentAnimationType) {
                case AnimationType.Idle:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.idle != null && character.bootsData.animationData.idle.Length != 0) ? character.bootsData.animationData.idle : null;
                    break;
                case AnimationType.Walk:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.walk != null && character.bootsData.animationData.walk.Length != 0) ? character.bootsData.animationData.walk : null;
                    break;
                case AnimationType.Death:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.death != null && character.bootsData.animationData.death.Length != 0) ? character.bootsData.animationData.death : null;
                    break;
                case AnimationType.Damage:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.damage != null && character.bootsData.animationData.damage.Length != 0) ? character.bootsData.animationData.damage : null;
                    break;
                case AnimationType.ChargeAttack:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.chargeAttack != null && character.bootsData.animationData.chargeAttack.Length != 0) ? character.bootsData.animationData.chargeAttack : null;
                    break;
                case AnimationType.Attack:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.attack != null && character.bootsData.animationData.attack.Length != 0) ? character.bootsData.animationData.attack : null;
                    break;
                case AnimationType.Logging:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.logging != null && character.bootsData.animationData.logging.Length != 0) ? character.bootsData.animationData.logging : null;
                    break;
                case AnimationType.Mining:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.mining != null && character.bootsData.animationData.mining.Length != 0) ? character.bootsData.animationData.mining : null;
                    break;
                case AnimationType.Harvesting:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.harvesting != null && character.bootsData.animationData.harvesting.Length != 0) ? character.bootsData.animationData.harvesting : null;
                    break;
                case AnimationType.WoodWork:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.woodWork != null && character.bootsData.animationData.woodWork.Length != 0) ? character.bootsData.animationData.woodWork : null;
                    break;
                case AnimationType.JewelryWork:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.jewelryWork != null && character.bootsData.animationData.jewelryWork.Length != 0) ? character.bootsData.animationData.jewelryWork : null;
                    break;
                case AnimationType.BlacksmithWork:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.blacksmithWork != null && character.bootsData.animationData.blacksmithWork.Length != 0) ? character.bootsData.animationData.blacksmithWork : null;
                    break;
                case AnimationType.AlchemyWork:
                    bootsSprites = (character.bootsData != null && character.bootsData.animationData.alchemyWork != null && character.bootsData.animationData.alchemyWork.Length != 0) ? character.bootsData.animationData.alchemyWork : null;
                    break;
                default:
                    break;
            }
        }

        private void Character_OnGlovesEquipped(object sender, System.EventArgs e) {
            // Instantly update currently used sprites for current animation sequence when changing equipped items
            switch (currentAnimationType) {
                case AnimationType.Idle:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.idle != null && character.glovesData.animationData.idle.Length != 0) ? character.glovesData.animationData.idle : null;
                    break;
                case AnimationType.Walk:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.walk != null && character.glovesData.animationData.walk.Length != 0) ? character.glovesData.animationData.walk : null;
                    break;
                case AnimationType.Death:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.death != null && character.glovesData.animationData.death.Length != 0) ? character.glovesData.animationData.death : null;
                    break;
                case AnimationType.Damage:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.damage != null && character.glovesData.animationData.damage.Length != 0) ? character.glovesData.animationData.damage : null;
                    break;
                case AnimationType.ChargeAttack:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.chargeAttack != null && character.glovesData.animationData.chargeAttack.Length != 0) ? character.glovesData.animationData.chargeAttack : null;
                    break;
                case AnimationType.Attack:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.attack != null && character.glovesData.animationData.attack.Length != 0) ? character.glovesData.animationData.attack : null;
                    break;
                case AnimationType.Logging:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.logging != null && character.glovesData.animationData.logging.Length != 0) ? character.glovesData.animationData.logging : null;
                    break;
                case AnimationType.Mining:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.mining != null && character.glovesData.animationData.mining.Length != 0) ? character.glovesData.animationData.mining : null;
                    break;
                case AnimationType.Harvesting:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.harvesting != null && character.glovesData.animationData.harvesting.Length != 0) ? character.glovesData.animationData.harvesting : null;
                    break;
                case AnimationType.WoodWork:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.woodWork != null && character.glovesData.animationData.woodWork.Length != 0) ? character.glovesData.animationData.woodWork : null;
                    break;
                case AnimationType.JewelryWork:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.jewelryWork != null && character.glovesData.animationData.jewelryWork.Length != 0) ? character.glovesData.animationData.jewelryWork : null;
                    break;
                case AnimationType.BlacksmithWork:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.blacksmithWork != null && character.glovesData.animationData.blacksmithWork.Length != 0) ? character.glovesData.animationData.blacksmithWork : null;
                    break;
                case AnimationType.AlchemyWork:
                    glovesSprites = (character.glovesData != null && character.glovesData.animationData.alchemyWork != null && character.glovesData.animationData.alchemyWork.Length != 0) ? character.glovesData.animationData.alchemyWork : null;
                    break;
                default:
                    break;
            }
        }

        private void Character_OnHairSwapped(object sender, System.EventArgs e) {
            // Instantly update currently used sprites for current animation sequence when changing equipped items
            switch (currentAnimationType) {
                case AnimationType.Idle:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.idle != null && character.hairData.animationData.idle.Length != 0) ? character.hairData.animationData.idle : null;
                    break;
                case AnimationType.Walk:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.walk != null && character.hairData.animationData.walk.Length != 0) ? character.hairData.animationData.walk : null;
                    break;
                case AnimationType.Death:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.death != null && character.hairData.animationData.death.Length != 0) ? character.hairData.animationData.death : null;
                    break;
                case AnimationType.Damage:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.damage != null && character.hairData.animationData.damage.Length != 0) ? character.hairData.animationData.damage : null;
                    break;
                case AnimationType.ChargeAttack:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.chargeAttack != null && character.hairData.animationData.chargeAttack.Length != 0) ? character.hairData.animationData.chargeAttack : null;
                    break;
                case AnimationType.Attack:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.attack != null && character.hairData.animationData.attack.Length != 0) ? character.hairData.animationData.attack : null;
                    break;
                case AnimationType.Logging:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.logging != null && character.hairData.animationData.logging.Length != 0) ? character.hairData.animationData.logging : null;
                    break;
                case AnimationType.Mining:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.mining != null && character.hairData.animationData.mining.Length != 0) ? character.hairData.animationData.mining : null;
                    break;
                case AnimationType.Harvesting:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.harvesting != null && character.hairData.animationData.harvesting.Length != 0) ? character.hairData.animationData.harvesting : null;
                    break;
                case AnimationType.WoodWork:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.woodWork != null && character.hairData.animationData.woodWork.Length != 0) ? character.hairData.animationData.woodWork : null;
                    break;
                case AnimationType.JewelryWork:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.jewelryWork != null && character.hairData.animationData.jewelryWork.Length != 0) ? character.hairData.animationData.jewelryWork : null;
                    break;
                case AnimationType.BlacksmithWork:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.blacksmithWork != null && character.hairData.animationData.blacksmithWork.Length != 0) ? character.hairData.animationData.blacksmithWork : null;
                    break;
                case AnimationType.AlchemyWork:
                    hairSprites = (character.glovesData != null && character.hairData.animationData.alchemyWork != null && character.hairData.animationData.alchemyWork.Length != 0) ? character.hairData.animationData.alchemyWork : null;
                    break;
                default:
                    break;
            }
        }

        private void Character_OnHatEquipped(object sender, System.EventArgs e) {
            // Instantly update currently used sprites for current animation sequence when changing equipped items
            switch (currentAnimationType) {
                case AnimationType.Idle:
                    hatSprites = (character.hatData != null && character.hatData.animationData.idle != null && character.hatData.animationData.idle.Length != 0) ? character.hatData.animationData.idle : null;
                    break;
                case AnimationType.Walk:
                    hatSprites = (character.hatData != null && character.hatData.animationData.walk != null && character.hatData.animationData.walk.Length != 0) ? character.hatData.animationData.walk : null;
                    break;
                case AnimationType.Death:
                    hatSprites = (character.hatData != null && character.hatData.animationData.death != null && character.hatData.animationData.death.Length != 0) ? character.hatData.animationData.death : null;
                    break;
                case AnimationType.Damage:
                    hatSprites = (character.hatData != null && character.hatData.animationData.damage != null && character.hatData.animationData.damage.Length != 0) ? character.hatData.animationData.damage : null;
                    break;
                case AnimationType.ChargeAttack:
                    hatSprites = (character.hatData != null && character.hatData.animationData.chargeAttack != null && character.hatData.animationData.chargeAttack.Length != 0) ? character.hatData.animationData.chargeAttack : null;
                    break;
                case AnimationType.Attack:
                    hatSprites = (character.hatData != null && character.hatData.animationData.attack != null && character.hatData.animationData.attack.Length != 0) ? character.hatData.animationData.attack : null;
                    break;
                case AnimationType.Logging:
                    hatSprites = (character.hatData != null && character.hatData.animationData.logging != null && character.hatData.animationData.logging.Length != 0) ? character.hatData.animationData.logging : null;
                    break;
                case AnimationType.Mining:
                    hatSprites = (character.hatData != null && character.hatData.animationData.mining != null && character.hatData.animationData.mining.Length != 0) ? character.hatData.animationData.mining : null;
                    break;
                case AnimationType.Harvesting:
                    hatSprites = (character.hatData != null && character.hatData.animationData.harvesting != null && character.hatData.animationData.harvesting.Length != 0) ? character.hatData.animationData.harvesting : null;
                    break;
                case AnimationType.WoodWork:
                    hatSprites = (character.hatData != null && character.hatData.animationData.woodWork != null && character.hatData.animationData.woodWork.Length != 0) ? character.hatData.animationData.woodWork : null;
                    break;
                case AnimationType.JewelryWork:
                    hatSprites = (character.hatData != null && character.hatData.animationData.jewelryWork != null && character.hatData.animationData.jewelryWork.Length != 0) ? character.hatData.animationData.jewelryWork : null;
                    break;
                case AnimationType.BlacksmithWork:
                    hatSprites = (character.hatData != null && character.hatData.animationData.blacksmithWork != null && character.hatData.animationData.blacksmithWork.Length != 0) ? character.hatData.animationData.blacksmithWork : null;
                    break;
                case AnimationType.AlchemyWork:
                    hatSprites = (character.hatData != null && character.hatData.animationData.alchemyWork != null && character.hatData.animationData.alchemyWork.Length != 0) ? character.hatData.animationData.alchemyWork : null;
                    break;
                default:
                    break;
            }
        }

        private void Character_OnMaskEquipped(object sender, System.EventArgs e) {
            // Instantly update currently used sprites for current animation sequence when changing equipped items
            switch (currentAnimationType) {
                case AnimationType.Idle:
                    maskSprites = (character.maskData != null && character.maskData.animationData.idle != null && character.maskData.animationData.idle.Length != 0) ? character.maskData.animationData.idle : null;
                    break;
                case AnimationType.Walk:
                    maskSprites = (character.maskData != null && character.maskData.animationData.walk != null && character.maskData.animationData.walk.Length != 0) ? character.maskData.animationData.walk : null;
                    break;
                case AnimationType.Death:
                    maskSprites = (character.maskData != null && character.maskData.animationData.death != null && character.maskData.animationData.death.Length != 0) ? character.maskData.animationData.death : null;
                    break;
                case AnimationType.Damage:
                    maskSprites = (character.maskData != null && character.maskData.animationData.damage != null && character.maskData.animationData.damage.Length != 0) ? character.maskData.animationData.damage : null;
                    break;
                case AnimationType.ChargeAttack:
                    maskSprites = (character.maskData != null && character.maskData.animationData.chargeAttack != null && character.maskData.animationData.chargeAttack.Length != 0) ? character.maskData.animationData.chargeAttack : null;
                    break;
                case AnimationType.Attack:
                    maskSprites = (character.maskData != null && character.maskData.animationData.attack != null && character.maskData.animationData.attack.Length != 0) ? character.maskData.animationData.attack : null;
                    break;
                case AnimationType.Logging:
                    maskSprites = (character.maskData != null && character.maskData.animationData.logging != null && character.maskData.animationData.logging.Length != 0) ? character.maskData.animationData.logging : null;
                    break;
                case AnimationType.Mining:
                    maskSprites = (character.maskData != null && character.maskData.animationData.mining != null && character.maskData.animationData.mining.Length != 0) ? character.maskData.animationData.mining : null;
                    break;
                case AnimationType.Harvesting:
                    maskSprites = (character.maskData != null && character.maskData.animationData.harvesting != null && character.maskData.animationData.harvesting.Length != 0) ? character.maskData.animationData.harvesting : null;
                    break;
                case AnimationType.WoodWork:
                    maskSprites = (character.maskData != null && character.maskData.animationData.woodWork != null && character.maskData.animationData.woodWork.Length != 0) ? character.maskData.animationData.woodWork : null;
                    break;
                case AnimationType.JewelryWork:
                    maskSprites = (character.maskData != null && character.maskData.animationData.jewelryWork != null && character.maskData.animationData.jewelryWork.Length != 0) ? character.maskData.animationData.jewelryWork : null;
                    break;
                case AnimationType.BlacksmithWork:
                    maskSprites = (character.maskData != null && character.maskData.animationData.blacksmithWork != null && character.maskData.animationData.blacksmithWork.Length != 0) ? character.maskData.animationData.blacksmithWork : null;
                    break;
                case AnimationType.AlchemyWork:
                    maskSprites = (character.maskData != null && character.maskData.animationData.alchemyWork != null && character.maskData.animationData.alchemyWork.Length != 0) ? character.maskData.animationData.alchemyWork : null;
                    break;
                default:
                    break;
            }
        }
    }
}
