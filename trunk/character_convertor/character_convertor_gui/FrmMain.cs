using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace CharacterConverter.Gui {
	public partial class FrmMain : Form, IConverterView {
		private ConverterPresenter _presenter;
		public FrmMain() {
			InitializeComponent();

			_presenter = new ConverterPresenter(this);
		}

		public string Host {
			get { return textBox1.Text; }
			set { textBox1.Text = value; }
		}

		public string Port {
			get { return textBox2.Text; }
			set { textBox2.Text = value; }
		}

		public string Base {
			get { return textBox3.Text; }
			set { textBox3.Text = value; }
		}

		public string User {
			get { return textBox4.Text; }
			set { textBox4.Text = value; }
		}

		public string Pass {
			get { return textBox5.Text; }
			set { textBox5.Text = value; }
		}

		public void SetPresenter(ConverterPresenter presenter) {
			_presenter = presenter;
		}

		private void button1_Click(object sender, EventArgs e) {
			_presenter.Convert();
		}
	}
}
