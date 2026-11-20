using UnityEngine;
using UnityEngine.UIElements;

namespace ZB
{
    [RequireComponent(typeof(PanelRenderer))]
    public class HpBar : MonoBehaviour
    {
        private const float HorizontalTiling = 0.5f;

        private static readonly int UvScaleId = Shader.PropertyToID("_UVScale");
        private static readonly int FillId = Shader.PropertyToID("_Fill");
        private static readonly int FillBobId = Shader.PropertyToID("_FillBob");
        private static readonly int AgitationId = Shader.PropertyToID("_Agitation");
        private static readonly int BubbleTimeOffsetId = Shader.PropertyToID("_BubbleTimeOffset");

        private readonly HpBarAnimation barAnimation = new();

        private PanelRenderer panelRenderer;
        private VisualElement beerFill;
        private Material runtimeMaterial;

        private void Awake()
        {
            panelRenderer = GetComponent<PanelRenderer>();
            EventManager.AddListener<(int Health, int MaxHealth)>(Consts.Events.Player.PLAYER_HEALTH_LOST, OnHealthLost);
        }

        private void OnEnable()
        {
            panelRenderer.RegisterUIReloadCallback(OnUIReload);
            
            EventManager.AddListener<float>(Consts.Events.Player.PLAYER_MOVE_STARTED, OnMovementStarted);
            EventManager.AddListener<float>(Consts.Events.Player.PLAYER_TURN_STARTED, OnTurnStarted);
        }

        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
            barAnimation.StopMovement();

            EventManager.RemoveListener<float>(Consts.Events.Player.PLAYER_MOVE_STARTED, OnMovementStarted);
            EventManager.RemoveListener<float>(Consts.Events.Player.PLAYER_TURN_STARTED, OnTurnStarted);
            
            ReleaseBinding();
        }

        private void OnDestroy()
        {
            EventManager.RemoveListener<(int Health, int MaxHealth)>(Consts.Events.Player.PLAYER_HEALTH_LOST, OnHealthLost);
        }

        private void Update()
        {
            if (runtimeMaterial == null || !barAnimation.IsPlaying)
                return;

            ApplyAnimation(Time.timeAsDouble, Time.deltaTime);
        }

        private void OnHealthLost((int Health, int MaxHealth) health)
        {
            var now = Time.timeAsDouble;
            var healthChanged = barAnimation.LoseHealth(health.Health, health.MaxHealth, now);

            if (healthChanged && runtimeMaterial != null)
                ApplyAnimation(now);
        }

        private void OnMovementStarted(float duration) => barAnimation.Step(duration, Time.timeAsDouble);

        private void OnTurnStarted(float duration) => barAnimation.Turn(duration, Time.timeAsDouble);

        private void OnUIReload(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            ReleaseBinding();

            beerFill = rootElement.Q<VisualElement>(className: "health-bar__fill");

            if (beerFill == null)
                return;

            beerFill.RegisterCallback<GeometryChangedEvent>(OnBeerGeometryChanged);
            ApplyUvScale(beerFill.layout.size);
        }

        private void OnBeerGeometryChanged(GeometryChangedEvent evt) => ApplyUvScale(evt.newRect.size);

        private void ApplyUvScale(Vector2 size)
        {
            if (!(size.x > 0f && size.y > 0f))
                return;

            if (runtimeMaterial == null)
            {
                var sourceMaterial = beerFill.resolvedStyle.unityMaterial.material;

                if (sourceMaterial == null)
                    return;

                runtimeMaterial = new Material(sourceMaterial);
                barAnimation.IdleAgitation = sourceMaterial.GetFloat(AgitationId);
                
                ApplyAnimation(Time.timeAsDouble);
                
                beerFill.style.unityMaterial = new StyleMaterialDefinition(runtimeMaterial);
            }

            var verticalTiling = HorizontalTiling * size.y / size.x;
            runtimeMaterial.SetVector(UvScaleId, new Vector4(HorizontalTiling, verticalTiling, 0f, 0f));
        }

        private void ApplyAnimation(double now, float deltaTime = 0f)
        {
            var (fill, fillOffset, agitation, bubbleOffset) = barAnimation.Evaluate(now, deltaTime);
            
            runtimeMaterial.SetFloat(FillId, fill);
            runtimeMaterial.SetFloat(FillBobId, fillOffset);
            runtimeMaterial.SetFloat(AgitationId, agitation);
            runtimeMaterial.SetFloat(BubbleTimeOffsetId, bubbleOffset);
        }

        private void ReleaseBinding()
        {
            if (beerFill != null)
                beerFill.UnregisterCallback<GeometryChangedEvent>(OnBeerGeometryChanged);

            if (runtimeMaterial != null)
            {
                beerFill.style.unityMaterial = new StyleMaterialDefinition(StyleKeyword.Null);
                Destroy(runtimeMaterial);
                runtimeMaterial = null;
            }

            beerFill = null;
        }
    }
}
