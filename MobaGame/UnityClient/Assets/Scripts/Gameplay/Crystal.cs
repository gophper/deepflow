using UnityEngine;

namespace MobaGame.Gameplay
{
    public class Crystal : MonoBehaviour
    {
        [Header("Crystal Configuration")]
        public int teamId; // 0 = Blue, 1 = Red
        public float maxHealth = 5000f;
        public float currentHealth;

        void Start()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            currentHealth -= damage;

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        void Die()
        {
            // Game Over - notify game manager
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                gameManager.EndGame(teamId == 0 ? 1 : 0);
            }

            Destroy(gameObject);
        }
    }
}
