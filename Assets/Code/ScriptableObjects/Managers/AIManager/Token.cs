﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assets.Code.AI;
using UnityEngine;

namespace SpectralDaze.ScriptableObjects.AIManager
{
    public class Token : ScriptableObject
    {
        public bool InUse = false;
        public bool NoLongerInUse = false;
        public BaseAI Requestor;
    }
}
