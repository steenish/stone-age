using UnityEngine;

public class WoodAging : MonoBehaviour {

    [SerializeField]
    private GameObject agingObject;

    public void PerformAging() {
        Debug.Log("Aging " + agingObject.name);
	}
}
