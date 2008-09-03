using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CharacterConverter.Con {
	class Program : IConverterView {
		private ConverterPresenter _presenter;

		public Program() {
			_presenter = new ConverterPresenter(this);
		}

		private string _host;
		public string Host {
			get {
				if(_host == null) {
					Console.Write("Enter DB host: ");
					_host = Console.ReadLine();
				}
				return _host;
			}
			set { _host = value; }
		}

		private string _port;
		public string Port {
			get {
				if(_port == null) {
					Console.Write("Enter DB port: ");
					_port = Console.ReadLine();
				}
				return _port;
			}
			set { _port = value; }
		}

		private string _base;
		public string Base {
			get {
				if(_base == null) {
					Console.Write("Enter DB name: ");
					_base = Console.ReadLine();
				}
				return _base;
			}
			set { _base = value; }
		}

		private string _user;
		public string User {
			get {
				if(_user == null) {
					Console.Write("Enter DB user name: ");
					_user = Console.ReadLine();
				}
				return _user;
			}
			set { _user = value; }
		}

		private string _pass;
		public string Pass {
			get {
				if(_pass == null) {
					Console.Write("Enter DB password: ");
					_pass = Console.ReadLine();
				}
				return _pass;
			}
			set { _pass = value; }
		}

		public void SetPresenter(ConverterPresenter presenter) {
			_presenter = presenter;
		}

		private void Convert() {
			_presenter.Convert();
		}

		static void Main(string[] args) {
			Console.WriteLine("Characters convertor for MaNGoS");
			Console.WriteLine("Client version 2.4.2->2.4.3");
			Console.WriteLine("Written by TOM_RUS");

			AppDomain currentDomain = AppDomain.CurrentDomain;
			currentDomain.UnhandledException += new UnhandledExceptionEventHandler(TopLevelErrorHandler);

			Program program = new Program();
			program.Convert();

			Console.WriteLine("Done!");
			Console.ReadKey();
		}

		private static void TopLevelErrorHandler(object sender, UnhandledExceptionEventArgs args) {
			Exception e = (Exception)args.ExceptionObject;
			Console.WriteLine("Error Occured : " + e.ToString());
		}
	}
}
