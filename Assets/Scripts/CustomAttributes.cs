﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
	public class LObjectPropertyAliasAsAttribute
		: Attribute
	{
		public string Alias { get; private set; }

		public LObjectPropertyAliasAsAttribute(string alias)
		{
			Alias = alias;
		}
	}
}