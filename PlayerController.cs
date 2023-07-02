using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float playerSpeed;
    public float gravity = 9.8f;
    public Camera playerCamera;
    private Vector3 direction;
    private float verticalSpeed = 0f;
    private Animator anim;
    private CharacterController characterController;
    public GameObject efeito1;
    public GameObject efeito2;



    public float animationSpeed = 5f;
    public float attackDuration = 1f;
    public float timeBetweenAttacks = 0.5f;
    private bool isAttacking = false;
    private bool isFirstAttack = true;
    private bool isComboStarted = false;

    private bool wasInAir = false;

    public float dashDistance = 5f;
    public float dashDuration = 0.5f;
    private bool isDashActivated = false;

    public float teleportDelay = 0.1f;
    public GameObject teleportEffect;

    private bool isTargetLocked = false;
    private Transform lockedTarget;

    private bool isMiraTravada = false;
    private Vector3 lockedTargetPosition;

    private void Start()
    {
        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        efeito1.SetActive(false);
        efeito2.SetActive(false);
    }

    private void Update()
    {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");

        Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = playerCamera.transform.right;

        Vector3 movementDirection = (cameraForward * inputY) + (cameraRight * inputX);
        movementDirection.Normalize();

        if (!isTargetLocked && !isMiraTravada)
        {
            if (movementDirection != Vector3.zero)
            {
                Quaternion toRotation = Quaternion.LookRotation(movementDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * animationSpeed);

                anim.SetBool("Walk", true);
            }
            else
            {
                anim.SetBool("Walk", false);
            }
        }

        if (isTargetLocked || isMiraTravada)
        {
            if (lockedTarget != null)
            {
                Vector3 targetDirection = lockedTarget.position - transform.position;
                targetDirection.y = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * animationSpeed);
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            isComboStarted = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isComboStarted = false;
        }

        if (isComboStarted && !isAttacking)
        {
            if (isFirstAttack)
            {
                anim.SetBool("Attack", true);
                isFirstAttack = false;
                StartCoroutine(ResetAttack());
            }
            else
            {
                anim.SetBool("Attack2", true);
                isFirstAttack = true;
                StartCoroutine(ResetAttack2());
            }
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            anim.SetTrigger("Habilidade1");
            efeito1.SetActive(true);
            efeito2.SetActive(true);
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Dash());
        }

        if (characterController.isGrounded)
        {
            if (wasInAir)
            {
                anim.SetBool("Fall", false);
                anim.SetBool("Ground", true);
                wasInAir = false;
            }

            verticalSpeed = 0f;
        }
        else
        {
            if (!wasInAir)
            {
                anim.SetBool("Fall", true);
                anim.SetBool("Ground", false);
                wasInAir = true;
            }

            verticalSpeed -= gravity * Time.deltaTime;
        }

        movementDirection.y = verticalSpeed;

        characterController.Move(movementDirection * playerSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isMiraTravada = !isMiraTravada;

            if (isMiraTravada)
            {
                LockTarget();
            }
            else
            {
                UnlockTarget();
            }
        }
    }

    private IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(attackDuration);
        anim.SetBool("Attack", false);
    }

    private IEnumerator ResetAttack2()
    {
        yield return new WaitForSeconds(attackDuration);
        anim.SetBool("Attack2", false);
    }

    private IEnumerator Dash()
    {
        if (!isDashActivated)
        {
            isDashActivated = true;

            Vector3 dashDestination = transform.position + transform.forward * dashDistance;

            Instantiate(teleportEffect, transform.position, Quaternion.identity);

            yield return new WaitForSeconds(teleportDelay);

            characterController.enabled = false;
            transform.position = dashDestination;
            characterController.enabled = true;

            Instantiate(teleportEffect, transform.position, Quaternion.identity);

            yield return new WaitForSeconds(dashDuration - teleportDelay);

            isDashActivated = false;
        }
    }

    private void LockTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearestEnemy = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            isTargetLocked = true;
            lockedTarget = nearestEnemy.transform;
            lockedTargetPosition = nearestEnemy.transform.position;
        }
    }

    private void UnlockTarget()
    {
        isTargetLocked = false;
        lockedTarget = null;
    }
}
