using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ImbaXIV
{
    public enum HideReason
    {
        PcNotVisible,
        UserSetting
    }

    public partial class MinimapWindow : Window
    {
        private readonly ImbaXIVCore _core;
        public Config Config;

        private Point arrowIconPos;
        private Point arrowIconSize;
        private Point canvasSize;
        private double _currentRelativeSize;

        private HideReason hideReason;

        public MinimapWindow(ImbaXIVCore core, Config config)
        {
            InitializeComponent();

            _core = core;
            Config = config;

            Top = Config.MinimapPos.Y;
            Left = Config.MinimapPos.X;
            FixUpPositioning();
            _currentRelativeSize = Config.MinimapSize;

            arrowIconPos.X = Canvas.GetLeft(MinimapArrowImg);
            arrowIconPos.Y = Canvas.GetTop(MinimapArrowImg);
            arrowIconSize.X = MinimapArrowImg.Width;
            arrowIconSize.Y = MinimapArrowImg.Height;
            canvasSize.X = MinimapCanvas.Width;
            canvasSize.Y = MinimapCanvas.Height;

            // Add a gap between the minimap circle and canvas so that the icons don't clip
            double iconRadius = MinimapCanvas.Width / 25;
            MinimapEllipse.Width -= 2 * iconRadius;
            MinimapEllipse.Height -= 2 * iconRadius;
            Canvas.SetLeft(MinimapEllipse, iconRadius);
            Canvas.SetTop(MinimapEllipse, iconRadius);
        }

        protected override void OnClosed(EventArgs e)
        {
            UpdateConfig();
            base.OnClosed(e);
        }

        public void UpdateConfig()
        {
            Config.MinimapPos = new Point(Left, Top);
            Config.MinimapSize = _currentRelativeSize;
        }

        private void FixUpPositioning()
        {
            // In case an external screen was used and it got disconnected.
            if (IsInsideAnyScreen())
                return;
            Left = 0;
            Top = 0;
        }

        private bool IsInsideAnyScreen()
        {
            System.Drawing.Point topLeft = new System.Drawing.Point((int)Left, (int)Top);
            foreach (var screen in Screen.AllScreens)
            {
                if (screen.Bounds.Contains(topLeft))
                    return true;
            }
            return false;
        }

        public void HideMinimap(HideReason reason)
        {
            hideReason = reason;
            this.Visibility = Visibility.Hidden;
        }

        public void UnhideMinimap(HideReason reason)
        {
            if (reason == HideReason.UserSetting || reason == hideReason)
                this.Visibility = Visibility.Visible;
        }

        public void Resize(double ratio)
        {
            ratio = ratio == 0 ? 1 : ratio;
            _currentRelativeSize = ratio;
            this.Width = ratio * canvasSize.X;
            this.Height = ratio * canvasSize.Y;
            MinimapCanvas.Width = ratio * canvasSize.X;
            MinimapCanvas.Height = ratio * canvasSize.Y;
            MinimapEllipse.Width = ratio * canvasSize.X;
            MinimapEllipse.Height = ratio * canvasSize.Y;
            MinimapArrowImg.Width = ratio * arrowIconSize.X;
            MinimapArrowImg.Height = ratio * arrowIconSize.Y;
            Canvas.SetLeft(MinimapArrowImg, ratio * arrowIconPos.X);
            Canvas.SetTop(MinimapArrowImg, ratio * arrowIconPos.Y);

            double iconRadius = MinimapCanvas.Width / 25;
            MinimapEllipse.Width -= 2 * iconRadius;
            MinimapEllipse.Height -= 2 * iconRadius;
            Canvas.SetLeft(MinimapEllipse, iconRadius);
            Canvas.SetTop(MinimapEllipse, iconRadius);
        }

        public void Update()
        {
            RemoveMinimapIcons();

            PosInfo mainCharPos = _core.MainCharEntity.Pos;
            foreach (var entity in _core.QuestEntities)
                addQuestEntityToMinimap(mainCharPos, entity.Pos);

            double rotation = mainCharPos.A / Math.PI * 180;
            RotateTransform transform = new RotateTransform(-rotation);
            MinimapArrowImg.RenderTransform = transform;
        }

        private void addCircle(double angle, double relativeXYDistance)
        {
            double minimapRadius = MinimapEllipse.Width / 2;
            double circleRadius = getIconRadius(relativeXYDistance);
            double distanceFromCenter = relativeXYDistance * minimapRadius;
            double xDistanceFromCenter = Math.Cos(angle) * distanceFromCenter;
            double yDistanceFromCenter = Math.Sin(angle) * distanceFromCenter;
            double xPos = MinimapCanvas.Width / 2 + xDistanceFromCenter - circleRadius;
            double yPos = MinimapCanvas.Height / 2 + yDistanceFromCenter - circleRadius;
            Ellipse myEllipse = new Ellipse();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = getIconColour(relativeXYDistance);
            myEllipse.Fill = mySolidColorBrush;
            myEllipse.Stroke = Brushes.Black;
            myEllipse.StrokeThickness = 1;
            myEllipse.Width = circleRadius * 2;
            myEllipse.Height = circleRadius * 2;
            Canvas.SetTop(myEllipse, xPos);
            Canvas.SetLeft(myEllipse, yPos);
            MinimapCanvas.Children.Add(myEllipse);
        }

        private void addTriangle(double angle, double relativeXYDistance, double zDistance)
        {
            double minimapRadius = MinimapEllipse.Width / 2;
            double circleRadius = getIconRadius(relativeXYDistance);
            double distanceFromCenter = relativeXYDistance * minimapRadius;
            double xDistanceFromCenter = Math.Cos(angle) * distanceFromCenter;
            double yDistanceFromCenter = Math.Sin(angle) * distanceFromCenter;
            double xPos = MinimapCanvas.Width / 2 + xDistanceFromCenter - circleRadius;
            double yPos = MinimapCanvas.Height / 2 + yDistanceFromCenter - circleRadius;

            int zSign = Math.Sign(zDistance);
            double topBtmPointX = circleRadius;
            double topBtmPointY = zDistance > 0 ? 0 : 2 * circleRadius;
            double leftPointX = Math.Cos(zSign * 150 * Math.PI / 180) * circleRadius + circleRadius;
            double leftPointY = Math.Sin(zSign * 150 * Math.PI / 180) * circleRadius + circleRadius;
            double rightPointX = Math.Cos(zSign * 30 * Math.PI / 180) * circleRadius + circleRadius;
            double rightPointY = Math.Sin(zSign * 30 * Math.PI / 180) * circleRadius + circleRadius;

            PointCollection myPointCollection = new PointCollection();
            myPointCollection.Add(new Point(topBtmPointX, topBtmPointY));
            myPointCollection.Add(new Point(leftPointX, leftPointY));
            myPointCollection.Add(new Point(rightPointX, rightPointY));
            myPointCollection.Add(new Point(topBtmPointX, topBtmPointY));

            Polygon myPolygon = new Polygon();
            myPolygon.Points = myPointCollection;
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = getIconColour(relativeXYDistance);
            myPolygon.Fill = mySolidColorBrush;
            myPolygon.Width = circleRadius * 2;
            myPolygon.Height = circleRadius * 2;
            myPolygon.Stretch = Stretch.Fill;
            myPolygon.Stroke = Brushes.Black;
            myPolygon.StrokeThickness = 1;
            Canvas.SetTop(myPolygon, xPos);
            Canvas.SetLeft(myPolygon, yPos);
            MinimapCanvas.Children.Add(myPolygon);
        }

        private Color getIconColour(double relativeXYDistance)
        {
            Color c = relativeXYDistance > 0.9 ? Color.FromArgb(255, 255, 63, 0) : Color.FromArgb(133, 33, 33, 37);
            return c;
        }

        private double getIconRadius(double relativeXYDistance)
        {
            double r = relativeXYDistance > 0.9 ? MinimapCanvas.Width / 25 : MinimapCanvas.Width / 30;
            return r;
        }

        private void addQuestEntityToMinimap(PosInfo mainCharPos, PosInfo entityPos)
        {
            double xyLimit = 60;
            double xDelta = entityPos.X - mainCharPos.X;
            double yDelta = entityPos.Y - mainCharPos.Y;
            double xyDistance = Math.Sqrt(xDelta * xDelta + yDelta * yDelta);
            double relativeXYDistance = Math.Min(xyDistance, xyLimit) / xyLimit;
            double xyAngle = Math.Atan2(xDelta, yDelta);

            double zLimit = 15;
            double zDistance = entityPos.Z - mainCharPos.Z;
            double AbsZDistance = Math.Abs(zDistance);
            double relativeZDistance = Math.Min(AbsZDistance, zLimit) / zLimit;

            if (relativeZDistance > 0.25)
            {
                addTriangle(xyAngle, relativeXYDistance, zDistance);
            }
            else
            {
                addCircle(xyAngle, relativeXYDistance);
            }
        }

        private void RemoveMinimapIcons()
        {
            for (int i = MinimapCanvas.Children.Count - 1; i >= 0; i--)
            {
                MinimapCanvas.Children.RemoveAt(i);
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Resize(_currentRelativeSize);
        }
    }
}
