using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MerchantItemController : MonoBehaviour
{
    public List<ItemSO> itemList = new List<ItemSO>(); // ItemSO t�r�nde bir liste
    public GameObject itemPrefab; // Item prefab'i
    public Transform content; // ScrollView'in content nesnesi

    // Start is called before the first frame update
    void Start()
    {
        PopulateItems();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PopulateItems()
    {
        foreach (ItemSO itemSO in itemList)
        {
            GameObject newItem = Instantiate(itemPrefab, content);
            Item itemComponent = newItem.GetComponent<Item>();
            itemComponent.itemSO = itemSO;
            itemComponent.SetupItem();
        }
    }
}
