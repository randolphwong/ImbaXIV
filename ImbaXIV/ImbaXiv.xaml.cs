using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Threading;

namespace ImbaXIV
{
    public partial class MainWindow : Window
    {
        private ImbaXIVCore core;
        private bool minified = false;
        private bool debugMode = true;

        public MainWindow()
        {
            InitializeComponent();
            core = new ImbaXIVCore();
            ToggleDebug();
            ToggleMinify();
            this.Topmost = true;
            Task.Run(CoreWorker);
        }

        private void addCircle(double angle, double relativeXYDistance, double relativeZDistance)
        {
            double minimapRadius = MinimapCanvas.Width / 2;
            double minCircleRadius = MinimapCanvas.Width / 50;
            double maxCircleRadius = MinimapCanvas.Width / 30;
            double circleRadius = minCircleRadius + (maxCircleRadius - minCircleRadius) * (1 - relativeZDistance);
            double distanceFromCenter = relativeXYDistance * minimapRadius;
            double xDistanceFromCenter = Math.Cos(angle) * distanceFromCenter;
            double yDistanceFromCenter = Math.Sin(angle) * distanceFromCenter;
            double xPos = MinimapCanvas.Width / 2 + xDistanceFromCenter - circleRadius;
            double yPos = MinimapCanvas.Height / 2 + yDistanceFromCenter - circleRadius;
            Ellipse myEllipse = new Ellipse();
            SolidColorBrush mySolidColorBrush = new SolidColorBrush();
            mySolidColorBrush.Color = Color.FromArgb(133, 33, 33, 37);
            myEllipse.Fill = mySolidColorBrush;
            myEllipse.Width = circleRadius * 2;
            myEllipse.Height = circleRadius * 2;
            Canvas.SetTop(myEllipse, xPos);
            Canvas.SetLeft(myEllipse, yPos);
            MinimapCanvas.Children.Add(myEllipse);
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
            double zDistance = Math.Abs(entityPos.Z - mainCharPos.Z);
            double relativeZDistance = Math.Min(zDistance, zLimit) / zLimit;

            addCircle(xyAngle, relativeXYDistance, relativeZDistance);
        }

        private void RemoveMinimapIcons()
        {
            for (int i = MinimapCanvas.Children.Count - 1; i >= 0; i--)
            {
                MinimapCanvas.Children.RemoveAt(i);
            }
        }

        private void eventloop()
        {
            if (!core.IsAttached)
            {
                if (!core.AttachProcess())
                {
                    return;
                }
            }
            if (!core.Update())
            {
                return;
            }
            RemoveMinimapIcons();

            PosInfo mainCharPos = core.MainCharEntity.Pos;
            MainCharPosTextBox.Text = $"{mainCharPos.X,4:N1} {mainCharPos.Y,4:N1} {mainCharPos.Z,4:N1}";
            StructCTextBox.Text = core.TargetInfo;

            String questEntityText = "";
            foreach (var entity in core.QuestEntities)
            {
                addQuestEntityToMinimap(mainCharPos, entity.Pos);
                questEntityText += $"{entity.Name}: {entity.Pos.X,4:N1} {entity.Pos.Y,4:N1} {entity.Pos.Z,4:N1}\n";
            }
            QuestEntitiesTextBox.Text = questEntityText;

            double rotation = mainCharPos.A / Math.PI * 180;
            RotateTransform transform = new RotateTransform(-rotation);
            MinimapArrowImg.RenderTransform = transform;
        }

        private void CoreWorker()
        {
            while (true)
            {
                Dispatcher.Invoke(eventloop);
                Thread.Sleep(25);
            }
        }

        private void TargetsBtn_Click(object sender, RoutedEventArgs e)
        {
            String[] targets;

            if (TargetsTextBox.Text == "")
            {
                targets = new string[0];
            }
            else
            {
                targets = TargetsTextBox.Text.Split(',');
            }
            core.Targets = targets;
        }


        private void AlwaysOnTopMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Topmost = !this.Topmost;
            if (this.Topmost)
            {
                AlwaysOnTopMenuItem.Header = "Disable always on top";
            }
            else
            {
                AlwaysOnTopMenuItem.Header = "Enable always on top";
            }
        }

        private void ToggleMinify()
        {
            minified = !minified;
            if (minified)
            {
                this.Width = 238;
            }
            else
            {
                this.Width = 628;
            }
        }

        private void MinifyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ToggleMinify();
            if (minified)
            {
                MinifyMenuItem.Header = "Show full window";
            }
            else
            {
                MinifyMenuItem.Header = "Show minimap only";
            }
        }

        private void ToggleDebug()
        {
            debugMode = !debugMode;
            if (debugMode)
            {
                DebugGrid.Visibility = Visibility.Visible;
                QuestEntitiesTextBox.Height = 60;
            }
            else
            {
                DebugGrid.Visibility = Visibility.Hidden;
                QuestEntitiesTextBox.Height = 188;
            }
        }

        private void DebugMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ToggleDebug();
            if (debugMode)
            {
                DebugMenuItem.Header = "Disable debug mode";
            }
            else
            {
                DebugMenuItem.Header = "Enable debug mode";
            }
        }
    }
}
