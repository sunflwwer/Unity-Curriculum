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

    [Header("�÷��̾� ü�� �� UI")]
    public int maxHealth = 100;
    private int currentHealth;
    [SerializeField]
    private Slider healthBar;  // ü�¹� �����̴�
    [SerializeField]
    private Image healthFill;  // ü�¹��� Fill ����

    [Header("ü�� ���¿� ���� ����")]
    [SerializeField]
    private Color normalHealthColor = Color.green;  // ü�� 30 �̻��� �� ����
    [SerializeField]
    private Color lowHealthColor = Color.red;  // ü�� 30 ������ �� ����

    [Header("Damage Effect")]
    [SerializeField]
    private Image damageImage;  // ȭ�� ��ü�� ���� ������ �̹���
    [SerializeField]
    private Color damageColor = new Color(1f, 0f, 0f, 0.5f);  // �ǰ� �� ������
    [SerializeField]
    private float damageEffectDuration = 0.5f;  // ������ �̹����� ���� �ð�
    private bool isTakingDamage = false;

    // �÷��̾ �׾����� Ȯ���ϴ� ���� �߰�
    public bool isDead { get; private set; } = false;  // ��� ���¸� �����ϴ� ����

    [Header("Damage Sound Effect")]
    [SerializeField] private AudioClip damageSound;  // �÷��̾ �������� �Ծ��� �� ����� ����


    void Start()
    {
        input = GetComponent<StarterAssetsInputs>();
        controller = GetComponent<ThirdPersonController>();
        anim = GetComponent<Animator>();
        weaponSound = GetComponent<AudioSource>();

        // ü�� �� ü�¹� �ʱ�ȭ
        currentHealth = maxHealth;
        healthBar.maxValue = maxHealth;
        healthBar.value = currentHealth;
        healthFill.color = normalHealthColor;  // �ʱ� ü�� ���� ����

        // Damage Image �ʱ�ȭ
        if (damageImage != null)
        {
            damageImage.color = new Color(damageColor.r, damageColor.g, damageColor.b, 0f);  // ó������ �����ϰ�
        }
    }

    void Update()
    {
        if (isDead)
        {
            // �÷��̾ ����� ��쿡�� �� �̻� �������� ó������ ����
            return;
        }

        if (GameManager.instance.isReady)
        {
            AimControll(false);
            SetRigWeight(0);
            return;
        }

        AimCheck();

        // ���ظ� �Ծ��� �� ���� ȭ�� ȿ���� ������ ������� ��
        if (isTakingDamage && damageImage != null)
        {
            damageImage.color = Color.Lerp(damageImage.color, new Color(damageColor.r, damageColor.g, damageColor.b, 0f), Time.deltaTime / damageEffectDuration);
            if (damageImage.color.a <= 0.01f)
            {
                isTakingDamage = false;
                damageImage.color = new Color(damageColor.r, damageColor.g, damageColor.b, 0f);  // ������ ���������� ȿ�� ����
            }
        }
    }

    // ������ ���ݹ޾��� �� ü���� ���ҽ�Ű�� �޼���
    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;  // ������ ��ŭ ü�� ����
        healthBar.value = currentHealth;  // ü�¹� ������Ʈ

        // ü���� 30 ���Ϸ� �������� ü�¹� ���� ����
        if (currentHealth <= 30)
        {
            healthFill.color = lowHealthColor;
        }
        else
        {
            healthFill.color = normalHealthColor;
        }

        // ���ظ� �Ծ��� �� ���� ȭ�� ȿ�� Ȱ��ȭ
        if (damageImage != null)
        {
            damageImage.color = damageColor;  // ���� ȭ�� ȿ���� ��� ǥ��
            isTakingDamage = true;
        }

        // ������ ���� ���
        if (damageSound != null)
        {
            weaponSound.PlayOneShot(damageSound);  // ���� ���
        }

        Debug.Log("�÷��̾ �������� �޾ҽ��ϴ�. ���� ü��: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();  // ü���� 0 ���ϰ� �Ǹ� ��� ó��
        }
    }


    private void Die()
    {
        if (isDead)
            return;

        Debug.Log("�÷��̾ ����߽��ϴ�.");
        isDead = true;  // �÷��̾ �׾��ٰ� ǥ��

        // �״� �ִϸ��̼� ����
        anim.SetTrigger("DieTrigger");

        // ��� ó�� (���� ���� ó��)
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
