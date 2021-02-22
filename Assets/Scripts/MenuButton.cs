using System.Collections;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Utilities;
using UnityEngine;

public class MenuButton : MonoBehaviour
{
    public void GoToMenu(GameObject newMenu)
    {
        newMenu.SetActive(true);
        Transform currentMenu = transform;
        while (!currentMenu.CompareTag("Menu"))
        {
            currentMenu = currentMenu.parent;
        }
        foreach (Transform sibling in currentMenu)
        {
            if (!sibling.CompareTag("Menu"))
            {
                sibling.gameObject.SetActive(false);
            }
        }
    }
    public void GoBack()
    {
        Transform menu = transform;
        while (!menu.CompareTag("Menu"))
        {
            menu = menu.parent;
            if (menu == null)
            {
                Debug.LogError("No \"Menu\" tagged parent found");
                return;
            }
        }
        if (menu.parent.tag != "Menu") {
            Debug.LogError("Menu parent is not tagged as \"Menu\"");
            return;
        }
        foreach(Transform sibling in menu.parent)
        {
                sibling.gameObject.SetActive(!sibling.CompareTag("Menu"));
        }
    }
}
