using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CharacterConverter {
	public class ConverterPresenter {
		private Converter _converter;
		private IConverterView _view;
		public ConverterPresenter(IConverterView view) {
			_view = view;
			_view.SetPresenter(this);
		}

		public void Convert() {
			var cs = string.Format("Server={0};Port={1};Database={2};Uid={3};Pwd={4}",
				_view.Host, _view.Port, _view.Base, _view.User, _view.Pass);

			_converter = new Converter(cs);
			_converter.Convert();
		}
	}
}
