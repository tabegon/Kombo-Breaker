using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SkinManager3D : MonoBehaviour
{
    public static SkinManager3D Instance;
    
    [System.Serializable]
    public class Skin3D
    {
        public string name;
        public GameObject characterPrefab; // Modèle 3D complet
        public Material characterMaterial; // Ou juste le matériau
        public RuntimeAnimatorController animatorController;
        public bool isUnlocked = false;
    }
    
    [Header("Skins")]
    public List<Skin3D> availableSkins = new List<Skin3D>();
    public int currentSkinIndex = 0;
    
    [Header("UI References")]
    public GameObject skinPanel;
    public Transform skinButtonContainer;
    public GameObject skinButtonPrefab;
    
    [Header("Character References")]
    public Transform playerSpawnPoint;
    private GameObject currentPlayerModel;
    
    private const string CURRENT_SKIN_KEY = "CurrentSkin";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
            
        LoadSkinPreference();
    }

    void Start()
    {
        if (availableSkins.Count > 0)
        {
            availableSkins[0].isUnlocked = true;
        }
        
        ApplySkin(currentSkinIndex);
        PopulateSkinMenu();
    }

    public void ShowSkinMenu()
    {
        skinPanel.SetActive(true);
    }

    public void HideSkinMenu()
    {
        skinPanel.SetActive(false);
    }

    void PopulateSkinMenu()
    {
        foreach (Transform child in skinButtonContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < availableSkins.Count; i++)
        {
            int index = i;
            GameObject buttonObj = Instantiate(skinButtonPrefab, skinButtonContainer);
            
            Button button = buttonObj.GetComponent<Button>();
            Text buttonText = buttonObj.transform.Find("SkinName").GetComponent<Text>();
            GameObject lockIcon = buttonObj.transform.Find("LockIcon").gameObject;
            
            // Preview 3D optionnel
            Transform previewContainer = buttonObj.transform.Find("Preview3D");
            if (previewContainer != null && availableSkins[i].characterPrefab != null)
            {
                GameObject preview = Instantiate(availableSkins[i].characterPrefab, previewContainer);
                preview.transform.localPosition = Vector3.zero;
                preview.transform.localScale = Vector3.one * 0.5f;
                
                // Rotation pour l'aperçu
                preview.transform.Rotate(0, 45, 0);
            }
            
            buttonText.text = availableSkins[i].name;
            
            if (availableSkins[i].isUnlocked)
            {
                lockIcon.SetActive(false);
                button.onClick.AddListener(() => SelectSkin(index));
            }
            else
            {
                lockIcon.SetActive(true);
                button.interactable = false;
            }
            
            if (i == currentSkinIndex)
            {
                buttonObj.GetComponent<Image>().color = Color.yellow;
            }
        }
    }

    public void SelectSkin(int skinIndex)
    {
        if (skinIndex >= 0 && skinIndex < availableSkins.Count && availableSkins[skinIndex].isUnlocked)
        {
            currentSkinIndex = skinIndex;
            ApplySkin(skinIndex);
            SaveSkinPreference();
            PopulateSkinMenu();
        }
    }

    void ApplySkin(int skinIndex)
    {
        if (skinIndex >= 0 && skinIndex < availableSkins.Count)
        {
            Skin3D skin = availableSkins[skinIndex];
            
            // Détruire l'ancien modèle
            if (currentPlayerModel != null)
            {
                Destroy(currentPlayerModel);
            }
            
            // Instancier le nouveau modèle
            if (skin.characterPrefab != null && playerSpawnPoint != null)
            {
                currentPlayerModel = Instantiate(skin.characterPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
                currentPlayerModel.transform.SetParent(playerSpawnPoint);
                
                // Mettre à jour la référence dans le GameManager
                if (GameManager3D.Instance != null)
                {
                    GameManager3D.Instance.playerTransform = currentPlayerModel.transform;
                    
                    Animator animator = currentPlayerModel.GetComponent<Animator>();
                    if (animator != null)
                    {
                        GameManager3D.Instance.playerAnimator = animator;
                        
                        if (skin.animatorController != null)
                        {
                            animator.runtimeAnimatorController = skin.animatorController;
                        }
                    }
                }
            }
            // Ou appliquer juste le matériau si on garde le même modèle
            else if (skin.characterMaterial != null && currentPlayerModel != null)
            {
                Renderer renderer = currentPlayerModel.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = skin.characterMaterial;
                }
            }
        }
    }

    public void UnlockSkin(int skinIndex)
    {
        if (skinIndex >= 0 && skinIndex < availableSkins.Count)
        {
            availableSkins[skinIndex].isUnlocked = true;
            PopulateSkinMenu();
        }
    }

    void SaveSkinPreference()
    {
        PlayerPrefs.SetInt(CURRENT_SKIN_KEY, currentSkinIndex);
        PlayerPrefs.Save();
    }

    void LoadSkinPreference()
    {
        currentSkinIndex = PlayerPrefs.GetInt(CURRENT_SKIN_KEY, 0);
    }
}