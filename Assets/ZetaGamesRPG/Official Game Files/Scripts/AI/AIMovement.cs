using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace ZetaGames.RPG {
    public class AIMovement : AILerp {

        private static readonly int animShouldMove = Animator.StringToHash("AnimShouldMove");
        private static readonly int animMoveX = Animator.StringToHash("AnimMoveX");
        private static readonly int animMoveY = Animator.StringToHash("AnimMoveY");
        private static readonly int animLastMoveX = Animator.StringToHash("AnimLastMoveX");
        private static readonly int animLastMoveY = Animator.StringToHash("AnimLastMoveY");
        private Animator animator;
        private float baseSpeed = 2;
        private WorldTile currentTile;
        private Vector3 direction, previousDirection;

        protected override void Awake() {
            base.Awake();
            animator = GetComponentInChildren<Animator>();
        }

        protected override void Start() {
            baseSpeed = speed;
            isStopped = true;
            base.Start();
        }

        public override void OnTargetReached() {
            isStopped = true;
        }

        public override void SearchPath() {
            isStopped = false;
            base.SearchPath();
        }

        protected override void Update() {
            base.Update();
            
            if (!isStopped && path != null && remainingDistance > 0.1) {
                currentTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(transform.position);

                if (currentTile != null) {
                    speed = baseSpeed * (currentTile.speedPercent / 100);
                }
                
                CalculateNextPosition(out direction, Time.deltaTime);

                previousDirection = direction;

                animator.SetFloat(animMoveX, direction.normalized.x);
                animator.SetFloat(animMoveY, direction.normalized.y);
                animator.SetBool(animShouldMove, true);
            } else if (!isStopped) {
                animator.SetFloat(animLastMoveX, previousDirection.normalized.x);
                animator.SetFloat(animLastMoveY, previousDirection.normalized.y);
                animator.SetBool(animShouldMove, false);
            } else {
                //animator.SetBool(animShouldMove, !isStopped);
            }
        }
    }
}
