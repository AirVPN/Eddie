using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Eddie.Core;
using Eddie.Core.ConfigBuilder;

namespace Eddie.Forms.Forms
{
	public class awgprofilewindow : Skin.SkinForm
	{
		public string savedpath = "";
		private string directory;
		private awgprofile profile = awgprofile.create(2);
		private readonly Dictionary<string, Skin.TextBox> fields = new Dictionary<string, Skin.TextBox>();
		private readonly TabControl tabs = new TabControl { Dock = DockStyle.Fill };
		private readonly Skin.TextBox preview = new Skin.TextBox { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Both, WordWrap = false, AcceptsReturn = true, AcceptsTab = true };
		private readonly Skin.ComboBox protocol = new Skin.ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 170 };
		private readonly Skin.ComboBox peer = new Skin.ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 70 };
		private readonly Skin.ComboBox presets = new Skin.ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 180 };
		private readonly Skin.TextBox localpublic = new Skin.TextBox { Dock = DockStyle.Fill, ReadOnly = true };
		private readonly List<string[]> cps = new List<string[]>();
		private readonly Dictionary<Control, int> awgcontrols = new Dictionary<Control, int>();
		private bool loading;
		private int peerindex;

		private static string label(LanguageItems item) { return LanguageManager.GetText(item); }

		public awgprofilewindow(string path)
		{
			OnPreInitializeComponent();
			directory = path;
			Text = label(LanguageItems.awgtitle);
			ClientSize = new Size(880, 690);
			MinimumSize = new Size(660, 500);
			StartPosition = FormStartPosition.CenterParent;
			ShowInTaskbar = false;
			var actions = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(8), WrapContents = true };
			button(actions, LanguageItems.awgnew, () => { profile = awgprofile.create(protocol.SelectedIndex); savedpath = ""; loadfields(); if (tabs.SelectedIndex == 1) preview.Text = profile.build().Replace("\n", Environment.NewLine); });
			button(actions, LanguageItems.awgopen, open);
			button(actions, LanguageItems.awgsave, save);
			button(actions, LanguageItems.awgcheck, () => { if (validate()) MessageBox.Show(this, label(LanguageItems.awgvalid), Text); });
			button(actions, LanguageItems.awgclose, Close);
			var settings = new TabPage(label(LanguageItems.awgfields));
			var config = new TabPage(label(LanguageItems.awgpreview));
			config.Controls.Add(preview);
			var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true, Padding = new Padding(12) };
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 175));
			layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
			protocol.Items.AddRange(new object[] { "wireguard", "amneziawg 1.0", "amneziawg 2.0" });
			row(layout, label(LanguageItems.awgprotocol), protocol);
			foreach (string key in awgprofile.interfacekeys) field(layout, key, "interface.");
			var generators = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
			button(generators, LanguageItems.awgprivate, () => { fields["interface.PrivateKey"].Text = awgprofile.privatekey(); derivepublic(); });
			button(generators, LanguageItems.awgpublic, derivepublic);
			row(layout, "", generators);
			row(layout, label(LanguageItems.awglocalpublic), localpublic);
			fields["interface.PrivateKey"].TextChanged += (sender, args) => localpublic.Text = "";
			var peers = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
			peers.Controls.Add(peer);
			button(peers, LanguageItems.awgaddpeer, () => { readfields(); profile.peers.Add(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)); peerindex = profile.peers.Count - 1; loadfields(); });
			button(peers, LanguageItems.awgremovepeer, () => { if (profile.peers.Count > 1) { readfields(); profile.peers.RemoveAt(peerindex); peerindex = 0; loadfields(); } });
			row(layout, label(LanguageItems.awgpeer), peers);
			foreach (string key in awgprofile.peerkeys) field(layout, key, "peer.");
			foreach (string key in awgprofile.awgkeys) field(layout, key, "interface.");
			var junk = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
			awgcontrols[junk] = 1;
			button(junk, LanguageItems.awgjunk, () => { if (profile.version == 0) return; readfields(); profile.randomjunk(); loadfields(); });
			row(layout, "", junk);
			var cpsactions = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
			awgcontrols[cpsactions] = 2;
			cpsactions.Controls.Add(presets);
			button(cpsactions, LanguageItems.awgrandompreset, () => { if (cps.Count > 0 && profile.version == 2) { presets.SelectedIndex = new Random().Next(1, cps.Count + 1); } });
			var quic = new Skin.Button { Text = "quic", AutoSize = true };
			quic.Click += (sender, args) => generatecps(true);
			cpsactions.Controls.Add(quic);
			var dns = new Skin.Button { Text = "dns", AutoSize = true };
			dns.Click += (sender, args) => generatecps(false);
			cpsactions.Controls.Add(dns);
			button(cpsactions, LanguageItems.awgclearcps, () => { for (int i = 1; i <= 5; i++) fields["interface.I" + i].Text = ""; presets.SelectedIndex = 0; });
			row(layout, label(LanguageItems.awgpresets), cpsactions);
			var note = new Skin.Label { Text = label(LanguageItems.awgnote), AutoSize = true, MaximumSize = new Size(650, 0), Dock = DockStyle.Top };
			row(layout, "", note);
			settings.Controls.Add(layout);
			tabs.TabPages.Add(settings);
			tabs.TabPages.Add(config);
			Controls.Add(tabs);
			Controls.Add(actions);
			protocol.SelectedIndexChanged += (sender, args) =>
			{
				if (loading) return;
				readfields();
				int selected = protocol.SelectedIndex;
				if (selected == 0) foreach (string key in awgprofile.awgkeys) profile.iface.Remove(key);
				if (selected == 1) foreach (string key in new[] { "S3", "S4", "I1", "I2", "I3", "I4", "I5" }) profile.iface.Remove(key);
				if (profile.version == 0 && selected > 0) { profile.version = selected; profile.defaults(); }
				profile.version = selected;
				loadfields();
			};
			peer.SelectedIndexChanged += (sender, args) => { if (!loading && peer.SelectedIndex >= 0) { readfields(); peerindex = peer.SelectedIndex; loadfields(); } };
			tabs.Selecting += (sender, args) =>
			{
				try
				{
					if (args.TabPage == config) { readfields(); preview.Text = profile.build().Replace("\n", Environment.NewLine); }
					else { int version = profile.version; profile = awgprofile.parse(preview.Text); if (version == 2 && profile.version == 1) profile.version = 2; peerindex = 0; loadfields(); }
				}
				catch (Exception error) { args.Cancel = true; showerror(error.Message); }
			};
			loadpresets();
			presets.SelectedIndexChanged += (sender, args) =>
			{
				if (presets.SelectedIndex <= 0 || profile.version != 2) return;
				string[] values = cps[presets.SelectedIndex - 1];
				for (int i = 1; i <= 5; i++) fields["interface.I" + i].Text = values[i + 1];
			};
			loadfields();
			OnInitializeComponent();
		}

		private void loadpresets()
		{
			presets.Items.Add(label(LanguageItems.awgmanual));
			using (var stream = typeof(awgprofile).Assembly.GetManifestResourceStream("awg.cps.csv"))
			using (var reader = new StreamReader(stream, Encoding.UTF8))
			{
				while (!reader.EndOfStream)
				{
					string[] values = reader.ReadLine().Split(',');
					if (values.Length != 7) continue;
					string[] compact = awgprofile.compactcps(values[2], values[3], values[4], values[5], values[6]);
					if (compact[0] == "") continue;
					Array.Copy(compact, 0, values, 2, 5);
					bool valid = true;
					for (int i = 2; i < 7; i++) if (values[i] != "" && awgprofile.validatesignature(values[i]) != "") valid = false;
					if (!valid) continue;
					cps.Add(values);
					presets.Items.Add(values[0].ToLowerInvariant());
				}
			}
			presets.SelectedIndex = 0;
		}

		private void generatecps(bool quic)
		{
			if (profile.version != 2) return;
			fields["interface.I1"].Text = quic ? awgprofile.quic(true) : awgprofile.dns();
			fields["interface.I2"].Text = quic ? awgprofile.quic(false) : awgprofile.dns();
			fields["interface.I3"].Text = quic ? awgprofile.quic(false) : awgprofile.junk();
			for (int i = 4; i <= 5; i++) fields["interface.I" + i].Text = awgprofile.junk();
			presets.SelectedIndex = 0;
		}

		private void derivepublic()
		{
			var start = new ProcessStartInfo(Path.Combine(Platform.Instance.GetApplicationPath(), "amneziawg-eddie.exe"), "--public-key")
			{
				UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true
			};
			using (var process = System.Diagnostics.Process.Start(start))
			{
				process.StandardInput.WriteLine(fields["interface.PrivateKey"].Text.Trim());
				process.StandardInput.Close();
				if (!process.WaitForExit(5000)) { process.Kill(); throw new TimeoutException("public key derivation timed out"); }
				if (process.ExitCode != 0) throw new FormatException(process.StandardError.ReadToEnd().Trim());
				string value = process.StandardOutput.ReadToEnd().Trim();
				if (value.Length != 44 || Convert.FromBase64String(value).Length != 32) throw new FormatException("invalid public key from amneziawg engine");
				localpublic.Text = value;
			}
		}

		private void row(TableLayoutPanel layout, string name, Control control)
		{
			int index = layout.RowCount++;
			layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
			layout.Controls.Add(new Skin.Label { Text = name, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(4, 8, 4, 8) }, 0, index);
			control.Margin = new Padding(4, 4, 4, 4);
			layout.Controls.Add(control, 1, index);
		}

		private void field(TableLayoutPanel layout, string name, string section)
		{
			var input = new Skin.TextBox { Dock = DockStyle.Fill };
			fields[section + name] = input;
			row(layout, name.ToLowerInvariant(), input);
		}

		private void button(FlowLayoutPanel panel, LanguageItems item, Action action)
		{
			var button = new Skin.Button { Text = label(item), AutoSize = true };
			button.Click += (sender, args) => { try { action(); } catch (Exception error) { showerror(error.Message); } };
			panel.Controls.Add(button);
		}

		private void readfields()
		{
			foreach (var pair in fields)
			{
				string[] parts = pair.Key.Split('.');
				var values = parts[0] == "interface" ? profile.iface : profile.peers[peerindex];
				string value = pair.Value.Text.Trim();
				if (value == "") values.Remove(parts[1]); else values[parts[1]] = value;
			}
		}

		private void loadfields()
		{
			loading = true;
			try
			{
				protocol.SelectedIndex = profile.version;
				peerindex = Math.Min(peerindex, Math.Max(0, profile.peers.Count - 1));
				if (profile.peers.Count == 0) profile.peers.Add(new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));
				peer.Items.Clear();
				for (int i = 0; i < profile.peers.Count; i++) peer.Items.Add((i + 1).ToString());
				peer.SelectedIndex = peerindex;
				foreach (var control in awgcontrols) control.Key.Enabled = profile.version >= control.Value;
				foreach (var pair in fields)
				{
					string[] parts = pair.Key.Split('.');
					pair.Value.Text = awgprofile.get(parts[0] == "interface" ? profile.iface : profile.peers[peerindex], parts[1]);
					pair.Value.Enabled = !awgprofile.isawgkey(parts[1]) || (profile.version > 0 && (profile.version == 2 || (parts[1] != "S3" && parts[1] != "S4" && !parts[1].StartsWith("I"))));
				}
			}
			finally { loading = false; }
		}

		private void showerror(string text) { MessageBox.Show(this, text, label(LanguageItems.awgerror), MessageBoxButtons.OK, MessageBoxIcon.Error); }

		private bool validate()
		{
			if (tabs.SelectedIndex == 1) profile = awgprofile.parse(preview.Text); else readfields();
			var errors = profile.validate();
			if (errors.Count > 0) { showerror(string.Join(Environment.NewLine, errors)); return false; }
			return true;
		}

		private void open()
		{
			using (var dialog = new OpenFileDialog { Filter = "wg / awg (*.conf)|*.conf", InitialDirectory = directory })
			{
				if (dialog.ShowDialog(this) != DialogResult.OK) return;
				var parsed = awgprofile.parse(File.ReadAllText(dialog.FileName));
				profile = parsed;
				savedpath = dialog.FileName;
				peerindex = 0;
				loadfields();
				if (tabs.SelectedIndex == 1) preview.Text = profile.build().Replace("\n", Environment.NewLine);
			}
		}

		private void save()
		{
			if (!validate()) return;
			using (var dialog = new SaveFileDialog { Filter = "wg / awg (*.conf)|*.conf", DefaultExt = "conf", AddExtension = true, InitialDirectory = directory, FileName = savedpath == "" ? "profile.conf" : Path.GetFileName(savedpath) })
			{
				if (dialog.ShowDialog(this) != DialogResult.OK) return;
				File.WriteAllText(dialog.FileName, profile.build(), new UTF8Encoding(false));
				savedpath = dialog.FileName;
				directory = Path.GetDirectoryName(savedpath);
			}
		}
	}
}
