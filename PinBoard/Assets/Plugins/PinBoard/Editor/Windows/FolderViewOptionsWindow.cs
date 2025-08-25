using PinBoard.Editor.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;

namespace PinBoard.Editor.Windows
{
    internal sealed class FolderViewOptionsWindow : EditorWindow
    {
        private PinFolderView _folderView;
        private TextField _nameField;
        private ColorField _colorField;

        private string _originalFolderName;
        private Color _originalFolderColor;

        public static void Open(PinFolderView folderView)
        {
            var window = CreateInstance<FolderViewOptionsWindow>();
            window.titleContent = new GUIContent("Folder View Options");
            window.minSize = new Vector2(250, 80);
            window._folderView = folderView;
            window._originalFolderName = new string(folderView.Folder.Name);
            window._originalFolderColor = folderView.Folder.Color;
            window.ShowUtility();
            //window.ShowAuxWindow();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            root.style.paddingLeft = 8;
            root.style.paddingRight = 8;
            root.style.paddingTop = 8;
            root.style.paddingBottom = 8;

            _nameField = new TextField("Name");
            _colorField = new ColorField("Color:");
            var saveBtn = new Button(OnApplySettingsClicked) { text = "Apply" };
            root.Add(_nameField);
            root.Add(_colorField);
            root.Add(saveBtn);
        }

        private bool _shouldApplySettings = false;

        private void OnApplySettingsClicked()
        {
            _shouldApplySettings = true;
            Close();
        }

        private void OnDisable()
        {
            _nameField?.UnregisterValueChangedCallback(OnFolderNameChanged);

            if(_shouldApplySettings)
                _folderView.ForceFolderSettingsUpdateNotification();
            else
            {
                _folderView.UpdateFolderColorWithoutNotify(_originalFolderColor);
                _folderView.UpdateFolderNameWithoutNotify(_originalFolderName);
            }
        }

        private void OnEnable() => EditorApplication.delayCall += SetupValues;

        private void SetupValues()
        {
            _nameField.value = _folderView.Folder.Name;
            _colorField.value = _folderView.Folder.Color;
            _nameField.RegisterValueChangedCallback(OnFolderNameChanged);
            _colorField.RegisterValueChangedCallback(OnFolderColorChanged);
        }

        private void OnFolderColorChanged(ChangeEvent<Color> @event) => _folderView.UpdateFolderColorWithoutNotify(@event.newValue);
        private void OnFolderNameChanged(ChangeEvent<string> @event) => _folderView.UpdateFolderNameWithoutNotify(@event.newValue);
    }
}
