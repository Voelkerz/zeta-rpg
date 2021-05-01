namespace ZetaGames.RPG {
    public class OakTreePool : ObjectPool {

        public static OakTreePool SharedInstance;

        private void Awake() {
            SharedInstance = this;
        }
    }
}
