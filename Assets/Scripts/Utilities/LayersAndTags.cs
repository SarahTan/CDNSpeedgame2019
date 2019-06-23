/// <summary>
/// Don't use this enum in serialized content as the values may change as the layers do.
/// </summary>
public enum Layers
{
    Default = 0,
    TransparentFX = 1,
    IgnoreRaycast = 2,
    Water = 4,
    UI = 5,
    Enemy = 8,
    EnemySegment = 9,
    Player = 10,
}

/// <summary>
/// Don't use this enum in serialized content as the values may change as the layers do.
/// </summary>
public enum LayerMasks
{
    Enemy = 1 << Layers.Enemy,
    EnemySegmeny = 1 << Layers.EnemySegment,
    LaserBlocker = (1 << Layers.Enemy) | (1 << Layers.EnemySegment),
}

public static class Tags
{
    public const string Enemy = "Enemy";
}