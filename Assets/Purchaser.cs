using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;

public class Purchaser : MonoBehaviour, IStoreListener
{
    public static Purchaser instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public CashAndExp cashAndExp;
    public Text buy1, buy2, buy3;

    private static IStoreController m_StoreController;
    private static IExtensionProvider m_StoreExtensionProvider;

    private static string cashsTier1 = "5000cashs";
    private static string cashsTier2 = "10000cashs";
    private static string cashsTier3 = "50000cashs";

    void Start()
    {
        if (m_StoreController == null)
        {
            InitializePurchasing();
        }
    }

    public void InitializePurchasing()
    {
        if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(cashsTier1, ProductType.Consumable);
        builder.AddProduct(cashsTier2, ProductType.Consumable);
        builder.AddProduct(cashsTier3, ProductType.Consumable);

        UnityPurchasing.Initialize(this, builder);

        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(cashsTier1);
            buy1.text = product.metadata.localizedPriceString;
            product = m_StoreController.products.WithID(cashsTier2);
            buy2.text = product.metadata.localizedPriceString;
            product = m_StoreController.products.WithID(cashsTier3);
            buy3.text = product.metadata.localizedPriceString;
        }
    }

    private bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }

    //Callable Methods
    public void BuyConsumable_5000Cash()
    {
        BuyProductID(cashsTier1);
    }

    public void BuyConsumable_10000Cash()
    {
        BuyProductID(cashsTier2);
    }

    public void BuyConsumable_50000Cash()
    {
        BuyProductID(cashsTier3);
    }
    //

    void BuyProductID(string productId)
    {
        if (IsInitialized())
        {
            Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));

                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
            }
        }
        else
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
            TextPopup.instance.GeneratePopup("PURCHASE FAILED: Not Initialized");
        }
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        Debug.Log("OnInitialized: PASS");

        m_StoreController = controller;

        m_StoreExtensionProvider = extensions;
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        if (String.Equals(args.purchasedProduct.definition.id, cashsTier1, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

            cashAndExp.ChangeInCashAmount(5000);
            TextPopup.instance.GeneratePopup("PURCHASE SUCCESSFUL");
        }
        else if (String.Equals(args.purchasedProduct.definition.id, cashsTier2, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

            cashAndExp.ChangeInCashAmount(10000);
            TextPopup.instance.GeneratePopup("PURCHASE SUCCESSFUL");
        }
        else if (String.Equals(args.purchasedProduct.definition.id, cashsTier3, StringComparison.Ordinal))
        {
            Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));

            cashAndExp.ChangeInCashAmount(50000);
            TextPopup.instance.GeneratePopup("PURCHASE SUCCESSFUL");
        }
        else
        {
            Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));

            TextPopup.instance.GeneratePopup("PURCHASE FAILED: Unrecongnized product");
        }

        return PurchaseProcessingResult.Complete;
    }

    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));

        TextPopup.instance.GeneratePopup("PURCHASE FAILED: " + failureReason);
    }
}
