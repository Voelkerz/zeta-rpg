namespace ZetaGames.RPG {
    public abstract class State {
        
        public abstract float actionScore { get; set; }
        public abstract bool isFinished { get; } // if something is NOT interruptable, then make sure to set a finished state
        public abstract bool isInterruptable { get; } // mark something as interruptable if you want states to change as soon as a transition's conditions are true


        public abstract void Tick();
        public abstract void OnEnter();
        public abstract void OnExit();
    }
}

