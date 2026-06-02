using Core.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Shapez2Multiplayer
{
    public abstract class OtherPlayerHUDMassSelectionBase<TSelectable, TCoordinate> where TSelectable : struct, IEquatable<TSelectable> where TCoordinate : struct
    {
        public ISelection<TSelectable> Selection = new Selection<TSelectable>();
        public IConnection? connection;
        public void Update(TCoordinate? areaSelectionEnd_G, TCoordinate? areaSelectionStart_G, HUDMassSelectionMode currentMode, HashSet<TSelectable> pendingSelection, List<OtherPlayerHUDMassSelectionBase<TSelectable, TCoordinate>.HoverAnimation> hoverAnimations)
        {
            AreaSelectionEnd_G = areaSelectionEnd_G;
            AreaSelectionStart_G = areaSelectionStart_G;
            CurrentMode = currentMode;
            PendingSelection = pendingSelection;
            HoverAnimations = hoverAnimations;
        }
        public void OnGameUpdate(InputDownstreamContext context, FrameDrawOptions drawOptions)
        {
            var cursor = connection != null ? HUDMultiplayerCursors.Instance.GetOrAddCursor(connection) : HUDMultiplayerCursors.Instance.GetOrAddHostCursor();
            if (cursor.PlayerInteractionState.HasValue && cursor.PlayerInteractionState.Value != GetTargetScopeState())
            {
                this.DrawAndUpdateHoverAnimations(drawOptions);
                this.CurrentMode = HUDMassSelectionMode.None;
                if (this.PendingSelection.Count > 0)
                {
                    this.PendingSelection.Clear();
                }
                this.Draw_ExistingSelection(drawOptions, this.Selection);
                return;
            }
            TSelectable tselectable = this.FindEntityBelowCursor();
            HUDMassSelectionMode currentMode = this.CurrentMode;
            switch (this.CurrentMode)
            {
                case HUDMassSelectionMode.None:
                    this.UpdateMode_None(context, drawOptions, tselectable);
                    break;
                case HUDMassSelectionMode.SingleUndecided:
                case HUDMassSelectionMode.SingleAdditive:
                case HUDMassSelectionMode.SingleSubtractive:
                    this.UpdateMode_Single(context, drawOptions);
                    break;
                case HUDMassSelectionMode.AreaAdditive:
                case HUDMassSelectionMode.AreaSubtractive:
                case HUDMassSelectionMode.AreaDelete:
                    this.UpdateMode_Area(context, drawOptions);
                    break;
                case HUDMassSelectionMode.QuickDelete:
                    this.UpdateMode_QuickDelete(context, drawOptions);
                    break;
            }
            this.Draw_ExistingSelection(drawOptions, Selection);
            this.DrawAndUpdateHoverAnimations(drawOptions);
        }
        protected void UpdateMode_None(InputDownstreamContext context, FrameDrawOptions drawOptions, TSelectable selectableBelowCursor)
        {
            TSelectable tselectable = default;
            if (!tselectable.Equals(selectableBelowCursor))
            {
                this.SpawnHoverAnimation(drawOptions, selectableBelowCursor);
            }
        }
        protected void SpawnHoverAnimation(FrameDrawOptions options, TSelectable target)
        {
            if (this.HoverAnimations.Count > 100)
            {
                return;
            }
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            for (int i = 0; i < this.HoverAnimations.Count; i++)
            {
                OtherPlayerHUDMassSelectionBase<TSelectable, TCoordinate>.HoverAnimation hoverAnimation = this.HoverAnimations[i];
                if (hoverAnimation.Target.Equals(target))
                {
                    this.HoverAnimations[i] = new OtherPlayerHUDMassSelectionBase<TSelectable, TCoordinate>.HoverAnimation
                    {
                        Target = hoverAnimation.Target,
                        LastHoverTime = realtimeSinceStartup,
                        InitialHoverTime = hoverAnimation.InitialHoverTime
                    };
                    return;
                }
            }
            this.OnHover(target);
            this.HoverAnimations.Add(new OtherPlayerHUDMassSelectionBase<TSelectable, TCoordinate>.HoverAnimation
            {
                Target = target,
                LastHoverTime = realtimeSinceStartup,
                InitialHoverTime = realtimeSinceStartup
            });
        }
        protected abstract TSelectable FindEntityBelowCursor();
        protected abstract void OnHover(TSelectable target);
        protected abstract PlayerInteractionState GetTargetScopeState();
        protected void UpdateMode_Single(InputDownstreamContext context, FrameDrawOptions drawOptions)
        {
            HUDMassSelectionSelectionType hudmassSelectionSelectionType;
            if (CurrentMode != HUDMassSelectionMode.SingleAdditive)
            {
                if (CurrentMode != HUDMassSelectionMode.SingleSubtractive)
                {
                    hudmassSelectionSelectionType = HUDMassSelectionSelectionType.Select;
                }
                else
                {
                    hudmassSelectionSelectionType = HUDMassSelectionSelectionType.Deselect;
                }
            }
            else
            {
                hudmassSelectionSelectionType = HUDMassSelectionSelectionType.Select;
            }
            this.Draw_PendingSelection(drawOptions, PendingSelection, hudmassSelectionSelectionType);
        }
        protected abstract void Draw_PendingSelection(FrameDrawOptions options, IReadOnlyCollection<TSelectable> entities, HUDMassSelectionSelectionType type);
        protected void UpdateMode_Area(InputDownstreamContext context, FrameDrawOptions drawOptions)
        {
            if (this.AreaSelectionStart_G == null || this.AreaSelectionEnd_G == null)
            {
                return;
            }
            HUDMassSelectionSelectionType hudmassSelectionSelectionType = HUDMassSelectionSelectionType.Select;
            switch (this.CurrentMode)
            {
                case HUDMassSelectionMode.AreaAdditive:
                    hudmassSelectionSelectionType = HUDMassSelectionSelectionType.Select;
                    break;
                case HUDMassSelectionMode.AreaSubtractive:
                    hudmassSelectionSelectionType = HUDMassSelectionSelectionType.Deselect;
                    break;
                case HUDMassSelectionMode.AreaDelete:
                    hudmassSelectionSelectionType = HUDMassSelectionSelectionType.Delete;
                    break;
            }
            this.Draw_PendingSelection(drawOptions, this.PendingSelection, hudmassSelectionSelectionType);
            this.Draw_AreaSelection(drawOptions, this.AreaSelectionStart_G.Value, this.AreaSelectionEnd_G.Value, hudmassSelectionSelectionType);
        }
        protected abstract void Draw_AreaSelection(FrameDrawOptions options, TCoordinate from, TCoordinate to, HUDMassSelectionSelectionType type);
        protected void UpdateMode_QuickDelete(InputDownstreamContext context, FrameDrawOptions drawOptions)
        {
            this.Draw_PendingSelection(drawOptions, this.PendingSelection, HUDMassSelectionSelectionType.Delete);
        }
        protected abstract void Draw_ExistingSelection(FrameDrawOptions options, IReadOnlyCollection<TSelectable> selection);
        private void DrawAndUpdateHoverAnimations(FrameDrawOptions drawOptions)
        {
            float realtimeSinceStartup = Time.realtimeSinceStartup;
            for (int i = this.HoverAnimations.Count - 1; i >= 0; i--)
            {
                OtherPlayerHUDMassSelectionBase<TSelectable, TCoordinate>.HoverAnimation hoverAnimation = this.HoverAnimations[i];
                if (realtimeSinceStartup - hoverAnimation.LastHoverTime > 0.15f)
                {
                    this.HoverAnimations.RemoveAt(i);
                }
                else
                {
                    float num = 1f;
                    num *= 1f - math.saturate((realtimeSinceStartup - hoverAnimation.LastHoverTime) / 0.15f);
                    num *= math.saturate((realtimeSinceStartup - hoverAnimation.InitialHoverTime) / 0.04f);
                    this.Draw_HoverState(drawOptions, hoverAnimation.Target, num);
                }
            }
        }
        protected abstract void Draw_HoverState(FrameDrawOptions options, TSelectable selection, float alpha);
        private TCoordinate? AreaSelectionEnd_G;
        private TCoordinate? AreaSelectionStart_G;
        private HUDMassSelectionMode CurrentMode;
        private List<OtherPlayerHUDMassSelectionBase<TSelectable, TCoordinate>.HoverAnimation> HoverAnimations = new List<OtherPlayerHUDMassSelectionBase<TSelectable, TCoordinate>.HoverAnimation>();
        private HashSet<TSelectable> PendingSelection = new HashSet<TSelectable>();
        public static List<OtherPlayerHUDMassSelectionBase<TSelectable, TCoordinate>.HoverAnimation> HoverAnimationsFromIList(IList list)
        {
            var hoverAnimations = new List<OtherPlayerHUDMassSelectionBase<TSelectable, TCoordinate>.HoverAnimation>();
            foreach (var anim in list)
            {
                hoverAnimations.Add(OtherPlayerHUDMassSelectionBase<TSelectable, TCoordinate>.HoverAnimation.FromOther(anim));
            }
            return hoverAnimations;
        }
        public struct HoverAnimation
        {
            public TSelectable Target;

            public float LastHoverTime;

            public float InitialHoverTime;
            public static HoverAnimation FromOther(object hoverAnimation)
            {
                var t = hoverAnimation.GetType();
                var anim = new HoverAnimation();
                anim.Target = (TSelectable)t.GetField("Target").GetValue(hoverAnimation);
                anim.LastHoverTime = (float)t.GetField("LastHoverTime").GetValue(hoverAnimation);
                anim.InitialHoverTime = (float)t.GetField("InitialHoverTime").GetValue(hoverAnimation);
                return anim;
            }
        }
    }
}
