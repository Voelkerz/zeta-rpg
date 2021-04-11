namespace ZetaGames.RPG {
    public interface IState {
        // mark something as interruptable if you want states to change as soon as a transition's conditions are true
        bool isInterruptable { get; }
        // if something is NOT interruptable, then make sure to set a finished state
        bool isFinished { get; }
        void Tick();
        void OnEnter();
        void OnExit();
    }
}

