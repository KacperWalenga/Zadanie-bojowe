
using UnityEngine;
using UnityEngine.UIElements;

namespace ZB
{
    [RequireComponent(typeof(PanelRenderer))]
    public class GameViewport : MonoBehaviour
    {
        private PanelRenderer panelRenderer;
        private Camera camera;
        private Image view;

        private RenderTexture originalCameraTarget;
        private RenderTexture viewportTexture;

        private void Awake()
        {
            panelRenderer = GetComponent<PanelRenderer>();
        }

        private void OnEnable()
        {
            camera = Camera.main;
            originalCameraTarget = camera.targetTexture;

            panelRenderer.RegisterUIReloadCallback(OnUIReload);
        }

        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);

            if (camera != null)
            {
                camera.targetTexture = originalCameraTarget;
                camera.ResetAspect();
            }
  
            viewportTexture.Release();
            Destroy(viewportTexture);
            viewportTexture = null;
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            view = rootElement.Q<Image>("game-view");

            viewportTexture ??= new RenderTexture(originalCameraTarget) { name = "GameView" };
            view.image = viewportTexture;
            view.scaleMode = ScaleMode.StretchToFill;
        }

        private void LateUpdate()
        {
            if (view?.panel == null)
                return;

            var targetSize = view.contentRect.size;
            var original = new Vector2Int(originalCameraTarget.width, originalCameraTarget.height);
            var dimensions = GetRenderDimensions(targetSize, view.scaledPixelsPerPoint, original);

            var hasCorrectSize = viewportTexture.width == dimensions.x && viewportTexture.height == dimensions.y;

            if (!hasCorrectSize)
            {
                viewportTexture.Release();
                viewportTexture.width = dimensions.x;
                viewportTexture.height = dimensions.y;

                camera.targetTexture = viewportTexture;
                camera.aspect = (float)dimensions.x / dimensions.y;
            }
        }

        private static Vector2Int GetRenderDimensions(Vector2 targetSize, float pixelsPerPoint, Vector2Int original)
        {
            var scale = Mathf.Min(pixelsPerPoint, original.x / targetSize.x, original.y / targetSize.y);

            return new Vector2Int(
                Mathf.Clamp(Mathf.RoundToInt(targetSize.x * scale), 1, original.x),
                Mathf.Clamp(Mathf.RoundToInt(targetSize.y * scale), 1, original.y)
            );
        }
    }
}
