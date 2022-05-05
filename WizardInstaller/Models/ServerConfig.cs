﻿using System.Collections.Generic;

namespace WizardInstaller.Template.Models
{
    public class ServerConfig
	{
		public int LastServerUsed { get; set; }
		public List<DBServer> Servers { get; set; }

		public ServerConfig()
		{
			LastServerUsed = 0;
			Servers = new List<DBServer>();
		}
	}
}
