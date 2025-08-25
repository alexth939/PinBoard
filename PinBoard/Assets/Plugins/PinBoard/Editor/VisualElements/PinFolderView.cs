using System;
using System.Collections.Generic;
using PinBoard.Editor.Windows;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PinBoard.Editor.VisualElements
{
    internal sealed class PinFolderView : VisualElement
    {
        private readonly List<PinReference> _pins;
        private readonly PinListView _listView;
        private readonly VisualElement _header;
        private readonly Label _titleLabel;
        private bool _isCollapsed;
        private readonly PinFolder _folder;

        internal PinFolder Folder => _folder;

        public event Action<PinFolder> PinsModified;
        public event Action<PinFolder> FolderSettingsUpdated;

        private void OnPinsChanged()
        {
            PinsModified?.Invoke(Folder);
        }

        public event Action<PinFolder> RemovedFromHierarchy;

        public PinFolderView(PinFolder folder)
        {
            _folder = folder;
            _pins = folder.Pins;

            style.flexDirection = FlexDirection.Column;
            style.marginBottom = 4;

            // === Header ===
            _header = new VisualElement
            {
                style =
                {
                    flexDirection = FlexDirection.Row,
                    alignItems = Align.Center,
                    backgroundColor = _folder.Color,
                    paddingLeft = 4,
                    paddingRight = 4,
                    paddingTop = 2,
                    paddingBottom = 2,
                    flexShrink = 0f,

                }
            };

            _header.RegisterCallback<MouseDownEvent>(evt =>
            {
                if(evt.button is (int)MouseButton.MiddleMouse)
                {
                    this.RemoveFromHierarchy();
                    RemovedFromHierarchy?.Invoke(_folder);
                    evt.StopPropagation();
                }
            });

            _titleLabel = new Label(folder.Name)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    flexGrow = 1f,
                    flexShrink = 0f,
                }
            };
            _header.Add(_titleLabel);

            _header.RegisterCallback<MouseDownEvent>(evt =>
            {
                if(evt.button == (int)MouseButton.LeftMouse)
                {
                    ToggleFold();
                    evt.StopPropagation();
                }
            });

            var optionsBtn = new Button(() => FolderViewOptionsWindow.Open(this))
            {
                style =
                {
                    width = 20,
                    height = 20,
                    //marginLeft = 50,
                    //marginRight = 50,
                    paddingLeft = 0,
                    paddingRight = 20,
                    paddingTop = 0,
                    paddingBottom = 0
                }
            };

            var optionsImage = new Image
            {
                image = EditorGUIUtility.IconContent("d_TerrainInspector.TerrainToolSettings On").image
            };
            optionsImage.style.width = 20;
            optionsImage.style.height = 20;

            optionsBtn.Add(optionsImage);

            _header.Add(optionsBtn);

            Add(_header);

            // === ListView ===
            _listView = new PinListView(_pins)
            {
                itemsSource = _pins,
                showAlternatingRowBackgrounds = AlternatingRowBackground.ContentOnly,
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight,
                reorderMode = ListViewReorderMode.Animated, // feels nice
                showBorder = true,
                reorderable = true,
                showBoundCollectionSize = false,
                style =
                {
                flexGrow = 1,
                    marginLeft = 10,
                    flexShrink = 0,
                },
            };

            _listView.Modified += OnPinsChanged;

            Add(_listView);
        }

        internal void UpdateFolderColorWithoutNotify(Color newValue)
        {
            _folder.Color = newValue;
            _header.style.backgroundColor = newValue;
        }

        public void UpdateFolderNameWithoutNotify(string newValue)
        {
            _folder.Name = newValue;
            _titleLabel.text = newValue;
        }

        public void ForceFolderSettingsUpdateNotification() => FolderSettingsUpdated?.Invoke(_folder);

        private void ToggleFold()
        {
            _isCollapsed = !_isCollapsed;
            _listView.style.display = _isCollapsed ? DisplayStyle.None : DisplayStyle.Flex;
        }

        public void RefreshPins()
        {
            _listView.Rebuild();
        }
    }
}
