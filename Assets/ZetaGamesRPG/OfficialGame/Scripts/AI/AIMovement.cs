using UnityEngine;
using Pathfinding;

namespace ZetaGames.RPG {
    public class AIMovement : AILerp {

        private AnimationController animationController;
        private float baseSpeed = 2;
        private WorldTile currentTile;
        private WorldTile previousTile;
        WorldTile neighbor1;
        WorldTile neighbor2;
        private Vector3 direction;
        private Vector3 previousDirection = Vector3.zero;
        public bool useSimplePathing;
        private float tickTimer;

        protected override void Awake() {
            base.Awake();
            animationController = GetComponent<AnimationController>();
        }

        protected override void Start() {
            baseSpeed = speed;
            isStopped = true;
            base.Start();
        }

        public override void OnTargetReached() {
            isStopped = true;
            animationController.PlayAnimation(AnimationType.Idle);

            /*
            if (!useSimplePathing) {
                currentTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(transform.position);
                currentTile.walkable = false;

                AstarPath.active.AddWorkItem(ctx => {
                    var grid = AstarPath.active.data.gridGraph;
                    int x = currentTile.x;
                    int y = currentTile.y;

                    // Mark a single node as unwalkable
                    grid.GetNode(x, y).Walkable = false;

                    // Recalculate the connections for that node as well as its neighbours
                    grid.CalculateConnectionsForCellAndNeighbours(x, y);
                });
            }
            */
        }

        public override void SearchPath() {
            /*
            if (!useSimplePathing) {
                currentTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(transform.position);
                currentTile.walkable = true;

                AstarPath.active.AddWorkItem(ctx => {
                    var grid = AstarPath.active.data.gridGraph;
                    int x = currentTile.x;
                    int y = currentTile.y;

                    // Mark a single node as walkable
                    grid.GetNode(x, y).Walkable = true;

                    // Recalculate the connections for that node as well as its neighbours
                    grid.CalculateConnectionsForCellAndNeighbours(x, y);
                });
            }
            */

            base.SearchPath();
            isStopped = false;
            animationController.PlayAnimation(AnimationType.Walk);
        }

        protected override void Update() {
            base.Update();

            tickTimer += Time.deltaTime;

            if (tickTimer >= 0.2f) {
                // No animation if off screen
                animationController.isPlaying = (!useSimplePathing) ? true : false;

                // If moving, do stuff
                if (!isStopped && path != null && remainingDistance > 0.1) {
                    CalculateNextPosition(out direction, Time.deltaTime);
                    
                    // Do char animation if on-screen
                    if (!useSimplePathing) {
                        animationController.StartPlaying();
                        animationController.animMoveX = direction.normalized.x;
                        animationController.animMoveY = direction.normalized.y;
                    } else {
                        animationController.StopPlaying();
                    }

                    // If previous tile is different than current tile
                    currentTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(transform.position);

                    if (currentTile != null) {
                        if (previousTile != null && previousTile != currentTile) {
                            speed = baseSpeed * (currentTile.speedPercent / 100);

                            if (currentTile.terrainType.Equals(ZetaUtilities.TERRAIN_GRASS) || currentTile.terrainType.Equals(ZetaUtilities.TERRAIN_DIRT_PATH)) {
                                currentTile.AddTrampleAmount(1f);
                                currentTile.TrampleSpread(1f);
                            }
                        }

                        previousTile = currentTile;
                    }

                    previousDirection = direction;
                } else if (!isStopped) {

                }

                tickTimer = 0;
            }
        }

        public void SetPreviousDirection(Vector3 previousDirection) {
            this.previousDirection = previousDirection;
        }
    }
}
