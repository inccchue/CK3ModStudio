using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace WpfPrismFrameworkTemplate.Model
{
    // 简单的输入对话框类
    public class InputDialog : Window
    {
        private TextBox _textBox;

        public string Answer
        {
            get { return _textBox.Text; }
        }

        public InputDialog(string title, string initialText)
        {
            this.Title = title;
            this.Width = 300;
            this.Height = 150;
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // 创建布局
            Grid grid = new Grid();
            grid.Margin = new Thickness(10);
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            // 创建文本框
            _textBox = new TextBox { Text = initialText, AcceptsReturn = true };
            grid.Children.Add(_textBox);
            Grid.SetRow(_textBox, 0);

            // 创建按钮面板
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 10, 0, 0)
            };

            // 创建确定按钮
            Button okButton = new Button
            {
                Content = "确定",
                Width = 75,
                Height = 25,
                Margin = new Thickness(0, 0, 10, 0)
            };
            okButton.Click += (s, e) => { this.DialogResult = true; };
            buttonPanel.Children.Add(okButton);

            // 创建取消按钮
            Button cancelButton = new Button
            {
                Content = "取消",
                Width = 75,
                Height = 25
            };
            cancelButton.Click += (s, e) => { this.DialogResult = false; };
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(buttonPanel);
            Grid.SetRow(buttonPanel, 1);

            this.Content = grid;
        }
    }
}
