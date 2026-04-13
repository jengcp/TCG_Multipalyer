using UnityEngine;

namespace TCG.Items
{
    public enum CosmeticType { CardBack, Avatar, BoardSkin, CardSleeve, Emote }

    /// <summary>
    /// ScriptableObject for a cosmetic item (card backs, avatars, board skins).
    /// Cosmetics are not stackable and can only be owned once.
    /// </summary>
    [CreateAssetMenu(fileName = "New Cosmetic", menuName = "TCG/Items/Cosmetic")]
    public class CosmeticData : ItemData
    {
        [Header("Cosmetic Details")]
        public CosmeticType cosmeticType;
        public Sprite previewImage;
        [Tooltip("Prefab or asset used to apply the cosmetic in-game.")]
        public GameObject cosmeticPrefab;

        private void Awake()
        {
            itemType = ItemType.Cosmetic;
            isStackable = false;
            maxStack = 1;
        }
    }
}
