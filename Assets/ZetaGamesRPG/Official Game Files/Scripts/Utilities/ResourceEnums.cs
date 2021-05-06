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
        Small,
        Large,

        // Wood
        Oak,        //grass
        Hickory,    //eastern grass
        Pine,       //mountain
        Willow,     //swamp
        Birch,      //northern grass

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
        Flax,       //grass
        Cotton,     //swamp
        Hemp,       //grass
        Ramie,      //mountain
        Agave,      //desert

        // Herb
        Mandrake,   //swamp
        Mushroom,   //anywhere
        Henbane,    //desert
        Belladonna, //grass
        Poppy       //grass
    }

    public enum ResourceState {
        Raw,
        Refined
    }
}
