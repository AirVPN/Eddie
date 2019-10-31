// <eddie_source_header>
// This file is part of Eddie/AirVPN software.
// Copyright (C)2014-2019 AirVPN (support@airvpn.org) / https://airvpn.org
//
// Eddie is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Eddie is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Eddie. If not, see <http://www.gnu.org/licenses/>.
// </eddie_source_header>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Eddie.Core;

namespace Eddie.Forms.Forms
{
    public partial class WindowShellExternalPermission : Eddie.Forms.Form
    {
		public Json Data;
		public Json Answer;

        public WindowShellExternalPermission()
        {
            OnPreInitializeComponent();
            InitializeComponent();
            OnInitializeComponent();
        }

        public override void OnInitializeComponent()
        {
            base.OnInitializeComponent();            
        }

        public override void OnApplySkin()
        {
            base.OnApplySkin();


        }

        protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			CommonInit(LanguageManager.GetText("WindowsShellExternalPermissionTitle"));

			lblHostHelp.Text = LanguageManager.GetText("WindowsShellExternalPermissionTop", Data["path"].Value as string);

			cmdNo.Text = LanguageManager.GetText("WindowsShellExternalPermissionNo");
			cmdYes.Text = LanguageManager.GetText("WindowsShellExternalPermissionYes");
			cmdRuleSign.Text = LanguageManager.GetText("WindowsShellExternalPermissionRuleSign", Data["sign-id"].Value as string);
			if ((Data["sign-id"].Value as string).StartsWith("No: "))
				cmdRuleSign.Enabled = false;
			cmdRuleHash.Text = LanguageManager.GetText("WindowsShellExternalPermissionRuleHash", Data["sha256"].Value as string);
			cmdRulePath.Text = LanguageManager.GetText("WindowsShellExternalPermissionRulePath", Data["path"].Value as string);
			cmdRuleAll.Text = LanguageManager.GetText("WindowsShellExternalPermissionRuleAll");			

			Answer = new Json();
			Answer["allow"].Value = false;

			EnableIde();
		}
        
		private void EnableIde()
		{
					
		}
				
		private void cmdNo_Click(object sender, EventArgs e)
		{	
			Answer["allow"].Value = false;
			Close();
		}

		private void cmdYes_Click(object sender, EventArgs e)
		{
			Answer["allow"].Value = true;
			Close();
		}

		private void cmdRuleSign_Click(object sender, EventArgs e)
		{
			Answer.RemoveKey("allow");
			Answer["type"].Value = "sign";
			Answer["id"].Value = Data["sign-id"].Value;
			Close();
		}

		private void cmdRuleHash_Click(object sender, EventArgs e)
		{
			Answer.RemoveKey("allow");
			Answer["type"].Value = "sha256";
			Answer["hash"].Value = Data["sha256"].Value;
			Close();
		}

		private void cmdRulePath_Click(object sender, EventArgs e)
		{
			Answer.RemoveKey("allow");
			Answer["type"].Value = "path";
			Answer["path"].Value = Data["path"].Value;
			Close();
		}

		private void cmdRuleAll_Click(object sender, EventArgs e)
		{
			Answer.RemoveKey("allow");
			Answer["type"].Value = "all";
			Close();
		}
	}
}
