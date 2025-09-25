using System.Collections.Generic;
using _Lofty.Hidden.Helpers;
using UnityEngine;
using VInspector;

namespace _Lofty.Hidden.Scriptable
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Tarot", fileName = "NewTarotData", order = 0)]
    public class TarotData : ScriptableObject
    {
        // public TarotEffectType EffectType;
        public TarotGrade Grade;
        public ClassType Class;

        [Space]
        [Header("GUI")]
        public string TarotName;
        public Sprite TarotSprite;

        [Space]
        [TextArea]
        public string Description;

        [Space(10f)]
        public TarotUpgradeType UpgradeType;

        [Space]
        [ShowIf("UpgradeType", TarotUpgradeType.Status)]

        [Header("About Health")]
        public int UpgradeHp;
        public int UpgradeMaxHp;
        public int UpgradeYellowHeart;
        public float UpgradeHealMultiple;

        [Header("About Combat")]
        public int UpgradeDamage;
        public int UpgradeKnockBackRange;
        [Tooltip("Not a real damage just a chance to make this damage.")]
        public int UpgradeTempDamage; // not real damage just a chance to create this damage

        [Header("About Movement")]
        public int UpgradeSpeed;
        public int UpgradeActionPoint;

        [Space]
        [ShowIf("UpgradeType", TarotUpgradeType.Currency)]
        [Header("About Currency")]
        public float UpgradeFlameSoulMultiple;
        public float UpgradeCoinMultiple;

        [Space]
        [ShowIf("UpgradeType", TarotUpgradeType.Ability)]
        [Header("About Ability")]
        public TarotSkill TarotSkill;

        [Space]
        public bool DebuffNoEffect;
        public List<DebuffType> debuffTypes = new List<DebuffType>();

        #region # Random card variables

        [Space]
        public bool IsRandomCard;

        [ShowIf("IsRandomCard")]
        public bool RandomByGrade;
        public bool RandomByClass;
        // if want to random by grade
        public TarotGrade RandomGrade;
        // if want to random by class
        public ClassType RandomClass;

        #endregion
    }
}
