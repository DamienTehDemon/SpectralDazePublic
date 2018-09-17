﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SpectralDaze.Managers.InputManager
{
    [CreateAssetMenu(menuName = "Spectral Daze/Managers/InputManager/Controls")]
    public class Controls : ScriptableObject
    {
        public List<Control> ControlList;
        public List<AxisControl> AxisControls;
    }
}
