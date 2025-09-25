using System;
using System.Collections.Generic;
using _Lofty.Hidden.Helpers;
using UnityEngine;
using UnityEngine.Serialization;
using VInspector;
using Random = UnityEngine.Random;

namespace _Lofty.Hidden.Skill
{
    public class ClassPassive : MonoBehaviour
    {
        [SerializeField] private NewPlayer player;

        private Dictionary<ClassType, ClassTypePassive> classTypeDictionary;

        public Dictionary<ClassType, ClassTypePassive> ClassTypeDictionary = new Dictionary<ClassType, ClassTypePassive>();
        // {
        //     // get
        //     // {
        //     //     if (classTypeDictionary != null) return classTypeDictionary;
        //     //     return classTypeDictionary = new Dictionary<TarotClass, ClassTypePassive>
        //     //     {
        //     //         {TarotClass.SwordKnight, new ClassTypePassive(new ClassTypePassiveConfig
        //     //         {
        //     //             ClassType = TarotClass.SwordKnight,
        //     //             PassiveOneGetter = () => player.PlayerTarot.SwordKnightPassiveOne,
        //     //             PassiveTwoGetter = () => player.PlayerTarot.SwordKnightPassiveTwo
        //     //         })},
        //     //         {TarotClass.BladeMaster, new ClassTypePassive(new ClassTypePassiveConfig
        //     //         {
        //     //             ClassType = TarotClass.BladeMaster,
        //     //             PassiveOneGetter = () => player.PlayerTarot.BladeMasterPassiveOne,
        //     //             PassiveTwoGetter = () => player.PlayerTarot.BladeMasterPassiveTwo
        //     //         })},
        //     //         {TarotClass.ShootingCaster, new ClassTypePassive(new ClassTypePassiveConfig
        //     //         {
        //     //             ClassType = TarotClass.ShootingCaster,
        //     //             PassiveOneGetter = () => player.PlayerTarot.ShootCasterPassiveOne,
        //     //             PassiveTwoGetter = () => player.PlayerTarot.ShootCasterPassiveTwo
        //     //         })}
        //     //     };
        //     // }
        // }

        [Tab("SwordKnight Passive Var")]
        [SerializeField] private int getAttackCount;
        [SerializeField] private GameObject stunAreaPrefab;

        #region -Collapse backup-

        public void ActivatePassive(ClassType _classType, int _damage, Enemy _enemy)
        {
            if (!ClassTypeDictionary.TryGetValue(_classType, out var _class)) return;

            switch (_class.ClassType)
            {
                case ClassType.SwordKnight:
                    // if (_class.PassiveOneGetter())
                    // {
                    //     var _rand = Random.Range(0f, 1f);
                    //     if (_rand <= 0.3f) // add 1 yellow heart to player
                    //     {
                    //         print("add 1 yellow heart");
                    //         // if(player.isTakingDamage)
                    //         //     player.UpgradeHealthTemp(1);
                    //     }

                    //     if (_class.PassiveTwoGetter())
                    //     {
                    //         getAttackCount++;
                    //         if(getAttackCount < 3 && !player.isTakingDamage) return;
                    //         print("stun after get attack 3 times");
                    //         var _stunArea = Instantiate(stunAreaPrefab, player.transform.position, player.transform.rotation);
                    //         // _stunArea.GetComponent<SkillAction>().ActiveSkill();
                    //         getAttackCount = 0;
                    //     }
                    // }
                    break;
                case ClassType.BladeMaster:
                    if (_class.PassiveOneGetter())
                    {
                        // var _rand = Random.Range(0f, 1f);
                        // if (_rand <= 0.15f)
                        // {
                        //     print("heal 1 hp");
                        //     // if(player.PlayerMovementGrid.IsAttacking)
                        //     //     player.TakeHealth(1);
                        // }

                        // if (_class.PassiveTwoGetter() && player.isTakingDamage)
                        // {
                        //     var _rand2 = Random.Range(0f, 1f);
                        //     if(_rand2 > 0.25f)
                        //     {
                        //         // normal damage
                        //         print("fail evade");
                        //         player.DamageCalculation(_damage);
                        //     }
                        //     else
                        //     {
                        //         print("success evade");
                        //         VisualEffectManager.Instance.CallEffect(EffectName.Miss, transform, 1.5f);
                        //         TurnManager.Instance.AddLog(player.Name, "", LogList.Evade, true);
                        //         Debug.Log("Miss");
                        //     }
                        // }
                    }
                    break;
                case ClassType.ShootingCaster:
                    // if (_class.PassiveOneGetter())
                    // {
                    //     var _rand = Random.Range(0f, 1f);
                    //     if (_rand <= 0.4f)
                    //     {
                    //         print("enemy get burn with 40% jackpot!");
                    //         // if(player.PlayerMovementGrid.IsAttacking)
                    //             // player.PlayerMovementGrid.currentEnemy.AddCurseStatus(DebuffType.Burn, 2);
                    //     }

                    //     if (_class.PassiveTwoGetter())
                    //     {
                    //         var _rand2 = Random.Range(0f, 1f);

                    //         if(_enemy != null && !_enemy.IsDead) return;
                    //         if(_rand2 > 0.6f) return;
                    //         print("add more 1 move point");
                    //         player.PlayerMovementGrid.MovePoint += 1;
                    //     }
                    // }
                    break;

            } // end switch-case check by ClassType Brackets

        } // end ActivatePassive(ClassType, Damage, Enemy)

        #endregion

        public void ActivatePassive(int _damage = default, Enemy _enemy = default)
        {
            foreach (ClassType _classType in Enum.GetValues(typeof(ClassType)))
            {
                if (_classType == ClassType.Normal) continue;

                if (!ClassTypeDictionary.TryGetValue(_classType, out var _class)) continue;

                switch (_class.ClassType)
                {
                    case ClassType.SwordKnight:
                        // if (_class.PassiveOneGetter())
                        // {
                        //     var _rand = Random.Range(0f, 1f);
                        //     if (_rand <= 0.3f) // add 1 yellow heart to player
                        //     {
                        //         print("add 1 yellow heart");
                        //         if(player.isTakingDamage)
                        //             player.UpgradeHealthTemp(1);
                        //     }

                        //     if (_class.PassiveTwoGetter())
                        //     {
                        //         getAttackCount++;
                        //         if (getAttackCount < 3 && !player.isTakingDamage) continue;
                        //         print("stun after get attack 3 times");
                        //         var _stunArea = Instantiate(stunAreaPrefab, player.transform.position, player.transform.rotation);
                        //         // _stunArea.GetComponent<SkillAction>().ActiveSkill(); // need improve
                        //         // _stunArea.GetComponent<SkillAction>().CheckOverlapEnemy();
                        //         getAttackCount = 0;
                        //     }
                        // }
                        break;
                    case ClassType.BladeMaster:
                        // if (_class.PassiveOneGetter())
                        // {
                        //     var _rand = Random.Range(0f, 1f);
                        //     if (_rand <= 0.15f)
                        //     {
                        //         print("heal 1 hp");
                        //         if(player.PlayerMovementGrid.IsAttacking)
                        //             player.TakeHealth(1);
                        //     }

                        //     if (_class.PassiveTwoGetter() && player.isTakingDamage)
                        //     {
                        //         var _rand2 = Random.Range(0f, 1f);
                        //         if(_rand2 > 0.25f)
                        //         {
                        //             // normal damage
                        //             print("fail evade");
                        //             player.DamageCalculation(_damage);
                        //         }
                        //         else
                        //         {
                        //             print("success evade");
                        //             VisualEffectManager.Instance.CallEffect(EffectName.Miss, transform, 1.5f);
                        //             TurnManager.Instance.AddLog(player.playerName, "", LogList.Evade, true);
                        //             Debug.Log("Miss");
                        //         }
                        //     }
                        // }
                        break;
                    case ClassType.ShootingCaster:
                        // if (_class.PassiveOneGetter())
                        // {
                        //     var _rand = Random.Range(0f, 1f);
                        //     if (_rand <= 0.4f)
                        //     {
                        //         print("enemy get burn with 40% jackpot!");
                        //         // if(player.PlayerMovementGrid.IsAttacking)
                        //             // player.PlayerMovementGrid.currentEnemy.AddCurseStatus(DebuffType.Burn, 2);
                        //     }

                        //     if (_class.PassiveTwoGetter())
                        //     {
                        //         if(_enemy != null && !_enemy.IsDead) continue;
                        //         var _rand2 = Random.Range(0f, 1f);
                        //         if(_rand2 > 0.6f) continue;
                        //         print("add more 1 move point");
                        //         player.PlayerMovementGrid.MovePoint += 1;
                        //     }
                        // }
                        break;

                } // end switch-case by ClassType

            } // end foreach brackets

        } // end ActivatePassive(Damage, Enemy)


        public bool IsClassPassiveOneActive(ClassType _classType)
        {
            return ClassTypeDictionary.TryGetValue(_classType, out var _class) && _class.PassiveOneGetter();
        }

        public bool IsClassPassiveTwoActive(ClassType _classType)
        {
            return ClassTypeDictionary.TryGetValue(_classType, out var _class) && _class.PassiveTwoGetter();
        }

    }
}
