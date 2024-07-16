using UnityEngine;

public class TargetObject : MonoBehaviour
{
    public HealthRestoreManager healthRestoreManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("GameController"))
        {
            healthRestoreManager.OnPlayerContact(this.gameObject);
        }
    }
}
