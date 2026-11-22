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
        [SerializeField] private float showAnimationDuration = 0.2f;
        private const string PLAYER_TAG = "Player";
        
        public MobData Data { get; private set; }
        private MobModel model;
        private MotionHandle motion;
        private CombatSystem combatSystem;
        private Health playerHealth;
        private bool isInCombat;
        private Transform cameraTransform;
        private Coroutine animationCoroutine;
        private int mobHealth;

        public Action MobKilled;

        private void Awake()
        {
            combatSystem = FindAnyObjectByType<CombatSystem>();
            playerHealth = FindAnyObjectByType<Health>();
        }

        private void OnEnable()
        {
            EventManager.AddListener(Consts.Events.Player.MOVED, PlayerMoved);
        }

        private void OnDisable()
        {
            EventManager.RemoveListener(Consts.Events.Player.MOVED, PlayerMoved);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(PLAYER_TAG))
                return;
            
            combatSystem.StartCombat(this, CombatEnded);
            isInCombat = true;
            
            cameraTransform = other.transform.GetComponentInChildren<Camera>().transform;
        }

        private void PlayerMoved()
        {
            if (!isInCombat)
                return;
            
            ShowMob();
            playerHealth.StartDealingDamage(Data.DamagePerSecond);
        }

        private void CombatEnded()
        {
            playerHealth.StopDealingDamage();
            HideMob();
        }

        public void Init(MobData data, Vector2Int position)
        {
            motion.TryCancel();
            
            Data = data;
            var newPosition = Data.Offset + new Vector3(position.x, 0, position.y);
            transform.localPosition = newPosition;

            model = Instantiate(Data.Model, transform);
            model.ShowModel1();
            model.transform.localPosition = Data.HiddenOffset;
        }

        private void ShowMob()
        {
            var targetPosition = cameraTransform.TransformPoint(new Vector3(0f, 0f, 0.3f));
            
            var startPosition = targetPosition + new Vector3(0f, Data.HiddenOffset.y, 0f);
            
            model.transform.position = startPosition;
            model.transform.LookAt(cameraTransform);
            
            var rot = model.transform.eulerAngles;
            model.transform.rotation = Quaternion.Euler(0f, rot.y, 0f);
            
            motion = LMotion.Create(startPosition, targetPosition, showAnimationDuration)
                .WithEase(Ease.InOutSine)
                .BindToPosition(model.transform)
                .AddTo(this);
            
            animationCoroutine = StartCoroutine(AnimationCoroutine());
        }

        private void HideMob(bool applyColor = true)
        {
            if(applyColor)
                model.SetColor(Color.red);
            
            motion = LMotion.Create(model.transform.localPosition, Data.HiddenOffset, showAnimationDuration)
                .WithEase(Ease.InOutSine)
                .WithOnComplete(() =>
                {
                    if (animationCoroutine != null)
                        StopCoroutine(animationCoroutine);

                    MobKilled?.Invoke();
                    Destroy(gameObject);
                })
                .BindToLocalPosition(model.transform)
                .AddTo(gameObject);
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

        public void HitPlayer()
        {
            playerHealth.DealDamage(Data.DamageOnMistake);
        }
    }
}