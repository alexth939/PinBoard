using System.Collections.Generic;
using System.IO;
using PinBoard.Editor.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PinBoard.Editor
{
    internal class PinBoardWindow : EditorWindow
    {
        private readonly Dictionary<PinContext, VisualElement> _contextContent = new();

        private PinContext _currentContext = PinContext.Project;

        private PinBoardPage _pinBoardPage;

        private Button _folderCreationButton;
        private ReorderableScrollView _projectScrollView;

        private enum PinContext
        {
            Project,
            Scene,
        }

        [MenuItem("Window/PinBoard")]
        public static void ShowWindow()
        {
            var window = GetWindow<PinBoardWindow>();
            window.titleContent = new GUIContent("PinBoard");

            // scenario: pinboard is opened. closing editor. open editor.
            // pinboard window is restored, but not showing entries.
            //window.LoadPinsPage();
            //window.RefreshPins();
            // so doing it on creategui
        }

        public void CreateGUI()
        {
            // Root layout
            var root = rootVisualElement;
            root.style.paddingLeft = 10;
            root.style.paddingTop = 10;
            root.style.paddingRight = 10;
            root.style.paddingBottom = 10;

            // Toolbar
            var toolbar = new Toolbar();
            var projectButton = new ToolbarToggle { text = "Project" };
            var sceneButton = new ToolbarToggle { text = "Scene" };

            projectButton.RegisterValueChangedCallback(evt =>
            {
                if(evt.newValue)
                {
                    sceneButton.SetValueWithoutNotify(false);
                    ExitContext(_currentContext);
                    EnterContext(PinContext.Project);
                }
            });

            sceneButton.RegisterValueChangedCallback(evt =>
            {
                if(evt.newValue)
                {
                    projectButton.SetValueWithoutNotify(false);
                    ExitContext(_currentContext);
                    EnterContext(PinContext.Scene);
                }
            });

            ToolbarToggle currentToggle = _currentContext switch
            {
                PinContext.Project => projectButton,
                PinContext.Scene => sceneButton,
                _ => throw default,
            };

            currentToggle.value = true;

            toolbar.Add(projectButton);
            toolbar.Add(sceneButton);
            root.Add(toolbar);

            _folderCreationButton = new Button(CreateNewProjectPinsFolder) { text = "Create" };

            _pinBoardPage = PluginDataStorage.LoadOrCreatePinBoardPage();

            BuildAllContent();
            EnterContext(_currentContext);

            _contextContent[PinContext.Project].Add(_folderCreationButton);
        }

        private void BuildAllContent()
        {
            _projectScrollView = new ReorderableScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    flexDirection = FlexDirection.Column,
                },
            };

            var sceneScroll = new ScrollView(ScrollViewMode.Vertical)
            {
                style =
                {
                    flexGrow = 1,
                    flexShrink = 1,
                    flexDirection = FlexDirection.Column
                }
            };

            var projectContextContent = new VisualElement();
            //projectContextContent.style.flexShrink = 0f; <- this one was the problem!

            projectContextContent.Add(_projectScrollView);

            var sceneContextContent = new VisualElement();
            sceneContextContent.Add(sceneScroll);

            _contextContent.Add(PinContext.Project, projectContextContent);
            _contextContent.Add(PinContext.Scene, sceneContextContent);

            var scenePinsListView = new PinListView(_pinBoardPage.ScenePins);
            sceneScroll.Add(scenePinsListView);

            foreach(PinFolder folder in _pinBoardPage.ProjectFolders)
            {
                CreateProjectPinsFolder(folder);
            }
        }

        private void EnterContext(PinContext context)
        {
            rootVisualElement.Add(_contextContent[context]);
            _currentContext = context;
        }

        private void ExitContext(PinContext context)
        {
            rootVisualElement.Remove(_contextContent[context]);

        }

        private void CreateNewProjectPinsFolder()
        {
            Debug.Log($"creating new pins folder");

            var folderPins = new List<PinReference>();
            var folder = new PinFolder("Pin Folder", folderPins);
            _pinBoardPage.ProjectFolders.Add(folder);

            CreateProjectPinsFolder(folder);
            SavePins();
        }

        private void CreateProjectPinsFolder(PinFolder folder)
        {
            Debug.Log($"creating pins folder");
            var root = rootVisualElement;

            var folderView = new PinFolderView(folder);
            folderView.RefreshPins();

            folderView.RemovedFromHierarchy += OnProjectFolderRemoved;
            folderView.PinsModified += OnProjectFolderPinsModified;
            folderView.FolderSettingsUpdated += OnFolderSettingsUpdated;

            _projectScrollView.AddItem(folderView);
            //_contextContent[PinContext.Project].Add(listView);

            _folderCreationButton.BringToFront();
        }

        private void OnFolderSettingsUpdated(PinFolder folder) => SavePins();

        private void OnProjectFolderPinsModified(PinFolder folder) => SavePins();

        private void OnProjectFolderRemoved(PinFolder folder)
        {
            _pinBoardPage.ProjectFolders.Remove(folder);
            SavePins();
        }

        private void SavePins()
        {
            Debug.Log($"saving");
            PluginDataStorage.SavePinBoardPage(_pinBoardPage);
        }
    }
}
