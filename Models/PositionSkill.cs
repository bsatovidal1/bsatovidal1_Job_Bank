﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace bsatovidal1_Job_Bank.Models
{
    public class PositionSkill
    {
        public int PositionID { get; set; }
        public Position Position { get; set; }

        public int SkillID { get; set; }
        public Skill Skill { get; set; }
    }
}
