using UnityEngine;
using UnityEngine.UI;
using TCG.Currency;
using TCG.Inventory;

namespace TCG.Shop
{
    /// <summary>
    /// Scene-level MonoBehaviour that bootstraps the shop systems.
    /// Attach to a root GameObject in the Shop scene along with
    /// CurrencyManager, PlayerInventory, and ShopManager.
    /// </summary>
    public class ShopController : MonoBehaviour
    {
        [Header("Open / Close")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button     openShopButton;
        [SerializeField] private Button     closeShopButton;

        [Header("Required Managers (auto-resolved if null)")]
        [SerializeField] private CurrencyManager  currencyManager;
        [SerializeField] private PlayerInventory  playerInventory;
        [SerializeField] private ShopManager      shopManager;

        private void Awake()
        {
            openShopButton?.onClick.AddListener(OpenShop);
            closeShopButton?.onClick.AddListener(CloseShop);

            // Auto-resolve if not wired in Inspector
            currencyManager ??= CurrencyManager.Instance;
            playerInventory ??= PlayerInventory.Instance;
            shopManager     ??= ShopManager.Instance;

            if (currencyManager == null)
                Debug.LogError("[ShopController] CurrencyManager not found. Add it to the scene.");
            if (playerInventory == null)
                Debug.LogError("[ShopController] PlayerInventory not found. Add it to the scene.");
            if (shopManager == null)
                Debug.LogError("[ShopController] ShopManager not found. Add it to the scene.");
        }

        private void Start()
        {
            CloseShop();
        }

        public void OpenShop()
        {
            if (shopPanel != null) shopPanel.SetActive(true);
        }

        public void CloseShop()
        {
            if (shopPanel != null) shopPanel.SetActive(false);
        }
    }
}
