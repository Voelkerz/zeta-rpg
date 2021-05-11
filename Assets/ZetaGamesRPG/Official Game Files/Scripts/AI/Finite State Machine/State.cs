namespace ZetaGames.RPG {
    public abstract class State {
        // mark something as interruptable if you want states to change as soon as a transition's conditions are true
        public abstract bool IsInterruptable { get; }
        // if something is NOT interruptable, then make sure to set a finished state
        public abstract bool IsFinished { get; }
        public abstract void Tick();
        public abstract void OnEnter();
        public abstract void OnExit();
    }
}

