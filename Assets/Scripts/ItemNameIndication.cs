using UnityEngine;
using UnityEngine.UI;

public class ItemNameIndication : MonoBehaviour
{
    [SerializeField] private Text itemNameText;
    [SerializeField] private Animator animator;

    public void UpdateIndication(string itemName, bool isDiscovered)
    {
        // Si l'objet a déjà été dévouvert, on affiche son nom
        itemNameText.text = isDiscovered ? itemName : "???";
    }

    public void DestroySelf()
    {
        animator.SetTrigger("Hide");

        Destroy(gameObject, 0.2f);
    }

}
