using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
        public ImbaXIVCore Core { get; set; }

        private Point arrowIconPos;
        private Point arrowIconSize;
        private Point canvasSize;

        private HideReason hideReason;

        public MinimapWindow()
        {
            InitializeComponent();
            arrowIconPos.X = Canvas.GetLeft(MinimapArrowImg);
            arrowIconPos.Y = Canvas.GetTop(MinimapArrowImg);
            arrowIconSize.X = MinimapArrowImg.Width;
            arrowIconSize.Y = MinimapArrowImg.Height;
            canvasSize.X = MinimapCanvas.Width;
            canvasSize.Y = MinimapCanvas.Height;
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
        }

        public void Update()
        {
            RemoveMinimapIcons();

            PosInfo mainCharPos = Core.MainCharEntity.Pos;
            foreach (var entity in Core.QuestEntities)
                addQuestEntityToMinimap(mainCharPos, entity.Pos);

            double rotation = mainCharPos.A / Math.PI * 180;
            RotateTransform transform = new RotateTransform(-rotation);
            MinimapArrowImg.RenderTransform = transform;
        }

        private void addCircle(double angle, double relativeXYDistance)
        {
            double minimapRadius = MinimapCanvas.Width / 2;
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
            myEllipse.Width = circleRadius * 2;
            myEllipse.Height = circleRadius * 2;
            Canvas.SetTop(myEllipse, xPos);
            Canvas.SetLeft(myEllipse, yPos);
            MinimapCanvas.Children.Add(myEllipse);
        }

        private void addTriangle(double angle, double relativeXYDistance, double zDistance)
        {
            double minimapRadius = MinimapCanvas.Width / 2;
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
            Canvas.SetTop(myPolygon, xPos);
            Canvas.SetLeft(myPolygon, yPos);
            MinimapCanvas.Children.Add(myPolygon);
        }

        private Color getIconColour(double relativeXYDistance)
        {
            Color c = relativeXYDistance > 0.9 ? Color.FromArgb(255, 112, 84, 109) : Color.FromArgb(133, 33, 33, 37);
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
    }
}
