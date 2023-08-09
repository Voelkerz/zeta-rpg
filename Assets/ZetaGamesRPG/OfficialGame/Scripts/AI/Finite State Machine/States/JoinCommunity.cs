using UnityEngine;

namespace ZetaGames.RPG {
    public class JoinCommunity : State {
        public override float actionScore { get; set; } = 30;
        public override bool isFinished { get => finished; }
        public override bool isInterruptable { get => npc.inCombat; }

        private bool finished;
        private readonly AIBrain npc;
        public Settlement targetCommunity;
        public bool canCreateNewCommunity = true;
        private int bonusChance = 0;
        public bool hasCommunity;

        public JoinCommunity(AIBrain npc) {
            this.npc = npc;
        }

        public override void OnEnter() {
            if (hasCommunity) {
                SetStateFinished();
            } else {
                if (canCreateNewCommunity) {
                    CreateNewCommunity(bonusChance);
                }
            }
        }

        public override void OnExit() {
            finished = false;
            targetCommunity = null;
        }

        public override void Tick() {
            if (!finished) {
                // If we have a target community move towards it, else get one.
                if (targetCommunity != null) {
                    // Target settlement and destination set. Move towards settlement.
                    if (npc.pathMovement.isStopped && Vector3.Distance(npc.transform.position, targetCommunity.bulletinBoardPos) <= 2f) {
                        if (npc.stats.settlement != null) {
                            // Created a settlement and at entrance
                            SetStateFinished();
                        } else {
                            // At the community board and attempt to join settlement
                            npc.stats.settlement = CommunityManager.Instance.JoinCommunity(targetCommunity.settlementName, npc.gameObject);

                            if (npc.stats.settlement != null) {
                                // Joined community
                                SetStateFinished();
                            } else {
                                // If unable to join, get a new target community
                                targetCommunity = null;
                            }
                        }
                    }
                } else {
                    // Find a random settlement to join. Null value means no viable communities.
                    targetCommunity = CommunityManager.Instance.GetRandomSettlement();
                    
                    if (targetCommunity != null) {
                        // Set NPC destination to move towards community
                        npc.pathMovement.destination = targetCommunity.bulletinBoardPos;
                        npc.pathMovement.SearchPath();
                    } else {
                        // Start a new community, no viable ones available.
                        canCreateNewCommunity = true;
                        bonusChance = 100;
                    }
                }

                // If allowed to start new settlement, give chance.
                if (canCreateNewCommunity) {
                    CreateNewCommunity(bonusChance);
                }
            }
        }

        private void SetStateFinished() {
            hasCommunity = true;
            finished = true;
        }

        private void CreateNewCommunity(int additionalChance) {
            // Give chance to create a new community
            if (CommunityManager.Instance.settlementList.Count > 0) {
                if (Random.Range(0, 100f) <= npc.personality.startCommunityChance + additionalChance && CommunityManager.Instance.viableRegions.Count > 0) {
                    npc.stats.settlement = CommunityManager.Instance.CreateNewSettlement(npc.gameObject);
                    targetCommunity = npc.stats.settlement;
                    Debug.Log("Creating a new settlement: " + npc.stats.settlement.settlementName + " || Viable Regions: " + CommunityManager.Instance.viableRegions.Count);
                }
            } else {
                npc.stats.settlement = CommunityManager.Instance.CreateNewSettlement(npc.gameObject);
                targetCommunity = npc.stats.settlement;
                Debug.Log("Creating first settlement: " + npc.stats.settlement.settlementName + " || Viable Regions: " + CommunityManager.Instance.viableRegions.Count);
            }

            if (targetCommunity != null) {
                // Set NPC destination to move towards community
                npc.pathMovement.destination = targetCommunity.bulletinBoardPos;
                npc.pathMovement.SearchPath();
            }

            canCreateNewCommunity = false;
            bonusChance = 0;
        }
    }
}
