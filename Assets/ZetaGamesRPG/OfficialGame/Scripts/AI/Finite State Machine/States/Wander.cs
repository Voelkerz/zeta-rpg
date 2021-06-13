using UnityEngine;

namespace ZetaGames.RPG {
    public class Wander : State {
        public override int priority => 0;
        public override bool isFinished => finished;
        public override bool isInterruptable => true; //{ get => npcBrain.inCombat || cycleCount > 1; } // state will not be interrupted until specified full 'wandering' cycles are finished (unless combat is initiated)
        

        private bool finished;
        private AIBrain npc;
        public float wanderRadius;
        public float wanderCycle;
        private float tickTimer;
        Vector3 destination;
        Vector3 homeDoorPos;

        public Wander(AIBrain npcBrain) {
            this.npc = npcBrain;
            wanderRadius = npcBrain.personality.wanderRadius;
            wanderCycle = npcBrain.personality.wanderCycle;
        }

        public override void Tick() {
            tickTimer += npc.deltaTime;
            
            if (tickTimer >= wanderCycle) {
                tickTimer = 0;

                if (npc.pathMovement.isStopped && Vector3.Distance(npc.transform.position, destination) <= 2f) {
                    if (npc.stats.homePropertyData != null) {
                        // Go visit other settlements and then walk back home!
                        if (homeDoorPos == null || homeDoorPos == Vector3.zero) {
                            homeDoorPos = (Vector3)npc.memory.RetrieveMemory(ZetaUtilities.MEMORY_LOCATION_HOME);
                            homeDoorPos.x += npc.stats.homePropertyData.doorTile.x;
                            homeDoorPos.y += npc.stats.homePropertyData.doorTile.y;
                        }
                        
                        if (homeDoorPos != Vector3.zero && Vector3.Distance(npc.transform.position, homeDoorPos) >= 2f) {
                            // If not at home, go home!
                            destination = homeDoorPos;
                            npc.pathMovement.destination = destination;
                            npc.pathMovement.SearchPath();

                            if (npc.debugLogs) {
                                Debug.Log("Wander.Tick(): Just wandering to home at (" + destination.x + ", " + destination.y + ").");
                            }
                        } else if (homeDoorPos != Vector3.zero) {
                            // If at home, pick a random settlement to visit
                            int rng = Random.Range(0, CommunityManager.Instance.settlementList.Count);
                            destination = CommunityManager.Instance.settlementList[rng].bulletinBoardPos;
                            npc.pathMovement.destination = destination;
                            npc.pathMovement.SearchPath();

                            if (npc.debugLogs) {
                                Debug.Log("Wander.Tick(): Just wandering to a random settlment at (" + destination.x + ", " + destination.y + ").");
                            }
                        } else {
                            Debug.LogError("Wander.Tick(): Home door position is not set.");
                        }
                    } else {
                        // Wander in a random direction
                        destination = new Vector3(npc.transform.position.x + Random.Range(-wanderRadius, wanderRadius), npc.transform.position.y + Random.Range(-wanderRadius, wanderRadius));

                        if ((int)destination.x < MapManager.Instance.mapWidth && (int)destination.y < MapManager.Instance.mapHeight && (int)destination.x >= 0 && (int)destination.y >= 0) {
                            WorldTile destinationTile = MapManager.Instance.GetWorldTileGrid().GetGridObject(destination);
                            if (destinationTile != null && destinationTile.walkable && destinationTile.tileObstacle == null) {
                                npc.pathMovement.destination = destination;
                                npc.pathMovement.SearchPath();

                                if (npc.debugLogs) {
                                    Debug.Log("Wander.Tick(): Just wandering to (" + destination.x + ", " + destination.y + "), nothing better to do...");
                                }
                            } else {
                                destination = npc.transform.position;
                            }
                        } else {
                            destination = npc.transform.position;
                        }
                    }
                } 
            } 
        }

        public override void OnEnter() {
            tickTimer = 0;
            destination = npc.transform.position;
            npc.wanderCooldown = 0;
        }

        public override void OnExit() {
            homeDoorPos = Vector3.zero;
            destination = Vector3.zero;
        }
    }
}

