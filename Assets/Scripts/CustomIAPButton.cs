using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(Button))]
public class CustomIAPButton : MonoBehaviour {
	public ProductIDs id;

	public Text price;
	public UnityEvent onPurchase;

	void Start () {
		Button button = GetComponent<Button>();
		button.onClick.AddListener(RequestBuy);

		var product = IAPManager.GetProduct(id.ToString());
		if(price) price.text = product.metadata.localizedPriceString;
	}

	void RequestBuy()
    {
		IAPManager.BuyProductID(id.ToString(), onPurchase.Invoke);
	}
}
