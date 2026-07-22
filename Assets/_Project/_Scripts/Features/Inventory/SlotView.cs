using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SlotView : MonoBehaviour
{
    public Image selectedImage;
    public Image unselectedImage;
    public Image itemImage;
    public TMP_Text amountText;

    public void UpdateView(bool isSelected, ItemConfig item, int amount)
    {
        SetSelected(isSelected);

        if (item != null)
        {
            SetIcon(item.icon);
            UpdateAmountText(item is BlockConfig, amount);
        }
        else
        {
            itemImage.sprite = null;
            itemImage.gameObject.SetActive(false);
            UpdateAmountText(false);
        }
    }

    private void SetSelected(bool isSelected)
    {
        selectedImage.gameObject.SetActive(isSelected);
        unselectedImage.gameObject.SetActive(!isSelected);
    }

    private void SetIcon(Sprite image)
    {
        itemImage.sprite = image;
    }
    
    private void UpdateAmountText(bool isEnabled, int amount)
    {
        if (isEnabled)
        {
            amountText.text = amount.ToString();
        }
        else
        {
            amountText.text = "";
        }
    }
    
    private void UpdateAmountText(bool isEnabled)
    {
        if (isEnabled)
        {
            amountText.text = "0";
        }
        else
        {
            amountText.text = "";
        }
    }
}
