
using System.Collections.Generic;
using SO;
using UnityEngine;

namespace Global
{
    public enum GameMode
    {
        Lobby,
        InMatch
    }

    public static class GlobalVariable
    {
        public static int CurrentSeed { get; set; }
        public static Vector3 PlayerSpawnPosition { get; set; }
        public static int TotalRoomCount { get; set; }
        public static GameMode CurrentGameMode { get; set; } = GameMode.Lobby;

        /// <summary>Màn đang chọn từ lobby (null = dùng fallback trên scene).</summary>
        public static SO.LevelSO CurrentLevel { get; set; }

        /// <summary>Index 0-based trong LevelCatalog của màn hiện tại.</summary>
        public static int CurrentLevelIndex { get; set; } = -1;

        /// <summary>Sau khi thắng và về Hub, tự mở level select.</summary>
        public static bool OpenLevelSelectOnLobbyReturn { get; set; }

        /// <summary>Vũ khí đang equip (player-wide).</summary>
        public static string EquippedWeaponId { get; set; }

        // 1. Màu Nền của Thẻ (Dành cho Image Component làm Background - Tông tối để nổi bật chữ)
        public static readonly Dictionary<CardTier, Color> CardBackgroundColor = new Dictionary<CardTier, Color>()
        {
            // Xám đen rất nhẹ cho Common
            { CardTier.Common,    new Color(0.12f, 0.14f, 0.16f, 1f) }, // Hex tương đương: #1E2328
            // Xanh dương cực tối cho Rare
            { CardTier.Rare,      new Color(0.08f, 0.15f, 0.23f, 1f) }, // Hex tương đương: #14263B
            // Tím sẫm cho Epic
            { CardTier.Epic,      new Color(0.16f, 0.11f, 0.22f, 1f) }, // Hex tương đương: #291C38
            // Cam cháy/Nâu vàng tối cho Legendary
            { CardTier.Legendary, new Color(0.24f, 0.16f, 0.05f, 1f) }  // Hex tương đương: #3D290D
        };

        // 2. Màu Chữ của Tiêu Đề Thẻ (Dành cho TMP_Text - Màu sáng, rực rỡ theo đúng Tier)
        public static readonly Dictionary<CardTier, Color> CardTextColor = new Dictionary<CardTier, Color>()
        {
            // Trắng bạc
            { CardTier.Common,    new Color(0.88f, 0.88f, 0.88f, 1f) }, // #E0E0E0
            // Xanh lam sáng
            { CardTier.Rare,      new Color(0.20f, 0.60f, 0.86f, 1f) }, // #3498DB
            // Tím thạch anh sáng
            { CardTier.Epic,      new Color(0.61f, 0.35f, 0.71f, 1f) }, // #9B59B6
            // Vàng kim rực rỡ
            { CardTier.Legendary, new Color(0.95f, 0.61f, 0.07f, 1f) }  // #F39C12
        };
    }

}
