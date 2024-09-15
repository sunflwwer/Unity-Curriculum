using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Playables;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Exit Sculptures")]
    [SerializeField] private GameObject[] exitSculptures;  // Exit Sculptures �迭
    [SerializeField] private int totalSculptures = 5;  // �� �ʿ� ���� ����
    private int collectedSculptures = 0;  // ������ ���� ����

    [Header("UI")]
    [SerializeField] private Text sculptureCountText;  // ���� ���� ������ ǥ���ϴ� UI
    [SerializeField] private GameObject successPanel;  // ���� �� ��Ÿ�� �г�
    [SerializeField] private Button successHomeButton;  // Success Panel�� Home ��ư
    [SerializeField] private GameObject overPanel;  // ���� ���� �г�
    [SerializeField] private Button overHomeButton;  // Over Panel�� Home ��ư
    [SerializeField] private GameObject damageImage;  // Damage Image UI �߰�

    [Header("Bullet")]
    [SerializeField] private Transform bulletPoint;
    [SerializeField] private GameObject bulletObj;
    [SerializeField] private float maxShootDelay = 0.2f;
    [SerializeField] private float currentShootDelay = 0.2f;
    [SerializeField] private Text bulletText;
    private int maxBullet = 30;
    private int currentBullet = 0;

    [Header("Weapon FX")]
    [SerializeField] private GameObject weaponFlashFX;
    [SerializeField] private Transform bulletCasePoint;
    [SerializeField] private GameObject bulletCaseFX;
    [SerializeField] private Transform weaponClipPoint;
    [SerializeField] private GameObject weaponClipFX;

    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;  // �� ������
    [SerializeField] private GameObject[] spawnPoint;  // �� ���� ���� �迭
    private List<Enemy> activeEnemies = new List<Enemy>();  // Ȱ��ȭ�� �� ����Ʈ
    private int maxEnemyCount = 15;  // �� �ִ� ���� ����

    [Header("Kill Count UI")]
    [SerializeField] private Text killCountText;  // ���� ���� Ƚ���� ǥ���ϴ� UI �ؽ�Ʈ
    private int killCount = 0;  // ���� ���� Ƚ���� �����ϴ� ����

    [Header("BGM")]
    [SerializeField] private AudioClip bgmSound;
    private AudioSource BGM;

    [Header("UI")]
    [SerializeField] private GameObject startPanel;  // ���� �г�
    [SerializeField] private Button startButton;  // ���� ��ư
    [SerializeField] private GameObject gamePanel;  // ���� �г�

    [Header("Cutscene")]
    [SerializeField] private PlayableDirector cutsceneDirector;  // �ƽ��� ����ϴ� PlayableDirector
    public bool isReady = false;

    [Header("Game Result Sounds")]
    [SerializeField] private AudioClip successSound;  // ���� ���� �� ����� ����
    [SerializeField] private AudioClip gameOverSound;  // ���� ���� �� ����� ����

    [Header("Start Panel BGM")]
    [SerializeField] private AudioClip startPanelBGM;  // ���� �гο��� ����� BGM
    private AudioSource startPanelBGMSource;  // StartPanel BGM�� ����ϴ� AudioSource

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.0f;  // ���� �� BGM ����
    [Range(0f, 1f)] public float startPanelBGMVolume = 0.0f;  // ���� �г� BGM ����
    [Range(0f, 1f)] public float successSoundVolume = 0.0f;  // ���� ���� ����
    [Range(0f, 1f)] public float gameOverSoundVolume = 0.0f;  // ���� ���� ���� ����

    public bool isGameOver = false;  // ���� ���� ����
    public bool isGameSuccess = false;  // ���� ���� ����


    private void Start()
    {
        instance = this;
        currentShootDelay = 0;

        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        overPanel.SetActive(false);
        successPanel.SetActive(false);  // ���� �г��� ���� �� ��Ȱ��ȭ
        cutsceneDirector.gameObject.SetActive(false);  // �ƽ��� ��Ȱ��ȭ ����
        damageImage.SetActive(false);  // ���� ���� �� Damage Image�� ��Ȱ��ȭ
        gameObject.SetActive(true);  // GameManager�� Ȱ��ȭ�մϴ�.

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Home ��ư Ŭ�� �̺�Ʈ ����
        successHomeButton.onClick.AddListener(RestartGame);  // Success Panel�� Home ��ư
        overHomeButton.onClick.AddListener(RestartGame);  // Over Panel�� Home ��ư

        startButton.onClick.AddListener(OnStartButtonClicked);
        cutsceneDirector.stopped += OnCutsceneEnded;

        InitBullet();
        UpdateKillCountUI();
        UpdateSculptureUI();  // ���� UI �ʱ�ȭ

        // StartPanel�� BGM�� ���
        if (startPanelBGM != null)
        {
            startPanelBGMSource = gameObject.AddComponent<AudioSource>();
            startPanelBGMSource.clip = startPanelBGM;
            startPanelBGMSource.loop = true;
            startPanelBGMSource.volume = startPanelBGMVolume;  // ���� �г� BGM ���� ����
            startPanelBGMSource.Play();
        }
    }

    private void Update()
    {
        bulletText.text = currentBullet + " / " + maxBullet;

        if (Input.GetKeyDown(KeyCode.V))
        {
            // �÷��̾ Exit Sculpture�� ��ȣ�ۿ� ���� �� ���� ȹ��
            CollectSculpture();  // V ��ư�� ������ �� ������ ȹ��
        }
    }

    private void OnStartButtonClicked()
    {
        startPanel.SetActive(false);
        PlayCutscene();
    }

    public void PlayCutscene()
    {
        cutsceneDirector.gameObject.SetActive(true);
        cutsceneDirector.Play();
        isReady = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        damageImage.SetActive(false);  // �ƽ� �߿��� Damage Image ��Ȱ��ȭ

        // �ƽ� ���� �� StartPanel BGM�� ����
        if (startPanelBGMSource != null && startPanelBGMSource.isPlaying)
        {
            startPanelBGMSource.Stop();
        }
    }


    private void OnCutsceneEnded(PlayableDirector director)
    {
        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("GameManager is inactive, cannot start the coroutine.");
            return;
        }

        if (director == cutsceneDirector)
        {
            isReady = false;
            StartGame();
        }
    }

    public void StartGame()
    {
        gamePanel.SetActive(true);
        damageImage.SetActive(true);  // ���� ���� �� Damage Image Ȱ��ȭ

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayBGMSound();
        StartCoroutine(EnemySpawn());
    }

    public void RegisterEnemy(Enemy enemy)
    {
        activeEnemies.Add(enemy);  // �� ���
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        activeEnemies.Remove(enemy);  // �� ����
    }

    public void MuteAllEnemies()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            enemy.MuteAudio();  // ��� ���� ����� ���Ұ�
        }
    }

    public void Shooting(Vector3 targetPosition, Enemy enemy, AudioSource weaponSound, AudioClip shootingSound)
    {
        currentShootDelay += Time.deltaTime;

        if (currentShootDelay < maxShootDelay || currentBullet <= 0)
            return;

        currentBullet -= 1;
        currentShootDelay = 0;

        weaponSound.clip = shootingSound;
        weaponSound.Play();

        Vector3 aim = (targetPosition - bulletPoint.position).normalized;

        GameObject flashFX = PoolManager.instance.ActivateObj(1);
        SetObjPosition(flashFX, bulletPoint);
        flashFX.transform.rotation = Quaternion.LookRotation(aim, Vector3.up);

        GameObject caseFX = PoolManager.instance.ActivateObj(2);
        SetObjPosition(caseFX, bulletCasePoint);

        GameObject prefabToSpawn = PoolManager.instance.ActivateObj(0);
        SetObjPosition(prefabToSpawn, bulletPoint);
        prefabToSpawn.transform.rotation = Quaternion.LookRotation(aim, Vector3.up);
    }

    public void ReroadClip()
    {
        GameObject clipFX = PoolManager.instance.ActivateObj(3);
        SetObjPosition(clipFX, weaponClipPoint);
        InitBullet();
    }

    private void InitBullet()
    {
        currentBullet = maxBullet;
    }

    private void SetObjPosition(GameObject obj, Transform targetTransform)
    {
        obj.transform.position = targetTransform.position;
    }

    IEnumerator EnemySpawn()
    {
        while (true)
        {
            if (activeEnemies.Count < maxEnemyCount)
            {
                GameObject enemyObj = Instantiate(enemyPrefab, spawnPoint[Random.Range(0, spawnPoint.Length)].transform.position, Quaternion.identity);
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                RegisterEnemy(enemy);  // ���� ���
                enemy.onDeath += () =>
                {
                    UnregisterEnemy(enemy);
                    IncrementKillCount();
                };
            }

            yield return new WaitForSeconds(2f);
        }
    }

    private void IncrementKillCount()
    {
        killCount++;
        UpdateKillCountUI();
    }

    private void UpdateKillCountUI()
    {
        killCountText.text = "���� ����: " + killCount;
    }

    public void SculptureCollected()
    {
        collectedSculptures++;
        UpdateSculptureUI();

        if (collectedSculptures == totalSculptures)
        {
            OnSuccess();
        }
    }

    private void CollectSculpture()
    {
        foreach (GameObject sculpture in exitSculptures)
        {
            if (sculpture.activeInHierarchy)
            {
                sculpture.SetActive(false);
                collectedSculptures++;
                UpdateSculptureUI();

                if (collectedSculptures == totalSculptures)
                {
                    OnSuccess();
                }
                break;
            }
        }
    }

    private void UpdateSculptureUI()
    {
        string updatedText = "���� ����: " + (totalSculptures - collectedSculptures);
        Debug.Log(updatedText);
        sculptureCountText.text = updatedText;
    }

    private void OnSuccess()
    {
        isGameSuccess = true;
        Debug.Log("���� ����! ������ �� ã�ҽ��ϴ�.");
        successPanel.SetActive(true);
        gamePanel.SetActive(false);
        damageImage.SetActive(false);

        // ��� ���� ���带 ���Ұ�
        MuteAllEnemies();

        if (successSound != null)
        {
            BGM.Stop();
            BGM.PlayOneShot(successSound, successSoundVolume);
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void PlayBGMSound()
    {
        BGM = GetComponent<AudioSource>();
        BGM.clip = bgmSound;
        BGM.loop = true;
        BGM.volume = bgmVolume;
        BGM.Play();
    }

    public void PlayerDied()
    {
        isGameOver = true;
        Debug.Log("���� ����! �÷��̾ ����߽��ϴ�.");
        Time.timeScale = 0f;
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        damageImage.SetActive(false);

        // ��� ���� ���带 ���Ұ�
        MuteAllEnemies();

        if (gameOverSound != null)
        {
            BGM.Stop();
            BGM.PlayOneShot(gameOverSound, gameOverSoundVolume);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
