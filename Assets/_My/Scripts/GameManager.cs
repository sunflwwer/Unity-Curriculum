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
    [SerializeField] private GameObject[] exitSculptures;  // Exit Sculptures 배열
    [SerializeField] private int totalSculptures = 5;  // 총 필요 조각 개수
    private int collectedSculptures = 0;  // 수집한 조각 개수

    [Header("UI")]
    [SerializeField] private Text sculptureCountText;  // 남은 조각 개수를 표시하는 UI
    [SerializeField] private GameObject successPanel;  // 성공 시 나타날 패널
    [SerializeField] private Button successHomeButton;  // Success Panel의 Home 버튼
    [SerializeField] private GameObject overPanel;  // 게임 오버 패널
    [SerializeField] private Button overHomeButton;  // Over Panel의 Home 버튼
    [SerializeField] private GameObject damageImage;  // Damage Image UI 추가

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
    [SerializeField] private GameObject enemyPrefab;  // 적 프리팹
    [SerializeField] private GameObject[] spawnPoint;  // 적 스폰 지점 배열
    private List<Enemy> activeEnemies = new List<Enemy>();  // 활성화된 적 리스트
    private int maxEnemyCount = 15;  // 적 최대 개수 제한

    [Header("Kill Count UI")]
    [SerializeField] private Text killCountText;  // 적을 죽인 횟수를 표시하는 UI 텍스트
    private int killCount = 0;  // 적을 죽인 횟수를 추적하는 변수

    [Header("BGM")]
    [SerializeField] private AudioClip bgmSound;
    private AudioSource BGM;

    [Header("UI")]
    [SerializeField] private GameObject startPanel;  // 시작 패널
    [SerializeField] private Button startButton;  // 시작 버튼
    [SerializeField] private GameObject gamePanel;  // 게임 패널

    [Header("Cutscene")]
    [SerializeField] private PlayableDirector cutsceneDirector;  // 컷신을 재생하는 PlayableDirector
    public bool isReady = false;

    [Header("Game Result Sounds")]
    [SerializeField] private AudioClip successSound;  // 게임 성공 시 재생할 사운드
    [SerializeField] private AudioClip gameOverSound;  // 게임 오버 시 재생할 사운드

    [Header("Start Panel BGM")]
    [SerializeField] private AudioClip startPanelBGM;  // 시작 패널에서 재생할 BGM
    private AudioSource startPanelBGMSource;  // StartPanel BGM을 재생하는 AudioSource

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float bgmVolume = 0.0f;  // 게임 중 BGM 볼륨
    [Range(0f, 1f)] public float startPanelBGMVolume = 0.0f;  // 시작 패널 BGM 볼륨
    [Range(0f, 1f)] public float successSoundVolume = 0.0f;  // 성공 사운드 볼륨
    [Range(0f, 1f)] public float gameOverSoundVolume = 0.0f;  // 게임 오버 사운드 볼륨

    public bool isGameOver = false;  // 게임 오버 여부
    public bool isGameSuccess = false;  // 게임 성공 여부


    private void Start()
    {
        instance = this;
        currentShootDelay = 0;

        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        overPanel.SetActive(false);
        successPanel.SetActive(false);  // 성공 패널은 시작 시 비활성화
        cutsceneDirector.gameObject.SetActive(false);  // 컷신은 비활성화 상태
        damageImage.SetActive(false);  // 게임 시작 시 Damage Image는 비활성화
        gameObject.SetActive(true);  // GameManager를 활성화합니다.

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Home 버튼 클릭 이벤트 연결
        successHomeButton.onClick.AddListener(RestartGame);  // Success Panel의 Home 버튼
        overHomeButton.onClick.AddListener(RestartGame);  // Over Panel의 Home 버튼

        startButton.onClick.AddListener(OnStartButtonClicked);
        cutsceneDirector.stopped += OnCutsceneEnded;

        InitBullet();
        UpdateKillCountUI();
        UpdateSculptureUI();  // 조각 UI 초기화

        // StartPanel의 BGM을 재생
        if (startPanelBGM != null)
        {
            startPanelBGMSource = gameObject.AddComponent<AudioSource>();
            startPanelBGMSource.clip = startPanelBGM;
            startPanelBGMSource.loop = true;
            startPanelBGMSource.volume = startPanelBGMVolume;  // 시작 패널 BGM 볼륨 설정
            startPanelBGMSource.Play();
        }
    }

    private void Update()
    {
        bulletText.text = currentBullet + " / " + maxBullet;

        if (Input.GetKeyDown(KeyCode.V))
        {
            // 플레이어가 Exit Sculpture와 상호작용 중일 때 조각 획득
            CollectSculpture();  // V 버튼을 눌렀을 때 조각을 획득
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
        damageImage.SetActive(false);  // 컷신 중에는 Damage Image 비활성화

        // 컷신 시작 시 StartPanel BGM을 멈춤
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
        damageImage.SetActive(true);  // 게임 시작 시 Damage Image 활성화

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        PlayBGMSound();
        StartCoroutine(EnemySpawn());
    }

    public void RegisterEnemy(Enemy enemy)
    {
        activeEnemies.Add(enemy);  // 적 등록
    }

    public void UnregisterEnemy(Enemy enemy)
    {
        activeEnemies.Remove(enemy);  // 적 제거
    }

    public void MuteAllEnemies()
    {
        foreach (Enemy enemy in activeEnemies)
        {
            enemy.MuteAudio();  // 모든 적의 오디오 음소거
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
                RegisterEnemy(enemy);  // 적을 등록
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
        killCountText.text = "죽인 좀비: " + killCount;
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
        string updatedText = "남은 사인: " + (totalSculptures - collectedSculptures);
        Debug.Log(updatedText);
        sculptureCountText.text = updatedText;
    }

    private void OnSuccess()
    {
        isGameSuccess = true;
        Debug.Log("게임 성공! 사인을 다 찾았습니다.");
        successPanel.SetActive(true);
        gamePanel.SetActive(false);
        damageImage.SetActive(false);

        // 모든 적의 사운드를 음소거
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
        Debug.Log("게임 오버! 플레이어가 사망했습니다.");
        Time.timeScale = 0f;
        gamePanel.SetActive(false);
        overPanel.SetActive(true);
        damageImage.SetActive(false);

        // 모든 적의 사운드를 음소거
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
