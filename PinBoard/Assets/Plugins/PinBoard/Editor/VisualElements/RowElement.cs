using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PinBoard.Editor.VisualElements
{
    internal class RowElement : VisualElement
    {
        private readonly Image _image = new();

        private static System.Lazy<Texture> _lazyHandIcon = new(() => EditorGUIUtility.IconContent("scenepicking_pickable_hover@2x").image);

        public RowElement()
        {
            style.flexDirection = FlexDirection.Row;
            style.flexGrow = 1f;
            style.width = Length.Percent(100);

            _image.image = _lazyHandIcon.Value;
            Add(_image);
        }
    }
}
