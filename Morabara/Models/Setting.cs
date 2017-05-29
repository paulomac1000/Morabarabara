namespace Morabara.Models
{
    public static class Setting
    {
        public static int BallRadius { get; } = 24;
        public static int BoardMarginX { get; } = 60;
        public static int BoardMarginY { get; } = 60;
        public static int NumberOfPlayerBall { get; } = 9;
        public static string PlayerName { get; set; } = "Player";
    }
}