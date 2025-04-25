using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using Microsoft.Xaml.Behaviors;
using System.Windows.Controls;
using System.Windows;

namespace WpfPrismFrameworkTemplate.Helper
{
    public class RichTextBoxBindingBehavior : Behavior<RichTextBox>
    {
        public static readonly DependencyProperty DocumentProperty =
            DependencyProperty.Register("Document", typeof(FlowDocument),
            typeof(RichTextBoxBindingBehavior),
            new PropertyMetadata(null, DocumentPropertyChanged));

        public FlowDocument Document
        {
            get { return (FlowDocument)GetValue(DocumentProperty); }
            set { SetValue(DocumentProperty, value); }
        }

        private static void DocumentPropertyChanged(DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as RichTextBoxBindingBehavior;
            if (behavior != null && behavior.AssociatedObject != null)
            {
                behavior.AssociatedObject.Document = e.NewValue as FlowDocument;
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (Document != null)
            {
                AssociatedObject.Document = Document;
            }
        }
    }
}
