using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    private Slider HPBar;

    private float enemyMaxHP = 10;
    public float enemyCurrentHP = 0;

    private NavMeshAgent agent;
    private Animator animator;

    private GameObject targetPlayer;
    private float targetDelay = 0.5f;

    private CapsuleCollider enemyCollider;

    public event System.Action onDeath;  // 적이 사망할 때 발생하는 이벤트

    [Header("Death Sound Effect")]
    [SerializeField] private AudioClip deathSound;  // 적이 죽을 때 재생할 사운드
    [SerializeField] private AudioClip approachSound;  // 적이 가까이 다가올 때 재생할 사운드
    private AudioSource audioSource;  // 사운드 재생을 위한 AudioSource

    private bool isPlayingApproachSound = false;  // 사운드 재생 중인지 여부 확인

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyCollider = GetComponent<CapsuleCollider>();
        audioSource = GetComponent<AudioSource>();

        targetPlayer = GameObject.FindWithTag("Player");
        InitEnemyHP();
    }

    void Update()
    {
        HPBar.value = enemyCurrentHP / enemyMaxHP;

        if (enemyCurrentHP <= 0)
        {
            StartCoroutine(EnemyDie());
            return;
        }

        if (targetPlayer != null)
        {
            float maxDelay = 0.5f;
            targetDelay += Time.deltaTime;

            if (targetDelay < maxDelay)
            {
                return;
            }

            agent.destination = targetPlayer.transform.position;
            transform.LookAt(targetPlayer.transform.position);

            bool isRange = Vector3.Distance(transform.position, targetPlayer.transform.position) <= agent.stoppingDistance;

            HandleApproachSound();

            if (isRange)
            {
                animator.SetTrigger("Attack");
                DealDamageToPlayer();
            }
            else
            {
                animator.SetFloat("MoveSpeed", agent.velocity.magnitude);
            }

            targetDelay = 0;
        }
    }

    private void InitEnemyHP()
    {
        enemyCurrentHP = enemyMaxHP;
    }

    private void DealDamageToPlayer()
    {
        PlayerManager playerManager = targetPlayer.GetComponent<PlayerManager>();
        if (playerManager != null && !playerManager.isDead)
        {
            playerManager.TakeDamage(10);
        }
    }

    IEnumerator EnemyDie()
    {
        agent.speed = 0;
        animator.SetTrigger("Dead");
        enemyCollider.enabled = false;

        if (deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        yield return new WaitForSeconds(3f);

        onDeath?.Invoke();  // 적 사망 이벤트 호출

        gameObject.SetActive(false);
    }

    private void HandleApproachSound()
    {
        if (GameManager.instance.isGameOver || GameManager.instance.isGameSuccess)
        {
            if (!audioSource.mute)
            {
                audioSource.mute = true;
                isPlayingApproachSound = false;  // 사운드가 멈췄을 때 false로 설정
            }
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.transform.position);
        float soundRange = 7.0f;

        if (distanceToPlayer <= soundRange && !isPlayingApproachSound)
        {
            audioSource.mute = false;
            audioSource.clip = approachSound;
            audioSource.loop = true;
            audioSource.Play();
            isPlayingApproachSound = true;  // 사운드가 시작되었을 때 true로 설정
        }
        else if (distanceToPlayer > soundRange && isPlayingApproachSound)
        {
            audioSource.mute = true;
            isPlayingApproachSound = false;  // 사운드가 멈췄을 때 false로 설정
        }
    }


    public void MuteAudio()
    {
        audioSource.mute = true;  // 적 사운드 음소거
    }
}
