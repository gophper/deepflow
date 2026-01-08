using UnityEngine;
using MobaGame.Gameplay;

namespace MobaGame.Core
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        public Hero hero;
        public VirtualJoystick movementJoystick;

        [Header("Input")]
        private Vector3 moveDirection;
        private GameObject currentAttackTarget;

        void Start()
        {
            if (hero == null)
            {
                hero = GetComponent<Hero>();
            }
        }

        void Update()
        {
            HandleMovement();
        }

        void HandleMovement()
        {
            if (movementJoystick != null)
            {
                // Get joystick input
                Vector2 input = movementJoystick.GetInputDirection();
                moveDirection = new Vector3(input.x, 0, input.y);
            }
            else
            {
                // Keyboard fallback for testing in editor
                float horizontal = Input.GetAxis("Horizontal");
                float vertical = Input.GetAxis("Vertical");
                moveDirection = new Vector3(horizontal, 0, vertical);
            }

            if (hero != null && moveDirection.magnitude > 0.1f)
            {
                hero.Move(moveDirection);
            }
        }

        public void OnBasicAttackButton()
        {
            if (hero == null) return;

            // Find nearest enemy
            GameObject target = FindNearestEnemy();
            if (target != null)
            {
                hero.BasicAttack(target);
            }
        }

        public void OnSkill1Button()
        {
            if (hero == null) return;

            // Use skill at hero's forward position
            Vector3 targetPos = transform.position + transform.forward * 5f;
            hero.UseSkill1(targetPos);
        }

        public void OnSkill2Button()
        {
            if (hero == null) return;

            Vector3 targetPos = transform.position + transform.forward * 5f;
            hero.UseSkill2(targetPos);
        }

        public void OnUltimateButton()
        {
            if (hero == null) return;

            Vector3 targetPos = transform.position + transform.forward * 5f;
            hero.UseUltimate(targetPos);
        }

        GameObject FindNearestEnemy()
        {
            Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, 15f);
            float closestDistance = float.MaxValue;
            GameObject closestEnemy = null;

            foreach (var col in nearbyObjects)
            {
                Hero enemyHero = col.GetComponent<Hero>();
                if (enemyHero != null && enemyHero.teamId != hero.teamId)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = col.gameObject;
                    }
                }

                Minion enemyMinion = col.GetComponent<Minion>();
                if (enemyMinion != null && enemyMinion.teamId != hero.teamId)
                {
                    float distance = Vector3.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = col.gameObject;
                    }
                }
            }

            return closestEnemy;
        }
    }
}
