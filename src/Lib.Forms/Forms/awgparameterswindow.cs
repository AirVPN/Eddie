using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Eddie.Core;
using Eddie.Core.ConfigBuilder;

namespace Eddie.Forms.Forms
{
	public class awgparameterswindow : Skin.SkinForm
	{
		public string directives;
		private awgprofile profile;
		private readonly Dictionary<string, Skin.TextBox> fields = new Dictionary<string, Skin.TextBox>();
		private readonly List<string[]> cps = awgprofile.presets();
		private readonly Skin.ComboBox presets = new Skin.ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList };

		public awgparameterswindow(string value)
		{
			OnPreInitializeComponent();
			directives = value;
			profile = awgprofile.airvpn(value);
			Text = LanguageManager.GetText(LanguageItems.awgsettings);
			ClientSize = new Size(860, 710);
			MinimumSize = new Size(680, 540);
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			var layout = new TableLayoutPanel { Dock = DockStyle.Fill, AutoScroll = true, ColumnCount = 2, Padding = new Padding(12) };
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			foreach (string key in awgprofile.awgkeys)
			{
				var input = new Skin.TextBox { Dock = DockStyle.Fill, ReadOnly = key.StartsWith("S") || key.StartsWith("H") };
				fields[key] = input;
				row(layout, key.ToLowerInvariant(), input);
			}
			presets.Items.Add(LanguageManager.GetText(LanguageItems.awgmanual));
			foreach (var preset in cps) presets.Items.Add(preset[0].ToLowerInvariant());
			presets.SelectedIndex = 0;
			presets.SelectedIndexChanged += (sender, args) =>
			{
				if (presets.SelectedIndex <= 0) return;
				var preset = cps[presets.SelectedIndex - 1];
				for (int i = 1; i <= 5; i++) fields["I" + i].Text = preset[i + 1];
			};
			row(layout, LanguageManager.GetText(LanguageItems.awgpresets), presets);
			var generators = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
			button(generators, LanguageManager.GetText(LanguageItems.awgjunk), () => { read(); profile.randomjunk(); load(); });
			button(generators, LanguageManager.GetText(LanguageItems.awgrandompreset), () => { if (cps.Count > 0) presets.SelectedIndex = new Random().Next(1, cps.Count + 1); });
			button(generators, "quic", () => generate(true));
			button(generators, "dns", () => generate(false));
			button(generators, LanguageManager.GetText(LanguageItems.awgclearcps), () => { for (int i = 1; i <= 5; i++) fields["I" + i].Text = ""; presets.SelectedIndex = 0; });
			button(generators, LanguageManager.GetText(LanguageItems.awgdefaults), () => { profile = awgprofile.airvpn(""); presets.SelectedIndex = 0; load(); });
			row(layout, "", generators);
			row(layout, "", new Skin.Label { Text = LanguageManager.GetText(LanguageItems.awgairvpnnote), AutoSize = true, MaximumSize = new Size(630, 0), Dock = DockStyle.Top });
			var actions = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(10), FlowDirection = FlowDirection.RightToLeft };
			var cancel = new Skin.Button { Text = LanguageManager.GetText(LanguageItems.awgclose), AutoSize = true, DialogResult = DialogResult.Cancel };
			actions.Controls.Add(cancel);
			button(actions, LanguageManager.GetText(LanguageItems.awgapply), () => { read(); profile = awgprofile.airvpn(profile.buildawg()); directives = profile.buildawg(); DialogResult = DialogResult.OK; Close(); });
			CancelButton = cancel;
			Controls.Add(layout);
			Controls.Add(actions);
			load();
			OnInitializeComponent();
		}

		private void row(TableLayoutPanel layout, string text, Control input)
		{
			int index = layout.RowCount++;
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.Controls.Add(new Skin.Label { Text = text, AutoSize = true, Margin = new Padding(3, 9, 3, 6) }, 0, index);
			input.Margin = new Padding(3, 5, 3, 5);
			layout.Controls.Add(input, 1, index);
		}

		private void button(Control parent, string text, Action action)
		{
			var button = new Skin.Button { Text = text, AutoSize = true };
			button.Click += (sender, args) => { try { action(); } catch (Exception error) { MessageBox.Show(this, error.Message, LanguageManager.GetText(LanguageItems.awgerror), MessageBoxButtons.OK, MessageBoxIcon.Error); } };
			parent.Controls.Add(button);
		}

		private void read()
		{
			foreach (var pair in fields) { if (pair.Value.Text.Trim() == "") profile.iface.Remove(pair.Key); else profile.iface[pair.Key] = pair.Value.Text.Trim(); }
		}

		private void load()
		{
			foreach (var pair in fields) pair.Value.Text = awgprofile.get(profile.iface, pair.Key);
		}

		private void generate(bool quic)
		{
			fields["I1"].Text = quic ? awgprofile.quic(true) : awgprofile.dns();
			fields["I2"].Text = quic ? awgprofile.quic(false) : awgprofile.dns();
			fields["I3"].Text = quic ? awgprofile.quic(false) : awgprofile.junk();
			for (int i = 4; i <= 5; i++) fields["I" + i].Text = awgprofile.junk();
			presets.SelectedIndex = 0;
		}
	}
}
