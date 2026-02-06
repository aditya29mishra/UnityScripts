using UnityEngine;

public class outlineManger : MonoBehaviour
{
    public void outlineOff()
    {
        gameObject.GetComponent<Outline>().enabled = false;
    }
    public void outlineOn()
    {
        gameObject.GetComponent<Outline>().enabled = true;
    }
}
