using System;
using System.Collections.Generic;
using _Lofty.Hidden.Scriptable;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Lofty.Hidden.Helpers
{
    #region -Tarot Enum Type-

    public enum TarotUpgradeType
    {
        Status, Ability, Currency
    }

    public enum TarotGrade
    {
        Common, Rare, Epic, All
    }

    public enum ClassType
    {
        Normal, SwordKnight, BladeMaster, ShootingCaster
    }

    // public enum TarotEffectType
    // {
    //     None,
    //     HeartOfFlames,
    //     HeartOfFlamesII,
    //     BattleReady,
    //     PatienceControl,
    //     FighterHeart,
    //     LotsOfExperience,
    //     TheLostFeather,
    //     Jackpot,
    //     GodOfWar,
    //     TheEyeOfTheKing,
    //     Motivation,
    //     DeathDoor,
    //     GiftOfDeath,
    //     FlameMessage,
    //     Checkmate,
    //     CrownOfInfluence,
    //     BlazingBomb,
    //     BlueFlameDroplet,
    //     IronBody,
    //     LastChance
    // }

    public enum TarotSkill
    {
        None,
        DeactivateTrap,
        Revive,
        Warp,
        Controlling,
        Bomb,
        Shield
    }

    #endregion

    public class TarotTypeHelper : MonoBehaviour
    {
        #region -Tarot Data Lists-

        [SerializeField] private List<TarotData> allTarotInGame;

        [Header("Tarot By Grade")]
        private List<TarotData> allCommonGrade;
        private List<TarotData> allRareGrade;
        private List<TarotData> allEpicGrade;

        public List<TarotData> AllTarotInGame { get => allTarotInGame; set => allTarotInGame = value; }
        public List<TarotData> AllCommonGrade { get => allCommonGrade; set => allCommonGrade = value; }
        public List<TarotData> AllRareGrade { get => allRareGrade; set => allRareGrade = value; }
        public List<TarotData> AllEpicGrade { get => allEpicGrade; set => allEpicGrade = value; }

        [Header("Tarot By Class")]
        private List<TarotData> allNormalClass;
        private List<TarotData> allSwordKnightClass;
        private List<TarotData> allBladeMasterClass;
        private List<TarotData> allShootCasterClass;

        public List<TarotData> AllNormalClass { get => allNormalClass; set => allNormalClass = value; }
        public List<TarotData> AllSwordKnightClass { get => allSwordKnightClass; set => allSwordKnightClass = value; }
        public List<TarotData> AllBladeMasterClass { get => allBladeMasterClass; set => allBladeMasterClass = value; }
        public List<TarotData> AllShootCasterClass { get => allShootCasterClass; set => allShootCasterClass = value; }

        #endregion

        #region # Tarot Dictionaries

        // private bool initialTarotEffectDict;

        // private Dictionary<TarotEffectType, TarotAction> tarotEffectActions;

        // public Dictionary<TarotEffectType, TarotAction> TarotEffectActions
        // {
        //     get
        //     {
        //         if (initialTarotEffectDict) return tarotEffectActions;
        //         tarotEffectActions = new Dictionary<TarotEffectType, TarotAction>
        //         {
        //             {TarotEffectType.HeartOfFlames, new TarotAction(
        //                 _val => HeartOfFlames = _val,
        //                 () => { })},
        //             {TarotEffectType.HeartOfFlamesII, new TarotAction(
        //                 _val => HeartOfFlamesII = _val,
        //                 () => { })},
        //             {TarotEffectType.BattleReady, new TarotAction(
        //                 _val => BattleReady = _val,
        //                 () => { })},
        //             {TarotEffectType.PatienceControl, new TarotAction(
        //                 _val => PatienceControl = _val,
        //                 () => { })},
        //             {TarotEffectType.FighterHeart, new TarotAction(
        //                 _val => FighterHeart = _val,
        //                 () => { })},
        //             {TarotEffectType.LotsOfExperience, new TarotAction(
        //                 _val => LotsOfExperience = _val,
        //                 () => { })},
        //             {TarotEffectType.TheLostFeather, new TarotAction(
        //                 _val => TheLostFeather = _val,
        //                 () => { })},
        //             {TarotEffectType.Jackpot, new TarotAction(
        //                 _val => Jackpot = _val,
        //                 () => { })},
        //             {TarotEffectType.GodOfWar, new TarotAction(
        //                 _val => GodOfWar = _val,
        //                 () => { })},
        //             {TarotEffectType.TheEyeOfTheKing, new TarotAction(
        //                 _val => TheEyeOfTheKing = _val,
        //                 () => { })},
        //             {TarotEffectType.Motivation, new TarotAction(
        //                 _val => Motivation = _val,
        //                 () => { })},
        //             {TarotEffectType.DeathDoor, new TarotAction(
        //                 _val => DeathDoor = _val,
        //                 () => { })},
        //             {TarotEffectType.GiftOfDeath, new TarotAction(
        //                 _val => GiftOfDeath = _val,
        //                 () => { })},
        //             {TarotEffectType.FlameMessage, new TarotAction(
        //                 _val => FlameMessage = _val,
        //                 () => { })},
        //             {TarotEffectType.Checkmate, new TarotAction(
        //                 _val => Checkmate = _val,
        //                 () => { })},
        //             {TarotEffectType.CrownOfInfluence, new TarotAction(
        //                 _val => CrownOfInfluence = _val,
        //                 () => { })},
        //             {TarotEffectType.BlazingBomb, new TarotAction(
        //                 _val => BlazingBomb = _val,
        //                 () => { })},
        //             {TarotEffectType.BlueFlameDroplet, new TarotAction(
        //                 _val => BlueFlameDroplet = _val,
        //                 () => { })},
        //             {TarotEffectType.IronBody, new TarotAction(
        //                 _val => IronBody = _val,
        //                 () => { })},
        //             {TarotEffectType.LastChance, new TarotAction(
        //                 _val => LastChance = _val,
        //                 () => { })}
        //         };

        //         AssignTarot();
        //         initialTarotEffectDict = true;
        //         return tarotEffectActions;
        //     }
        //     set => tarotEffectActions = value;
        // }


        private bool initialTarotByGradeDict;

        private Dictionary<TarotGrade, List<TarotData>> tarotByGradeType;

        public Dictionary<TarotGrade, List<TarotData>> TarotByGradeType
        {
            get
            {
                if (initialTarotByGradeDict) return tarotByGradeType;
                tarotByGradeType = new Dictionary<TarotGrade, List<TarotData>>
                {
                    {TarotGrade.Common, AllCommonGrade},
                    {TarotGrade.Rare, AllRareGrade},
                    {TarotGrade.Epic, AllEpicGrade}
                };

                SortTarotByGrade();
                // initialTarotEffectDict = true;
                return tarotByGradeType;
            }
        }

        private bool initialTarotByClassDict;
        private Dictionary<ClassType, List<TarotData>> tarotByClassType;

        public Dictionary<ClassType, List<TarotData>> TarotByClassType
        {
            get
            {
                if (initialTarotByClassDict) return tarotByClassType;
                tarotByClassType = new Dictionary<ClassType, List<TarotData>>
                {
                    {ClassType.Normal, AllNormalClass},
                    {ClassType.SwordKnight, AllSwordKnightClass},
                    {ClassType.BladeMaster, AllBladeMasterClass},
                    {ClassType.ShootingCaster, AllShootCasterClass}
                };
                SortTarotByClass();
                initialTarotByClassDict = true;
                return tarotByClassType;
            }
        }

        #endregion

        private void AssignTarot()
        {
            // foreach (var _tarot in AllTarotInGame)
            // {
            //     if (!tarotEffectActions.TryGetValue(_tarot.EffectType, out var _tarotEffect)) continue;
            //     _tarotEffect.TarotData = _tarot;
            // }
        }

        private void SortTarotByGrade()
        {
            foreach (var _tarot in AllTarotInGame)
            {
                if (!tarotByGradeType.TryGetValue(_tarot.Grade, out var _tarotGrade)) continue;
                _tarotGrade.Add(_tarot);
            }
        }

        private void SortTarotByClass()
        {
            foreach (var _tarot in AllTarotInGame)
            {
                if (!tarotByClassType.TryGetValue(_tarot.Class, out var _tarotClass)) continue;
                _tarotClass.Add(_tarot);
            }
        }
    }
}
