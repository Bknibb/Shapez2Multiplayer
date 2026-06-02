using Core.Dependency;
using Shapez2UILib;
using System.Collections.Generic;
using UnityEngine;

namespace Shapez2Multiplayer
{
    public class HUDMultiplayerCursors : HUDPart
    {
        public static HUDMultiplayerCursors Instance { get; private set; }
        public readonly List<HUDCursor> Cursors = new List<HUDCursor>();
        public HUDCursor? HostCursor = null;
        public RectTransform RectTransform { get; private set; }
        public Canvas Canvas { get; private set; }
        public override bool NeedsGraphicsRaycaster => false;
        [Construct]
        private void Construct()
        {
            Instance = this;
            RectTransform = GetComponent<RectTransform>();
            Canvas = GetComponent<Canvas>();
        }
        public void AddCursor(IConnection connection)
        {
            GameObject CursorObject = new GameObject(connection.Name);
            HUDCursor cursorEntry = CursorObject.AddComponent<HUDCursor>();
            cursorEntry.Connection = connection;
            CursorObject.transform.SetParent(transform);
            CursorObject.transform.localScale = Vector3.one;
            CursorObject.layer = LayerMask.NameToLayer("UI");
            RectTransform cursorRectTransform = CursorObject.AddComponent<RectTransform>();
            cursorRectTransform.sizeDelta = new Vector2(25, 25);
            UnityEngine.UI.Image cursorImage = CursorObject.AddComponent<UnityEngine.UI.Image>();
            cursorEntry.CursorImage = cursorImage;
            cursorEntry.uiRectTransform = RectTransform;
            cursorEntry.uiCamera = Canvas.worldCamera;
            cursorEntry.RectTransform = cursorRectTransform;
            this.GetDependencyResolver().Inject(cursorEntry);
            Cursors.Add(cursorEntry);
        }
        public void AddHostCursor()
        {
            GameObject CursorObject = new GameObject("Host");
            HUDCursor cursorEntry = CursorObject.AddComponent<HUDCursor>();
            CursorObject.transform.SetParent(transform);
            CursorObject.transform.localScale = Vector3.one;
            CursorObject.layer = LayerMask.NameToLayer("UI");
            RectTransform cursorRectTransform = CursorObject.AddComponent<RectTransform>();
            cursorRectTransform.sizeDelta = new Vector2(25, 25);
            UnityEngine.UI.Image cursorImage = CursorObject.AddComponent<UnityEngine.UI.Image>();
            cursorEntry.CursorImage = cursorImage;
            cursorEntry.uiRectTransform = RectTransform;
            cursorEntry.uiCamera = Canvas.worldCamera;
            cursorEntry.RectTransform = cursorRectTransform;
            this.GetDependencyResolver().Inject(cursorEntry);
            HostCursor = cursorEntry;
        }
        public bool TryGetCursor(IConnection connection, out HUDCursor cursor)
        {
            foreach (var c in Cursors)
            {
                if (c.Connection.Equals(connection))
                {
                    cursor = c;
                    return true;
                }
            }
            cursor = null;
            return false;
        }
        public HUDCursor GetOrAddCursor(IConnection connection)
        {
            if (TryGetCursor(connection, out var cursor)) return cursor;
            AddCursor(connection);
            return Cursors[^1];
        }
        public HUDCursor GetOrAddHostCursor()
        {
            if (HostCursor == null) AddHostCursor();
            return HostCursor;
        }
        public void RemoveCursor(IConnection connection)
        {
            foreach (var cursor in Cursors)
            {
                if (cursor.Connection.Equals(connection))
                {
                    cursor.Dispose();
                    GameObject.Destroy(cursor.gameObject);
                    Cursors.Remove(cursor);
                    return;
                }
            }
        }
        protected override void OnUpdate(InputDownstreamContext context)
        {
            HostCursor?.DoUpdate(context);
            foreach (var cursor in Cursors) cursor.DoUpdate(context);
        }
        protected override void OnDispose()
        {
            Instance = null;
        }
    }
}
