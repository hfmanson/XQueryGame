using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Reflection.Metadata;
using System.Xml.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

namespace Xslt2Game.ViewModels
{
    public class BoxNodeViewModel : NodeViewModelBase
    {
        private double _left;
        private double _top;
        private double _boxRotation;
        private PointCollection _points;
        private Brush _stroke;
        private Brush _fill;
        private double _strokeThickNess;

        public double Left
        {
            get => _left;
            set { _left = value; OnPropertyChanged(); }
        }

        public double Top
        {
            get => _top;
            set { _top = value; OnPropertyChanged(); }
        }

        public double BoxRotation
        {
            get => _boxRotation;
            set { _boxRotation = value; OnPropertyChanged(); }
        }

        public PointCollection Points
        {
            get => _points;
            set { _points = value; OnPropertyChanged(); }
        }

        public Brush Stroke
        {
            get => _stroke;
            set { _stroke = value; OnPropertyChanged(); }
        }

        public double StrokeThickness
        {
            get => _strokeThickNess;
            set { _strokeThickNess = value; OnPropertyChanged(); }
        }

        public Brush Fill
        {
            get => _fill;
            set { _fill = value; OnPropertyChanged(); }
        }

        public override void UpdateField(string AttrName, string AttrValue)
        {
            double doubleValue = double.Parse(AttrValue);
            if (AttrName == "row")
            {
                Top = doubleValue - 1.0;
            }
            else if (AttrName == "column")
            {
                Left = doubleValue - 1.0;
            }
        }

        public void UpdateRotation(double dx, double dy)
        {
            BoxRotation = dy == 1.0 ? 0 : dy == -1.0 ? 180.0 : dx == 1 ? -90.0 : 90.0;
        }
        public static class GameAssets
        {
            private static readonly Dictionary<string, (PointCollection Points, Brush Stroke, double StrokeThickness, Brush Fill)> TypesRegistry =
                new Dictionary<string, (PointCollection, Brush, double, Brush)>();

            private static string BigBoxPoints = "0.1,0.1 0.1,0.9 0.9,0.9 0.9,0.1";
            private static string SmallBoxPoints = "0.225,0.225 0.225,0.8 0.8,0.8 0.8,0.225";
            private static string MoverPoints = "0.375,0.375 0.625,0.375 0.625,0.625 0.375,0.625";
            private static double BoxStrokeThickness = 0.075;
            static GameAssets()
            {
                // Register your "box-types" with native objects
                TypesRegistry["normal"] = (
                    (PointCollection)XamlBindingHelper.ConvertValue(typeof(PointCollection), GameAssets.BigBoxPoints),
                    new SolidColorBrush(Colors.Black),
                    GameAssets.BoxStrokeThickness,
                    null
                );
                TypesRegistry["destination"] = (
                    (PointCollection)XamlBindingHelper.ConvertValue(typeof(PointCollection), GameAssets.BigBoxPoints),
                    new SolidColorBrush(Colors.Blue),
                    GameAssets.BoxStrokeThickness,
                    null
                );
                TypesRegistry["source"] = (
                    (PointCollection)XamlBindingHelper.ConvertValue(typeof(PointCollection), GameAssets.SmallBoxPoints),
                    new SolidColorBrush(Colors.Red),
                    GameAssets.BoxStrokeThickness,
                    null
                );
                TypesRegistry["mover"] = (
                    (PointCollection)XamlBindingHelper.ConvertValue(typeof(PointCollection), GameAssets.MoverPoints),
                    null,
                    0.0,
                    new SolidColorBrush(Colors.Black)
                );
            }

            public static (PointCollection Points, Brush Stroke, double StrokeThickness, Brush Fill) GetAssetsForType(string typeName)
            {
                if (TypesRegistry.TryGetValue(typeName ?? "", out var assets)) return assets;
                return TypesRegistry["mover"];
            }
        }
        public BoxNodeViewModel(XElement element)
        {
            // 1. Position Setup (Converts Row/Column to Left/Top)
            string row = element.Attribute("row").Value;
            UpdateField("row", row);
            string column = element.Attribute("column").Value;
            UpdateField("column", column);

            double dx = double.Parse(element.Attribute("dx")?.Value ?? "0");
            double dy = double.Parse(element.Attribute("dy")?.Value ?? "0");
            UpdateRotation(dx, dy);

            string boxType = element.Attribute("box-type")?.Value;
            var assets = GameAssets.GetAssetsForType(boxType);
            Points = assets.Points;
            Fill = assets.Fill;
            Stroke = assets.Stroke;
            StrokeThickness = assets.StrokeThickness;
        }
    }
}
