using System;
using System.Linq;
using UnityEngine;

public class ModifierIndicator : MonoBehaviour
{
    public int IndicatorLevel => _indicatorLevel;
    private int _indicatorLevel = 0;
    private readonly int _maxIndicatorLevel = 3;

    [SerializeField] private GameObject _oneStar;
    [SerializeField] private GameObject _twoStar;
    [SerializeField] private GameObject _threeStar;


    private void Start()
    {
        // Should reset everything
        ResolveIndicatorLevel();
    }

    public bool CanReceiveMoreModifiers()
    {
        return _indicatorLevel < _maxIndicatorLevel;
    }

    public void IncreaseIndicatorLevel()
    {
        _indicatorLevel++;
        ResolveIndicatorLevel();
    }

    private void ResolveIndicatorLevel()
    {
        _oneStar.SetActive(false);
        _twoStar.SetActive(false);
        _threeStar.SetActive(false);

        if (_indicatorLevel == 0) return;

        // NOTE: different materials are currently not in use
        //int metalLevel = Mathf.FloorToInt(levelZeroIsOne / 3f);
        //Material mat = _bronceMat;

        //switch (metalLevel)
        //{
        //    case 0: mat = _bronceMat;
        //        break;
        //    case 1: mat = _silverMat;
        //        break;
        //    case 2: mat = _goldMat;
        //        break;
        //}

        //foreach (Renderer rend in _starRenderers) {
        //    rend.material = mat;
        //}

        int starAmount = _indicatorLevel - 1;

        // NOTE: currently only 3 levels of upgrades are displayed, if more are collected, visuals stay to 3
        if (starAmount > 2) _threeStar.SetActive(true);
        else
        {
            switch (starAmount)
            {
                case 0:
                    _oneStar.SetActive(true);
                    break;

                case 1:
                    _twoStar.SetActive(true);
                    break;

                case 2:
                    _threeStar.SetActive(true);
                    break;

                default:
                    break;
            }
        }
    }
}
