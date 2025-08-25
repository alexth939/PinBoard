using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PinBoard.Editor.VisualElements
{
    /// <summary>
    /// Transparent overlay that draws and positions the insertion indicator.
    /// </summary>
    internal sealed class InsertionIndicatorVisualizingPanel : VisualElement
    {
        private readonly VisualElement _indicator;

        public int InsertionIndex { get; private set; } = -1;

        public InsertionIndicatorVisualizingPanel()
        {
            pickingMode = PickingMode.Ignore; // clicks pass through
            style.position = Position.Absolute;
            style.left = 0;
            style.right = 0;
            style.top = 0;
            style.bottom = 0;

            _indicator = new VisualElement
            {
                style =
                {
                    height = 4,
                    backgroundColor = Color.blue,
                    position = Position.Absolute,
                    left = 0,
                    right = 0,
                    display = DisplayStyle.None
                }
            };

            Add(_indicator);
        }

        public void ShowAt(float y, int insertionIndex)
        {
            InsertionIndex = insertionIndex;
            _indicator.style.top = y;
            _indicator.style.display = DisplayStyle.Flex;
        }

        public void Hide()
        {
            InsertionIndex = -1;
            _indicator.style.display = DisplayStyle.None;
        }
    }

    /// <summary>
    /// A reorderable list container with a ScrollView and an overlay panel for the insertion indicator.
    /// </summary>
    public sealed class ReorderableScrollView : VisualElement
    {
        private readonly ScrollView _scrollView;
        private readonly InsertionIndicatorVisualizingPanel _indicatorPanel;

        private VisualElement _draggedItem;
        private VisualElement Content => _scrollView.contentContainer;

        /// <summary>
        /// Called when an element is reordered (oldIndex, newIndex).
        /// </summary>
        public Action<int, int> OnReordered;

        public ReorderableScrollView(ScrollViewMode mode = ScrollViewMode.Vertical)
        {
            style.flexGrow = 1;
            style.flexShrink = 1;

            // Base layout: container with scrollview + overlay panel
            _scrollView = new ScrollView(mode)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1
                }
            };

            base.Add(_scrollView);

            _indicatorPanel = new InsertionIndicatorVisualizingPanel();

            base.Add(_indicatorPanel);

            this.RegisterCallback<MouseDownEvent>(OnMouseDown, TrickleDown.TrickleDown);
        }

        private void OnMouseDown(MouseDownEvent evt)
        {
            if(evt.button != (int)MouseButton.RightMouse)
                return;

            var clicked = evt.target as VisualElement;
            if(clicked == null || clicked == _indicatorPanel)
                return;

            // Walk up until we find a direct child of Content
            VisualElement associatedChildOfMyContainer = clicked;
            while(associatedChildOfMyContainer != null && associatedChildOfMyContainer.parent != Content)
            {
                associatedChildOfMyContainer = associatedChildOfMyContainer.parent;
            }

            if(associatedChildOfMyContainer == null)
                return; // Click was outside our Content items

            _draggedItem = associatedChildOfMyContainer;

            this.CaptureMouse();
            RegisterCallback<MouseMoveEvent>(OnMouseMove);
            RegisterCallback<MouseUpEvent>(OnMouseUp);

            UpdateInsertionIndicator(evt.mousePosition);
            evt.StopPropagation();
        }

        private void OnMouseMove(MouseMoveEvent evt)
        {
            UpdateInsertionIndicator(evt.mousePosition);
        }

        private void OnMouseUp(MouseUpEvent evt)
        {
            if(_draggedItem == null)
                return;

            int oldIndex = Content.IndexOf(_draggedItem);
            int newIndex = _indicatorPanel.InsertionIndex;

            if(newIndex > oldIndex)
                newIndex--;

            // Clamp to valid range
            newIndex = Mathf.Clamp(newIndex, 0, Content.childCount - 1);

            if(newIndex >= Content.childCount)
                Content.Add(_draggedItem);
            else
                Content.Insert(newIndex, _draggedItem);

            _indicatorPanel.Hide();
            OnReordered?.Invoke(oldIndex, newIndex);

            _draggedItem = null;

            UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            UnregisterCallback<MouseUpEvent>(OnMouseUp);
            this.ReleaseMouse();
        }

        private void UpdateInsertionIndicator(Vector2 globalMousePos)
        {
            if(_draggedItem == null)
                return;

            Vector2 localMouse = Content.WorldToLocal(globalMousePos);

            int insertIndex = 0;

            for(int i = 0; i < Content.childCount; i++)
            {
                var child = Content[i];
                float childMid = child.layout.y + child.layout.height / 2f;

                if(localMouse.y < childMid)
                {
                    insertIndex = i;
                    break;
                }

                // if mouse is after the last element's center → end
                if(i == Content.childCount - 1)
                    insertIndex = Content.childCount;
            }

            float indicatorY;
            if(insertIndex == Content.childCount)
                indicatorY = Content.layout.height - 2;
            else
                indicatorY = Content[insertIndex].layout.y - 2;

            _indicatorPanel.ShowAt(indicatorY, insertIndex);
        }

        public new void Add(VisualElement child)
        {
            throw new Exception();
        }

        // For external code to add children
        public void AddItem(VisualElement item) => Content.Add(item);
    }
}
