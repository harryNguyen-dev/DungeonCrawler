using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    public int health;

    public void TakeDamage(int damage)
    {
        health -= damage;
    }
}
