
namespace ZetaGames.RPG {
    internal class PlaceResourcesInStockpile : IState {
        public bool isFinished { get => finished; }
        public bool isInterruptable { get => true; }
        private bool finished;
        private readonly AIBrain npcBrain;

        public PlaceResourcesInStockpile(AIBrain npcBrain) {
            this.npcBrain = npcBrain;
        }

        public void Tick() {
        }

        public void OnEnter() { }

        public void OnExit() { }
    }
}

