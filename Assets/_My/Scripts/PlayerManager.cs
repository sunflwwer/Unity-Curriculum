using Cinemachine;
using StarterAssets;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    private StarterAssetsInputs input;
    private ThirdPersonController controller;
    private Animator anim;

    [Header("Aim")]
    [SerializeField]
    private CinemachineVirtualCamera aimCam;
    [SerializeField]
    private GameObject aimImage;
    [SerializeField]
    private GameObject aimObj;
    [SerializeField]
    private float aimObjDis = 10f;
    [SerializeField]
    private LayerMask targetLayer;

    [Header("IK")]
    [SerializeField]
    private Rig handRig;
    [SerializeField]
    private Rig aimRig;

    [Header("Weapon Sound Effect")]
    [SerializeField]
    private AudioClip shootingSound;
    [SerializeField]
    private AudioClip[] reroadSound;
    private AudioSource weaponSound;

    private Enemy enemy;

    [Header("플레이어 체력 및 UI")]
    public int maxHealth = 100;
    private int currentHealth;
    [SerializeField]
    private Slider healthBar;  // 체력바 슬라이더
    [SerializeField]
    private Image healthFill;  // 체력바의 Fill 영역

    [Header("체력 상태에 따른 색상")]
    [SerializeField]
    private Color normalHealthColor = Color.green;  // 체력 30 이상일 때 색상
    [SerializeField]
    private Color lowHealthColor = Color.red;  // 체력 30 이하일 때 색상

    [Header("Damage Effect")]
    [SerializeField]
    private Image damageImage;  // 화면 전체에 덮을 빨간색 이미지
    [SerializeField]
    private Color damageColor = new Color(1f, 0f, 0f, 0.5f);  // 피격 시 빨간색
    [SerializeField]
    private float damageEffectDuration = 0.5f;  // 빨간색 이미지가 보일 시간
    private bool isTakingDamage = false;

    // 플레이어가 죽었는지 확인하는 변수 추가
    public bool isDead { get; private set; } = false;  // 사망 상태를 저장하는 변수

    [Header("Damage Sound Effect")]
    [SerializeField] private AudioClip damageSound;  // 플레이어가 데미지를 입었을 때 재생할 사운드


    void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
        controller = GetComponent<ThirdPersonController>();
        anim = GetComponent<Animator>();
        weaponSound = GetComponent<AudioSource>();

        // 체력 및 체력바 초기화
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        healthFill.color = normalHealthColor;  // 초기 체력 색상 설정

        // Damage Image 초기화
        if (damageImage != null)
        {
            damageImage.color = new Color(damageColor.r, damageColor.g, damageColor.b, 0f);  // 처음에는 투명하게
        }
    }

    void Update()
    {
        if (isDead)
        {
            // 플레이어가 사망한 경우에는 더 이상 움직임을 처리하지 않음
            return;
        }

        if (GameManager.instance.isReady)
        {
            AimControll(false);
            SetRigWeight(0);
            return;
        }

        AimCheck();

        // 피해를 입었을 때 빨간 화면 효과를 서서히 사라지게 함
        if (isTakingDamage && damageImage != null)
        {
            damageImage.color = Color.Lerp(damageImage.color, new Color(damageColor.r, damageColor.g, damageColor.b, 0f), Time.deltaTime / damageEffectDuration);
            if (damageImage.color.a <= 0.01f)
            {
                isTakingDamage = false;
                damageImage.color = new Color(damageColor.r, damageColor.g, damageColor.b, 0f);  // 완전히 투명해지면 효과 종료
            }
        }
    }

    // 적에게 공격받았을 때 체력을 감소시키는 메서드
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;  // 데미지 만큼 체력 감소
        healthBar.value = currentHealth;  // 체력바 업데이트

        // 체력이 30 이하로 떨어지면 체력바 색상 변경
        if (currentHealth <= 30)
        {
            healthFill.color = lowHealthColor;
        }
        else
        {
            healthFill.color = normalHealthColor;
        }

        // 피해를 입었을 때 빨간 화면 효과 활성화
        if (damageImage != null)
        {
            damageImage.color = damageColor;  // 빨간 화면 효과를 즉시 표시
            isTakingDamage = true;
        }

        // 데미지 사운드 재생
        if (damageSound != null)
        {
            weaponSound.PlayOneShot(damageSound);  // 사운드 재생
        }

        Debug.Log("플레이어가 데미지를 받았습니다. 남은 체력: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();  // 체력이 0 이하가 되면 사망 처리
        }
    }


    private void Die()
    {
        if (isDead)
            return;

        Debug.Log("플레이어가 사망했습니다.");
        isDead = true;  // 플레이어가 죽었다고 표시

        // 죽는 애니메이션 실행
        anim.SetTrigger("DieTrigger");

        // 사망 처리 (게임 오버 처리)
        GameManager.instance.PlayerDied();
    }

    private void AimCheck()
    {
        if (input.reroad)
        {
            input.reroad = false;

            if (controller.isReroad)
            {
                return;
            }

            AimControll(false);
            SetRigWeight(0);
            anim.SetLayerWeight(1, 1);
            anim.SetTrigger("Reroad");
            controller.isReroad = true;
        }

        if (controller.isReroad)
        {
            return;
        }

        if (input.aim)
        {
            AimControll(true);

            anim.SetLayerWeight(1, 1);

            Vector3 targetPosition = Vector3.zero;
            Transform camTransform = Camera.main.transform;
            RaycastHit hit;

            if (Physics.Raycast(camTransform.position, camTransform.forward, out hit, Mathf.Infinity, targetLayer))
            {
                targetPosition = hit.point;
                aimObj.transform.position = hit.point;

                enemy = hit.collider.gameObject.GetComponent<Enemy>();
            }
            else
            {
                targetPosition = camTransform.position + camTransform.forward * aimObjDis;
                aimObj.transform.position = camTransform.position + camTransform.forward * aimObjDis;
            }

            Vector3 targetAim = targetPosition;
            targetAim.y = transform.position.y;
            Vector3 aimDir = (targetAim - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDir, Time.deltaTime * 50f);

            SetRigWeight(1);

            if (input.shoot)
            {
                anim.SetBool("Shoot", true);
                GameManager.instance.Shooting(targetPosition, enemy, weaponSound, shootingSound);
            }
            else
            {
                anim.SetBool("Shoot", false);
            }
        }
        else
        {
            AimControll(false);
            SetRigWeight(0);
            anim.SetLayerWeight(1, 0);
            anim.SetBool("Shoot", false);
        }
    }

    private void AimControll(bool isCheck)
    {
        aimCam.gameObject.SetActive(isCheck);
        aimImage.SetActive(isCheck);
        controller.isAimMove = isCheck;
    }

    public void Reroad()
    {
        controller.isReroad = false;
        SetRigWeight(1);
        anim.SetLayerWeight(1, 0);
        PlayWeaponSound(reroadSound[2]);
    }

    private void SetRigWeight(float weight)
    {
        aimRig.weight = weight;
        handRig.weight = weight;
    }

    public void ReroadWeaponClip()
    {
        GameManager.instance.ReroadClip();
        PlayWeaponSound(reroadSound[0]);
    }

    public void ReroadInsertClip()
    {
        PlayWeaponSound(reroadSound[1]);
    }

    private void PlayWeaponSound(AudioClip sound)
    {
        weaponSound.clip = sound;
        weaponSound.Play();
    }
}
