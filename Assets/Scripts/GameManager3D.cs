using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager3D : MonoBehaviour
{
    public static GameManager3D Instance;
    
    [Header("Game State")]
    public int currentLevel = 1;
    public int enemiesDefeated = 0;
    public int totalScore = 0;
    public int playerLives = 3;
    public bool isGameActive = false;
    public bool hasSlowMotionPowerUp = false;
    public bool hasExtraLifePowerUp = false;
    
    [Header("UI References")]
    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject gameOverPanel;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject[] lifeIcons;
    public TextMeshProUGUI finalScoreText;
    public GameObject powerUpPanel;
    public TextMeshProUGUI powerUpText;
    public TextMeshProUGUI countdownText; // NOUVEAU : Texte pour le compte à rebours
    
    [Header("3D Characters")]
    public Transform playerTransform;
    public Transform enemyTransform;
    public Animator playerAnimator;
    public Animator enemyAnimator;
    public Renderer playerRenderer;
    public Renderer enemyRenderer;
    
    [Header("Camera")]
    public Camera mainCamera;
    public Vector3 cameraStartPosition = new Vector3(0, 8, -12);
    public Vector3 cameraGamePosition = new Vector3(0, 5, -10);
    public float cameraTransitionSpeed = 2f;
    
    [Header("Combat Settings")]
    public float comboPreviewTime = 2f; // NOUVEAU : Temps de prévisualisation
    public float baseComboTime = 3.5f;
    public float timeDecreasePerLevel = 0.08f;
    public int baseComboLength = 3;
    public int scorePerEnemy = 100;
    public int bonusScorePerLevel = 50;
    
    [Header("Character Movement")]
    public float dodgeDistance = 1.5f;
    public float dodgeSpeed = 10f;
    public float attackDistance = 0.016f;
    
    [Header("Animation Settings")]
    public float idleAnimationSpeed = 0.3f; // NOUVEAU : Vitesse des animations au ralenti
    
    [Header("Visual Effects")]
    public ParticleSystem hitParticles;
    public ParticleSystem dodgeParticles;
    public TrailRenderer playerTrail;
    public float cameraShakeIntensity = 0.3f;
    public float cameraShakeDuration = 0.2f;
    
    [Header("Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioClip menuMusic;
    public AudioClip gameMusic;
    public AudioClip bossMusic;
    public AudioClip swipeSound;
    public AudioClip hitSound;
    public AudioClip victorySound;
    public AudioClip defeatSound;
    public AudioClip powerUpSound;
    
    [Header("Enemy Types")]
    public EnemyData[] enemyTypes;
    public EnemyData bossData;
    private EnemyData currentEnemy;
    private bool isBossFight = false;
    
    [Header("Power-Ups")]
    public float powerUpChance = 0.25f;
    public float slowMotionDuration = 5f;
    public float slowMotionScale = 0.6f;
    
    [Header("Randomization")]
    public bool randomizeEnemyAppearance = true; 
    public Color[] randomEnemyColors; 
    public bool randomizePowerUpChance = true;

    
    private List<SwipeDirection> currentCombo = new List<SwipeDirection>();
    private int currentComboIndex = 0;
    private float comboTimer;
    private bool waitingForAttack = false;
    private bool isPreviewPhase = false; // NOUVEAU : Phase de prévisualisation
    private bool comboCompleted = false; // NOUVEAU : Combo réussi ou non
    private Vector3 playerStartPosition;
    private Vector3 enemyStartPosition;
    private Vector3 cameraOriginalPosition;
    private bool isShaking = false;

    [System.Serializable]
    public class EnemyData
    {
        public string enemyName;
        public Material enemyMaterial;
        public int health = 1;
        public float speedMultiplier = 1f;
        public int scoreMultiplier = 1;
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (playerTransform != null)
            playerStartPosition = playerTransform.position;
        if (enemyTransform != null)
            enemyStartPosition = enemyTransform.position;
        if (mainCamera != null)
            cameraOriginalPosition = mainCamera.transform.position;
        
        if (playerTrail != null)
            playerTrail.enabled = false;
        
        ShowStartScreen();
        PlayMusic(menuMusic);
    }

    void Update()
    {
        if (isGameActive && !waitingForAttack && !isPreviewPhase)
        {
            float timeScale = hasSlowMotionPowerUp ? slowMotionScale : 1f;
            comboTimer -= Time.deltaTime * timeScale;
            UpdateTimerDisplay();
            
            if (comboTimer <= 0)
            {
                PlayerLost();
            }
        }
    }

    public void StartGame()
    {
        startPanel.SetActive(false);
        gamePanel.SetActive(true);
        gameOverPanel.SetActive(false);
        
        currentLevel = 1;
        enemiesDefeated = 0;
        totalScore = 0;
        playerLives = 3;
        isGameActive = true;
        
        UpdateLivesDisplay();
        
        StartCoroutine(TransitionCamera(cameraGamePosition));
        
        PlayMusic(gameMusic);
        StartNewRound();
    }

    void StartNewRound()
    {
        currentCombo.Clear();
        currentComboIndex = 0;
        waitingForAttack = false;
        comboCompleted = false;
        hasSlowMotionPowerUp = false;
        hasExtraLifePowerUp = false;
        
        if (playerTransform != null)
            playerTransform.position = playerStartPosition;
        if (enemyTransform != null)
            enemyTransform.position = enemyStartPosition;
        
        // Boss tous les 5 niveaux
        isBossFight = (currentLevel % 5 == 0);
        
        if (isBossFight)
        {
            currentEnemy = bossData;
            PlayMusic(bossMusic);
            ShowPowerUpText("BOSS FIGHT!");
        }
        else if (enemyTypes != null && enemyTypes.Length > 0)
        {
            int enemyIndex;
            if (currentLevel <= 3)
                enemyIndex = 0;
            else if (currentLevel <= 7)
                enemyIndex = Mathf.Min(1, enemyTypes.Length - 1);
            else
                enemyIndex = Mathf.Min(2, enemyTypes.Length - 1);
            
            currentEnemy = enemyTypes[enemyIndex];
        }
        
        // Randomisation de l'apparence
        if (enemyRenderer != null && enemyRenderer.sharedMaterial != null)
        {
            Material mat = enemyRenderer.sharedMaterial;

            if (isBossFight)
            {
                // Boss toujours noir
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", Color.black);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", Color.black);
            }
            else if (randomizeEnemyAppearance && randomEnemyColors != null && randomEnemyColors.Length > 0)
            {
                Color randomColor = randomEnemyColors[Random.Range(0, randomEnemyColors.Length)];

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", randomColor);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", randomColor);
            }
        }

        
        // Génération du combo
        int comboLength = baseComboLength + (currentLevel / 3);
        if (isBossFight)
            comboLength += 2;
        
        for (int i = 0; i < comboLength; i++)
        {
            currentCombo.Add((SwipeDirection)Random.Range(0, 4));
        }
        currentCombo.Add(SwipeDirection.Tap);
        
        // Calcul du temps
        float speedMult = currentEnemy != null ? currentEnemy.speedMultiplier : 1f;
        comboTimer = Mathf.Max(2f, (baseComboTime - (currentLevel * timeDecreasePerLevel)) / speedMult);
        
        UpdateUI();
        
        if (ComboDisplay3D.Instance != null)
        {
            ComboDisplay3D.Instance.ShowCombo(currentCombo);
        }
        
        if (playerTrail != null)
            playerTrail.enabled = true;
        
        // 🆕 LANCER LA PHASE DE PRÉVISUALISATION
        StartCoroutine(ComboPreviewPhase());
    }

    // 🆕 NOUVELLE FONCTION : Phase de prévisualisation
    IEnumerator ComboPreviewPhase()
    {
        isPreviewPhase = true;
        
        // Ralentir les animations
        SetAnimationSpeed(idleAnimationSpeed);
        
        // Afficher le compte à rebours
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = "PRÊT ?";
        }
        
        yield return new WaitForSeconds(0.5f);
        
        // Compte à rebours
        float remainingTime = comboPreviewTime;
        while (remainingTime > 0)
        {
            if (countdownText != null)
            {
                countdownText.text = Mathf.Ceil(remainingTime).ToString();
            }
            remainingTime -= Time.deltaTime;
            yield return null;
        }
        
        // GO !
        if (countdownText != null)
        {
            countdownText.text = "GO!";
        }
        
        yield return new WaitForSeconds(0.3f);
        
        // Cacher le compte à rebours
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(false);
        }
        
        // Phase de jeu commence
        isPreviewPhase = false;
    }

    public void OnSwipe(SwipeDirection direction)
    {
        if (!isGameActive || waitingForAttack || isPreviewPhase) return;

        PlaySFX(swipeSound);

        if (currentComboIndex < currentCombo.Count)
        {
            if (currentCombo[currentComboIndex] == direction)
            {
                currentComboIndex++;
                
                if (ComboDisplay3D.Instance != null)
                {
                    ComboDisplay3D.Instance.UpdateProgress(currentComboIndex);
                }
                
                if (currentComboIndex == currentCombo.Count - 1)
                {
                    waitingForAttack = true;
                }
            }
            else
            {
                PlayerLost();
            }
        }
    }

    public void OnTap()
    {        
        if (!isGameActive || !waitingForAttack || isPreviewPhase) return;

        if (currentCombo[currentComboIndex] == SwipeDirection.Tap)
        {
            PlayerWon();
        }
        else
        {
            PlayerLost();
        }
    }

    void PlayerWon()
    {
        isGameActive = false;
        comboCompleted = true;
        
        // Ralentir les animations
        SetAnimationSpeed(idleAnimationSpeed);
        
        StartCoroutine(PlayerAttackSequence());
    }

    IEnumerator PlayerAttackSequence()
    {
        // Sauvegarde de la position d'origine
        Vector3 originPos = playerTransform.position;

        // Position d'avance (+1 sur X)
        Vector3 forwardPos = originPos + new Vector3(1f, 0f, 0f);

        float moveDuration = 0.15f;
        float t = 0f;

        // ▶️ AVANCE AVANT L'ANIMATION
        while (t < moveDuration)
        {
            playerTransform.position = Vector3.Lerp(originPos, forwardPos, t / moveDuration);
            t += Time.deltaTime;
            yield return null;
        }
        playerTransform.position = forwardPos;

        // Petite pause dramatique
        yield return new WaitForSeconds(0.3f);

        // ✅ ANIMATION ATTACK
        SetAnimationSpeed(1f);

        if (playerAnimator != null)
            playerAnimator.SetTrigger("Attack");

        PlaySFX(hitSound);

        yield return new WaitForSeconds(1.2f);

        if (hitParticles != null)
        {
            hitParticles.transform.position = enemyTransform.position;
            hitParticles.Play();
        }

        StartCoroutine(CameraShake());

        // ✅ HIT ENNEMI
        if (enemyAnimator != null)
            enemyAnimator.SetTrigger("Hit");

        // ▶️ RETOUR À LA POSITION D'ORIGINE (−2,0,0 par rapport à l'avance)
        t = 0f;
        while (t < moveDuration)
        {
            playerTransform.position = Vector3.Lerp(forwardPos, originPos, t / moveDuration);
            t += Time.deltaTime;
            yield return null;
        }
        playerTransform.position = originPos;

        // Calcul du score
        enemiesDefeated++;
        int scoreGain = scorePerEnemy + (currentLevel * bonusScorePerLevel);
        if (currentEnemy != null)
            scoreGain *= currentEnemy.scoreMultiplier;
        
        totalScore += scoreGain;
        currentLevel++;
        
        UpdateUI();
        
        // Power-up
        float actualPowerUpChance = powerUpChance;
        if (randomizePowerUpChance)
        {
            actualPowerUpChance = Random.Range(0.2f, 0.4f);
        }
        
        if (Random.value < actualPowerUpChance && !isBossFight)
        {
            GivePowerUp();
        }
        
        PlaySFX(victorySound);
        
        yield return new WaitForSeconds(1f);
        
        isGameActive = true;
        StartNewRound();
    }

    void PlayerLost()
    {
        isGameActive = false;
        comboCompleted = false;
        
        // Ralentir les animations
        SetAnimationSpeed(idleAnimationSpeed);
        
        if (hasExtraLifePowerUp)
        {
            hasExtraLifePowerUp = false;
            ShowPowerUpText("VIE SAUVÉE!");
            StartCoroutine(RestartAfterExtraLife());
            return;
        }
        
        playerLives--;
        UpdateLivesDisplay();
        
        if (playerLives > 0)
        {
            StartCoroutine(PlayerLoseSequenceWithRetry());
        }
        else
        {
            StartCoroutine(PlayerLoseSequence());
        }
    }

    IEnumerator PlayerLoseSequenceWithRetry()
    {
        // Petite pause dramatique
        yield return new WaitForSeconds(0.3f);
        
        // Vitesse normale pour l'attaque
        SetAnimationSpeed(1f);
        
        // ✅ ANIMATION ATTACK DE L'ENNEMI
        if (enemyAnimator != null)
            enemyAnimator.SetTrigger("Attack");
        
        PlaySFX(hitSound);
        
        yield return new WaitForSeconds(0.3f);
        
        // ✅ ANIMATION HIT DU JOUEUR
        if (playerAnimator != null)
            playerAnimator.SetTrigger("Hit");
        
        StartCoroutine(CameraShake());
        
        // Recul du joueur
        Vector3 playerOriginal = playerTransform.position;
        Vector3 knockbackPos = playerOriginal + Vector3.back * 2f;
        
        float elapsedTime = 0;
        float knockbackDuration = 0.3f;
        
        while (elapsedTime < knockbackDuration)
        {
            playerTransform.position = Vector3.Lerp(playerOriginal, knockbackPos, elapsedTime / knockbackDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        // Retour du joueur
        elapsedTime = 0;
        while (elapsedTime < knockbackDuration)
        {
            playerTransform.position = Vector3.Lerp(knockbackPos, playerOriginal, elapsedTime / knockbackDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        playerTransform.position = playerOriginal;
        
        ShowPowerUpText($"{playerLives} vies restantes");
        
        yield return new WaitForSeconds(1.5f);
        
        isGameActive = true;
        StartNewRound();
    }

    IEnumerator PlayerLoseSequence()
    {
        // Petite pause dramatique
        yield return new WaitForSeconds(0.3f);
        
        // Vitesse normale pour l'attaque
        SetAnimationSpeed(1f);
        
        if (enemyAnimator != null)
            enemyAnimator.SetTrigger("Attack");
        
        PlaySFX(hitSound);
        
        yield return new WaitForSeconds(0.3f);
        
        if (playerAnimator != null)
            playerAnimator.SetTrigger("Hit");
        
        StartCoroutine(CameraShake());
        
        // Recul du joueur
        Vector3 playerOriginal = playerTransform.position;
        Vector3 knockbackPos = playerOriginal + Vector3.back * 2f;
        
        float elapsedTime = 0;
        float knockbackDuration = 0.3f;
        
        while (elapsedTime < knockbackDuration)
        {
            playerTransform.position = Vector3.Lerp(playerOriginal, knockbackPos, elapsedTime / knockbackDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        PlaySFX(defeatSound);
        
        yield return new WaitForSeconds(0.5f);
        
        ShowGameOver();
    }

    IEnumerator RestartAfterExtraLife()
    {
        yield return new WaitForSeconds(1f);
        isGameActive = true;
        StartNewRound();
    }

    // 🆕 NOUVELLE FONCTION : Contrôler la vitesse des animations
    void SetAnimationSpeed(float speed)
    {
        if (playerAnimator != null)
            playerAnimator.speed = speed;
        if (enemyAnimator != null)
            enemyAnimator.speed = speed;
    }

    IEnumerator CameraShake()
    {
        if (isShaking) yield break;
        
        isShaking = true;
        Vector3 originalPos = mainCamera.transform.position;
        float elapsed = 0f;
        
        while (elapsed < cameraShakeDuration)
        {
            float x = Random.Range(-1f, 1f) * cameraShakeIntensity;
            float y = Random.Range(-1f, 1f) * cameraShakeIntensity;
            
            mainCamera.transform.position = originalPos + new Vector3(x, y, 0);
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.transform.position = originalPos;
        isShaking = false;
    }

    IEnumerator TransitionCamera(Vector3 targetPosition)
    {
        Vector3 startPos = mainCamera.transform.position;
        float elapsedTime = 0;
        float transitionDuration = 1f / cameraTransitionSpeed;
        
        while (elapsedTime < transitionDuration)
        {
            mainCamera.transform.position = Vector3.Lerp(startPos, targetPosition, elapsedTime / transitionDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        mainCamera.transform.position = targetPosition;
        cameraOriginalPosition = targetPosition;
    }

    void GivePowerUp()
    {
        int powerUpType = Random.Range(0, 2);
        
        PlaySFX(powerUpSound);
        
        switch (powerUpType)
        {
            case 0:
                hasSlowMotionPowerUp = true;
                ShowPowerUpText("TEMPS RALENTI!");
                StartCoroutine(SlowMotionCountdown());
                break;
            case 1:
                hasExtraLifePowerUp = true;
                ShowPowerUpText("VIE BONUS!");
                if (playerLives < 3)
                {
                    playerLives += 1;
                    UpdateLivesDisplay();
                }
                break;
        }
    }

    IEnumerator SlowMotionCountdown()
    {
        yield return new WaitForSeconds(slowMotionDuration);
        hasSlowMotionPowerUp = false;
    }

    void ShowPowerUpText(string text)
    {
        if (powerUpText != null)
        {
            powerUpText.text = text;
            if (powerUpPanel != null)
            {
                powerUpPanel.SetActive(true);
                StartCoroutine(HidePowerUpText());
            }
        }
    }

    IEnumerator HidePowerUpText()
    {
        yield return new WaitForSeconds(2f);
        if (powerUpPanel != null)
            powerUpPanel.SetActive(false);
    }

    void ShowStartScreen()
    {
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameOverPanel.SetActive(false);
        isGameActive = false;
        
        if (playerTrail != null)
            playerTrail.enabled = false;
        
        // Vitesse normale pour le menu
        SetAnimationSpeed(1f);
    }

    void ShowGameOver()
    {
        gameOverPanel.SetActive(true);
        gamePanel.SetActive(false);
        
        PlayMusic(menuMusic);
        
        if (playerTrail != null)
            playerTrail.enabled = false;
        
        // Vitesse normale
        SetAnimationSpeed(1f);
        
        if (finalScoreText != null)
        {
            finalScoreText.text = $"Score: {totalScore}\nEnnemis: {enemiesDefeated}\nNiveau: {currentLevel - 1}";
        }
    }

    public void RetryGame()
    {
        PlaySFX(swipeSound);
        StartGame();
    }

    public void GoToMainMenu()
    {
        PlaySFX(swipeSound);
        StartCoroutine(TransitionCamera(cameraStartPosition));
        ShowStartScreen();
    }

    void UpdateUI()
    {
        if (levelText != null)
        {
            if (isBossFight)
                levelText.text = "BOSS " + (currentLevel / 5);
            else
                levelText.text = "N. " + currentLevel;
        }
        
        if (scoreText != null)
            scoreText.text = totalScore.ToString();
    }

    void UpdateLivesDisplay()
    {
        if (lifeIcons != null && lifeIcons.Length > 0)
        {
            for (int i = 0; i < lifeIcons.Length; i++)
            {
                if (lifeIcons[i] != null)
                {
                    lifeIcons[i].SetActive(i < playerLives);
                }
            }
        }
    }

    void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            timerText.text = Mathf.Ceil(comboTimer).ToString();
            
            if (comboTimer <= 1f)
                timerText.color = Color.red;
            else if (comboTimer <= 2f)
                timerText.color = Color.yellow;
            else
                timerText.color = Color.white;
        }
    }

    void PlayMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            musicSource.Play();
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}

public enum SwipeDirection
{
    Up,
    Down,
    Left,
    Right,
    Tap
}