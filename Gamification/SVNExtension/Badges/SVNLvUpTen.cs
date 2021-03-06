﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Extension;
using Extension.Badge;
using MongoDB.Bson;
namespace SVNExtension.Badges
{
    public class SVNLvUpTen : IBadge
    {
        public ObjectId id { get; private set; }
        public BadgeLevel Level { get; private set; }
        public string Content { get; private set; }
        public string IconPath { get; private set; }
        public string Name { get; private set; }
        public string ExtensionName { get; private set; }
        public bool Secret { get; private set; }
        public bool Gained { get; private set; }

        public SVNLvUpTen()
        {
            ExtensionName = "SVN";
            Level = BadgeLevel.topaz;
            Content = GetContent();
            IconPath = ".\\res\\Badges\\SVN\\SVNLevelUpTen.png";
            Name = "Level 10!";
            Secret = false;
            Gained = false;
        }

        public string GetContent()
        {
            var content = "Reach Subversion level 10";
            return content;
        }

        public void Compute(IUser user)
        {
            var exp = new SVNExperience("current", ".\\Experience\\UserLevel.prop", "Alias");
            exp.AddModel((SVNModel)user.ExtensionPoint["SVNExtension"]);
            if (exp.Level > 10)
            {
                Gained = true;
            }
        }
    }
}
