using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ZB
{
    [RequireComponent(typeof(UIDocument))]
    public class MainMenu : MonoBehaviour
    {
        private static readonly int AgitationId = Shader.PropertyToID("_Agitation");
        private static readonly int BubbleTimeOffsetId = Shader.PropertyToID("_BubbleTimeOffset");
        private static readonly int UvScaleId = Shader.PropertyToID("_UVScale");

        [SerializeField] private AudioMixer mixer;

        private VisualElement actions;
        private VisualElement options;
        private Button optionsButton;
        private SliderInt musicSlider;
        private VisualElement credits;
        private Button creditsButton;
        private Button closeCreditsButton;

        private VisualElement beerSurface;
        private Material beerMaterial;
        private float idleAgitation;
        private float agitation;

        private void Start()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            
            actions = root.Q("menu-actions");
            options = root.Q("audio-options");
            optionsButton = root.Q<Button>("open-options");
            musicSlider = root.Q<SliderInt>("music-volume");
            credits = root.Q("credits");
            creditsButton = root.Q<Button>("open-credits");
            closeCreditsButton = root.Q<Button>("close-credits");

            optionsButton.clicked += OpenOptions;
            root.Q<Button>("new-game").clicked += StartGame;
            root.Q<Button>("quit-game").clicked += () => Application.Quit();
            root.Q<Button>("close-options").clicked += CloseOptions;
            creditsButton.clicked += OpenCredits;
            closeCreditsButton.clicked += CloseCredits;
            root.RegisterCallback<KeyDownEvent>(HandleKeyDown);

            BindVolume(musicSlider, root.Q<Label>("music-value"), "MusicVolume", -25f);
            BindVolume(root.Q<SliderInt>("sfx-volume"), root.Q<Label>("sfx-value"), "SfxVolume", -20f);

            beerSurface = root.Q(className: "main-menu__beer");
            beerSurface.RegisterCallback<GeometryChangedEvent>(OnBeerGeometryChanged);
            
            ResizeBeer(beerSurface.layout.size);
        }

        private void Update()
        {
            if (beerMaterial == null)
                return;

            const float MaxAgitation = 0.95f;
            const float FullAgitationSpeed = 0.8f;
            const float WaveRise = 18f;
            const float WaveDecay = 7f;
            const float BubbleSpeed = 0.35f;

            var delta = Time.unscaledDeltaTime;
            var mouseDelta = Mouse.current?.delta.ReadValue() ?? Vector2.zero;
            var speed = mouseDelta.magnitude / (Screen.height * Mathf.Max(delta, 0.001f));
            var target = Mathf.Lerp(idleAgitation, MaxAgitation, speed / FullAgitationSpeed);

            var response = target > agitation ? WaveRise : WaveDecay;
            
            agitation = Mathf.Lerp(agitation, target, 1f - Mathf.Exp(-response * delta));
            
            beerMaterial.SetFloat(AgitationId, agitation);
            beerMaterial.SetFloat(BubbleTimeOffsetId, Time.time * (BubbleSpeed - 1f));
        }

        private void OnDestroy()
        {
            beerSurface?.UnregisterCallback<GeometryChangedEvent>(OnBeerGeometryChanged);

            if (beerMaterial != null)
                Destroy(beerMaterial);
        }

        private void OnBeerGeometryChanged(GeometryChangedEvent evt) => ResizeBeer(evt.newRect.size);

        private void ResizeBeer(Vector2 size)
        {
            if (!(size.x > 0f && size.y > 0f))
                return;

            if (beerMaterial == null)
            {
                var source = beerSurface.resolvedStyle.unityMaterial.material;

                if (source == null)
                    return;

                beerMaterial = new Material(source);
                idleAgitation = beerMaterial.GetFloat(AgitationId);
                agitation = idleAgitation;
                beerSurface.style.unityMaterial = new StyleMaterialDefinition(beerMaterial);
            }

            var scale = beerMaterial.GetVector(UvScaleId);
            
            scale.x = scale.y * size.x / size.y;
            beerMaterial.SetVector(UvScaleId, scale);
        }

        private void StartGame()
        {
            actions.SetEnabled(false);
            SceneManager.LoadScene("Game");
        }

        private void OpenOptions()
        {
            actions.SetEnabled(false);
            options.RemoveFromClassList("main-menu__overlay--hidden");
            musicSlider.Focus();
        }

        private void CloseOptions()
        {
            options.AddToClassList("main-menu__overlay--hidden");
            actions.SetEnabled(true);
            optionsButton.Focus();
            PlayerPrefs.Save();
        }

        private void OpenCredits()
        {
            actions.SetEnabled(false);
            credits.RemoveFromClassList("main-menu__overlay--hidden");
            closeCreditsButton.Focus();
        }

        private void CloseCredits()
        {
            credits.AddToClassList("main-menu__overlay--hidden");
            actions.SetEnabled(true);
            creditsButton.Focus();
        }

        private void HandleKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != KeyCode.Escape)
                return;

            if (!options.ClassListContains("main-menu__overlay--hidden"))
                CloseOptions();
            else if (!credits.ClassListContains("main-menu__overlay--hidden"))
                CloseCredits();
            else
                return;

            evt.StopPropagation();
        }

        private void BindVolume(SliderInt slider, Label label, string parameter, float baseDecibels)
        {
            var value = Mathf.Clamp(PlayerPrefs.GetInt(parameter, slider.highValue), slider.lowValue, slider.highValue);
            
            slider.SetValueWithoutNotify(value);
            ApplyVolume(label, parameter, baseDecibels, value);

            slider.RegisterValueChangedCallback(evt =>
            {
                ApplyVolume(label, parameter, baseDecibels, evt.newValue);
                PlayerPrefs.SetInt(parameter, evt.newValue);
            });
        }

        private void ApplyVolume(Label label, string parameter, float baseDecibels, int percentage)
        {
            label.text = $"{percentage}%";
            
            var decibels = percentage == 0 ? -80f : baseDecibels + 20f * Mathf.Log10(percentage / 100f);
            
            mixer.SetFloat(parameter, decibels);
        }
    }
}
