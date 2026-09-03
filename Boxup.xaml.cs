using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xslt2Game.EngineJS;
using Xslt2Game.ViewModels;

namespace Xslt2Game
{
    public sealed partial class Boxup : Page
    {
        private double _width;
        private double _height;
        private XDocument _document;
        private NodeViewModelsBase _NodeViewModels;
        private FontoXPath _fontoXPath;

        public Boxup()
        {
            InitializeComponent();
            _NodeViewModels = new BoxNodeViewModels();
            _document = _NodeViewModels.LoadGame(game);
            XElement root = _document.Root;
            Width = double.Parse(root.Attribute("columns").Value);
            Height = double.Parse(root.Attribute("rows").Value);
            _fontoXPath = new FontoXPath(_document);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double Width
        {
            get => _width;
            set { _width = value; OnPropertyChanged(); }
        }

        public double Height
        {
            get => _height;
            set { _height = value; OnPropertyChanged(); }
        }


        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            string xquf = @"
declare namespace boxup=""http://mansoft.nl/boxup"";

declare %updating function local:move-mover($model-mover, $column, $row) {
  replace value of node $model-mover/@column with $column,
  replace value of node $model-mover/@row with $row  
};

let $model-element := /boxup:boxup
    , $model-mover := $model-element/boxup:mover

return local:move-mover($model-mover, 2, 2)
";
            //_fontoXPath.testXQuery(text.Text);
            //_fontoXPath.testUpdateXQuery(text.Text);
            _fontoXPath.testUpdateXQuery(xquf);
        }
    }
}
