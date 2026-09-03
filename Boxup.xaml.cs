using Acornima.Ast;
using Jint;
using Jint.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Windows.Media.Editing;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Xslt2Game.EngineJS;
using Xslt2Game.ViewModels;

namespace Xslt2Game
{
    public sealed partial class Boxup : Page
    {
        private XDocument _document;
        private NodeViewModelsBase _NodeViewModels;
        private FontoXPath _fontoXPath;
        public Boxup()
        {
            InitializeComponent();
            _NodeViewModels = new BoxNodeViewModels();
            _document = _NodeViewModels.LoadGame(game);
            _fontoXPath = new FontoXPath(_document);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            string xquf = @"
declare %updating function local:move-mover($model-mover, $column, $row) {
  replace value of node $model-mover/@column with $column,
  replace value of node $model-mover/@row with $row  
};

let $model-element := /boxup
    , $model-mover := $model-element/mover

return local:move-mover($model-mover, 2, 2)
";
            //_fontoXPath.testXQuery(text.Text);
            //_fontoXPath.testUpdateXQuery(text.Text);
            _fontoXPath.testUpdateXQuery(xquf);
        }
    }
}
