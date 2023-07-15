using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour
{

    

    public float RunSpeed = 10;

    public float normalSpeed = 5;
    public float playerSpeed;
    public float gravity = 9.8f;
    public Camera playerCamera;
    private Vector3 direction;
    private float verticalSpeed = 0f;
    private Animator anim;
    private CharacterController characterController;

    public float dashDistance = 5f;
    public float dashDuration = 0.5f;
    private bool isDashActivated = false;

    public float teleportDelay = 0.1f;

    public float detectionRadius = 12f; // Raio de detecção da área para encontrar inimigos
    public Transform detectionCenter; // Ponto central da área de detecção
    public float rotationSpeed = 5f; // Velocidade de rotação do personagem
    public float moveSpeed = 5f;
    public float attackRange = 3f;

    public float minDamage = 5f;
    public float maxDamage = 10f;
    public float damage;
    public float damageMultiplier = 1f;
    public float HeavyMultiplier = 2f;
    public float attackSpeedMultiplier = 1f;
    public float hitInterval = 0.5f;

    public Slider healthSlider;
    public Slider manaSlider;
    public Slider StaminaSlider;
    public float maxHealth = 100f;
    public float maxMana = 100f;
    public float maxStamina = 100f;
    public float currentHealth;
    public float currentMana;
    public float currentStamina;
    public float healthDrainAmount = 1f;
    public float healthDrainDelay = 1f;
    public float healthrecoveryamount = 0.5f;
    public float healthMultiply = 1f;
    public float HealthRecoveryInterval = 1.5f;
    public float manaRecoveryAmount = 1f;
    public float manaRecoveryDelay = 2f;
    private bool isAbilityActive = false;
    public float staminaDrainAmountPerSecond = 6f;
    public float staminaRecoveryDelay = 1f; // Tempo de atraso antes de começar a recuperar a stamina
    public float staminaRecoveryAmount = 5f; // Quantidade de stamina a ser recuperada a cada intervalo
    public float staminaRecoveryInterval = 1f;
    public float staminaMultiply = 1f;
    private bool isRunning = false;
    private bool isRecoveringStamina = false;
    private bool isRecoveringHealth = false;

    public bool Espada = false;

    private void Start()
    {
        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;

 
        playerSpeed = normalSpeed;

        StartCoroutine(ManaRecovery());

        StaminaSlider.maxValue = maxStamina;
        StaminaSlider.value = maxStamina;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            Espada = !Espada;
        }

        if (Input.GetKeyDown(KeyCode.Space) && currentStamina >= 30f)
        {
            anim.SetTrigger("Roll");
            currentStamina -= 30f;
        }


        StaminaSlider.value = currentStamina;

        if (!isRecoveringStamina && currentStamina < maxStamina)
        {
            StartCoroutine(RecoverStamina());
        }
        if (!isRecoveringHealth && currentHealth < maxHealth && isAbilityActive == false)
        {
            StartCoroutine(RecoverHealth());
        }

        

        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");


        if (Input.GetKey(KeyCode.W))
        {
            anim.SetBool("walk", true);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            anim.SetBool("walk", true);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            anim.SetBool("walk", true);

        }
        else if (Input.GetKey(KeyCode.A))
        {
            anim.SetBool("walk", true);
        }
        else
        {
            anim.SetBool("walk", false);
        }


            Vector3 cameraForward = playerCamera.transform.forward;
        cameraForward.y = 0f;
        cameraForward.Normalize();

        Vector3 cameraRight = playerCamera.transform.right;

        Vector3 movementDirection = (cameraForward * inputY) + (cameraRight * inputX);
        movementDirection.Normalize();

        if (movementDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, Time.deltaTime * moveSpeed);
        }

        if (characterController.isGrounded)
        {
            verticalSpeed = 0f;
            anim.SetBool("Fall", false);
        }
        else
        {
            verticalSpeed -= gravity * Time.deltaTime;
            anim.SetBool("Fall", true);
        }

        movementDirection.y = verticalSpeed;

        characterController.Move(movementDirection * playerSpeed * Time.deltaTime);

        if (currentHealth <= 0)
        {
            die();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && isAbilityActive == false && currentStamina > 0)
        {
            playerSpeed = RunSpeed;
            anim.SetBool("Run", true);
            isRunning = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift) || currentStamina <= 0)
        {
            playerSpeed = normalSpeed;
            anim.SetBool("Run", false);
            isRunning = false;
        }

        if (isRunning && Espada == false)
        {
            currentStamina -= staminaDrainAmountPerSecond * Time.deltaTime;
            staminaRecoveryAmount = 0f;
        }
        else if (isRunning && Espada == true)
        {
            currentStamina -= staminaDrainAmountPerSecond * Time.deltaTime * 1.8f;
        }
        else
        {
            staminaRecoveryAmount = 5f;
        }
        if (currentStamina <= 0)
        {
            playerSpeed = normalSpeed;
            anim.SetBool("Run", false);
            isRunning = false;
        }

        UpdateHealthSlider();
        UpdateManaSlider();
    }

    private IEnumerator ManaRecovery()
    {
        while (true)
        {
            yield return new WaitForSeconds(manaRecoveryDelay);
            if (!isAbilityActive)
            {
                currentMana += manaRecoveryAmount;
                if (currentMana > maxMana)
                {
                    currentMana = maxMana;
                }
            }
        }
    }

    private IEnumerator RecoverStamina()
    {
        isRecoveringStamina = true;

        // Aguardar um tempo antes de começar a recuperar a stamina
        yield return new WaitForSeconds(staminaRecoveryDelay);

        // Recuperar a stamina gradualmente até o valor máximo
        while (currentStamina < maxStamina)
        {
            currentStamina += staminaRecoveryAmount * staminaMultiply;
            yield return new WaitForSeconds(staminaRecoveryInterval);
        }

        currentStamina = maxStamina;
        isRecoveringStamina = false;
    }

    private IEnumerator RecoverHealth()
    {
        isRecoveringHealth = true;

        while (currentHealth < maxHealth)
        {
            currentHealth += healthrecoveryamount * healthMultiply;
            yield return new WaitForSeconds(HealthRecoveryInterval);
        }

        currentHealth = maxHealth;
        isRecoveringHealth = false;
    }

    private void UpdateHealthSlider()
    {
        healthSlider.value = currentHealth / maxHealth;
    }

    private void UpdateManaSlider()
    {
        manaSlider.value = currentMana / maxMana;
    }

    void die()
    {
        anim.SetBool("died", true);
        StartCoroutine(Died());
    }

    public IEnumerator Died()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
