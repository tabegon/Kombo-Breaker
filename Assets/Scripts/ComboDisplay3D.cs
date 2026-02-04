using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ComboDisplay3D : MonoBehaviour
{
    public static ComboDisplay3D Instance;
    
    [Header("UI References")]
    public Transform comboContainer;
    public GameObject arrowPrefab;
    
    [Header("Arrow Sprites")]
    public Sprite upArrow;
    public Sprite downArrow;
    public Sprite leftArrow;
    public Sprite rightArrow;
    public Sprite tapIcon;
    
    [Header("Colors")]
    public Color pendingColor = new Color(1f, 1f, 1f, 0.5f); // Blanc semi-transparent
    public Color completedColor = new Color(0f, 1f, 0f, 1f); // Vert
    public Color currentColor = new Color(1f, 0.92f, 0.016f, 1f); // Jaune vif
    public Color wrongColor = new Color(1f, 0f, 0f, 1f); // Rouge
    
    [Header("Animation Settings")]
    public float scaleAnimation = 1.2f;
    public float animationSpeed = 0.2f;
    
    private List<Image> comboIcons = new List<Image>();
    private int currentArrowIndex = 0;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void ShowCombo(List<SwipeDirection> combo)
    {
        ClearCombo();
        currentArrowIndex = 0;
        
        for (int i = 0; i < combo.Count; i++)
        {
            GameObject iconObj = Instantiate(arrowPrefab, comboContainer);
            Image iconImage = iconObj.GetComponent<Image>();
            
            // Définir le sprite selon la direction
            switch (combo[i])
            {
                case SwipeDirection.Up:
                    iconImage.sprite = upArrow;
                    break;
                case SwipeDirection.Down:
                    iconImage.sprite = downArrow;
                    break;
                case SwipeDirection.Left:
                    iconImage.sprite = leftArrow;
                    break;
                case SwipeDirection.Right:
                    iconImage.sprite = rightArrow;
                    break;
                case SwipeDirection.Tap:
                    iconImage.sprite = tapIcon;
                    break;
            }
            
            // Couleur par défaut : en attente
            iconImage.color = pendingColor;
            
            // Taille de base
            iconObj.transform.localScale = Vector3.one;
            
            comboIcons.Add(iconImage);
        }
        
        // Mettre en évidence la première flèche
        if (comboIcons.Count > 0)
        {
            HighlightCurrentArrow(0);
        }
    }

    public void UpdateProgress(int currentIndex)
    {
        if (currentIndex <= 0 || currentIndex > comboIcons.Count) return;
        
        // Marquer la flèche précédente comme complétée
        int previousIndex = currentIndex - 1;
        if (previousIndex >= 0 && previousIndex < comboIcons.Count)
        {
            comboIcons[previousIndex].color = completedColor;
            comboIcons[previousIndex].transform.localScale = Vector3.one * 0.8f; // Réduire un peu
        }
        
        // Mettre en évidence la flèche actuelle
        if (currentIndex < comboIcons.Count)
        {
            HighlightCurrentArrow(currentIndex);
        }
    }

    void HighlightCurrentArrow(int index)
    {
        if (index < 0 || index >= comboIcons.Count) return;
        
        currentArrowIndex = index;
        
        // Réinitialiser toutes les flèches
        for (int i = 0; i < comboIcons.Count; i++)
        {
            if (i < index)
            {
                // Flèches déjà complétées
                comboIcons[i].color = completedColor;
                comboIcons[i].transform.localScale = Vector3.one * 0.8f;
            }
            else if (i == index)
            {
                // Flèche actuelle
                comboIcons[i].color = currentColor;
                comboIcons[i].transform.localScale = Vector3.one * scaleAnimation;
            }
            else
            {
                // Flèches en attente
                comboIcons[i].color = pendingColor;
                comboIcons[i].transform.localScale = Vector3.one;
            }
        }
    }

    public void ShowWrongMove(int index)
    {
        if (index >= 0 && index < comboIcons.Count)
        {
            comboIcons[index].color = wrongColor;
        }
    }

    void ClearCombo()
    {
        foreach (Image icon in comboIcons)
        {
            if (icon != null)
                Destroy(icon.gameObject);
        }
        comboIcons.Clear();
        currentArrowIndex = 0;
    }
}