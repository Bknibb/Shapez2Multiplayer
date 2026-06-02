using Core.Dependency;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace Shapez2Multiplayer
{
    public class HUDCursor : HUDComponent
    {
        public IConnection Connection { get; set; }
        public Image CursorImage { get; set; }
        public RectTransform RectTransform { get; set; }
        public Camera uiCamera { get; set; }
        public RectTransform uiRectTransform { get; set; }
        public CursorHoverState? CurrentState { get; set; }
        public float3 WorldPosition { get; private set; }
        private float3 LatestWorldPosition { get; set; }
        private float3 LastWorldPosition { get; set; }
        public short? ViewportIslandLayer { get; set; }
        public short? ViewportBuildingLayer { get; set; }
        public bool? ViewportShowAllBuildingLayers { get; set; }
        public bool? ViewportShowAllIslandLayers { get; set; }
        public PlayerInteractionState? PlayerInteractionState { get; set; }
        private float elapsed = 0f;
        const float LerpDuration = 0.1f;
        protected override bool UpdateWhileHidden => true;
        [Construct]
        private void Construct()
        {
            
        }
        protected override void OnUpdate(InputDownstreamContext context)
        {
            if (Shapez2Multiplayer.GameSessionOrchestrator == null) return;
            if (elapsed < LerpDuration) elapsed += Time.deltaTime;
            float t = math.clamp(elapsed / LerpDuration, 0, 1);
            WorldPosition = math.lerp(LastWorldPosition, LatestWorldPosition, t);
            var screenPosition = (float2)ExtraScreenUtils.WorldToScreenPointDouble(Shapez2Multiplayer.GameSessionOrchestrator.Viewport, WorldPosition);
            if (screenPosition.x < 0 || screenPosition.y < 0 || screenPosition.x > Screen.width || screenPosition.y > Screen.height)
            {
                gameObject.SetActiveSelfExt(false);
                return;
            }
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiRectTransform,
                screenPosition,
                uiCamera,
                out var localPoint
            )) return;
            gameObject.SetActiveSelfExt(true);
            RectTransform.anchoredPosition = localPoint;
        }
        public void SetWorldPosition(float3 worldPosition)
        {
            LastWorldPosition = LatestWorldPosition;
            LatestWorldPosition = worldPosition;
            elapsed = 0f;
        }
        //public void SetFromWorldPosition(float3 worldPosition)
        //{
        //    if (Shapez2Multiplayer.GameSessionOrchestrator == null) return;
        //    SetFromScreenPosition((float2)ExtraScreenUtils.WorldToScreenPointDouble(Shapez2Multiplayer.GameSessionOrchestrator.Viewport, worldPosition));
        //}
        //public void SetFromScreenPosition(float2 screenPosition)
        //{
        //    if (screenPosition.x < 0 || screenPosition.y < 0 || screenPosition.x > Screen.width || screenPosition.y > Screen.height)
        //    {
        //        gameObject.SetActiveSelfExt(false);
        //        return;
        //    }
        //    if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //        uiRectTransform,
        //        screenPosition,
        //        uiCamera,
        //        out var localPoint
        //    )) SetFromUIPosition(localPoint);
        //}
        //public void SetFromUIPosition(Vector2 uiPosition)
        //{
        //    gameObject.SetActiveSelfExt(true);
        //    RectTransform.anchoredPosition = uiPosition;
        //}
        public void UpdateImage(Texture2D texture, Vector2 hotspot)
        {
            CursorImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            RectTransform.pivot = new Vector2(hotspot.x / texture.width, 1 - (hotspot.y / texture.height));
            CurrentState = null;
        }
        public void UpdateImageFromCursorConfig(GameCursorResources.CursorConfig cursorConfig)
        {
            UpdateImage(cursorConfig.CursorTexture, cursorConfig.CursorHotspot);
        }
        public void UpdateImageFromState(CursorHoverState state)
        {
            if (state == CurrentState) return;
            switch (state)
            {
                case CursorHoverState.Hover:
                    UpdateImageFromCursorConfig(Globals.Resources.CursorResources.CursorHover);
                    break;
                case CursorHoverState.HoverNonInteractable:
                    UpdateImageFromCursorConfig(Globals.Resources.CursorResources.CursorHoverNonInteractable);
                    break;
                case CursorHoverState.HoverTooltipOnly:
                    UpdateImageFromCursorConfig(Globals.Resources.CursorResources.CursorHoverTooltipOnly);
                    break;
                default:
                    UpdateImageFromCursorConfig(Globals.Resources.CursorResources.CursorNormal);
                    break;
            }
            CurrentState = state;
        }

        protected override void OnDispose()
        {

        }
    }
}
