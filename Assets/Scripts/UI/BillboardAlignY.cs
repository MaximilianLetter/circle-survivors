using UnityEngine;

public class BillboardAlignY : MonoBehaviour
{
    private void Start()
    {
        //Vector3 directionToLookAt = -Camera.main.transform.forward;
        Vector3 directionToLookAt = Quaternion.AngleAxis(45, Vector3.up) * -Vector3.forward;

        directionToLookAt.y = 0;

        if (directionToLookAt != Vector3.zero)
        {
            transform.SetParent(null, true);

            transform.rotation = Quaternion.LookRotation(directionToLookAt);
        }
    }
}
