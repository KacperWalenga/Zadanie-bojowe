
using System;
using System.Collections;
using LitMotion;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace ZB
{
    [RequireComponent(typeof(PanelRenderer))]
    public class Book : MonoBehaviour
    {
        [SerializeField] private Texture2D[] pages;
        [SerializeField] private InputActionReference nextPage;
        [SerializeField] private InputActionReference prevPage;

        private PanelRenderer panelRenderer;
        private int page = 0;

        private Image pageImage;
        private Button prevButton;
        private Button nextButton;

        private void Awake()
        {
            panelRenderer = GetComponent<PanelRenderer>();
        }

        private void OnEnable()
        {
            panelRenderer.RegisterUIReloadCallback(OnUIReload);

            nextPage.action.performed += LoadNextPage;
            prevPage.action.performed += LoadPrevPage;
        }

        private void OnDisable()
        {
            panelRenderer.UnregisterUIReloadCallback(OnUIReload);
            
            nextPage.action.performed -= LoadNextPage;
            prevPage.action.performed -= LoadPrevPage;
            
            if (prevButton != null)
                prevButton.clicked -= LoadPrevPage;
            
            if (nextButton != null)
                nextButton.clicked -= LoadNextPage;
        }

        private void LoadPage()
        {
            if (page > pages.Length && page < 0)
                return;

            if (pageImage == null)
                return;
            
            pageImage.image = pages[page];
        }

        private void OnUIReload(PanelRenderer renderer, VisualElement rootElement, int version)
        {
            pageImage = rootElement.Q<Image>("book-image");
            
            prevButton = rootElement.Q<Button>("book-btn-left"); 
            nextButton = rootElement.Q<Button>("book-btn-right");

            prevButton.clicked += LoadPrevPage;
            nextButton.clicked += LoadNextPage;

            LoadPage();
        }

        private void LoadNextPage(InputAction.CallbackContext _)
        {
            LoadNextPage();
            StartCoroutine(AnimateButton(nextButton));
        }

        private void LoadPrevPage(InputAction.CallbackContext _)
        {
            LoadPrevPage();
            StartCoroutine(AnimateButton(prevButton));
        }

        private void LoadNextPage()
        {
            page += 1;

            if (page >= pages.Length)
                page = 0;
            
            LoadPage();
        }
        
        private void LoadPrevPage()
        {
            page -= 1;

            if (page < 0)
                page = pages.Length - 1;

            LoadPage(); 
        }

        private IEnumerator AnimateButton(Button btn)
        {
            btn.style.scale = new StyleScale(new Vector2(1.02f, 1.02f));
            yield return new WaitForSeconds(0.100f);
            btn.style.scale = new StyleScale(new Vector2(0.9f, 0.9f));
            yield return new WaitForSeconds(0.200f);
            btn.style.scale = new StyleScale(StyleKeyword.Null);
        }
    }
}
