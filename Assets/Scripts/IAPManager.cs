using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;

public class IAPManager : IStoreListener
{
    private static IStoreController m_StoreController;
    private static ITransactionHistoryExtensions m_TransactionHistoryExtensions;

    private static bool m_PurchaseInProgress;

    static Dictionary<string, Action> callbacks = new Dictionary<string, Action>();

    public void Initlize()
    {
        if (IsInitialized()) return;

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
        builder.AddProduct(ProductIDs.lite.ToString(), ProductType.Subscription);
        builder.AddProduct(ProductIDs.vip.ToString(), ProductType.Subscription);
        builder.AddProduct(ProductIDs.vipGift.ToString(), ProductType.Consumable);
        UnityPurchasing.Initialize(this, builder);
    }

    static bool IsInitialized()
    {
        return m_StoreController != null;
    }

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        m_TransactionHistoryExtensions = extensions.GetExtension<ITransactionHistoryExtensions>();

        var lite = m_StoreController.products.WithID(ProductIDs.lite.ToString());
        if(lite.hasReceipt && ValidateReceipt(lite))
        {
            FirebaseManager.user.isLite = true;
        }
        var vip = m_StoreController.products.WithID(ProductIDs.vip.ToString());
        if (vip.hasReceipt && ValidateReceipt(vip))
        {
            FirebaseManager.user.isVIP = true;
        }

        // Check gifts
        foreach (var gift in FirebaseManager.user.gifts)
        {
            if(DateTime.Now < gift.expiryDate && gift.product == ProductIDs.vipGift.ToString())
            {
                FirebaseManager.user.isVIP = true;
            }
        }
    }

    public void OnInitializeFailed(InitializationFailureReason error)
    {
        ErrorHandler.Show("Cannot initialize purchases: " + error);
    }

    public void OnPurchaseFailed(Product item, PurchaseFailureReason r)
    {
        Debug.Log("Purchase failed: " + item.definition.id);
        Debug.Log(r);

        // Detailed debugging information
        Debug.Log("Store specific error code: " + m_TransactionHistoryExtensions.GetLastStoreSpecificPurchaseErrorCode());
        if (m_TransactionHistoryExtensions.GetLastPurchaseFailureDescription() != null)
        {
            Debug.Log("Purchase failure description message: " +
                      m_TransactionHistoryExtensions.GetLastPurchaseFailureDescription().message);
        }

        m_PurchaseInProgress = false;
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        string id = e.purchasedProduct.definition.id;
        if (id == ProductIDs.lite.ToString())
        {
            FirebaseManager.user.isLite = true;
        }
        else if (id == ProductIDs.vip.ToString())
        {
            FirebaseManager.user.isVIP = true;
        }
        else if (id == ProductIDs.vipGift.ToString())
        {
            // No default behaviour
        }
        
        if(callbacks.ContainsKey(id)) callbacks[id]?.Invoke();
        
        m_PurchaseInProgress = false;

        return PurchaseProcessingResult.Complete;
    }

    bool ValidateReceipt(Product product)
    {
        if (!product.hasReceipt) return false;
        string receipt = product.receipt;


#if RECEIPT_VALIDATION
        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);

        try
        {
            // On Google Play, result has a single product ID.
            // On Apple stores, receipts contain multiple products.
            var result = validator.Validate(receipt);
            // For informational purposes, we list the receipt(s)
            Debug.Log("Receipt is valid. Contents:");
            foreach (IPurchaseReceipt productReceipt in result)
            {
                Debug.Log(productReceipt.productID);
                Debug.Log(productReceipt.purchaseDate);
                Debug.Log(productReceipt.transactionID);
            }
        }
        catch (IAPSecurityException)
        {
            Debug.Log("Invalid receipt, not unlocking content");
            return false;
        }
#endif

        return true;
    }

    public static Product GetProduct(string productId)
    {
        return m_StoreController.products.WithID(productId);
    }

    public static void BuyProductID(string productId, Action callback)
    {
        if (!IsInitialized())
        {
            Debug.Log("BuyProductID FAIL. Not initialized.");
            return;
        }

        if (m_PurchaseInProgress == true)
        {
            Debug.Log("Please wait, purchase in progress");
            return;
        }

        if (m_StoreController.products.WithID(productId) == null)
        {
            Debug.LogError("No product has id " + productId);
            return;
        }

        if (callbacks.ContainsKey(productId))
        {
            callbacks[productId] = callback;
        }
        else
        {
            callbacks.Add(productId, callback);
        }

        m_PurchaseInProgress = true;
        Product product = m_StoreController.products.WithID(productId);

        // If the look up found a product for this device's store and that product is ready to be sold ... 
        if (product != null && product.availableToPurchase)
        {
            Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
            // ... buy the product. Expect a response either through ProcessPurchase or OnPurchaseFailed 
            // asynchronously.
            m_StoreController.InitiatePurchase(product);
        }
        else
        {
            Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
        }
    }
}
