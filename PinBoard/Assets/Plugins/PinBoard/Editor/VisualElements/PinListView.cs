using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace PinBoard.Editor.VisualElements
{
    internal sealed class PinListView : ListView
    {
        private readonly List<PinReference> _pins;

        public event Action Modified;

        public PinListView(List<PinReference> pins)
        {
            selectionType = SelectionType.Multiple;

            _pins = pins;
            BindContainerToPinList();
            RegisterCallback<DragPerformEvent>(OnDragPerform);
            RegisterCallback<DragUpdatedEvent>(OnDragUpdate);

            itemIndexChanged += PinListView_itemIndexChanged;
        }

        private void PinListView_itemIndexChanged(int arg1, int arg2) => Modified?.Invoke();

        private void OnDragPerform(DragPerformEvent evt)
        {
            DragAndDrop.AcceptDrag();

            foreach(Object draggedObject in DragAndDrop.objectReferences)
            {
                if(draggedObject == null)
                    continue;

                bool isPersistent = EditorUtility.IsPersistent(draggedObject);
                bool isNotPersistent = !isPersistent;

                //if((_currentContext is PinContext.Project && isNotPersistent) ||
                //    (_currentContext is PinContext.Scene && isPersistent))
                //{
                //    continue; // Skip incompatible objects
                //}

                if(PinReference.TryGetFromObject(draggedObject, out PinReference pin) is false)
                    return;

                if(_pins.Exists(p => p.GlobalIdString == pin.GlobalIdString) is false)
                    _pins.Add(pin);
            }

            Modified?.Invoke();
            RefreshPins();
            evt.StopPropagation();
        }

        private void OnDragUpdate(DragUpdatedEvent evt)
        {
            if(DragAndDrop.objectReferences.Length == 0)
                return;

            bool seenScene = false, seenProject = false;

            foreach(var obj in DragAndDrop.objectReferences)
            {
                if(obj == null)
                    continue;

                if(EditorUtility.IsPersistent(obj))
                    seenProject = true;
                else
                    seenScene = true;

                if(seenProject && seenScene)
                    break; // Mixed selection detected
            }

            if(seenProject && seenScene)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                evt.StopPropagation();
                return;
            }

            bool isValidForContext = true;
            //bool isValidForContext = _currentContext switch
            //{
            //    PinContext.Project => seenProject,
            //    PinContext.Scene => seenScene,
            //    _ => false
            //};

            DragAndDrop.visualMode = isValidForContext ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
            evt.StopPropagation();
        }

        public void RefreshPins()
        {
            //itemsSource = _pins;
            Rebuild();
        }

        private void BindContainerToPinList()
        {
            itemsSource = _pins;

            makeItem = () =>
            {
                var row = new RowElement();

                var objectField = new ObjectField
                {
                    objectType = typeof(Object),
                    allowSceneObjects = true,
                    style = { flexGrow = 1, minWidth = 0 }
                };
                row.Add(objectField);

                var removeButton = new Button
                {
                    style =
                    {
                        width = 20,
                        height = 20,
                        //marginLeft = 0,
                        //paddingLeft = 0,
                        paddingLeft = 0,
                        paddingRight = 20,
                        paddingTop = 0,
                        paddingBottom = 0
                    }
                };

                var closeImage = new Image();
                closeImage.image = EditorGUIUtility.IconContent("d_TreeEditor.Trash").image;
                closeImage.style.width = 20;
                closeImage.style.height = 20;

                removeButton.Add(closeImage);
                row.Add(removeButton);

                // --- Remove button handler ---
                removeButton.clicked += () =>
                {
                    // Use userData instead of capturing "i"
                    if(row.userData is PinReference pinRef)
                    {
                        _pins.Remove(pinRef);
                        RefreshPins();
                        Modified?.Invoke();
                    }
                };

                // --- Drag handler ---
                row.RegisterCallback<MouseDownEvent>(evt =>
                {
                    if(evt.button == 0 && row.userData is PinReference pinRef)
                    {
                        var obj = pinRef.Resolve();
                        if(obj == null)
                            return;

                        DragAndDrop.PrepareStartDrag();

                        if(selectedIndices.Count() == 0)
                        {
                            DragAndDrop.objectReferences = new Object[] { obj };
                        }
                        else
                        {
                            var selectedObjects = selectedIndices.Select(index => _pins[index].Resolve()).ToArray();
                            if(selectedObjects.Contains(obj))
                                DragAndDrop.objectReferences = selectedObjects;
                            else
                            {
                                SetSelection(-1);
                                DragAndDrop.objectReferences = new Object[] { obj };
                            }
                        }

                        DragAndDrop.StartDrag(obj.name);
                        evt.StopPropagation();
                    }
                });

                return row;
            };

            bindItem = (ve, i) =>
            {
                var pin = _pins[i];
                var obj = pin.Resolve();

                ve.userData = pin; // stash the data with the row (avoid closure bugs)

                var objectField = ve.Q<ObjectField>();
                objectField.value = obj;
                objectField.SetEnabled(false);
            };
        }
    }
}
