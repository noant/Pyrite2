﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pyrite.MainDomain;
using Pyrite.ActionsDomain.Attributes;

namespace Pyrite.Security.Permissions
{
    [HumanFriendlyName("Запретить в интерфейсе сервера")]
    public class DenyForServerUIPermission : IPermission
    {
        public bool IsAvailableForUser(UserBase user, ScenarioStartupSource source)
        {
            return source != ScenarioStartupSource.ServerUI;
        }
    }
}
