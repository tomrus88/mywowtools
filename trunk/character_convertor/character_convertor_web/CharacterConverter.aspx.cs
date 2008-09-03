using System;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using CharacterConverter;

public partial class CharacterConverterView : System.Web.UI.Page, IConverterView {
	private ConverterPresenter _presenter;
	public CharacterConverterView() {
		_presenter = new ConverterPresenter(this);
	}
	protected void Page_Load(object sender, EventArgs e) {

	}

	public string Host {
		get { return this.TextBox1.Text; }
		set { this.TextBox1.Text = value; }
	}

	public string Port {
		get { return this.TextBox2.Text; }
		set { this.TextBox2.Text = value; }
	}

	public string Base {
		get { return this.TextBox3.Text; }
		set { this.TextBox3.Text = value; }
	}

	public new string User {
		get { return this.TextBox4.Text; }
		set { this.TextBox4.Text = value; }
	}

	public string Pass {
		get { return this.TextBox5.Text; }
		set { this.TextBox5.Text = value; }
	}

	public void SetPresenter(ConverterPresenter presenter) {
		_presenter = presenter;
	}

	protected void Button1_Click(object sender, EventArgs e) {
		_presenter.Convert();
	}
}
