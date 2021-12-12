namespace StardewButYouSpeedup
{
    public class ModConfig
    {
        public float DefaultModifier { get; set; } = 1f;
        public float ModifierIncrement { get; set; } = 0.1f;
        public string ModifierOperation { get; set; } = "+";
        public float ModifierMaximum { get; set; } = -1f;
        public float ModifierMinimum { get; set; } = -1f;
        public bool IncreaseWithTileMovement { get; set; } = true;
        public int IncreaseEveryNTiles { get; set; } = 10;
        public bool IncreaseWithTime { get; set; } = false;
        public int IncreaseEveryNSeconds { get; set; } = 5;
    }
}
