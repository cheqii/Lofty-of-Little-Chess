using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Serialization;

namespace _Lofty.Hidden.UI
{
    public class HealthUIManager : Singleton<HealthUIManager>
    {
        public NewPlayer playerRosso;

        [Header("Player GUI")]
        // [SerializeField] private PlayerStatsUI playerStatsUI; // own player currently stats to set update the stats in UI

        // [Header("Heart Content")]
        // [SerializeField] private Transform healthUIParent;
        // [SerializeField] private Transform healthTempUIParent;
        //
        // [SerializeField] private GameObject healthUIPrefab;
        //
        // [SerializeField] private List<HealthUI> healthUIList;
        // [SerializeField] private List<HealthUI> healthTempUIList;

        [Header("Heart Slider")]
        [SerializeField] private Slider healthUISlider;
        [SerializeField] private TMP_Text healthAmountText;
        [SerializeField] private float hpLerpSpeed;

        [SerializeField] private Slider feedbackSlider;
        [SerializeField] private float feedbackLerpSpeed;

        [Header("Segment Material")]
        [SerializeField] private Material segmentMaterial;
        [SerializeField] private Image ImgMaterial;
        [SerializeField] private int segmentHealth; // relate with max HP

        // yellow heart => armor
        [SerializeField] private GameObject yellowHeartUIDisplay; // on player head
        [SerializeField] private TMP_Text yellowHeartAmountText;

        private Action<int> onHealthChanged;
        private Action<int> onTempHealthChanged;

        public Action<int> OnHealthChanged { get => onHealthChanged; set => onHealthChanged = value; }
        public Action<int> OnTempHealthChanged { get => onTempHealthChanged; set => onTempHealthChanged = value; }

        public int testMaxHP;
        public int testCurrentHP;

        private bool isHealing;

        private void Start()
        {
            healthUISlider.maxValue = playerRosso.MaxHealth;
            healthUISlider.value = playerRosso.CurrentHealth;

            feedbackSlider.maxValue = healthUISlider.maxValue;
            feedbackSlider.value = healthUISlider.value;

            // segmentMaterial = ImgMaterial.GetComponent<Image>().material;
            // segmentHealth =  playerRosso.MaxHealth;

            SegmentSetup(playerRosso.MaxHealth);

            OnHealthChanged = (_health) =>
            {
                UpdateHealth(_health);
            };

            // yellowHeartUIDisplay.transform.DOShakePosition(0.5f, new Vector3(0.2f, 0.2f, 0.2f)).SetLoops(-1, LoopType.Yoyo);
        }

        private void Update()
        {
            // for testing
            if (Input.GetKeyDown(KeyCode.A))
            {
                if(testCurrentHP <= 0) return;
                testCurrentHP -= 1;
                UpdateHealth();
            }
            else if (Input.GetKeyDown(KeyCode.D))
            {
                if(testCurrentHP >= testMaxHP) return;
                testCurrentHP += 1;
                UpdateHealth();
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                testMaxHP += 1;
                UpdateHealth();
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                testMaxHP -= 1;
                UpdateHealth();
            }

            // SegmentSetup(testMaxHP);

            // LerpHP();

        }

        private void SegmentSetup(int _amount)
        {
            segmentHealth = _amount;

            healthUISlider.maxValue = segmentHealth;
            feedbackSlider.maxValue = segmentHealth;

            if (segmentHealth < playerRosso.CurrentHealth)
            {
                UpdateHealth();
            }

            segmentMaterial.SetFloat("_SegmentAmount", segmentHealth);
        }

        public void UpdateHealth()
        {
            // healthUISlider.DOValue(testCurrentHP, 0.2f).SetEase(Ease.InOutBounce);
            // feedbackSlider.DOValue(testCurrentHP, 1f).SetEase(Ease.OutQuad);
            healthUISlider.DOValue(playerRosso.CurrentHealth, 0.2f).SetEase(Ease.InOutBounce);
            feedbackSlider.DOValue(playerRosso.CurrentHealth, 1f).SetEase(Ease.OutQuad);


            // UpdateYellowHeart();
        }

        public void UpdateHealth(int _amountHealth)
        {
            healthUISlider.DOValue(_amountHealth, 0.2f).SetEase(Ease.InOutBounce);
            feedbackSlider.DOValue(_amountHealth, 1f).SetEase(Ease.OutQuad);

            // UpdateYellowHeart();
        }

        private void UpdateYellowHeart()
        {
            yellowHeartUIDisplay.SetActive(playerRosso.TempHealth > 0);
            yellowHeartAmountText.text = playerRosso.TempHealth.ToString();
        }
    }
}
