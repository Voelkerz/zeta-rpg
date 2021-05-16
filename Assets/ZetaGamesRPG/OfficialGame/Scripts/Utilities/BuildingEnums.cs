namespace ZetaGames.RPG
{
    public enum EconomicClass {
        None,
        Poor,
        Lower,
        Middle,
        Upper,
        Rich
    }

    public enum StructureCategory {
        None,
        Home,
        Container,
        Municipal,
        Business
    }

    public enum StructureType {
        None,

        // Home
        Small_House,
        Medium_House,
        Large_House,

        // Business
        Small_Blacksmith,
        Small_Sawmill,       
        Small_Shop,
        Small_Inn,

        // Municipal
        Townhall
    }
}
