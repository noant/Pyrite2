﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pyrite.MainDomain
{
    public abstract class SecuritySettingsBase
    {
        public abstract bool IsAvailableForUser(UserBase user, ScenarioStartupSource source);
    }
}
