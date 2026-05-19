using UnityEngine;
using UnityEngine.UI;

public class HPGageGager : MonoBehaviour
{
    private int _myHp;
    private int _myMaxHp;
    [SerializeField] private Image _image;
    [SerializeField] private CharacterStats stats;

    private void Awake()
    {
        _myHp = stats.CurrentHP;
        _myMaxHp = stats.MaxHP;
    }

    private void Update()
    {
        _myHp = stats.CurrentHP;

        _image.fillAmount = (float)_myHp / _myMaxHp;
    }
}

    
