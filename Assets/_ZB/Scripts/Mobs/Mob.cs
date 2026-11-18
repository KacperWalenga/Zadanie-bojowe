using System;
using System.Collections;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace ZB.Mobs
{
    [RequireComponent(typeof(Collider))]
    public class Mob : MonoBehaviour
    {
        [SerializeField] private float showAnimationDuration = 0.5f;
        private const string PLAYER_TAG = "Player";
        
        private MobData mobData;
        private MobModel model;
        private MotionHandle motion;
        private CombatSystem combatSystem;
        private bool isInCombat;
        private Transform cameraTransform;
        private Coroutine animationCoroutine;

        private void Awake()
        {
            EventManager.AddListener(Consts.Events.PLAYER_MOVED, PlayerMoved);
            EventManager.AddListener(Consts.Events.Combat.COMBAT_ENDED, CombatEnded);

            combatSystem = FindAnyObjectByType<CombatSystem>();
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener(Consts.Events.PLAYER_MOVED, PlayerMoved);
            EventManager.RemoveListener(Consts.Events.Combat.COMBAT_ENDED, CombatEnded);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(PLAYER_TAG))
                return;
            
            combatSystem.StartCombat(mobData);
            isInCombat = true;
            
            cameraTransform = other.transform.GetComponentInChildren<Camera>().transform;
        }

        private void PlayerMoved()
        {
            if (!isInCombat)
                return;
            
            ShowMob();
        }

        private void CombatEnded()
        {
            throw new NotImplementedException();
        }

        public void Init(MobData data, Vector2Int position)
        {
            motion.TryCancel();
            
            mobData = data;
            var newPosition = mobData.Offset + new Vector3(position.x, 0, position.y);
            transform.position = newPosition;

            model = Instantiate(mobData.Model, transform);
            model.ShowModel1();
            model.transform.localPosition = mobData.HiddenOffset;
        }

        private void ShowMob()
        {
            var position = cameraTransform.TransformPoint(new Vector3(0, 0, 0.3f));
            
            model.transform.localPosition = position + new Vector3(0, mobData.HiddenOffset.y, 0);
            model.transform.LookAt(cameraTransform);

            var rot = model.transform.eulerAngles;
            model.transform.rotation = Quaternion.Euler(0f, rot.y, 0f);
            
            motion = LMotion.Create(model.transform.localPosition, position, showAnimationDuration)
                .WithEase(Ease.InOutSine)
                .BindToPosition(model.transform)
                .AddTo(this);

            animationCoroutine = StartCoroutine(AnimationCoroutine());
        }

        private void HideMob()
        {
            motion = LMotion.Create(model.transform.localPosition, mobData.HiddenOffset, showAnimationDuration)
                .WithEase(Ease.InOutSine)
                .BindToLocalPosition(model.transform)
                .AddTo(this);
            
            StopCoroutine(animationCoroutine);
        }

        private IEnumerator AnimationCoroutine()
        {
            while (true)
            {
                model.ShowModel1();
                
                yield return new WaitForSeconds(0.3f);
                
                model.ShowModel2();
                yield return new WaitForSeconds(0.3f);
            }
        }
    }
}