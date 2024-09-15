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

    public event System.Action onDeath;  // ���� ����� �� �߻��ϴ� �̺�Ʈ

    [Header("Death Sound Effect")]
    [SerializeField] private AudioClip deathSound;  // ���� ���� �� ����� ����
    [SerializeField] private AudioClip approachSound;  // ���� ������ �ٰ��� �� ����� ����
    private AudioSource audioSource;  // ���� ����� ���� AudioSource

    private bool isPlayingApproachSound = false;  // ���� ��� ������ ���� Ȯ��

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

        onDeath?.Invoke();  // �� ��� �̺�Ʈ ȣ��

        gameObject.SetActive(false);
    }

    private void HandleApproachSound()
    {
        if (GameManager.instance.isGameOver || GameManager.instance.isGameSuccess)
        {
            if (!audioSource.mute)
            {
                audioSource.mute = true;
                isPlayingApproachSound = false;  // ���尡 ������ �� false�� ����
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
            isPlayingApproachSound = true;  // ���尡 ���۵Ǿ��� �� true�� ����
        }
        else if (distanceToPlayer > soundRange && isPlayingApproachSound)
        {
            audioSource.mute = true;
            isPlayingApproachSound = false;  // ���尡 ������ �� false�� ����
        }
    }


    public void MuteAudio()
    {
        audioSource.mute = true;  // �� ���� ���Ұ�
    }
}
