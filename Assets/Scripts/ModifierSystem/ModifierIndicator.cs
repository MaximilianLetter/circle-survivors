using System;
using System.Linq;
using UnityEngine;

public class ModifierIndicator : MonoBehaviour
{
    private int _indicatorLevel = 0;

    [SerializeField] private GameObject _oneStar;
    [SerializeField] private GameObject _twoStar;
    [SerializeField] private GameObject _threeStar;

    // NOTE: different materials are currently not in use

    //[SerializeField] private Material _bronceMat;
    //[SerializeField] private Material _silverMat;
    //[SerializeField] private Material _goldMat;

    //private Renderer[] _starRenderers;

    // NOTE: this could be reworked so that it instead counts (local) modifications,
    // for now it just increases whenever something (a modifier bundle?) is picked up

    //private LocalModifierSystem _localModifierSystem;

    //private void Start()
    //{
    //    _localModifierSystem = GetComponent<LocalModifierSystem>();
    //}


    private void Start()
    {
        // NOTE: different materials are currently not in use
        //_starRenderers = new[]
        //{
        //    _oneStar.GetComponent<Renderer>(),
        //    _twoStar.GetComponent<Renderer>(),
        //    _threeStar.GetComponent<Renderer>(),
        //};

        // Should reset everything
        ResolveIndicatorLevel();
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
