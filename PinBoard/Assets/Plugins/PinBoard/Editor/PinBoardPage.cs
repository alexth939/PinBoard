using System;
using System.Collections.Generic;
using UnityEngine;

namespace PinBoard.Editor
{
    [Serializable]
    internal class PinBoardPage
    {
        [SerializeField] private List<PinReference> _scenePins = new();
        [SerializeField] private List<PinFolder> _projectFolders = new();//transient (for the user)

        public List<PinReference> ScenePins => _scenePins;
        public List<PinFolder> ProjectFolders => _projectFolders;
    }

    [Serializable]
    internal class PinFolder
    {
        [SerializeField] private string _name;
        [SerializeField] private List<PinReference> _pins = new();
        [SerializeField] private Color _color = new Color32(155,155,155,255);

        public PinFolder(string name, List<PinReference> pins)
        {
            _name = name;
            _pins = pins;
        }

        public List<PinReference> Pins => _pins;

        public string Name { get => _name; set => _name = value; }
        public Color Color { get => _color; set => _color = value; }
    }
}
