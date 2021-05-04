namespace ZetaGames.RPG {
    public enum ResourceCategory {
        None,

        // Resource
        Wood,
        Ore,
        Gem,
        Fiber,
        Herb,
        Stone,
        Coin,
        Food
    }

    public enum ResourceType {
        None,

        // Stone
        Rock,

        // Wood
        Oak,
        Hickory,
        Pine,
        Willow,
        Birch,

        // Ore
        Coal,
        Tin,
        Iron,
        Copper,
        Silver,
        Gold,

        // Gem
        Amethyst,
        Diamond,
        Emerald,
        Ruby,
        Topaz,

        // Fiber
        Flax,
        Cotton,
        Hemp,
        Ramie,
        Agave,

        // Herb
        Mandrake,
        Mushroom,
        Henbane,
        Belladonna,
        Poppy
    }

    public enum ResourceState {
        Raw,
        Refined
    }
}
